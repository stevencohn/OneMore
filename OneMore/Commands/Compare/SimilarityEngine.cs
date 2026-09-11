//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Compare
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// One weighted rubric's contribution to a SimilarityResult, along with a plain-language
	/// explanation of the score for display in the Compare Contents popup.
	/// </summary>
	internal class RubricScore
	{
		public string Name { get; set; }

		public double Weight { get; set; }

		public double Score { get; set; }

		public string Reasoning { get; set; }
	}


	/// <summary>
	/// The result of comparing two pages' content: an overall weighted score plus the
	/// individual rubric breakdown that produced it.
	/// </summary>
	internal class SimilarityResult
	{
		public double Overall { get; set; }

		public List<RubricScore> Rubrics { get; } = new();
	}


	/// <summary>
	/// The individual similarity rubrics SimilarityEngine can score. Compare Hierarchy always
	/// uses SimilarityOptions.Default (everything but Media); Remove Duplicate Pages lets the
	/// user pick a subset via checkboxes, including the opt-in Media rubric.
	/// </summary>
	[Flags]
	internal enum Rubric
	{
		None = 0,
		TfIdf = 1,
		Lexical = 2,
		Structural = 4,
		Stylistic = 8,
		Entity = 16,
		Media = 32
	}


	/// <summary>
	/// Which rubrics participate in a Compare call. Weights are fixed per rubric (see
	/// SimilarityEngine's *Weight constants) and renormalize across whichever rubrics are
	/// enabled, so disabling a rubric never changes the relative balance of the rest.
	/// </summary>
	internal readonly struct SimilarityOptions
	{
		public SimilarityOptions(Rubric enabled)
		{
			Enabled = enabled;
		}

		public Rubric Enabled { get; }

		public bool Has(Rubric rubric) => (Enabled & rubric) == rubric;

		/// <summary>
		/// The five rubrics Compare Hierarchy has always used. Their base weights already sum
		/// to 1.0, so renormalizing this set is a no-op - Compare Hierarchy's scores are
		/// unaffected by Media's addition to the engine.
		/// </summary>
		public static SimilarityOptions Default { get; } =
			new(Rubric.TfIdf | Rubric.Lexical | Rubric.Structural | Rubric.Stylistic | Rubric.Entity);

		public static SimilarityOptions All { get; } = new(Default.Enabled | Rubric.Media);
	}


	/// <summary>
	/// Everything extracted from a single Page that SimilarityEngine needs to score it against
	/// another page's profile. Building this once per candidate page - rather than re-extracting
	/// from raw Page/XElement data on every pairwise Compare call - is what makes an O(k^2)
	/// near-duplicate pass (Remove Duplicate Pages) affordable.
	/// </summary>
	internal class SimilarityProfile
	{
		public string Text { get; set; } = string.Empty;

		public List<string> TokenList { get; set; } = new();

		public HashSet<string> TokenSet { get; set; } = new(StringComparer.Ordinal);

		public HashSet<string> HeadingNames { get; set; } = new(StringComparer.Ordinal);

		public int ParagraphCount { get; set; }

		public HashSet<string> Entities { get; set; } = new(StringComparer.Ordinal);

		/// <summary>
		/// One hash per embedded one:Image/one:Data element. Empty unless the page was fetched
		/// with OneNote.PageDetail.BinaryData - same "nothing to compare" convention every other
		/// rubric already uses for an absent signal.
		/// </summary>
		public HashSet<string> ImageHashes { get; set; } = new(StringComparer.Ordinal);
	}


	/// <summary>
	/// A pure C#, local-only, no-AI page-content similarity scorer used by the Compare
	/// Hierarchy command's "Compare contents..." page action and by Remove Duplicate Pages'
	/// near-duplicate pass. Combines up to six deterministic rubrics - TF-IDF cosine, lexical
	/// (Jaccard) overlap, structural, stylistic, named entity, and embedded-media similarity -
	/// into a single weighted score, per the blueprint in
	/// specs/design_handoff_compare_hierarchy/Compare.md. This is a scoring aid, not a
	/// diff/merge tool: it never mutates either page's hierarchy, though profile-building does
	/// mutate the in-memory Page.Root passed to it (see BuildProfile).
	/// </summary>
	internal static class SimilarityEngine
	{
		private const double TfIdfWeight = 0.45;
		private const double LexicalWeight = 0.20;
		private const double StructuralWeight = 0.15;
		private const double StylisticWeight = 0.10;
		private const double EntityWeight = 0.10;

		// deliberately excluded from SimilarityOptions.Default: Media only matters once a
		// pair's text-based rubrics already read as near-identical, and it requires the more
		// expensive PageDetail.BinaryData page fetch, so it's opt-in only
		private const double MediaWeight = 0.15;

		private static readonly Regex WordPattern =
			new(@"[\p{L}\p{Nd}]+", RegexOptions.Compiled);

		private static readonly Regex CaseSensitiveWordPattern =
			new(@"\b[A-Za-z][A-Za-z0-9]*\b", RegexOptions.Compiled);

		private static readonly Regex DottedIdentifierPattern =
			new(@"\b[A-Za-z_]\w*(?:\.[A-Za-z_]\w*){1,4}\b", RegexOptions.Compiled);

		private static readonly Regex SentenceSplitPattern =
			new(@"[.!?]+", RegexOptions.Compiled);

		private static readonly Regex PassiveVoicePattern = new(
			@"\b(?:is|are|was|were|be|been|being)\s+\w+ed\b",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);


		/// <summary>
		/// Extracts everything SimilarityEngine needs to score this page against another, in
		/// one pass. Mutates page.Root the same way the old inline Compare did (TextValue(true)
		/// strips HTML from the XML it reads), so callers that also need the original,
		/// unmutated XML (e.g. Remove Duplicate Pages' exact-hash pass) must capture that first.
		/// </summary>
		/// <param name="page">The page to profile</param>
		/// <param name="one">Forwarded to Page.GetHeadings, which is called with linked:false
		/// here, so this is never actually dereferenced; a null instance is fine</param>
		public static SimilarityProfile BuildProfile(Page page, OneNote one)
		{
			var imageHashes = ExtractImageHashes(page);

			// TextValue(true) mutates the XML it strips HTML from (see RemoveDuplicatesCommand's
			// own use of it) - do it before any extraction that assumes the original
			// mixed-content shape, but after ImageHashes, which reads Image/Data elements that
			// TextValue never touches
			var text = page.Root.TextValue(true) ?? string.Empty;
			var tokenList = Tokenize(text);

			var headingNames = new HashSet<string>(
				SafeGetHeadings(page, one).Select(NormalizeHeading), StringComparer.Ordinal);

			return new SimilarityProfile
			{
				Text = text,
				TokenList = tokenList,
				TokenSet = new HashSet<string>(tokenList, StringComparer.Ordinal),
				HeadingNames = headingNames,
				ParagraphCount = CountParagraphs(page),
				Entities = ExtractEntities(text),
				ImageHashes = imageHashes
			};
		}


		/// <summary>
		/// Scores two already-built profiles against each other using only the rubrics named
		/// in <paramref name="options"/>. Each returned RubricScore's Weight is the *renormalized*
		/// weight actually used to compute Overall (base weight / sum of enabled base weights),
		/// so the popup's "wt NN%" display always matches what was actually calculated.
		/// </summary>
		public static SimilarityResult Compare(
			SimilarityProfile left, SimilarityProfile right, SimilarityOptions options)
		{
			var rubrics = new List<RubricScore>();

			if (options.Has(Rubric.TfIdf))
			{
				rubrics.Add(ScoreTfIdfCosine(left.TokenList, right.TokenList));
			}

			if (options.Has(Rubric.Lexical))
			{
				rubrics.Add(ScoreLexicalJaccard(left.TokenSet, right.TokenSet));
			}

			if (options.Has(Rubric.Structural))
			{
				rubrics.Add(ScoreStructural(left, right));
			}

			if (options.Has(Rubric.Stylistic))
			{
				rubrics.Add(ScoreStylistic(left.Text, right.Text));
			}

			if (options.Has(Rubric.Entity))
			{
				rubrics.Add(ScoreEntities(left.Entities, right.Entities));
			}

			if (options.Has(Rubric.Media))
			{
				rubrics.Add(ScoreMedia(left, right));
			}

			var totalWeight = rubrics.Sum(r => r.Weight);
			if (totalWeight > 0)
			{
				foreach (var rubric in rubrics)
				{
					rubric.Weight /= totalWeight;
				}
			}

			var result = new SimilarityResult { Overall = rubrics.Sum(r => r.Weight * r.Score) };
			result.Rubrics.AddRange(rubrics);
			return result;
		}


		/// <summary>
		/// Scores the content similarity of two pages using SimilarityOptions.Default (the
		/// original five rubrics, no Media) - unchanged from before Media/profiles existed, so
		/// Compare Hierarchy's behavior and scores are unaffected by this engine's expansion.
		/// </summary>
		/// <param name="left">The source (left) page</param>
		/// <param name="right">The target (right) page</param>
		/// <param name="one">Forwarded to BuildProfile/Page.GetHeadings; a null instance is fine</param>
		public static SimilarityResult Compare(Page left, Page right, OneNote one)
		{
			return Compare(BuildProfile(left, one), BuildProfile(right, one), SimilarityOptions.Default);
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Lexical...

		private static List<string> Tokenize(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return new List<string>();
			}

			return WordPattern.Matches(text)
				.Cast<Match>()
				.Select(m => m.Value.ToLowerInvariant())
				.Where(w => w.Length > 1)
				.ToList();
		}


		private static RubricScore ScoreLexicalJaccard(HashSet<string> a, HashSet<string> b)
		{
			double score;
			string reasoning;

			if (a.Count == 0 && b.Count == 0)
			{
				score = 1.0;
				reasoning = Resx.Similarity_reasonEmptyBoth;
			}
			else
			{
				var intersection = a.Count(b.Contains);
				var union = a.Count + b.Count - intersection;
				score = union == 0 ? 0.0 : (double)intersection / union;
				reasoning = string.Format(Resx.Similarity_reasonLexical, intersection, union);
			}

			return new RubricScore
			{
				Name = Resx.Similarity_rubricLexical,
				Weight = LexicalWeight,
				Score = score,
				Reasoning = reasoning
			};
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// TF-IDF cosine...

		// with only two documents to compare, naive IDF (log(N/df)) zeroes out every term
		// shared by both - exactly the terms most useful for "these are similar" - so this
		// uses smoothed IDF (log((N+1)/(df+1))+1) instead, which still favors terms unique
		// to one side but never fully discounts shared vocabulary
		private static RubricScore ScoreTfIdfCosine(List<string> tokensA, List<string> tokensB)
		{
			if (tokensA.Count == 0 && tokensB.Count == 0)
			{
				return new RubricScore
				{
					Name = Resx.Similarity_rubricTfIdf,
					Weight = TfIdfWeight,
					Score = 1.0,
					Reasoning = Resx.Similarity_reasonEmptyBoth
				};
			}

			if (tokensA.Count == 0 || tokensB.Count == 0)
			{
				return new RubricScore
				{
					Name = Resx.Similarity_rubricTfIdf,
					Weight = TfIdfWeight,
					Score = 0.0,
					Reasoning = Resx.Similarity_reasonOneEmpty
				};
			}

			var countsA = tokensA.GroupBy(t => t).ToDictionary(g => g.Key, g => g.Count());
			var countsB = tokensB.GroupBy(t => t).ToDictionary(g => g.Key, g => g.Count());

			var vocabulary = new HashSet<string>(countsA.Keys, StringComparer.Ordinal);
			vocabulary.UnionWith(countsB.Keys);

			double dot = 0, magA = 0, magB = 0;
			var shared = 0;

			foreach (var term in vocabulary)
			{
				var hasA = countsA.TryGetValue(term, out var ca);
				var hasB = countsB.TryGetValue(term, out var cb);
				var df = (hasA ? 1 : 0) + (hasB ? 1 : 0);
				var idf = Math.Log(3.0 / (df + 1)) + 1.0;

				var wa = (hasA ? 1.0 + Math.Log(ca) : 0.0) * idf;
				var wb = (hasB ? 1.0 + Math.Log(cb) : 0.0) * idf;

				dot += wa * wb;
				magA += wa * wa;
				magB += wb * wb;

				if (hasA && hasB)
				{
					shared++;
				}
			}

			var score = magA <= 0 || magB <= 0 ? 0.0 : dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
			score = Math.Max(0.0, Math.Min(1.0, score));

			return new RubricScore
			{
				Name = Resx.Similarity_rubricTfIdf,
				Weight = TfIdfWeight,
				Score = score,
				Reasoning = string.Format(Resx.Similarity_reasonTfIdf, shared, vocabulary.Count)
			};
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Structural...

		private static RubricScore ScoreStructural(SimilarityProfile a, SimilarityProfile b)
		{
			var sharedHeadings = a.HeadingNames.Count(b.HeadingNames.Contains);

			double headingScore;
			if (a.HeadingNames.Count == 0 && b.HeadingNames.Count == 0)
			{
				headingScore = 1.0;
			}
			else
			{
				var union = a.HeadingNames.Count + b.HeadingNames.Count - sharedHeadings;
				headingScore = union == 0 ? 0.0 : (double)sharedHeadings / union;
			}

			var maxCount = Math.Max(a.ParagraphCount, b.ParagraphCount);
			var countScore = maxCount == 0
				? 1.0 : 1.0 - (Math.Abs(a.ParagraphCount - b.ParagraphCount) / (double)maxCount);

			return new RubricScore
			{
				Name = Resx.Similarity_rubricStructural,
				Weight = StructuralWeight,
				Score = (headingScore + countScore) / 2.0,
				Reasoning = string.Format(Resx.Similarity_reasonStructural,
					sharedHeadings, Math.Max(a.HeadingNames.Count, b.HeadingNames.Count),
					a.ParagraphCount, b.ParagraphCount)
			};
		}


		private static List<Heading> SafeGetHeadings(Page page, OneNote one)
		{
			try
			{
				return page.GetHeadings(one, linked: false);
			}
			catch
			{
				// headings are a nice-to-have signal, not essential; fall back to "no
				// headings" rather than failing the whole comparison
				return new List<Heading>();
			}
		}


		private static string NormalizeHeading(Heading heading)
		{
			return (heading.Text ?? string.Empty).Trim().ToLowerInvariant();
		}


		private static int CountParagraphs(Page page)
		{
			return page.Root.Descendants(page.Namespace + "OE")
				.Count(oe => oe.Elements(page.Namespace + "T")
					.Any(t => !string.IsNullOrWhiteSpace((string)t)));
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Stylistic...

		private static RubricScore ScoreStylistic(string textA, string textB)
		{
			var avgA = AverageSentenceLength(textA);
			var avgB = AverageSentenceLength(textB);

			var maxAvg = Math.Max(avgA, avgB);
			var lengthScore = maxAvg.EstEquals(0.0, double.Epsilon)
				? 1.0 : 1.0 - (Math.Abs(avgA - avgB) / maxAvg);

			var passiveA = PassiveVoiceFrequency(textA);
			var passiveB = PassiveVoiceFrequency(textB);
			var maxPassive = Math.Max(passiveA, passiveB);
			var passiveScore = maxPassive.EstEquals(0.0, double.Epsilon)
				? 1.0 : 1.0 - (Math.Abs(passiveA - passiveB) / maxPassive);

			return new RubricScore
			{
				Name = Resx.Similarity_rubricStylistic,
				Weight = StylisticWeight,
				Score = (lengthScore + passiveScore) / 2.0,
				Reasoning = string.Format(Resx.Similarity_reasonStylistic,
					Math.Round(avgA, 1), Math.Round(avgB, 1))
			};
		}


		private static double AverageSentenceLength(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return 0;
			}

			var sentences = SentenceSplitPattern.Split(text)
				.Select(s => s.Trim())
				.Where(s => s.Length > 0)
				.ToList();

			return sentences.Count == 0
				? 0
				: sentences.Sum(s => Tokenize(s).Count) / (double)sentences.Count;
		}


		// occurrences per 100 words, so documents of different lengths are comparable
		private static double PassiveVoiceFrequency(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return 0;
			}

			var wordCount = Tokenize(text).Count;
			return wordCount == 0 ? 0 : PassiveVoicePattern.Matches(text).Count / (double)wordCount * 100.0;
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Named entities...

		private static RubricScore ScoreEntities(HashSet<string> entitiesA, HashSet<string> entitiesB)
		{
			var shared = entitiesA.Count(entitiesB.Contains);

			double score;
			if (entitiesA.Count == 0 && entitiesB.Count == 0)
			{
				score = 1.0;
			}
			else
			{
				var union = entitiesA.Count + entitiesB.Count - shared;
				score = union == 0 ? 0.0 : (double)shared / union;
			}

			return new RubricScore
			{
				Name = Resx.Similarity_rubricEntity,
				Weight = EntityWeight,
				Score = score,
				Reasoning = string.Format(Resx.Similarity_reasonEntity,
					shared, Math.Max(entitiesA.Count, entitiesB.Count))
			};
		}


		// case-sensitive on purpose: PascalCase/camelCase/ALLCAPS and capitalized-word
		// heuristics all depend on preserving original casing
		private static HashSet<string> ExtractEntities(string text)
		{
			var set = new HashSet<string>(StringComparer.Ordinal);
			if (string.IsNullOrWhiteSpace(text))
			{
				return set;
			}

			foreach (Match match in DottedIdentifierPattern.Matches(text))
			{
				set.Add(match.Value);
			}

			foreach (Match match in CaseSensitiveWordPattern.Matches(text))
			{
				if (IsEntityLike(match.Value))
				{
					set.Add(match.Value);
				}
			}

			return set;
		}


		private static bool IsEntityLike(string word)
		{
			if (word.Length < 2)
			{
				return false;
			}

			// ALLCAPS acronym (API, XML), Capitalized/PascalCase word, or camelCase
			// identifier (an internal capital after a lowercase start)
			return word.All(char.IsUpper)
				|| char.IsUpper(word[0])
				|| word.Skip(1).Any(char.IsUpper);
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Embedded media...

		private static RubricScore ScoreMedia(SimilarityProfile a, SimilarityProfile b)
		{
			double score;
			string reasoning;

			if (a.ImageHashes.Count == 0 && b.ImageHashes.Count == 0)
			{
				score = 1.0;
				reasoning = Resx.Similarity_reasonEmptyBoth;
			}
			else
			{
				var shared = a.ImageHashes.Count(b.ImageHashes.Contains);
				var union = a.ImageHashes.Count + b.ImageHashes.Count - shared;
				score = union == 0 ? 0.0 : (double)shared / union;
				reasoning = string.Format(Resx.Similarity_reasonMedia,
					shared, Math.Max(a.ImageHashes.Count, b.ImageHashes.Count));
			}

			return new RubricScore
			{
				Name = Resx.Similarity_rubricMedia,
				Weight = MediaWeight,
				Score = score,
				Reasoning = reasoning
			};
		}


		/// <summary>
		/// OneMore Extension >> Hashes each embedded image's base64 payload rather than storing
		/// it verbatim, keeping a page with many/large images cheap to carry on a SimilarityProfile.
		/// Only ever non-empty when the page was fetched with OneNote.PageDetail.BinaryData.
		/// </summary>
		private static HashSet<string> ExtractImageHashes(Page page)
		{
			var set = new HashSet<string>(StringComparer.Ordinal);

			var payloads = page.Root.Descendants(page.Namespace + "Image")
				.Elements(page.Namespace + "Data")
				.Select(e => (string)e)
				.Where(v => !string.IsNullOrEmpty(v))
				.ToList();

			if (payloads.Count == 0)
			{
				return set;
			}

			using var hasher = new SHA1CryptoServiceProvider();
			foreach (var payload in payloads)
			{
				set.Add(Convert.ToBase64String(hasher.ComputeHash(Encoding.UTF8.GetBytes(payload))));
			}

			return set;
		}
	}
}
