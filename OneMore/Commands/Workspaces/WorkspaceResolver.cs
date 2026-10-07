//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Workspaces
{
	using River.OneMoreAddIn.Commands.Favorites;
	using River.OneMoreAddIn.Commands.Layouts;
	using River.OneMoreAddIn.Identity;
	using System;
	using System.Threading;
	using System.Threading.Tasks;


	/// <summary>
	/// Finds where a remembered favorite points now, and says how to navigate there and what to
	/// save. The choices are pure and tested; only <see cref="Resolve"/> talks to OneNote.
	/// </summary>
	internal static class WorkspaceResolver
	{
		/// <summary>
		/// Reads the open notebooks and finds where the remembered target is now. This runs an
		/// identity pass, so call it when a favorite could not be opened, not before every click.
		/// </summary>
		public static async Task<TargetResolution> Resolve(
			TargetQuery query, CancellationToken token = default)
		{
			using var identity = new PageIdentityProvider();
			await using var source = new OneNoteHierarchySource();

			// someone is waiting, so do not spend time filling in GUIDs; the ones this needs to
			// match a page are read anyway
			var snapshot = await new IdentityPass(identity, source).Run(token, fillGuids: false);
			if (snapshot is null)
			{
				return new TargetResolution
				{
					Outcome = ResolveOutcome.Pending,
					Method = ResolveMethod.None,
					Reason = "OneNote could not be read"
				};
			}

			return new TargetResolver(snapshot, identity.Read).Resolve(query);
		}


		/// <summary>
		/// Reads the open notebooks once and returns a resolver for finding many targets in them, as an
		/// import does. Returns null if OneNote could not be read.
		/// </summary>
		public static async Task<TargetResolver> ReadResolver(CancellationToken token = default)
		{
			using var identity = new PageIdentityProvider();
			await using var source = new OneNoteHierarchySource();

			var snapshot = await new IdentityPass(identity, source).Run(token, fillGuids: false);
			return snapshot is null ? null : new TargetResolver(snapshot);
		}


		/// <summary>
		/// Determines whether OneNote ended up where a favorite's stored link points. OneNote does not
		/// report an error when a link names a page or a section that no longer exists: it opens
		/// something else, such as the section, or does nothing, and says it succeeded. So after
		/// navigating, the page or section that is now current is compared with the one the link names.
		/// </summary>
		/// <param name="favorite">The favorite that was opened</param>
		/// <param name="currentPageLink">A link to the page now showing, or null if none could be made</param>
		/// <param name="currentSectionLink">A link to the section now showing, or null</param>
		/// <returns>False only if it is certain OneNote is somewhere else. True if it is there, or if
		/// there is no way to tell, so that a doubt never turns a good click into an error.</returns>
		internal static bool LandedOn(Favorite favorite, string currentPageLink, string currentSectionLink)
		{
			// a notebook or a section group is opened by its ID, which fails loudly
			if (favorite.PageID is null &&
				(favorite.Kind == Favorite.KindNotebook || favorite.Kind == Favorite.KindSectionGroup))
			{
				return true;
			}

			var isPage = favorite.PageID is not null;
			var expected = isPage
				? LinkGuids.PageGuid(favorite.Uri)
				: LinkGuids.SectionGuid(favorite.Uri);

			var actual = isPage
				? LinkGuids.PageGuid(currentPageLink)
				: LinkGuids.SectionGuid(currentSectionLink);

			if (expected is null || actual is null)
			{
				return true;
			}

			return string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);
		}


		/// <summary>
		/// Gets what to hand to OneNote to open a resolved target. A page or a section is opened
		/// by a link generated now, because a stored link can be out of date. A notebook or a
		/// section group is opened by its ID, because links to those are unreliable.
		/// </summary>
		/// <param name="resolution">A resolved target</param>
		/// <param name="kind">The favorite's kind; see <see cref="Favorite.Kind"/></param>
		/// <param name="hyperlink">Makes a link to a page or a section from its ID</param>
		internal static string NavigationTarget(
			TargetResolution resolution, string kind, Func<string, string> hyperlink)
		{
			var byID = resolution.PageID is null &&
				(kind == Favorite.KindNotebook || kind == Favorite.KindSectionGroup);

			return byID
				? resolution.SectionID
				: hyperlink(resolution.PageID ?? resolution.SectionID);
		}


		/// <summary>
		/// Determines whether the stored link might be out of date. Making a link is a call into
		/// OneNote, so it is only made when something about the target changed, there is no link, or
		/// the page GUID in the stored link is not the one the page has now.
		/// </summary>
		private static bool NeedsLink(bool changed, string storedUri, TargetResolution resolution, bool isPage)
		{
			if (changed || string.IsNullOrEmpty(storedUri))
			{
				return true;
			}

			if (isPage && resolution.PageGuid is not null)
			{
				var stored = LinkGuids.PageGuid(storedUri);
				return stored is not null &&
					!string.Equals(stored, resolution.PageGuid, StringComparison.OrdinalIgnoreCase);
			}

			return false;
		}


		/// <summary>
		/// Copies what was found into a layout window, ready to be saved. The name, alias, layout and
		/// z-order are the user's and are never touched.
		/// </summary>
		/// <returns>True if anything changed</returns>
		internal static bool Apply(
			LayoutWindow window, TargetResolution resolution, Func<string, string> hyperlink)
		{
			var changed = false;
			var fields = new System.Collections.Generic.List<string>();

			void Set<T>(string name, T current, T value, Action<T> assign)
			{
				if (!Equals(current, value))
				{
					assign(value);
					changed = true;
					fields.Add(name);
				}
			}

			Set(nameof(window.NotebookID), window.NotebookID, resolution.NotebookID, v => window.NotebookID = v);
			Set(nameof(window.SectionID), window.SectionID, resolution.SectionID, v => window.SectionID = v);
			Set(nameof(window.PageID), window.PageID, resolution.PageID, v => window.PageID = v);
			Set(nameof(window.PageKey), window.PageKey, resolution.PageKey, v => window.PageKey = v);
			Set(nameof(window.Location), window.Location, resolution.Location, v => window.Location = v);

			// a link that could not be made leaves the one stored in place
			var uri = NeedsLink(changed, window.Uri, resolution, true)
				? hyperlink(resolution.PageID)
				: null;

			if (!string.IsNullOrEmpty(uri))
			{
				Set(nameof(window.Uri), window.Uri, uri, v => window.Uri = v);
			}

			if (changed)
			{
				Logger.Current.Verbose($"layout window {window.ID} changed: {string.Join(", ", fields)}");
			}

			return changed;
		}


		/// <summary>
		/// Copies what was found into an item of the reading list, ready to be saved. The name and the
		/// order are the user's and are never touched. A paragraph's object ID changes along with its
		/// page's ID, so a paragraph whose page was found at a new ID falls back to the page itself.
		/// </summary>
		/// <returns>True if anything changed</returns>
		internal static bool Apply(
			PinnedItem pinned, TargetResolution resolution, Func<string, string> hyperlink)
		{
			var changed = false;
			var fields = new System.Collections.Generic.List<string>();
			var info = pinned.Info;

			void Set<T>(string name, T current, T value, Action<T> assign)
			{
				if (!Equals(current, value))
				{
					assign(value);
					changed = true;
					fields.Add(name);
				}
			}

			var idChanged = !string.Equals(info.PageId, resolution.PageID, StringComparison.Ordinal);

			Set(nameof(info.NotebookId), info.NotebookId, resolution.NotebookID, v => info.NotebookId = v);
			Set(nameof(info.SectionId), info.SectionId, resolution.SectionID, v => info.SectionId = v);
			Set(nameof(info.PageId), info.PageId, resolution.PageID, v => info.PageId = v);
			Set(nameof(pinned.PageKey), pinned.PageKey, resolution.PageKey, v => pinned.PageKey = v);
			Set(nameof(pinned.NotebookKey), pinned.NotebookKey, resolution.NotebookKey, v => pinned.NotebookKey = v);
			Set(nameof(pinned.SectionKey), pinned.SectionKey, resolution.SectionKey, v => pinned.SectionKey = v);

			if (idChanged && !string.IsNullOrEmpty(info.ObjectId))
			{
				info.ObjectId = null;
				changed = true;
				fields.Add(nameof(info.ObjectId));
			}

			// a link to a paragraph is still good while its page keeps its ID, and a link made
			// from the page ID alone would lose the paragraph; a link that could not be made
			// leaves the one stored in place
			if (string.IsNullOrEmpty(info.ObjectId))
			{
				var uri = NeedsLink(changed, info.Link, resolution, true)
					? hyperlink(resolution.PageID)
					: null;

				if (!string.IsNullOrEmpty(uri))
				{
					Set(nameof(info.Link), info.Link, uri, v => info.Link = v);
				}
			}

			if (changed)
			{
				Logger.Current.Verbose($"pinned item {pinned.ID} changed: {string.Join(", ", fields)}");
			}

			return changed;
		}


		/// <summary>
		/// Copies what was found into the favorite, ready to be saved. The name, alias, folder and sort
		/// order are the user's and are never touched: a favorite follows its target when the target is
		/// renamed or moved, but keeps the name it was given.
		/// </summary>
		/// <param name="favorite">The favorite to bring up to date</param>
		/// <param name="resolution">A resolved target</param>
		/// <param name="hyperlink">Makes a link to a page or a section from its ID</param>
		/// <returns>True if anything changed</returns>
		internal static bool Apply(
			Favorite favorite, TargetResolution resolution, Func<string, string> hyperlink)
		{
			var changed = false;
			var fields = new System.Collections.Generic.List<string>();

			void Set<T>(string name, T current, T value, Action<T> assign)
			{
				if (!Equals(current, value))
				{
					assign(value);
					changed = true;
					fields.Add(name);
				}
			}

			var isPage = favorite.PageID is not null;

			Set(nameof(favorite.NotebookID), favorite.NotebookID, resolution.NotebookID, v => favorite.NotebookID = v);
			Set(nameof(favorite.SectionID), favorite.SectionID, resolution.SectionID, v => favorite.SectionID = v);
			Set(nameof(favorite.NotebookKey), favorite.NotebookKey, resolution.NotebookKey, v => favorite.NotebookKey = v);
			Set(nameof(favorite.SectionKey), favorite.SectionKey, resolution.SectionKey, v => favorite.SectionKey = v);
			Set(nameof(favorite.Location), favorite.Location, resolution.Location, v => favorite.Location = v);

			if (isPage)
			{
				Set(nameof(favorite.PageID), favorite.PageID, resolution.PageID, v => favorite.PageID = v);
				Set(nameof(favorite.PageKey), favorite.PageKey, resolution.PageKey, v => favorite.PageKey = v);
			}

			// a link that could not be made leaves the one stored in place
			var needsLink = NeedsLink(changed, favorite.Uri, resolution, isPage);
			var uri = NavigationTarget(resolution, favorite.Kind, id => needsLink ? hyperlink(id) : null);
			if (!string.IsNullOrEmpty(uri))
			{
				Set(nameof(favorite.Uri), favorite.Uri, uri, v => favorite.Uri = v);
			}

			if (changed)
			{
				Logger.Current.Verbose($"favorite {favorite.ID} changed: {string.Join(", ", fields)}");
			}

			return changed;
		}
	}
}
