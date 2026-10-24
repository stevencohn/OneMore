//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Windows.Automation;


	/// <summary>
	/// Scrolls OneNote's Notebooks navigation pane so the selected section is in view.
	/// OneNote has no API for its panels but they are exposed through UI Automation, so this
	/// finds the selected section in the Notebooks tree and calls ScrollItemPattern.ScrollIntoView.
	/// It never selects, expands or focuses anything, so the user's page focus is undisturbed.
	/// If the section's notebook is collapsed, the notebook itself is scrolled into view
	/// because a collapsed container still reports itself as selected. Failure is a normal
	/// outcome; enable Debug logging to see what was found.
	/// </summary>
	internal static class NavPaneScroller
	{
		private const string TreeClassName = "NetUINotebookScrollView";
		private const string ItemClassName = "NetUINavBarTreeViewItem";


		/// <summary>
		/// Scrolls the selected section in the Notebooks tree into view.
		/// </summary>
		/// <param name="oneNoteWindowHandle">The OneNote window handle, OneNote.WindowHandle</param>
		/// <returns>True if an item was scrolled</returns>
		public static bool Scroll(IntPtr oneNoteWindowHandle)
		{
			if (oneNoteWindowHandle == IntPtr.Zero)
			{
				return false;
			}

			try
			{
				// OneNote.WindowHandle is the OneNote::CWorkspace child (page canvas only);
				// the navigation pane is a sibling subtree under the top-level frame
				var root = Native.GetAncestor(oneNoteWindowHandle, Native.GA_ROOT);

				var window = AutomationElement.FromHandle(
					root == IntPtr.Zero ? oneNoteWindowHandle : root);

				if (window is null)
				{
					Logger.Current.Debug("NavPaneScroller: FromHandle returned null");
					return false;
				}

				// the Notebooks tree holds notebooks, section groups and sections as nested
				// NetUINavBarTreeViewItems; scope the search to it so we don't touch the
				// ribbon galleries, the page list or the top section tabs
				var tree = window.FindFirst(TreeScope.Descendants, new PropertyCondition(
					AutomationElement.ClassNameProperty, TreeClassName));

				if (tree is null)
				{
					Logger.Current.Debug("NavPaneScroller: notebooks tree not found");
					return false;
				}

				var selected = tree.FindAll(TreeScope.Descendants, new AndCondition(
					new PropertyCondition(SelectionItemPattern.IsSelectedProperty, true),
					new PropertyCondition(AutomationElement.ClassNameProperty, ItemClassName)));

				if (selected.Count == 0)
				{
					Logger.Current.Debug("NavPaneScroller: no selected item in notebooks tree");
					return false;
				}

				// items come in document order: notebook, then any section group, then the
				// section last; scroll only the deepest so the section is what's in view
				var item = selected[selected.Count - 1];

				if (!item.TryGetCurrentPattern(ScrollItemPattern.Pattern, out var pattern))
				{
					Logger.Current.Debug("NavPaneScroller: selected item does not support scrolling");
					return false;
				}

				((ScrollItemPattern)pattern).ScrollIntoView();
				return true;
			}
			catch (ElementNotAvailableException)
			{
				Logger.Current.Debug("NavPaneScroller: element not available");
				return false;
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("NavPaneScroller: threw", exc);
				return false;
			}
		}
	}
}
