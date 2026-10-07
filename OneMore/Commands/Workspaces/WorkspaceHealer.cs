//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Workspaces
{
	using River.OneMoreAddIn.Commands.Favorites;
	using River.OneMoreAddIn.Commands.Layouts;
	using System;
	using System.Collections.Generic;
	using System.Linq;


	/// <summary>
	/// What the healer found for one favorite.
	/// </summary>
	internal enum HealKind
	{
		/// <summary>Found by key, ID or GUID and something changed, so it should be saved.</summary>
		Healed,

		/// <summary>Found by key, ID or GUID and already up to date.</summary>
		Unchanged,

		/// <summary>Found only by its name, which is not enough to save.</summary>
		Guess,

		/// <summary>Not found yet, because some GUIDs have not been read. Try again later.</summary>
		Pending,

		/// <summary>In a notebook that is closed or a section that is locked.</summary>
		Offline,

		/// <summary>More than one thing fits, so none was chosen.</summary>
		Ambiguous,

		/// <summary>Nothing fits. It is left as it is, never deleted.</summary>
		Broken,

		/// <summary>Points at the same target as another favorite, which already holds the key.</summary>
		Duplicate
	}


	internal sealed class HealItem
	{
		/// <summary>Set for a favorite; otherwise <see cref="Window"/> is set.</summary>
		public Favorite Favorite { get; set; }

		/// <summary>Set for a window of a layout.</summary>
		public LayoutWindow Window { get; set; }

		/// <summary>Set for an item of the Navigator's reading list.</summary>
		public PinnedItem Pinned { get; set; }

		public TargetResolution Resolution { get; set; }
		public HealKind Kind { get; set; }

		/// <summary>Gets the database ID of the favorite, window or pinned item.</summary>
		public int ID => Favorite?.ID ?? Window?.ID ?? Pinned?.ID ?? 0;

		/// <summary>Gets where the favorite, window or pinned item pointed, for the log.</summary>
		public string Label => Favorite?.Location ?? Window?.Location ?? Pinned?.Info.Path;

		/// <summary>Gets what this is, for the log.</summary>
		public string What => Favorite is not null
			? "favorite"
			: Pinned is not null ? "pinned item" : "layout window";
	}

	/// <summary>
	/// What the healer decided for every favorite.
	/// </summary>
	internal sealed class HealPlan
	{
		public List<HealItem> Items { get; } = new List<HealItem>();

		/// <summary>Gets the favorites whose changes should be saved.</summary>
		public IEnumerable<HealItem> ToSave => Items.Where(i => i.Kind == HealKind.Healed);

		public int Count(HealKind kind) => Items.Count(i => i.Kind == kind);


		/// <summary>
		/// Gets a short summary for the log, or null if every favorite is already up to date.
		/// </summary>
		public string Summary()
		{
			var parts = new List<string>();

			foreach (var kind in new[]
			{
				HealKind.Healed, HealKind.Guess, HealKind.Pending, HealKind.Offline,
				HealKind.Ambiguous, HealKind.Broken, HealKind.Duplicate
			})
			{
				var count = Count(kind);
				if (count > 0)
				{
					parts.Add($"{count} {kind.ToString().ToLowerInvariant()}");
				}
			}

			return parts.Count == 0 ? null : string.Join(", ", parts);
		}
	}


	/// <summary>
	/// Decides how to bring every favorite up to date with where its target is now. Pure logic,
	/// so it can be tested with fixtures; <see cref="HealingStage"/> runs it and saves the result.
	/// </summary>
	/// <remarks>
	/// Only a match by key, ID or GUID is ever saved. A match by name is a guess, so the favorite
	/// is left as it is, though it can still be opened by that guess when clicked. Nothing is ever
	/// deleted: a favorite that cannot be found stays, and is reported.
	/// </remarks>
	internal static class WorkspaceHealer
	{
		/// <summary>
		/// Plans the repair of every favorite.
		/// </summary>
		/// <param name="favorites">Every favorite, in the order they should get a target when two
		/// turn out to be the same, usually by favorite ID</param>
		/// <param name="resolver">Finds where a favorite points now</param>
		/// <param name="hyperlink">Makes a link to a page or a section from its ID</param>
		/// <remarks>The favorites are updated in memory as they are planned; saving them is up to
		/// the caller, using <see cref="HealPlan.ToSave"/>.</remarks>
		public static HealPlan Plan(
			IEnumerable<Favorite> favorites, TargetResolver resolver, Func<string, string> hyperlink)
		{
			var list = favorites.ToList();
			var plan = new HealPlan();

			// a target can be held by only one favorite, because of the unique indexes on the
			// keys, so a favorite that already holds one keeps it
			var claimed = new Dictionary<string, int>(StringComparer.Ordinal);
			foreach (var favorite in list)
			{
				var claim = ClaimOf(favorite);
				if (claim is not null && !claimed.ContainsKey(claim))
				{
					claimed.Add(claim, favorite.ID);
				}
			}

			foreach (var favorite in list)
			{
				var resolution = resolver.Resolve(TargetQuery.From(favorite));
				var item = new HealItem { Favorite = favorite, Resolution = resolution };
				plan.Items.Add(item);

				switch (resolution.Outcome)
				{
					case ResolveOutcome.Pending:
						item.Kind = HealKind.Pending;
						continue;

					case ResolveOutcome.Offline:
						item.Kind = HealKind.Offline;
						continue;

					case ResolveOutcome.Ambiguous:
						item.Kind = HealKind.Ambiguous;
						continue;

					case ResolveOutcome.Broken:
						item.Kind = HealKind.Broken;
						continue;
				}

				if (!resolution.IsConfident)
				{
					item.Kind = HealKind.Guess;
					continue;
				}

				var found = ClaimOf(resolution, favorite.Kind);
				if (found is not null &&
					claimed.TryGetValue(found, out var holder) && holder != favorite.ID)
				{
					item.Kind = HealKind.Duplicate;
					continue;
				}

				if (found is not null && !claimed.ContainsKey(found))
				{
					claimed.Add(found, favorite.ID);
				}

				item.Kind = WorkspaceResolver.Apply(favorite, resolution, hyperlink)
					? HealKind.Healed
					: HealKind.Unchanged;
			}

			return plan;
		}


		/// <summary>
		/// Plans the repair of every window of every layout. The same page may be in two layouts, so
		/// a page is only a duplicate of another window of the same layout.
		/// </summary>
		/// <param name="windows">Every window, in the order they should get a target when two of one
		/// layout turn out to be the same page, usually by window ID</param>
		/// <param name="resolver">Finds where a window's page is now</param>
		/// <param name="hyperlink">Makes a link to a page from its ID</param>
		/// <remarks>The windows are updated in memory as they are planned; saving them is up to the
		/// caller, using <see cref="HealPlan.ToSave"/>.</remarks>
		public static HealPlan PlanWindows(
			IEnumerable<LayoutWindow> windows, TargetResolver resolver, Func<string, string> hyperlink)
		{
			var list = windows.ToList();
			var plan = new HealPlan();

			var claimed = new Dictionary<string, int>(StringComparer.Ordinal);
			foreach (var window in list)
			{
				if (window.PageKey is long held)
				{
					var claim = WindowClaim(window.LayoutID, held);
					if (!claimed.ContainsKey(claim))
					{
						claimed.Add(claim, window.ID);
					}
				}
			}

			foreach (var window in list)
			{
				var resolution = resolver.Resolve(TargetQuery.From(window));
				var item = new HealItem { Window = window, Resolution = resolution };
				plan.Items.Add(item);

				switch (resolution.Outcome)
				{
					case ResolveOutcome.Pending: item.Kind = HealKind.Pending; continue;
					case ResolveOutcome.Offline: item.Kind = HealKind.Offline; continue;
					case ResolveOutcome.Ambiguous: item.Kind = HealKind.Ambiguous; continue;
					case ResolveOutcome.Broken: item.Kind = HealKind.Broken; continue;
				}

				if (!resolution.IsConfident)
				{
					item.Kind = HealKind.Guess;
					continue;
				}

				if (resolution.PageKey is long found)
				{
					var claim = WindowClaim(window.LayoutID, found);
					if (claimed.TryGetValue(claim, out var holder) && holder != window.ID)
					{
						item.Kind = HealKind.Duplicate;
						continue;
					}

					if (!claimed.ContainsKey(claim))
					{
						claimed.Add(claim, window.ID);
					}
				}

				item.Kind = WorkspaceResolver.Apply(window, resolution, hyperlink)
					? HealKind.Healed
					: HealKind.Unchanged;
			}

			return plan;
		}


		/// <summary>
		/// Plans the repair of every item of the reading list. A page and a paragraph of the same
		/// page can both be on it, so a paragraph is only a duplicate of the same paragraph.
		/// </summary>
		/// <param name="items">Every item, in the order they should get a target when two turn out
		/// to be the same, usually by ID</param>
		/// <param name="resolver">Finds where an item's page is now</param>
		/// <param name="hyperlink">Makes a link to a page from its ID</param>
		/// <remarks>The items are updated in memory as they are planned; saving them is up to the
		/// caller, using <see cref="HealPlan.ToSave"/>.</remarks>
		public static HealPlan PlanPinned(
			IEnumerable<PinnedItem> items, TargetResolver resolver, Func<string, string> hyperlink)
		{
			var list = items.ToList();
			var plan = new HealPlan();

			var claimed = new Dictionary<string, int>(StringComparer.Ordinal);
			foreach (var pinned in list)
			{
				if (pinned.PageKey is long held)
				{
					var claim = PinnedClaim(held, pinned.Info.ObjectId);
					if (!claimed.ContainsKey(claim))
					{
						claimed.Add(claim, pinned.ID);
					}
				}
			}

			foreach (var pinned in list)
			{
				var resolution = resolver.Resolve(TargetQuery.From(pinned));
				var item = new HealItem { Pinned = pinned, Resolution = resolution };
				plan.Items.Add(item);

				switch (resolution.Outcome)
				{
					case ResolveOutcome.Pending: item.Kind = HealKind.Pending; continue;
					case ResolveOutcome.Offline: item.Kind = HealKind.Offline; continue;
					case ResolveOutcome.Ambiguous: item.Kind = HealKind.Ambiguous; continue;
					case ResolveOutcome.Broken: item.Kind = HealKind.Broken; continue;
				}

				if (!resolution.IsConfident)
				{
					item.Kind = HealKind.Guess;
					continue;
				}

				if (resolution.PageKey is long found)
				{
					var claim = PinnedClaim(found, pinned.Info.ObjectId);
					if (claimed.TryGetValue(claim, out var holder) && holder != pinned.ID)
					{
						item.Kind = HealKind.Duplicate;
						continue;
					}

					if (!claimed.ContainsKey(claim))
					{
						claimed.Add(claim, pinned.ID);
					}
				}

				item.Kind = WorkspaceResolver.Apply(pinned, resolution, hyperlink)
					? HealKind.Healed
					: HealKind.Unchanged;
			}

			return plan;
		}


		private static string PinnedClaim(long pageKey, string objectID)
		{
			return "p:" + pageKey + "|" + (objectID ?? string.Empty);
		}


		private static string WindowClaim(int layoutID, long pageKey)
		{
			return "l" + layoutID + "|p:" + pageKey;
		}


		// what a favorite already holds, as a string that is the same for the same target
		private static string ClaimOf(Favorite favorite)
		{
			if (favorite.PageKey is long key)
			{
				return "p:" + key;
			}

			return favorite.NotebookKey is null
				? null
				: Container(favorite.Kind, favorite.NotebookKey, favorite.SectionKey);
		}


		private static string ClaimOf(TargetResolution resolution, string kind)
		{
			if (resolution.PageKey is long key)
			{
				return "p:" + key;
			}

			return resolution.NotebookKey is null
				? null
				: Container(kind, resolution.NotebookKey, resolution.SectionKey);
		}


		private static string Container(string kind, string notebookKey, string sectionKey)
		{
			return "c:" + (kind ?? "section") + "\u001f" + notebookKey + "\u001f" + (sectionKey ?? string.Empty);
		}
	}
}
