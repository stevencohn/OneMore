//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using River.OneMoreAddIn.Commands.Workspaces;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;


	/// <summary>
	/// Finds which favorites are already showing in an open OneNote window.
	/// </summary>
	/// <remarks>
	/// Only a page and a section can be recognized in a window: a window that shows a page of a
	/// notebook is not "the notebook", so notebook and section group favorites are never reported
	/// as open.
	/// </remarks>
	internal static class OpenFavoriteWindows
	{
		/// <summary>
		/// Gets a test for the window that shows a favorite, or null if a favorite of this kind
		/// cannot be recognized in a window.
		/// </summary>
		/// <param name="favorite">The favorite to look for</param>
		/// <param name="resolution">Where the favorite is now, or null to use what it remembers</param>
		public static Func<WindowInfo, bool> MatcherFor(Favorite favorite, TargetResolution resolution)
		{
			var pageID = resolution?.PageID ?? favorite.PageID;
			if (!string.IsNullOrEmpty(pageID))
			{
				return w => string.Equals(w.CurrentPageId, pageID, StringComparison.Ordinal);
			}

			if (favorite.Kind == Favorite.KindNotebook || favorite.Kind == Favorite.KindSectionGroup)
			{
				return null;
			}

			var sectionID = resolution?.SectionID ?? favorite.SectionID;
			if (!string.IsNullOrEmpty(sectionID))
			{
				return w => string.Equals(w.CurrentSectionId, sectionID, StringComparison.Ordinal);
			}

			return null;
		}


		/// <summary>
		/// Maps each favorite that is showing in a window to the handle of that window. This looks
		/// only at the IDs a favorite remembers, which is fast; a favorite whose IDs have gone stale
		/// since its notebook was reopened is not reported.
		/// </summary>
		/// <param name="favorites">The favorites to look for</param>
		/// <param name="update">Called with the favorites found, first quickly by their remembered
		/// IDs and then, if some were missed, again after looking up where they are now</param>
		public static async Task Find(
			IEnumerable<Favorite> favorites, Action<Dictionary<int, IntPtr>> update)
		{
			try
			{
				var list = favorites.ToList();

				await using var one = new OneNote();
				var windows = await one.GetWindows(detailed: false);

				var map = Match(list, windows);
				update(map);

				// A favorite's remembered IDs go stale when its notebook is reopened, and a page
				// found by name alone is opened but never saved with its new ID. So look for what
				// is left over where the favorites are now, as opening them does. This reads the
				// notebooks, so it is done only when some window was not accounted for.
				var claimed = new HashSet<IntPtr>(map.Values);
				var leftover = windows
					.Where(w => !string.IsNullOrEmpty(w.WindowHandle) &&
						!claimed.Contains(ParseHandle(w.WindowHandle)))
					.ToList();

				var missed = list.Where(f => !map.ContainsKey(f.ID) && MatcherFor(f, null) is not null);

				if (leftover.Count == 0 || !missed.Any())
				{
					return;
				}

				var resolver = await WorkspaceResolver.ReadResolver();
				if (resolver is null)
				{
					return;
				}

				var refined = new Dictionary<int, IntPtr>(map);

				foreach (var favorite in missed)
				{
					var resolution = resolver.Resolve(TargetQuery.From(favorite));
					if (!resolution.IsResolved)
					{
						continue;
					}

					var window = leftover.FirstOrDefault(MatcherFor(favorite, resolution));
					if (window is not null)
					{
						refined[favorite.ID] = ParseHandle(window.WindowHandle);

						Logger.Current.Verbose(
							$"favorite {favorite.ID} {favorite.Location} is open in {window.WindowHandle} " +
							$"by {resolution.Method}");
					}
				}

				if (refined.Count > map.Count)
				{
					update(refined);
				}
			}
			catch (Exception exc)
			{
				// highlighting is a courtesy and must never stop the dialog from opening
				Logger.Current.WriteLine("could not find the windows showing favorites", exc);
			}
		}


		private static IntPtr ParseHandle(string hex)
		{
			return (IntPtr)Convert.ToInt64(hex, 16);
		}


		/// <summary>
		/// Pure part of <see cref="Find"/>: matches favorites to the given windows.
		/// </summary>
		internal static Dictionary<int, IntPtr> Match(
			IEnumerable<Favorite> favorites, IEnumerable<WindowInfo> windows)
		{
			var map = new Dictionary<int, IntPtr>();
			var open = windows.ToList();

			Logger.Current.Verbose(
				$"open windows: {string.Join(", ", open.Select(w => $"{w.WindowHandle}=page {w.CurrentPageId}"))}");

			foreach (var favorite in favorites)
			{
				var matcher = MatcherFor(favorite, null);
				if (matcher is null)
				{
					continue;
				}

				var window = open.FirstOrDefault(matcher);
				if (window is not null && !string.IsNullOrEmpty(window.WindowHandle))
				{
					map[favorite.ID] = ParseHandle(window.WindowHandle);
					Logger.Current.Verbose(
						$"favorite {favorite.ID} {favorite.Location} is open in {window.WindowHandle}");
				}
				else
				{
					Logger.Current.Verbose(
						$"favorite {favorite.ID} {favorite.Location} not open, page {favorite.PageID}");
				}
			}

			return map;
		}
	}
}
