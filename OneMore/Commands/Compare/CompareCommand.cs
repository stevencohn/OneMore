//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Compare
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Lets the user pick a source (left) and target (right) notebook, section, or section
	/// group of the same type, then presents a surface-level diff of their hierarchies -
	/// names, node types, and created/modified timestamps only, no page content - in
	/// CompareDialog.
	/// </summary>
	internal class CompareCommand : Command
	{
		private string sourceId;
		private IDisposable guard;


		public override async Task Execute(params object[] args)
		{
			guard = EnterOnce();
			if (guard is null)
			{
				return;
			}

			sourceId = null;

			await using var one = new OneNote();
			one.SelectLocation(
				Resx.CompareCommand_pickSourceTitle,
				Resx.CompareCommand_pickSourceDescription,
				OneNote.Scope.Containers,
				SourceChosen);

			await Task.Yield();
		}


		private async Task SourceChosen(string nodeId)
		{
			if (string.IsNullOrEmpty(nodeId))
			{
				// cancelled
				Release();
				return;
			}

			sourceId = nodeId;

			await using var one = new OneNote();
			one.SelectLocation(
				Resx.CompareCommand_pickTargetTitle,
				Resx.CompareCommand_pickTargetDescription,
				OneNote.Scope.Containers,
				TargetChosen);
		}


		private async Task TargetChosen(string nodeId)
		{
			if (string.IsNullOrEmpty(nodeId))
			{
				// cancelled
				Release();
				return;
			}

			var targetId = nodeId;

			await using var one = new OneNote();

			var source = one.GetHierarchyNode(sourceId);
			var target = one.GetHierarchyNode(targetId);

			if (source is null || target is null)
			{
				ShowError(Resx.CompareCommand_invalidSelection);
				Release();
				return;
			}

			if (source.NodeType != target.NodeType)
			{
				ShowError(Resx.CompareCommand_typeMismatch);
				Release();
				return;
			}

			if (sourceId == targetId ||
				IsAncestor(one, sourceId, targetId) ||
				IsAncestor(one, targetId, sourceId))
			{
				ShowError(Resx.CompareCommand_overlappingNodes);
				Release();
				return;
			}

			var left = await FetchSubtree(one, sourceId, source.NodeType);
			var right = await FetchSubtree(one, targetId, target.NodeType);

			if (left is null || right is null)
			{
				ShowError(Resx.CompareCommand_invalidSelection);
				Release();
				return;
			}

			var diff = HierarchyDiffBuilder.Build(left, right);

			// this runs on a throwaway thread with no message loop of its own (either the
			// QuickFiling callback thread or CommandFactory.RunCore's Task.Run), which would
			// otherwise force RunModeless into its blocking, nested Application.Run fallback -
			// freezing this thread, and with it OneNote's own responsiveness, for as long as
			// the dialog stays open. Marshal onto HotkeyManager's persistent message-pump
			// thread first so RunModeless sees an already-running loop and takes its
			// lightweight, non-blocking Show() path instead
			HotkeyManager.InvokeOnMessageThread(() =>
			{
				var dialog = new CompareDialog(diff, source.NodeType, source.Name, target.Name);
				dialog.RunModeless((sender, e) =>
				{
					Release();
					(sender as CompareDialog)?.Dispose();
				});
			});
		}


		// walks up the hierarchy from nodeId looking for ancestorId, guarding against
		// comparing a node to itself (a noop) or to one of its own ancestors/descendants
		// (which would make Mirror destructively delete the very thing it's mirroring from)
		private static bool IsAncestor(OneNote one, string ancestorId, string nodeId)
		{
			var id = one.GetParent(nodeId);
			while (!string.IsNullOrEmpty(id))
			{
				if (id == ancestorId)
				{
					return true;
				}

				id = one.GetParent(id);
			}

			return false;
		}


		private static async Task<XElement> FetchSubtree(OneNote one, string id, OneNote.NodeType type)
		{
			return type == OneNote.NodeType.Notebook
				? await one.GetNotebook(id, OneNote.Scope.Pages)
				: await one.GetSection(id);
		}


		private void Release()
		{
			guard?.Dispose();
			guard = null;
		}
	}
}
