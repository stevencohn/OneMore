//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Layouts
{
	using River.OneMoreAddIn.Commands.Workspaces;
	using River.OneMoreAddIn.Identity;
	using System;
	using System.Linq;


	/// <summary>
	/// The parts of exporting and importing layouts that decide things, kept apart from the
	/// commands so that they can be tested without OneNote or a file. The same approach as for
	/// favorites: a file carries a <see cref="TargetFingerprint"/> and never a page key, and an
	/// import looks each window's page up in the open notebooks before storing it.
	/// </summary>
	internal static class LayoutsExchange
	{
		/// <summary>
		/// Adds to each window the fingerprint of its page, for writing to a file.
		/// </summary>
		/// <param name="collection">The layouts to be exported</param>
		/// <param name="readKey">Reads a stored page identity by its key, or returns null</param>
		public static void AddFingerprints(
			LayoutsCollection collection, Func<long, IdentityRow> readKey)
		{
			foreach (var window in collection.Layouts.SelectMany(l => l.Windows))
			{
				window.Fingerprint = FingerprintOf(window, readKey);
			}
		}


		/// <summary>
		/// Gets the fingerprint of a window's page, or null if it is not yet known, which is the
		/// case for a window the healer has not found yet.
		/// </summary>
		internal static TargetFingerprint FingerprintOf(LayoutWindow window, Func<long, IdentityRow> readKey)
		{
			if (window.PageKey is long key && readKey(key) is IdentityRow row)
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


		/// <summary>
		/// Prepares a window read from a file to be stored: looks its page up in the open notebooks
		/// and, if it is found for certain, replaces what the file carried with what is true here,
		/// so that it is stored keyed and current and a duplicate is noticed.
		/// </summary>
		/// <param name="window">A window read from a file</param>
		/// <param name="resolver">Finds pages in the open notebooks, or null if OneNote could not be
		/// read, in which case the window is stored as it came and the healer finds it later</param>
		/// <param name="hyperlink">Makes a link to a page from its ID</param>
		/// <returns>What was found, or null if there was nothing to look in</returns>
		public static TargetResolution Prepare(
			LayoutWindow window, TargetResolver resolver, Func<string, string> hyperlink)
		{
			// a key never comes from a file: it belongs to the database that made it
			window.PageKey = null;

			var found = resolver?.Resolve(TargetQuery.From(window));

			if (found is not null && found.IsConfident)
			{
				WorkspaceResolver.Apply(window, found, hyperlink);
			}

			return found;
		}
	}
}
