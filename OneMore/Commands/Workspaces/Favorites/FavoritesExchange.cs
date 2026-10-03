//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using River.OneMoreAddIn.Commands.Workspaces;
	using River.OneMoreAddIn.Identity;
	using System;
	using System.Linq;


	/// <summary>
	/// The parts of exporting and importing favorites that decide things, kept apart from the
	/// commands so that they can be tested without OneNote or a file.
	/// </summary>
	/// <remarks>
	/// What a favorite stores about its target means nothing on another machine: the OneNote IDs
	/// are different there, and the page key belongs to one database. A file therefore carries a
	/// <see cref="TargetFingerprint"/> instead, and never a page key, and an import looks each
	/// favorite up in the notebooks that are open before it stores it.
	/// </remarks>
	internal static class FavoritesExchange
	{
		/// <summary>
		/// Adds to each favorite the fingerprint of its target, for writing to a file.
		/// </summary>
		/// <param name="collection">The favorites to be exported</param>
		/// <param name="readKey">Reads a stored page identity by its key, or returns null</param>
		public static void AddFingerprints(
			FavoritesCollection collection, Func<long, IdentityRow> readKey)
		{
			foreach (var favorite in collection.Folders.SelectMany(f => f.Items).Concat(collection.Items))
			{
				favorite.Fingerprint = FingerprintOf(favorite, readKey);
			}
		}


		/// <summary>
		/// Gets the fingerprint of a favorite's target, or null if it is not yet known, which is
		/// the case for a favorite the healer has not found yet.
		/// </summary>
		internal static TargetFingerprint FingerprintOf(Favorite favorite, Func<long, IdentityRow> readKey)
		{
			if (favorite.PageID is not null)
			{
				// a page: its title and creation time come from the identity catalog
				if (favorite.PageKey is long key && readKey(key) is IdentityRow row)
				{
					return new TargetFingerprint
					{
						Title = row.Title,
						Created = row.Created,
						NotebookKey = row.NotebookKey,
						SectionKey = row.SectionKey
					};
				}

				return null;
			}

			// a notebook, section group or section: the keys are all there is, and all it needs
			return favorite.NotebookKey is null
				? null
				: new TargetFingerprint
				{
					NotebookKey = favorite.NotebookKey,
					SectionKey = favorite.SectionKey
				};
		}


		/// <summary>
		/// Prepares a favorite read from a file to be stored: looks it up in the open notebooks and,
		/// if it is found for certain, replaces what the file carried with what is true here, so that
		/// it is stored keyed and current and a duplicate is noticed.
		/// </summary>
		/// <param name="favorite">A favorite read from a file</param>
		/// <param name="resolver">Finds targets in the open notebooks, or null if OneNote could not
		/// be read, in which case the favorite is stored as it came and the healer finds it later</param>
		/// <param name="hyperlink">Makes a link to a page or a section from its ID</param>
		/// <returns>What was found, or null if there was nothing to look in</returns>
		public static TargetResolution Prepare(
			Favorite favorite, TargetResolver resolver, Func<string, string> hyperlink)
		{
			// a key never comes from a file: it belongs to the database that made it
			favorite.PageKey = null;

			var found = resolver?.Resolve(TargetQuery.From(favorite));

			if (found is not null && found.IsConfident)
			{
				WorkspaceResolver.Apply(favorite, found, hyperlink);
				return found;
			}

			// not found now, perhaps because its notebook is closed. A notebook or a section is
			// still given the keys the file carried, which say where to look and mean the same on
			// any machine with that notebook, so the healer can find it when it opens. A page is
			// not: a page favorite with notebook and section keys but no page key would be taken
			// for a section favorite by the unique index on those keys.
			if (favorite.PageID is null && favorite.Fingerprint is not null)
			{
				favorite.NotebookKey = favorite.Fingerprint.NotebookKey;
				favorite.SectionKey = favorite.Fingerprint.SectionKey;
			}

			return found;
		}
	}
}
