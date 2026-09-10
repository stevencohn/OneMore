//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Compare
{
	using River.OneMoreAddIn.UI;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Repairs internal hyperlinks affected by HierarchyDiffSync's Copy/Mirror page sync.
	/// Deleting and recreating an already-matched target page (see HierarchyDiffSync.SyncPage)
	/// changes that page's id, which breaks two kinds of link: ones inside a just-copied page
	/// that still point at the source-side sibling they were authored against ("forward"), and
	/// ones on other, unselected pages that already pointed at the now-replaced target page
	/// ("backlinks"). Both are repaired the same way - by rewriting a matching anchor's href -
	/// using a rename map built by HierarchyDiffSync while it syncs (see there for why the map
	/// must be built inline, before a page is deleted, rather than reconstructed afterward).
	/// </summary>
	internal static class HierarchyLinkReconciler
	{
		// same shape as HyperlinkProvider's own private pattern; used here only for the cheap
		// "does this page's raw XML mention any id we care about" pre-filter before paying for
		// a full CDATA/XML parse
		private static readonly Regex PageIdPattern =
			new(@"page-id=(\{[^}]+?\})", RegexOptions.Compiled);

		private static readonly Regex AnchorPattern =
			new(@"<a\s+href=", RegexOptions.Compiled);


		/// <summary>
		/// Returns true if syncing the given node would replace an already-matched page's
		/// target-side id (HierarchyDiffSync.SyncPage deletes and recreates it), which is the
		/// condition that puts other, unselected pages' links to it at risk of breaking.
		/// </summary>
		public static bool HasReplacementRisk(DiffNode node)
		{
			if (node.NodeType == OneNote.NodeType.Page &&
				node.Status is DiffStatus.Same or DiffStatus.DifferentTimestamps)
			{
				// matched on both sides already - Copy/Mirror will delete-and-recreate
				// whichever side is the sync target
				return true;
			}

			foreach (var child in node.Children)
			{
				if (HasReplacementRisk(child))
				{
					return true;
				}
			}

			return false;
		}


		/// <summary>
		/// Extracts the hyperlink-space page-id from a stored onenote: href, or null if the
		/// href isn't a recognized internal link. This id is NOT the same value as OneNote's
		/// internal hierarchy ID/objectID (the ones DiffNode.LeftId/RightId carry) - it's a
		/// separate identifier OneNote mints specifically for hyperlink resolution, so the two
		/// can never be compared directly. The only reliable way to learn what a given page's
		/// own hyperlink-space id is is to generate its hyperlink and extract it, same as
		/// HyperlinkProvider.BuildHyperlinkMap does.
		/// </summary>
		public static string ExtractPageId(string href)
		{
			return string.IsNullOrEmpty(href) ? null : HyperlinkProvider.GetHyperKey(href, out _);
		}


		/// <summary>
		/// Pass 1 (always runs, unconditional): re-scans every page HierarchyDiffSync actually
		/// wrote this run for anchors matching renameMap, and rewrites them. Only pages that
		/// had at least one matching anchor get written back, so pages with no in-scope
		/// cross-links cost nothing beyond the scan itself.
		/// </summary>
		public static async Task RelinkSyncedPages(OneNote one, ProgressDialog dialog,
			CancellationToken token, List<string> syncedPageIds, Dictionary<string, string> renameMap)
		{
			if (syncedPageIds.Count == 0 || renameMap.Count == 0)
			{
				return;
			}

			dialog.SetMessage(Resx.CompareDialog_relinkingMessage);
			dialog.SetMaximum(syncedPageIds.Count);

			foreach (var pageId in syncedPageIds)
			{
				if (token.IsCancellationRequested)
				{
					return;
				}

				dialog.Increment();
				await RelinkPage(one, pageId, renameMap);
			}
		}


		/// <summary>
		/// Pass 2 (opt-in, only when the user chose to patch backlinks): scans every page
		/// currently in both of root's compared subtrees, cheaply pre-filtering via raw XML
		/// before paying for a full parse, and rewrites any anchor matching renameMap.
		/// </summary>
		public static async Task RepairBacklinks(OneNote one, ProgressDialog dialog,
			CancellationToken token, DiffNode root, Dictionary<string, string> renameMap)
		{
			if (renameMap.Count == 0)
			{
				return;
			}

			dialog.SetMessage(Resx.CompareDialog_scanningLinksMessage);

			var pageIds = new List<string>();

			if (root.LeftId is not null)
			{
				var left = await FetchSubtree(one, root.NodeType, root.LeftId);
				CollectPageIds(one, left, pageIds);
			}

			if (token.IsCancellationRequested)
			{
				return;
			}

			if (root.RightId is not null)
			{
				var right = await FetchSubtree(one, root.NodeType, root.RightId);
				CollectPageIds(one, right, pageIds);
			}

			dialog.SetMaximum(pageIds.Count);

			foreach (var pageId in pageIds)
			{
				if (token.IsCancellationRequested)
				{
					return;
				}

				dialog.Increment();

				// cheap pre-filter: skip the full CDATA/XML parse entirely unless the raw
				// XML actually mentions one of the ids we're looking for
				var xml = one.GetPageXml(pageId, OneNote.PageDetail.Basic);
				if (string.IsNullOrEmpty(xml) || !ContainsAnyRenameKey(xml, renameMap))
				{
					continue;
				}

				await RelinkPage(one, pageId, renameMap);
			}
		}


		private static bool ContainsAnyRenameKey(string xml, Dictionary<string, string> renameMap)
		{
			foreach (Match match in PageIdPattern.Matches(xml))
			{
				if (renameMap.ContainsKey(match.Groups[1].Value))
				{
					return true;
				}
			}

			return false;
		}


		// shared by both passes: fetch the page fully, rewrite any anchor whose extracted
		// page-id is a key in renameMap to point at the mapped new target, and save only if
		// at least one anchor actually changed. Internal (not private) so tests can drive it
		// directly with a hand-built renameMap, without needing a full Copy/Mirror sync.
		internal static async Task RelinkPage(OneNote one, string pageId, Dictionary<string, string> renameMap)
		{
			var page = await one.GetPage(pageId, OneNote.PageDetail.All);
			if (!page.IsValid)
			{
				return;
			}

			var changed = false;

			foreach (var cdata in page.Root.DescendantNodes().OfType<XCData>()
				.Where(c => AnchorPattern.IsMatch(c.Value)).ToList())
			{
				var wrapper = cdata.GetWrapper();
				var localChanged = false;

				foreach (var anchor in wrapper.Elements("a"))
				{
					var pageIdInHref = ExtractPageId(anchor.Attribute("href")?.Value);
					if (pageIdInHref is null || !renameMap.TryGetValue(pageIdInHref, out var newTargetId))
					{
						continue;
					}

					// always regenerate a fresh hyperlink rather than patching just the
					// page-id substring of the old href: the new target may live at an
					// entirely different section path/host, so the whole URL - not just
					// the id - can legitimately differ
					var newHref = one.GetHyperlink(newTargetId, string.Empty);
					if (!string.IsNullOrEmpty(newHref))
					{
						anchor.SetAttributeValue("href", newHref);
						localChanged = true;
					}
				}

				if (localChanged)
				{
					cdata.ReplaceWith(wrapper.GetInnerXml());
					changed = true;
				}
			}

			if (changed)
			{
				var ok = await one.Update(page);
				Logger.Current.WriteLine(ok
					? $"relinked hyperlinks on page '{page.Title}'"
					: $"failed to relink hyperlinks on page '{page.Title}'");
			}
		}


		private static async Task<XElement> FetchSubtree(OneNote one, OneNote.NodeType nodeType, string id)
		{
			return nodeType == OneNote.NodeType.Notebook
				? await one.GetNotebook(id, OneNote.Scope.Pages)
				: await one.GetSection(id);
		}


		private static void CollectPageIds(OneNote one, XElement subtree, List<string> pageIds)
		{
			var ns = one.GetNamespace(subtree);

			pageIds.AddRange(subtree.Descendants(ns + "Page")
				.Where(e => (string)e.Attribute("isInRecycleBin") != "true")
				.Select(e => (string)e.Attribute("ID")));
		}
	}
}
