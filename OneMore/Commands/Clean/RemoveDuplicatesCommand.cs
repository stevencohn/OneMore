//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
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
			public string Xml;
			public string Path;
			public string Link;
			public int? Distance;
			public DateTime LastModified;
			public MatchKind MatchKind = MatchKind.Exact;
			public double? Similarity;
			public string PlainText;
			public List<HashNode> Siblings = new();
		}


		internal enum MatchKind
		{
			Exact,
			Similar
		}

		// combined XML char length guard for the O(n*m) DistanceFrom pass in Deep mode
		private const int MaxDeepCompareLength = 500_000;

		// minimum normalized similarity (0..1) for the optional near-duplicate pass
		private const double SimilarityThreshold = 0.85;

		// skip the O(k^2) near-duplicate pass entirely above this many unique pages
		private const int MaxSimilarityCandidates = 2000;

		private OneNote one;
		private XNamespace ns;
		private readonly SHA1CryptoServiceProvider hasher;
		private readonly List<HashNode> hashes;
		private UI.ProgressDialog progress;

		private UI.SelectorScope scope;
		private bool includeTitles;
		private bool detectSimilar;
		private IEnumerable<string> books;
		private RemoveDuplicatesDialog.DepthKind depth;
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

				depth = dialog.Depth;
				scope = dialog.Scope;
				books = dialog.SelectedNotebooks;
				includeTitles = dialog.IncludeTitles;
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
			var deep = depth == RemoveDuplicatesDialog.DepthKind.Deep;

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

					var page = await one.GetPage(pageRef.Attribute("ID").Value,
						deep ? OneNote.PageDetail.BinaryData : OneNote.PageDetail.Basic);

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

					var sibling = hashes.Find(n =>
						n.TextHash == node.TextHash ||
						(n.XmlHash is not null && n.XmlHash == node.XmlHash));

					if (sibling != null)
					{
						var info = await one.GetPageInfo(node.PageID);
						node.Path = info.Path;
						node.Link = info.Link;
						if (sibling.Path == null)
						{
							info = await one.GetPageInfo(sibling.PageID);
							sibling.Path = info.Path;
							sibling.Link = info.Link;
						}

						if (deep)
						{
							if (node.XmlHash is not null && node.XmlHash == sibling.XmlHash)
							{
								// exact XML match, skip the O(n*m) pass entirely
								node.Distance = 0;
							}
							else if (node.Xml is not null && sibling.Xml is not null &&
								(node.Xml.Length + sibling.Xml.Length) <= MaxDeepCompareLength)
							{
								node.Distance = node.Xml.DistanceFrom(sibling.Xml);
							}
							// else leave Distance null; too large to compare, UI shows "-"

							node.Xml = null;
						}

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
			}

			if (!token.IsCancellationRequested && detectSimilar && !deep)
			{
				FindSimilarMatches(dialog, token);
			}

			if (!token.IsCancellationRequested)
			{
				dialog.SetMessage("Pruning results...");
				hashes.RemoveAll(n => !n.Siblings.Any());
				hashes.ForEach(n =>
				{
					n.Xml = null;
					n.PlainText = null;
				});

				if (empty.Siblings.Any())
				{
					hashes.Add(empty);
				}
			}

			return !token.IsCancellationRequested;
		}


		/// <summary>
		/// OneMore Extension >> Second pass, opt-in: compares the plain text of pages that
		/// did not already group by exact hash match, grouping pairs whose normalized edit
		/// distance clears SimilarityThreshold as "similar" (non-identical) matches.
		/// </summary>
		private void FindSimilarMatches(UI.ProgressDialog dialog, CancellationToken token)
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

			var matched = new HashSet<HashNode>();

			for (var i = 0; i < hashes.Count; i++)
			{
				if (token.IsCancellationRequested)
				{
					break;
				}

				dialog.Increment();

				var a = hashes[i];
				if (matched.Contains(a) || string.IsNullOrEmpty(a.PlainText))
				{
					continue;
				}

				for (var j = i + 1; j < hashes.Count; j++)
				{
					var b = hashes[j];
					if (matched.Contains(b) || string.IsNullOrEmpty(b.PlainText))
					{
						continue;
					}

					if (!PassesLengthPrefilter(
						a.PlainText.Length, b.PlainText.Length, SimilarityThreshold))
					{
						continue;
					}

					var distance = a.PlainText.DistanceFrom(b.PlainText);
					var similarity = NormalizedSimilarity(
						distance, a.PlainText.Length, b.PlainText.Length);

					if (similarity >= SimilarityThreshold)
					{
						b.MatchKind = MatchKind.Similar;
						b.Distance = distance;
						b.Similarity = similarity;
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
		/// similarity threshold before running the more expensive edit-distance calculation.
		/// </summary>
		internal static bool PassesLengthPrefilter(int lenA, int lenB, double threshold)
		{
			if (lenA == 0 || lenB == 0)
			{
				return false;
			}

			return Math.Abs(lenA - lenB) <= (1.0 - threshold) * Math.Max(lenA, lenB);
		}


		/// <summary>
		/// OneMore Extension >> Converts a Levenshtein edit distance into a 0..1 similarity
		/// score, normalized against the longer of the two compared strings.
		/// </summary>
		internal static double NormalizedSimilarity(int distance, int lenA, int lenB)
		{
			var maxLen = Math.Max(lenA, lenB);
			return maxLen == 0 ? 1.0 : 1.0 - ((double)distance / maxLen);
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

			node.LastModified = string.IsNullOrEmpty(modified)
				? DateTime.MinValue
				: DateTime.Parse(modified, CultureInfo.InvariantCulture);

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

			if (!includeTitles)
			{
				page.Root.Descendants(ns + "Title").Remove();
			}

			if (depth != RemoveDuplicatesDialog.DepthKind.Basic)
			{
				var xml = page.Root.ToString(SaveOptions.DisableFormatting);

				node.XmlHash = Convert.ToBase64String(
					hasher.ComputeHash(Encoding.Default.GetBytes(xml)));

				if (depth == RemoveDuplicatesDialog.DepthKind.Deep)
				{
					node.Xml = xml;
				}
			}

			// this is a fix added to accomodate HTML embedded within OCR text which otherwise
			// would interfer with the cdata.GetWrapper innards, breaking internal XML parsing
			page.Root.Descendants(ns + "OCRText")
				.DescendantNodes().OfType<XCData>()
				.ForEach(c =>
				{
					// HtmlEncode OCR text
					c.Value = HttpUtility.HtmlEncode(c.Value);
				});

			// extract plain text last, otherwise XmlHash will not be correct
			// because TextValue(true) will change the XML
			var plain = page.Root.TextValue(true).Trim();

			node.TextHash = plain.Length == 0
				? string.Empty
				: Convert.ToBase64String(hasher.ComputeHash(Encoding.Default.GetBytes(plain)));

			node.PlainText = plain;

			return node;
		}
	}
}
