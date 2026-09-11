//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Compare;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// Analyze pages in a given context, scanning for duplicates and close-matches and lets
	/// the user cherrypick which duplicates to delete
	/// </summary>
	internal class RemoveDuplicatesCommand : Command
	{
		internal sealed class HashNode
		{
			public string GroupID;
			public string PageID;
			public string XmlHash;
			public string TextHash;
			public string Title;
			public string Path;
			public string Link;
			public DateTime LastModified;
			public MatchKind MatchKind = MatchKind.Exact;

			// full rubric breakdown for a Similar match, so the results dialog's popup can
			// show it without recomputing anything; null for an Exact match (hashing already
			// proved identity, nothing to break down) and for unmatched singletons
			public SimilarityResult Result;

			// transient: built only when detectSimilar is on, for candidates that survive the
			// exact-hash pass; cleared once the near-duplicate pass finishes (see Scan)
			public SimilarityProfile Profile;

			public List<HashNode> Siblings = new();

			public double? Similarity => Result?.Overall;
		}


		internal enum MatchKind
		{
			Exact,
			Similar
		}

		// minimum overall SimilarityEngine score (0..1) for the optional near-duplicate pass
		// to group a pair as "similar" - a grouping threshold, independent of however the
		// results dialog colors/displays that score
		private const double SimilarityThreshold = 0.85;

		// skip the O(k^2) near-duplicate pass entirely above this many unique pages
		private const int MaxSimilarityCandidates = 2000;

		private OneNote one;
		private XNamespace ns;
		private readonly SHA1CryptoServiceProvider hasher;
		private readonly List<HashNode> hashes;
		private UI.ProgressDialog progress;

		private UI.SelectorScope scope;
		private bool detectSimilar;
		private IEnumerable<string> books;
		private Rubric enabledRubrics;
		private int scanCount;


		public RemoveDuplicatesCommand()
		{
			hashes = new List<HashNode>();

			// MD5 should be sufficient and performs best but is not FIPS compliant
			// so use SHA1 instead. Computers are configured to enable/disable FIPS via
			// HKLM\SYSTEM\CurrentControlSet\Control\Lsa\FipsAlgorithmPolicy\Enabled
			hasher = new SHA1CryptoServiceProvider();
		}


		public override async Task Execute(params object[] args)
		{
			using var guard = EnterOnce();
			if (guard is null) { return; }

			DialogResult result;

			using (var dialog = new RemoveDuplicatesDialog())
			{
				result = dialog.ShowDialog(owner);
				if (result != DialogResult.OK)
				{
					return;
				}

				enabledRubrics = dialog.EnabledRubrics;
				scope = dialog.Scope;
				books = dialog.SelectedNotebooks;
				detectSimilar = dialog.DetectSimilar;
			}

			// analyze pages, scanning for duplicates and close matches...

			logger.StartClock();

			using (progress = new UI.ProgressDialog())
			{
				result = progress.ShowDialogWithCancel(
					async (dialog, token) => await Scan(dialog, token));

				if (result != DialogResult.OK)
				{
					return;
				}
			}

			logger.WriteTime($"{hashes.Count} pages have one or more duplicates, scanned {scanCount} pages");

			if (hashes.Count == 0)
			{
				ShowInfo("No duplicate pages were found");
				return;
			}

			// let user cherrypick duplicate pages to delete...
			var navigator = new RemoveDuplicatesNavigator(hashes);
			navigator.RunModeless((sender, e) =>
			{
				var d = sender as RemoveDuplicatesNavigator;
				d.Dispose();
			}, 20);

			await Task.Yield();
		}


		private async Task<bool> Scan(UI.ProgressDialog dialog, CancellationToken token)
		{
			// only the opt-in Media rubric needs the heavier BinaryData fetch; every other
			// rubric only ever looks at extracted text/structure
			var detail = enabledRubrics.HasFlag(Rubric.Media)
				? OneNote.PageDetail.BinaryData
				: OneNote.PageDetail.Basic;

			var empty = new HashNode
			{
				Title = "Empty Pages"
			};

			await using (one = new OneNote(out _, out ns))
			{
				var hierarchy = await BuildHierarchy(scope, books);
				dialog.SetMaximum(hierarchy.Elements().Count());

				var pageRefs = hierarchy.Descendants(ns + "Page");
				foreach (var pageRef in pageRefs)
				{
					if (token.IsCancellationRequested)
					{
						break;
					}

					var page = await one.GetPage(pageRef.Attribute("ID").Value, detail);

					dialog.SetMessage($"Scanning {page.Title}...");
					dialog.Increment();

					var node = CalculateHash(page, pageRef);
					//logger.WriteLine($"text~ [{node.TextHash}] xml~ [{node.XmlHash}]");

					if (token.IsCancellationRequested)
					{
						break;
					}

					if (node.TextHash == String.Empty)
					{
						empty.Siblings.Add(node);
						continue;
					}

					// when the near-duplicate pass will run (detectSimilar), require BOTH hashes
					// to match for the no-popup "identical" bucket - a TextHash-only match
					// (same visible words, different XML/embedded media) falls through to that
					// scored pass instead, so the Media rubric gets a chance to catch it. With
					// "Show only exact duplicates" checked, that pass never runs at all, so
					// there's nothing left to catch a TextHash-only match if this still
					// required XmlHash too - it would just silently vanish instead of showing
					// up as exact; fall back to TextHash alone (content identity) in that mode
					var sibling = detectSimilar
						? hashes.Find(n => n.TextHash == node.TextHash && n.XmlHash == node.XmlHash)
						: hashes.Find(n => n.TextHash == node.TextHash);

					if (sibling != null)
					{
						//logger.WriteLine($"= [{node.Title}] with [{sibling.Title}]");
						node.GroupID = sibling.GroupID;
						sibling.Siblings.Add(node);
					}
					else
					{
						//logger.WriteLine($"+ [{node.Title}]");
						node.GroupID = node.PageID;
						hashes.Add(node);
					}

					scanCount++;
				}

				if (!token.IsCancellationRequested && detectSimilar)
				{
					ScoreNearDuplicates(dialog, token);
				}

				if (!token.IsCancellationRequested)
				{
					dialog.SetMessage("Pruning results...");
					hashes.RemoveAll(n => !n.Siblings.Any());

					foreach (var node in hashes)
					{
						node.Profile = null;
						foreach (var sibling in node.Siblings)
						{
							sibling.Profile = null;
						}
					}

					if (empty.Siblings.Any())
					{
						hashes.Add(empty);
					}

					// covers every surviving node - including the synthetic Empty Pages bucket
					// just added above - regardless of how it matched (exact hash or scored
					// near-duplicate); Path/Link used to only be looked up inline for exact
					// matches, which left near-duplicate rows with no path to show
					await LoadPaths(dialog, token);
				}
			}

			return !token.IsCancellationRequested;
		}


		/// <summary>
		/// OneMore Extension >> Loads the full hierarchy path (and onenote: link) for every
		/// matched node - both group heads and their siblings, whether they matched by exact
		/// hash or scored near-duplicate - so the results dialog can show/hyperlink it.
		/// </summary>
		private async Task LoadPaths(UI.ProgressDialog dialog, CancellationToken token)
		{
			dialog.SetMessage("Loading page paths...");

			foreach (var head in hashes)
			{
				if (token.IsCancellationRequested)
				{
					return;
				}

				if (head.PageID != null && head.Path == null)
				{
					var info = await one.GetPageInfo(head.PageID);
					head.Path = info.Path;
					head.Link = info.Link;
				}

				foreach (var sibling in head.Siblings)
				{
					if (token.IsCancellationRequested)
					{
						return;
					}

					if (sibling.PageID != null && sibling.Path == null)
					{
						var info = await one.GetPageInfo(sibling.PageID);
						sibling.Path = info.Path;
						sibling.Link = info.Link;
					}
				}
			}
		}


		/// <summary>
		/// OneMore Extension >> Second pass, opt-in: scores pages that did not already group by
		/// exact hash match using SimilarityEngine, weighted by whichever metrics the user
		/// enabled in the config dialog, grouping pairs whose overall score clears
		/// SimilarityThreshold as "similar" (non-identical) matches.
		/// </summary>
		private void ScoreNearDuplicates(UI.ProgressDialog dialog, CancellationToken token)
		{
			if (hashes.Count > MaxSimilarityCandidates)
			{
				logger.WriteLine(
					$"skipping near-duplicate pass; {hashes.Count} candidates exceeds " +
					$"the {MaxSimilarityCandidates} limit");
				return;
			}

			dialog.SetMessage("Comparing for similar pages...");
			dialog.SetMaximum(hashes.Count);

			var options = new SimilarityOptions(enabledRubrics);
			var matched = new HashSet<HashNode>();

			for (var i = 0; i < hashes.Count; i++)
			{
				if (token.IsCancellationRequested)
				{
					break;
				}

				dialog.Increment();

				var a = hashes[i];
				if (matched.Contains(a) || a.Profile is null)
				{
					continue;
				}

				for (var j = i + 1; j < hashes.Count; j++)
				{
					var b = hashes[j];
					if (matched.Contains(b) || b.Profile is null)
					{
						continue;
					}

					if (!PassesLengthPrefilter(
						a.Profile.TokenList.Count, b.Profile.TokenList.Count, SimilarityThreshold))
					{
						continue;
					}

					var result = SimilarityEngine.Compare(a.Profile, b.Profile, options);
					if (result.Overall >= SimilarityThreshold)
					{
						b.MatchKind = MatchKind.Similar;
						b.Result = result;
						b.GroupID = a.GroupID;
						a.Siblings.Add(b);
						matched.Add(b);
					}
				}
			}

			hashes.RemoveAll(n => matched.Contains(n));
		}


		/// <summary>
		/// OneMore Extension >> Cheap pre-filter to skip pairs that cannot possibly meet the
		/// similarity threshold before running the more expensive rubric calculation. Lengths
		/// are word (token) counts here rather than character counts, but the ratio math is
		/// identical either way.
		/// </summary>
		internal static bool PassesLengthPrefilter(int lenA, int lenB, double threshold)
		{
			if (lenA == 0 || lenB == 0)
			{
				return false;
			}

			return Math.Abs(lenA - lenB) <= (1.0 - threshold) * Math.Max(lenA, lenB);
		}


		private async Task<XElement> BuildHierarchy(
			UI.SelectorScope scope, IEnumerable<string> books)
		{
			var hierarchy = new XElement("pages");

			switch (scope)
			{
				case UI.SelectorScope.Section:
					(await one.GetSection()).Descendants(ns + "Page")
						.ForEach(p => hierarchy.Add(p));
					break;

				case UI.SelectorScope.Notebook:
					(await one.GetNotebook(OneNote.Scope.Pages)).Descendants(ns + "Page")
						.ForEach(p => hierarchy.Add(p));
					break;

				case UI.SelectorScope.Notebooks:
					(await one.GetNotebooks(OneNote.Scope.Pages)).Descendants(ns + "Page")
						.ForEach(p => hierarchy.Add(p));
					break;

				default:
					(await BuildSelectedHierarchy(books))
						.ForEach(p => hierarchy.Add(p));
					break;
			}

			// remove recyclebin nodes
			hierarchy.Descendants()
				.Where(n => n.Name.LocalName == "UnfiledNotes" ||
							n.Attribute("isRecycleBin") != null ||
							n.Attribute("isInRecycleBin") != null)
				.Remove();

			return hierarchy;
		}


		private async Task<IEnumerable<XElement>> BuildSelectedHierarchy(IEnumerable<string> books)
		{
			var pages = new List<XElement>();
			foreach (var id in books)
			{
				var book = await one.GetNotebook(id, OneNote.Scope.Pages);
				pages.AddRange(book.Descendants(ns + "Page"));
			}

			return pages;
		}


		private HashNode CalculateHash(Page page, XElement pageRef)
		{
			var node = new HashNode
			{
				PageID = page.PageId,
				Title = page.Title
			};

			// use the hierarchy's lastModifiedTime, not the page-content one, which
			// OneNote stamps with the current time on every GetPageContent call
			var modified = pageRef.Attribute("lastModifiedTime")?.Value;

			// RoundtripKind preserves the UTC "Z" suffix OneNote writes as DateTimeKind.Utc,
			// same as HierarchyDiff.cs's own parse - needed so ToShortFriendlyString() below
			// actually converts to local time instead of silently leaving Kind=Unspecified
			node.LastModified = string.IsNullOrEmpty(modified)
				? DateTime.MinValue
				: DateTime.Parse(modified, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

			// EditedByAttributes and the page ID
			page.Root.DescendantsAndSelf().Attributes().Where(a =>
				a.Name.LocalName == "ID"
				|| a.Name.LocalName == "dateTime"
				|| a.Name.LocalName == "callbackID"
				|| a.Name.LocalName == "author"
				|| a.Name.LocalName == "authorInitials"
				|| a.Name.LocalName == "authorResolutionID"
				|| a.Name.LocalName == "lastModifiedBy"
				|| a.Name.LocalName == "lastModifiedByInitials"
				|| a.Name.LocalName == "lastModifiedByResolutionID"
				|| a.Name.LocalName == "creationTime"
				|| a.Name.LocalName == "lastModifiedTime"
				|| a.Name.LocalName == "objectID")
				.Remove();

			// content only - the page title never factors into the similarity comparison
			page.Root.Descendants(ns + "Title").Remove();

			// XmlHash is now always computed, not just under a "Basic/Deep" toggle - it's what
			// lets the exact-match pass in Scan require both text AND structure to match,
			// leaving a text-only match to fall through to the scored near-duplicate pass
			var xml = page.Root.ToString(SaveOptions.DisableFormatting);
			node.XmlHash = Convert.ToBase64String(
				hasher.ComputeHash(Encoding.Default.GetBytes(xml)));

			// this is a fix added to accomodate HTML embedded within OCR text which otherwise
			// would interfer with the cdata.GetWrapper innards, breaking internal XML parsing
			page.Root.Descendants(ns + "OCRText")
				.DescendantNodes().OfType<XCData>()
				.ForEach(c =>
				{
					// HtmlEncode OCR text
					c.Value = HttpUtility.HtmlEncode(c.Value);
				});

			// extract plain text last, otherwise XmlHash above would not be correct, because
			// TextValue(true)/BuildProfile mutate the XML they strip HTML from
			string plain;
			if (detectSimilar)
			{
				node.Profile = SimilarityEngine.BuildProfile(page, one);
				plain = node.Profile.Text.Trim();
			}
			else
			{
				plain = (page.Root.TextValue(true) ?? string.Empty).Trim();
			}

			node.TextHash = plain.Length == 0
				? string.Empty
				: Convert.ToBase64String(hasher.ComputeHash(Encoding.Default.GetBytes(plain)));

			return node;
		}
	}
}
