//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Compare
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
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
	/// A pure C#, local-only, no-AI page-content similarity scorer used by the Compare
	/// Hierarchy command's "Compare contents..." page action. Combines five deterministic
	/// rubrics - TF-IDF cosine, lexical (Jaccard) overlap, structural, stylistic, and named
	/// entity similarity - into a single weighted score, per the blueprint in
	/// specs/design_handoff_compare_hierarchy/Compare.md. This is a scoring aid, not a
	/// diff/merge tool: it never mutates either page.
	/// </summary>
	internal static class SimilarityEngine
	{
		private const double TfIdfWeight = 0.45;
		private const double LexicalWeight = 0.20;
		private const double StructuralWeight = 0.15;
		private const double StylisticWeight = 0.10;
		private const double EntityWeight = 0.10;

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
		/// Scores the content similarity of two pages, weighting five deterministic rubrics
		/// per the blueprint's table (TF-IDF cosine 0.45, lexical 0.20, structural 0.15,
		/// stylistic 0.10, entity 0.10).
		/// </summary>
		/// <param name="left">The source (left) page</param>
		/// <param name="right">The target (right) page</param>
		/// <param name="one">Forwarded to Page.GetHeadings, which is called with linked:false
		/// here, so this is never actually dereferenced; a null instance is fine</param>
		/// <returns>The overall score and its rubric-by-rubric breakdown</returns>
		public static SimilarityResult Compare(Page left, Page right, OneNote one)
		{
			// TextValue(true) mutates the XML it strips HTML from (see RemoveDuplicatesCommand's
			// own use of it), which is fine here since these Page instances exist only for this
			// one comparison and are never saved back
			var textA = left.Root.TextValue(true) ?? string.Empty;
			var textB = right.Root.TextValue(true) ?? string.Empty;

			var tokenListA = Tokenize(textA);
			var tokenListB = Tokenize(textB);
			var tokenSetA = new HashSet<string>(tokenListA, StringComparer.Ordinal);
			var tokenSetB = new HashSet<string>(tokenListB, StringComparer.Ordinal);

			var rubrics = new List<RubricScore>
			{
				ScoreTfIdfCosine(tokenListA, tokenListB),
				ScoreLexicalJaccard(tokenSetA, tokenSetB),
				ScoreStructural(left, right, one),
				ScoreStylistic(textA, textB),
				ScoreEntities(textA, textB)
			};

			var result = new SimilarityResult { Overall = rubrics.Sum(r => r.Weight * r.Score) };
			result.Rubrics.AddRange(rubrics);
			return result;
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

		private static RubricScore ScoreStructural(Page left, Page right, OneNote one)
		{
			var headingsA = SafeGetHeadings(left, one);
			var headingsB = SafeGetHeadings(right, one);

			var namesA = new HashSet<string>(
				headingsA.Select(NormalizeHeading), StringComparer.Ordinal);
			var namesB = new HashSet<string>(
				headingsB.Select(NormalizeHeading), StringComparer.Ordinal);

			var sharedHeadings = namesA.Count(namesB.Contains);

			double headingScore;
			if (namesA.Count == 0 && namesB.Count == 0)
			{
				headingScore = 1.0;
			}
			else
			{
				var union = namesA.Count + namesB.Count - sharedHeadings;
				headingScore = union == 0 ? 0.0 : (double)sharedHeadings / union;
			}

			var countA = CountParagraphs(left);
			var countB = CountParagraphs(right);
			var maxCount = Math.Max(countA, countB);
			var countScore = maxCount == 0 ? 1.0 : 1.0 - (Math.Abs(countA - countB) / (double)maxCount);

			return new RubricScore
			{
				Name = Resx.Similarity_rubricStructural,
				Weight = StructuralWeight,
				Score = (headingScore + countScore) / 2.0,
				Reasoning = string.Format(Resx.Similarity_reasonStructural,
					sharedHeadings, Math.Max(namesA.Count, namesB.Count), countA, countB)
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

		private static RubricScore ScoreEntities(string textA, string textB)
		{
			var entitiesA = ExtractEntities(textA);
			var entitiesB = ExtractEntities(textB);

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
	}
}
