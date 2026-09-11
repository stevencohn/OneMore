//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Compare
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Which side of a DiffNode pair is the source of a Copy/Mirror action.
	/// </summary>
	internal enum SyncDirection
	{
		LeftToRight,
		RightToLeft
	}


	/// <summary>
	/// Implements the Compare Hierarchy command's Copy and Mirror actions: recursively
	/// syncs a DiffNode subtree from one side to the other, creating anything missing there
	/// and updating anything that already matches. Objects that exist only on the far
	/// (non-source) side are left untouched by Copy, per the spec's "left untouched" rule;
	/// Mirror additionally deletes them so the target matches the source exactly.
	/// </summary>
	internal static class HierarchyDiffSync
	{
		/// <summary>
		/// Raised when a sync step can't proceed, e.g. a selected node's own parent doesn't
		/// yet exist on the target side. Message is user-facing.
		/// </summary>
		internal sealed class SyncException : Exception
		{
			public SyncException(string message) : base(message)
			{
			}
		}


		/// <summary>
		/// Reports what a Copy/Mirror run actually did, so the caller can react without
		/// re-deriving it: which target-side page ids were created/replaced (used to mark
		/// those rows as definitively identical post-sync, see CompareDialog), and, for
		/// Mirror, the display names of anything deleted.
		/// </summary>
		internal sealed class SyncResult
		{
			public List<string> SyncedPageIds { get; } = new();

			public List<string> Deleted { get; } = new();
		}


		/// <summary>
		/// Copies the given node and its descendants from the source side to the target
		/// side. Anything missing on the target is created; anything already matched has
		/// its content updated to match the source. Nothing on the target-only side is
		/// touched or removed.
		/// </summary>
		/// <param name="one">The active OneNote wrapper</param>
		/// <param name="dialog">Used to report per-stage/per-page progress</param>
		/// <param name="token">Checked between pages so a long run can be cancelled</param>
		/// <param name="root">
		/// The full compared hierarchy's root (may differ from <paramref name="node"/>, the
		/// specific node being acted on) - used only to scope the backlink-repair scan
		/// </param>
		/// <param name="node">The node to copy</param>
		/// <param name="direction">Which side is the source</param>
		/// <param name="patchLinks">
		/// True to also repair other, unselected pages' links to any page replaced by this
		/// sync (mutates pages outside the caller's selection); false to leave them broken
		/// </param>
		public static async Task<SyncResult> Copy(OneNote one, ProgressDialog dialog, CancellationToken token,
			DiffNode root, DiffNode node, SyncDirection direction, bool patchLinks)
		{
			var renameMap = new Dictionary<string, string>();
			var result = new SyncResult();

			dialog.SetMaximum(Math.Max(1, CountPages(node)));
			await SyncNode(one, dialog, token, node, direction, renameMap, result.SyncedPageIds);

			if (token.IsCancellationRequested)
			{
				return result;
			}

			await HierarchyLinkReconciler.RelinkSyncedPages(one, dialog, token, result.SyncedPageIds, renameMap);

			if (patchLinks && !token.IsCancellationRequested)
			{
				await HierarchyLinkReconciler.RepairBacklinks(one, dialog, token, root, renameMap);
			}

			return result;
		}


		/// <summary>
		/// Copies like <see cref="Copy"/>, then deletes every descendant of the given node
		/// that exists only on the target side, so the target subtree matches the source
		/// exactly.
		/// </summary>
		public static async Task<SyncResult> Mirror(OneNote one, ProgressDialog dialog, CancellationToken token,
			DiffNode root, DiffNode node, SyncDirection direction, bool patchLinks)
		{
			var renameMap = new Dictionary<string, string>();
			var result = new SyncResult();

			dialog.SetMaximum(Math.Max(1, CountPages(node)));
			await SyncNode(one, dialog, token, node, direction, renameMap, result.SyncedPageIds);

			var targetOnlyStatus = TargetOnlyStatus(direction);

			foreach (var child in node.Children)
			{
				DeleteTargetOnly(one, child, direction, targetOnlyStatus, result.Deleted);
			}

			if (!token.IsCancellationRequested)
			{
				await HierarchyLinkReconciler.RelinkSyncedPages(one, dialog, token, result.SyncedPageIds, renameMap);
			}

			if (patchLinks && !token.IsCancellationRequested)
			{
				await HierarchyLinkReconciler.RepairBacklinks(one, dialog, token, root, renameMap);
			}

			return result;
		}


		private static int CountPages(DiffNode node)
		{
			var count = node.NodeType == OneNote.NodeType.Page ? 1 : 0;

			foreach (var child in node.Children)
			{
				count += CountPages(child);
			}

			return count;
		}


		/// <summary>
		/// Counts how many descendants of the given node exist only on the target side and
		/// would be deleted by Mirror; used to word a confirmation prompt before deleting.
		/// </summary>
		public static int CountTargetOnly(DiffNode node, SyncDirection direction)
		{
			var targetOnlyStatus = TargetOnlyStatus(direction);
			var count = 0;

			foreach (var child in node.Children)
			{
				CountTargetOnly(child, targetOnlyStatus, ref count);
			}

			return count;
		}


		/// <summary>
		/// Marks every page in the given (freshly rebuilt) tree whose post-sync target id
		/// appears in <paramref name="syncedIds"/> as definitively DiffStatus.Same, so
		/// HierarchyDiffView renders the classic green "=" immediately rather than the
		/// "different timestamps" warning a fresh timestamp-based rebuild would otherwise
		/// show - a page a Copy/Mirror just synced is provably byte-for-byte identical, a
		/// stronger claim than a measured similarity score, even though its new
		/// lastModifiedTime won't match the source's.
		/// </summary>
		public static void ApplySyncedStatus(DiffNode node, ICollection<string> syncedIds, SyncDirection direction)
		{
			if (syncedIds is null || syncedIds.Count == 0)
			{
				return;
			}

			if (node.NodeType == OneNote.NodeType.Page)
			{
				var targetId = direction == SyncDirection.LeftToRight ? node.RightId : node.LeftId;
				if (targetId is not null && syncedIds.Contains(targetId))
				{
					node.Status = DiffStatus.Same;
				}

				return;
			}

			foreach (var child in node.Children)
			{
				ApplySyncedStatus(child, syncedIds, direction);
			}
		}


		private static DiffStatus TargetOnlyStatus(SyncDirection direction)
		{
			// OrphanRight means the node exists only on the right; when copying left-to-
			// right, "right" is the target, so an OrphanRight node is target-only
			return direction == SyncDirection.LeftToRight
				? DiffStatus.OrphanRight
				: DiffStatus.OrphanLeft;
		}


		private static void CountTargetOnly(DiffNode node, DiffStatus targetOnlyStatus, ref int count)
		{
			if (node.Status == targetOnlyStatus)
			{
				// its descendants are implicitly removed along with it; don't recount them
				count++;
				return;
			}

			foreach (var child in node.Children)
			{
				CountTargetOnly(child, targetOnlyStatus, ref count);
			}
		}


		private static void DeleteTargetOnly(OneNote one, DiffNode node, SyncDirection direction,
			DiffStatus targetOnlyStatus, List<string> deleted)
		{
			if (node.Status == targetOnlyStatus)
			{
				var id = direction == SyncDirection.LeftToRight ? node.RightId : node.LeftId;
				one.DeleteHierarchy(id);
				deleted.Add(node.Name);
				return;
			}

			foreach (var child in node.Children)
			{
				DeleteTargetOnly(one, child, direction, targetOnlyStatus, deleted);
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Copy...

		private static async Task SyncNode(OneNote one, ProgressDialog dialog, CancellationToken token,
			DiffNode node, SyncDirection direction,
			Dictionary<string, string> renameMap, List<string> syncedPageIds)
		{
			if (token.IsCancellationRequested)
			{
				return;
			}

			var leftToRight = direction == SyncDirection.LeftToRight;
			var sourceId = leftToRight ? node.LeftId : node.RightId;

			if (sourceId is null)
			{
				// nothing on the source side to copy from; anything target-only under here
				// is Mirror's concern (DeleteTargetOnly), not Copy's
				return;
			}

			if (node.NodeType == OneNote.NodeType.Page)
			{
				await SyncPage(one, dialog, node, sourceId, leftToRight, renameMap, syncedPageIds);
				return;
			}

			var targetId = leftToRight ? node.RightId : node.LeftId;

			if (targetId is null)
			{
				targetId = await CreateContainer(one, node, leftToRight);
				SetTargetId(node, leftToRight, targetId);
			}

			foreach (var child in node.Children)
			{
				await SyncNode(one, dialog, token, child, direction, renameMap, syncedPageIds);
			}
		}


		// a similarity score at or above this is treated as "identical" for the purposes of
		// skipping a resync - not exactly 1.0 since summing five independently-weighted
		// scores (each itself exactly 1.0 for byte-identical extracted text) can accumulate
		// a tiny binary floating-point rounding error even when nothing actually differs
		private const double IdenticalSimilarityThreshold = 0.9999;


		private static async Task SyncPage(OneNote one, ProgressDialog dialog, DiffNode node,
			string sourceId, bool leftToRight,
			Dictionary<string, string> renameMap, List<string> syncedPageIds)
		{
			dialog.SetMessage(string.Format(Resx.CompareDialog_syncingPageFormat, node.Name));

			var targetId = leftToRight ? node.RightId : node.LeftId;

			if (targetId is not null && await IsAlreadyIdentical(one, node, sourceId, targetId))
			{
				// nothing to do: the target already matches the source, so leave it - and
				// its id - untouched rather than needlessly deleting and recreating it
				dialog.Increment();
				return;
			}

			var page = await one.GetPage(sourceId);

			// capture hyperlink-space ids (NOT the same value as OneNote's internal
			// hierarchy id/objectID - see HierarchyLinkReconciler.ExtractPageId) before any
			// mutation: the source's, always, so other just-copied pages that linked to it
			// can be relinked; the existing target's, only if one exists, since it's about
			// to be deleted below and GetHyperlinkToObject can't resolve an id that no
			// longer exists
			var sourceHyperId = HierarchyLinkReconciler.ExtractPageId(
				one.GetHyperlink(sourceId, string.Empty));

			string oldHyperId = null;

			if (targetId is not null)
			{
				oldHyperId = HierarchyLinkReconciler.ExtractPageId(
					one.GetHyperlink(targetId, string.Empty));

				// OneNote's UpdatePageContent merges by object ID rather than replacing the
				// page wholesale: submitted content whose objectID doesn't match anything
				// already on the target page is simply ADDED alongside the target's existing
				// content, not swapped in for it. Since the source page's own objectIDs are
				// stripped below (so OneNote assigns fresh ones), updating an already-existing
				// target page this way would append the source's content rather than replace
				// it. Deleting and recreating the target page guarantees an empty starting
				// point - same as the "doesn't exist yet" branch below - so the result is an
				// exact copy instead of source-plus-target. By this point IsAlreadyIdentical
				// has already ruled out the case where content matches, so this genuinely is
				// a change and the target-side id churn is unavoidable; any other page that
				// already linked to the old id is repaired separately, see
				// HierarchyLinkReconciler.
				one.DeleteHierarchy(targetId);
				targetId = null;
			}

			var parentId = ResolveParentTargetId(node, leftToRight);
			one.CreatePage(parentId, out targetId);

			// retarget the fetched source page onto the new target page ID and let OneNote
			// regenerate every object's ID on save
			page.Root.Attribute("ID").Value = targetId;
			page.Root.Descendants().Attributes("objectID").Remove();

			var ok = await one.Update(page);
			if (!ok)
			{
				throw new SyncException($"Failed to copy page '{page.Title}'.");
			}

			SetTargetId(node, leftToRight, targetId);

			if (sourceHyperId is not null)
			{
				renameMap[sourceHyperId] = targetId;
			}

			if (oldHyperId is not null)
			{
				renameMap[oldHyperId] = targetId;
			}

			syncedPageIds.Add(targetId);
			dialog.Increment();
		}


		// Status.Same (matching modified timestamps) is trusted outright and skips this
		// check entirely - about as strong a content-identity signal as OneNote's own
		// metadata offers, and cheap. Status.DifferentTimestamps is NOT trusted on its own:
		// once a page has been synced once, the target's lastModifiedTime becomes "time of
		// the copy" and will essentially never exactly match the source's own timestamp
		// again, even after the two have fully converged - so treating DifferentTimestamps
		// as "must resync" would force every subsequent Copy/Mirror to needlessly (and
		// destructively - see SyncPage's delete-and-recreate comment) redo pages whose
		// content hasn't actually changed since the last sync. Verifying with the same
		// content-similarity scorer "Compare contents..." uses (rather than only the
		// hierarchy-level timestamp) catches that case correctly.
		private static async Task<bool> IsAlreadyIdentical(
			OneNote one, DiffNode node, string sourceId, string targetId)
		{
			if (node.Status == DiffStatus.Same)
			{
				return true;
			}

			// fetch throwaway Page instances distinct from the one SyncPage goes on to use
			// for the actual copy below, since Page/XElement instances handed to
			// SimilarityEngine should never be assumed reusable afterward
			var sourcePage = await one.GetPage(sourceId);
			var targetPage = await one.GetPage(targetId);

			var similarity = SimilarityEngine.Compare(sourcePage, targetPage, one).Overall;
			var identical = similarity >= IdenticalSimilarityThreshold;

			if (identical)
			{
				Logger.Current.WriteLine(
					$"skipping resync of '{node.Name}': content is already {similarity:P1} similar");
			}

			return identical;
		}


		private static async Task<string> CreateContainer(OneNote one, DiffNode node, bool leftToRight)
		{
			var parentId = ResolveParentTargetId(node, leftToRight);
			var elementName = node.NodeType == OneNote.NodeType.SectionGroup ? "SectionGroup" : "Section";

			var parent = await FetchContainer(one, node.Parent.NodeType, parentId);
			var ns = one.GetNamespace(parent);

			parent.Add(new XElement(ns + elementName, new XAttribute("name", node.Name)));
			one.UpdateHierarchy(parent);

			// re-fetch and match by name rather than diffing IDs before/after: OneNote may
			// reassign more than just the new element's ID on update
			var updated = await FetchContainer(one, node.Parent.NodeType, parentId);

			var created = updated.Elements().FirstOrDefault(e =>
				e.Name.LocalName == elementName && (string)e.Attribute("name") == node.Name);

			if (created is null)
			{
				throw new SyncException(
					$"Could not create '{node.Name}': it was not found after creation.");
			}

			return (string)created.Attribute("ID");
		}


		// Must fetch deeply (Scope.Pages, same as GetSection's own hardcoded hsPages scope)
		// rather than shallowly: UpdateHierarchy takes the submitted XML as authoritative,
		// so a shallow fetch here would make every omitted descendant - every existing
		// page, section, and section group - look deleted and wipe it out on update
		private static async Task<XElement> FetchContainer(OneNote one, OneNote.NodeType parentType, string id)
		{
			return parentType == OneNote.NodeType.Notebook
				? await one.GetNotebook(id, OneNote.Scope.Pages)
				: await one.GetSection(id);
		}


		private static string ResolveParentTargetId(DiffNode node, bool leftToRight)
		{
			var parent = node.Parent
				?? throw new SyncException($"'{node.Name}' has no parent to create it under.");

			var parentTargetId = leftToRight ? parent.RightId : parent.LeftId;

			return parentTargetId ?? throw new SyncException(
				$"Cannot copy '{node.Name}' because its parent '{parent.Name}' does not yet " +
				"exist on the target side. Copy the parent first.");
		}


		private static void SetTargetId(DiffNode node, bool leftToRight, string targetId)
		{
			if (leftToRight)
			{
				node.RightId = targetId;
			}
			else
			{
				node.LeftId = targetId;
			}
		}
	}
}
