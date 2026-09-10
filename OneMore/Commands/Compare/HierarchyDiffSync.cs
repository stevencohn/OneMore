//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Compare
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


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
		/// Copies the given node and its descendants from the source side to the target
		/// side. Anything missing on the target is created; anything already matched has
		/// its content updated to match the source. Nothing on the target-only side is
		/// touched or removed.
		/// </summary>
		public static async Task Copy(OneNote one, DiffNode node, SyncDirection direction)
		{
			await SyncNode(one, node, direction);
		}


		/// <summary>
		/// Copies like <see cref="Copy"/>, then deletes every descendant of the given node
		/// that exists only on the target side, so the target subtree matches the source
		/// exactly. Returns the display names of everything deleted.
		/// </summary>
		public static async Task<List<string>> Mirror(OneNote one, DiffNode node, SyncDirection direction)
		{
			await SyncNode(one, node, direction);

			var deleted = new List<string>();
			var targetOnlyStatus = TargetOnlyStatus(direction);

			foreach (var child in node.Children)
			{
				DeleteTargetOnly(one, child, direction, targetOnlyStatus, deleted);
			}

			return deleted;
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

		private static async Task SyncNode(OneNote one, DiffNode node, SyncDirection direction)
		{
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
				await SyncPage(one, node, sourceId, leftToRight);
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
				await SyncNode(one, child, direction);
			}
		}


		private static async Task SyncPage(OneNote one, DiffNode node, string sourceId, bool leftToRight)
		{
			var page = await one.GetPage(sourceId);
			var targetId = leftToRight ? node.RightId : node.LeftId;

			if (targetId is null)
			{
				var parentId = ResolveParentTargetId(node, leftToRight);
				one.CreatePage(parentId, out targetId);
			}

			// retarget the fetched source page onto the (new or existing) target page ID
			// and let OneNote regenerate every object's ID on save
			page.Root.Attribute("ID").Value = targetId;
			page.Root.Descendants().Attributes("objectID").Remove();

			var ok = await one.Update(page);
			if (!ok)
			{
				throw new SyncException($"Failed to copy page '{page.Title}'.");
			}

			SetTargetId(node, leftToRight, targetId);
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
