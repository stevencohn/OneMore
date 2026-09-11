//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Compare
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;
	using Resx = Properties.Resources;


	/// <summary>
	/// Implements the Compare Hierarchy command's bulk "Deep scan" action: runs
	/// SimilarityEngine against every matched, not-yet-known-identical page pair under a
	/// given DiffNode, caching each result on the node itself so HierarchyDiffView can
	/// render a real numeric score instead of just "timestamps differ". Purely read-only -
	/// no OneNote content is mutated, unlike HierarchyDiffSync.
	/// </summary>
	internal static class HierarchyDiffScanner
	{
		/// <summary>
		/// Scores every scannable page pair under (and including) the given node, reporting
		/// progress via <paramref name="dialog"/> and invoking <paramref name="onScored"/>
		/// after each page so the caller can repaint. Stops early, leaving whatever has
		/// already been scored in place, if <paramref name="token"/> is cancelled.
		/// </summary>
		public static async Task Scan(OneNote one, ProgressDialog dialog, CancellationToken token,
			DiffNode node, Action onScored)
		{
			var pairs = CollectScannable(node);
			dialog.SetMaximum(Math.Max(1, pairs.Count));

			foreach (var pair in pairs)
			{
				if (token.IsCancellationRequested)
				{
					return;
				}

				dialog.SetMessage(string.Format(Resx.CompareDialog_deepScanPageFormat, pair.Name));

				var leftPage = await one.GetPage(pair.LeftId);
				var rightPage = await one.GetPage(pair.RightId);
				pair.Similarity = SimilarityEngine.Compare(leftPage, rightPage, one).Overall;

				dialog.Increment();
				onScored();
			}
		}


		// Status.Same pages are already known-identical by timestamp (the same trust
		// HierarchyDiffSync.IsAlreadyIdentical places in it) and render their own "="
		// glyph regardless, so scoring them would just spend COM round-trips confirming
		// what's already shown. Orphans have no counterpart to compare against.
		private static List<DiffNode> CollectScannable(DiffNode node)
		{
			var result = new List<DiffNode>();
			Collect(node, result);
			return result;
		}


		private static void Collect(DiffNode node, List<DiffNode> result)
		{
			if (node.NodeType == OneNote.NodeType.Page)
			{
				if (node.Status == DiffStatus.DifferentTimestamps &&
					node.LeftId is not null && node.RightId is not null)
				{
					result.Add(node);
				}

				return;
			}

			foreach (var child in node.Children)
			{
				Collect(child, result);
			}
		}
	}
}
