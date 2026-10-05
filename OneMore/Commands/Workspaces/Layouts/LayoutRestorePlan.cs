//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Layouts
{
	using River.OneMoreAddIn.Commands.Workspaces;
	using System;
	using System.Collections.Generic;
	using System.Linq;


	/// <summary>
	/// What to do about one window of a layout when restoring it.
	/// </summary>
	internal sealed class RestoreItem
	{
		public LayoutWindow Window { get; set; }

		/// <summary>Where the page is now, or null if it could not be looked up at all.</summary>
		public TargetResolution Resolution { get; set; }

		/// <summary>The page ID to look for among the windows that are already open.</summary>
		public string PageID { get; set; }

		/// <summary>What to open if there is no window showing the page yet.</summary>
		public string Uri { get; set; }

		/// <summary>Gets whether there is a page to open. If not, the window is to be reported.</summary>
		public bool CanOpen { get; set; }

		/// <summary>Gets why a window cannot be opened.</summary>
		public ResolveOutcome? Outcome { get; set; }

		public string Reason { get; set; }
	}


	/// <summary>
	/// Decides how to restore the windows of a layout. Pure logic with no OneNote access, so it can
	/// be tested with fixtures.
	/// </summary>
	/// <remarks>
	/// A window remembers the ID of its page, and OneNote regenerates every page ID when a notebook
	/// is reopened. Looking for an open window by that remembered ID finds none, so a window that is
	/// already open is opened a second time, and the new window is then waited for under the same
	/// stale ID and never found, so it is never positioned. The page is therefore looked up as it is
	/// now first, and both the search for an open window and the link to open use what was found.
	/// </remarks>
	internal static class LayoutRestorePlan
	{
		/// <summary>
		/// Plans each window of a layout, keeping their order.
		/// </summary>
		/// <param name="windows">The windows to restore</param>
		/// <param name="resolver">Finds pages in the open notebooks, or null if OneNote could not be
		/// read, in which case each window is tried with what it remembered, as before</param>
		/// <param name="hyperlink">Makes a link to a page from its ID</param>
		public static List<RestoreItem> Plan(
			IEnumerable<LayoutWindow> windows, TargetResolver resolver, Func<string, string> hyperlink)
		{
			var items = new List<RestoreItem>();

			foreach (var window in windows)
			{
				if (resolver is null)
				{
					items.Add(new RestoreItem
					{
						Window = window,
						PageID = window.PageID,
						Uri = window.Uri,
						CanOpen = true
					});

					continue;
				}

				var resolution = resolver.Resolve(TargetQuery.From(window));

				if (resolution.IsResolved)
				{
					items.Add(new RestoreItem
					{
						Window = window,
						Resolution = resolution,
						PageID = resolution.PageID,
						Uri = hyperlink(resolution.PageID) ?? window.Uri,
						CanOpen = true
					});
				}
				else
				{
					items.Add(new RestoreItem
					{
						Window = window,
						Resolution = resolution,
						Outcome = resolution.Outcome,
						Reason = resolution.Reason,
						CanOpen = false
					});
				}
			}

			return items;
		}


		/// <summary>
		/// Gives each window the key of the page it shows, so that the layout can still be restored
		/// after OneNote changes the IDs. A page that cannot be found for certain gets none.
		/// </summary>
		public static void AssignKeys(IEnumerable<LayoutWindow> windows, TargetResolver resolver)
		{
			if (resolver is null)
			{
				return;
			}

			foreach (var window in windows)
			{
				var found = resolver.Resolve(TargetQuery.From(window));
				if (found.IsConfident)
				{
					window.PageKey = found.PageKey;
				}
			}
		}


		/// <summary>
		/// Gets the name to show for a window: the one the user gave it, or its page's name.
		/// </summary>
		public static string NameOf(LayoutWindow window)
		{
			return string.IsNullOrWhiteSpace(window.Alias) ? window.Name : window.Alias;
		}
	}
}
