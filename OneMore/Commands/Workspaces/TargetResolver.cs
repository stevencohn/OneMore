//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Workspaces
{
	using River.OneMoreAddIn.Commands.Favorites;
	using River.OneMoreAddIn.Commands.Layouts;
	using River.OneMoreAddIn.Identity;
	using System;
	using System.Collections.Generic;
	using System.Linq;


	/// <summary>
	/// What became of an attempt to find where a remembered target is now.
	/// </summary>
	internal enum ResolveOutcome
	{
		/// <summary>Found, and the current IDs are in the resolution.</summary>
		Resolved,

		/// <summary>
		/// Not found yet, but it may be: the hyperlink GUIDs of some pages have not been read, and
		/// the one wanted might be among them. Try again later; it is not broken.
		/// </summary>
		Pending,

		/// <summary>
		/// Known, but not reachable now because its notebook is closed or its section is locked.
		/// </summary>
		Offline,

		/// <summary>More than one thing fits equally well, so none was chosen.</summary>
		Ambiguous,

		/// <summary>Nothing fits. It has been deleted, or renamed beyond recognition.</summary>
		Broken
	}


	/// <summary>
	/// The evidence by which a target was found, strongest first.
	/// </summary>
	internal enum ResolveMethod
	{
		None,

		/// <summary>The page key from the identity catalog, or the notebook and section keys.</summary>
		Key,

		/// <summary>The remembered OneNote ID still exists.</summary>
		Handle,

		/// <summary>The hyperlink GUID inside the remembered link, which survives reopens and moves.</summary>
		Guid,

		/// <summary>
		/// The title, creation time, notebook and section of an exported page, all the same as one page.
		/// This is how a favorite carried from another machine is found when its IDs mean nothing here.
		/// </summary>
		Fingerprint,

		/// <summary>The remembered path by name. Only a guess: another page can have the same name.</summary>
		Location
	}


	/// <summary>
	/// What is remembered about a target, whether it is a favorite or a window of a layout.
	/// </summary>
	internal sealed class TargetQuery
	{
		public long? PageKey { get; set; }
		public string NotebookKey { get; set; }
		public string SectionKey { get; set; }
		public string NotebookID { get; set; }
		public string SectionID { get; set; }
		public string PageID { get; set; }
		public string Uri { get; set; }
		public string Location { get; set; }

		/// <summary>The title of the page, from a fingerprint in an imported file.</summary>
		public string Title { get; set; }

		/// <summary>When the page was created, from a fingerprint in an imported file.</summary>
		public string Created { get; set; }

		/// <summary>null or "section", "sectiongroup" or "notebook"; see <see cref="Favorite.Kind"/>.</summary>
		public string Kind { get; set; }

		public bool IsPage => PageID is not null;


		public static TargetQuery From(Favorite favorite)
		{
			return new TargetQuery
			{
				PageKey = favorite.PageKey,

				// the keys from an imported file stand in for keys the favorite does not have yet; they
				// say where to look, and are not trusted until a target is found there
				NotebookKey = favorite.NotebookKey ?? favorite.Fingerprint?.NotebookKey,
				SectionKey = favorite.SectionKey ?? favorite.Fingerprint?.SectionKey,
				Title = favorite.Fingerprint?.Title,
				Created = favorite.Fingerprint?.Created,
				NotebookID = favorite.NotebookID,
				SectionID = favorite.SectionID,
				PageID = favorite.PageID,
				Uri = favorite.Uri,
				Location = favorite.Location,
				Kind = favorite.Kind
			};
		}


		public static TargetQuery From(LayoutWindow window)
		{
			return new TargetQuery
			{
				PageKey = window.PageKey,
				NotebookKey = window.Fingerprint?.NotebookKey,
				SectionKey = window.Fingerprint?.SectionKey,
				NotebookID = window.NotebookID,
				SectionID = window.SectionID,
				PageID = window.PageID,
				Uri = window.Uri,
				Location = window.Location,
				Title = window.Fingerprint?.Title,
				Created = window.Fingerprint?.Created
			};
		}
	}


	/// <summary>
	/// Where a remembered target is now.
	/// </summary>
	internal sealed class TargetResolution
	{
		public ResolveOutcome Outcome { get; set; }
		public ResolveMethod Method { get; set; }

		/// <summary>Says why a target was not resolved, for the log and for the user.</summary>
		public string Reason { get; set; }

		public long? PageKey { get; set; }
		public string NotebookKey { get; set; }
		public string SectionKey { get; set; }
		public string NotebookID { get; set; }
		public string SectionID { get; set; }
		public string PageID { get; set; }

		/// <summary>The name of the page, section, section group or notebook.</summary>
		public string Name { get; set; }

		/// <summary>The path by name, such as /Notebook/Section/Page.</summary>
		public string Location { get; set; }

		public bool IsResolved => Outcome == ResolveOutcome.Resolved;

		/// <summary>
		/// Gets whether it is safe to save what was found. A match by key, ID or GUID is; a match
		/// by name alone is only good enough to navigate to.
		/// </summary>
		public bool IsConfident => IsResolved && Method != ResolveMethod.Location;
	}


	/// <summary>
	/// Turns what is remembered about a page or a container into where it is now, using the
	/// identity snapshot of the open notebooks. Pure logic with no OneNote or database access, so
	/// it can be tested with fixtures.
	/// </summary>
	/// <remarks>
	/// The IDs in a hyperlink are not the IDs of the hierarchy and there is no mapping from one to
	/// the other, so a page is found by a GUID by comparing it with the GUIDs the identity pass
	/// has read for the pages it can see.
	/// </remarks>
	internal sealed class TargetResolver
	{
		private readonly IdentitySnapshot snapshot;
		private readonly Func<long, IdentityRow> readKey;
		private readonly List<IdentityNotebook> notebooks;
		private readonly Dictionary<long, IdentityPage> pagesByKey;
		private readonly Dictionary<string, IdentityPage> pagesByID;
		private readonly ILookup<string, IdentityPage> pagesByGuid;
		private readonly List<IdentityPage> pages;


		/// <param name="snapshot">The pages and containers of every open notebook</param>
		/// <param name="readKey">Reads a stored identity by its page key, or returns null. Used
		/// to tell a page that is merely not reachable from one that no longer exists. Optional.</param>
		public TargetResolver(IdentitySnapshot snapshot, Func<long, IdentityRow> readKey = null)
		{
			this.snapshot = snapshot;
			this.readKey = readKey;

			notebooks = snapshot.Notebooks.ToList();
			pages = snapshot.Pages.Where(p => p.PageKey > 0).ToList();

			pagesByKey = pages
				.GroupBy(p => p.PageKey)
				.ToDictionary(g => g.Key, g => g.First());

			pagesByID = pages
				.GroupBy(p => p.Ref.PageID, StringComparer.Ordinal)
				.ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

			pagesByGuid = pages
				.Where(p => p.PageGuid is not null)
				.ToLookup(p => p.PageGuid, StringComparer.OrdinalIgnoreCase);
		}


		/// <summary>
		/// Finds where a remembered target is now.
		/// </summary>
		public TargetResolution Resolve(TargetQuery query)
		{
			return query.IsPage ? ResolvePage(query) : ResolveContainer(query);
		}


		#region Pages
		private TargetResolution ResolvePage(TargetQuery query)
		{
			// 1. the page key, which follows the page through reopens, moves and renames
			if (query.PageKey is long key)
			{
				if (pagesByKey.TryGetValue(key, out var found))
				{
					return Found(found, ResolveMethod.Key);
				}

				var row = readKey?.Invoke(key);
				if (row is not null)
				{
					return Failed(ResolveOutcome.Offline, row.IsMissing
						? "the page is not in any open notebook"
						: "the page is in a locked section or a notebook that is not open");
				}

				// the page was forgotten; fall through and look for it another way
			}

			// 2. the remembered ID, if OneNote has not regenerated it since
			if (query.PageID is not null && pagesByID.TryGetValue(query.PageID, out var byID))
			{
				return Found(byID, ResolveMethod.Handle);
			}

			// 3. the GUID in the remembered link, which survives reopens and moves
			var guid = LinkGuids.PageGuid(query.Uri);
			if (guid is not null)
			{
				var matches = pagesByGuid[guid].ToList();
				if (matches.Count == 1)
				{
					return Found(matches[0], ResolveMethod.Guid);
				}

				if (matches.Count > 1)
				{
					return Failed(ResolveOutcome.Ambiguous,
						$"{matches.Count} pages have the same link GUID, probably copies of one page");
				}
			}

			// 3b. what an exported file recorded about the page: title, creation time and where
			var print = FingerprintOf(query);
			if (print is not null)
			{
				var same = pages
					.Where(p => p.Ref.Title == print.Title && p.Ref.Created == print.Created &&
						p.Ref.NotebookKey == print.NotebookKey && p.Ref.SectionKey == print.SectionKey)
					.ToList();

				if (same.Count == 1)
				{
					return Found(same[0], ResolveMethod.Fingerprint);
				}

				if (same.Count > 1)
				{
					return Failed(ResolveOutcome.Ambiguous, $"{same.Count} pages match the exported fingerprint");
				}
			}

			if (guid is not null && snapshot.GuidsPending > 0)
			{
				// the page wanted may be among those whose GUID has not been read yet
				return Failed(ResolveOutcome.Pending,
					$"{snapshot.GuidsPending} pages have not had their link GUID read yet");
			}

			// 4. the remembered path by name; a guess, so it is marked as one
			if (!string.IsNullOrEmpty(query.Location))
			{
				var named = pages
					.Where(p => string.Equals(
						p.SectionPath + "/" + p.Ref.Title, query.Location, StringComparison.OrdinalIgnoreCase))
					.ToList();

				if (named.Count == 1)
				{
					return Found(named[0], ResolveMethod.Location);
				}

				if (named.Count > 1)
				{
					return Failed(ResolveOutcome.Ambiguous, $"{named.Count} pages have that name and path");
				}
			}

			return NothingFits(query);
		}


		// the fingerprint of an imported page, or null if the query does not have a whole one
		private static TargetFingerprint FingerprintOf(TargetQuery query)
		{
			var print = new TargetFingerprint
			{
				Title = query.Title,
				Created = query.Created,
				NotebookKey = query.NotebookKey,
				SectionKey = query.SectionKey
			};

			return print.IdentifiesAPage ? print : null;
		}


		private static TargetResolution Found(IdentityPage page, ResolveMethod method)
		{
			return new TargetResolution
			{
				Outcome = ResolveOutcome.Resolved,
				Method = method,
				PageKey = page.PageKey,
				NotebookKey = page.Ref.NotebookKey,
				SectionKey = page.Ref.SectionKey,
				NotebookID = page.NotebookID,
				SectionID = page.SectionID,
				PageID = page.Ref.PageID,
				Name = page.Ref.Title,
				Location = page.SectionPath + "/" + page.Ref.Title
			};
		}
		#endregion Pages


		#region Containers
		private TargetResolution ResolveContainer(TargetQuery query)
		{
			var kind = query.Kind switch
			{
				Favorite.KindNotebook => (ContainerKind?)null,
				Favorite.KindSectionGroup => ContainerKind.SectionGroup,
				_ => ContainerKind.Section
			};

			var isNotebook = query.Kind == Favorite.KindNotebook;

			// 1. the keys, which follow it through reopens
			if (query.NotebookKey is not null)
			{
				var book = notebooks.FirstOrDefault(n => n.Key == query.NotebookKey);
				if (book is null)
				{
					return Failed(ResolveOutcome.Offline, "its notebook is not open");
				}

				if (isNotebook)
				{
					return FoundNotebook(book, ResolveMethod.Key);
				}

				if (query.SectionKey is not null)
				{
					var inside = book.Containers.FirstOrDefault(
						c => c.Kind == kind && c.SectionKey == query.SectionKey);

					if (inside is not null)
					{
						return FoundContainer(book, inside, ResolveMethod.Key);
					}
				}
			}

			// 2. the remembered ID, if OneNote has not regenerated it since
			if (isNotebook)
			{
				var book = notebooks.FirstOrDefault(n => n.ID == query.SectionID || n.ID == query.NotebookID);
				if (book is not null)
				{
					return FoundNotebook(book, ResolveMethod.Handle);
				}
			}
			else if (query.SectionID is not null)
			{
				foreach (var book in notebooks)
				{
					var inside = book.Containers.FirstOrDefault(c => c.Kind == kind && c.ID == query.SectionID);
					if (inside is not null)
					{
						return FoundContainer(book, inside, ResolveMethod.Handle);
					}
				}
			}

			// 3. the remembered path by name; a guess, so it is marked as one
			if (!string.IsNullOrEmpty(query.Location))
			{
				var parts = query.Location.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length > 0)
				{
					var books = notebooks
						.Where(n => string.Equals(n.Name, parts[0], StringComparison.OrdinalIgnoreCase))
						.ToList();

					if (books.Count > 1)
					{
						return Failed(ResolveOutcome.Ambiguous, "more than one open notebook has that name");
					}

					if (books.Count == 0)
					{
						return Failed(ResolveOutcome.Offline, $"the notebook '{parts[0]}' is not open");
					}

					if (isNotebook && parts.Length == 1)
					{
						return FoundNotebook(books[0], ResolveMethod.Location);
					}

					var named = books[0].Containers
						.Where(c => c.Kind == kind &&
							string.Equals(c.Path, query.Location, StringComparison.OrdinalIgnoreCase))
						.ToList();

					if (named.Count == 1)
					{
						return FoundContainer(books[0], named[0], ResolveMethod.Location);
					}

					if (named.Count > 1)
					{
						return Failed(ResolveOutcome.Ambiguous, $"{named.Count} containers have that name and path");
					}
				}
			}

			return NothingFits(query);
		}


		private static TargetResolution FoundNotebook(IdentityNotebook book, ResolveMethod method)
		{
			return new TargetResolution
			{
				Outcome = ResolveOutcome.Resolved,
				Method = method,
				NotebookKey = book.Key,
				SectionKey = null,
				NotebookID = book.ID,

				// a notebook favorite stores its own ID where a section ID would go
				SectionID = book.ID,
				Name = book.Name,
				Location = book.Path
			};
		}


		private static TargetResolution FoundContainer(
			IdentityNotebook book, IdentityContainer container, ResolveMethod method)
		{
			return new TargetResolution
			{
				Outcome = ResolveOutcome.Resolved,
				Method = method,
				NotebookKey = book.Key,
				SectionKey = container.SectionKey,
				NotebookID = book.ID,
				SectionID = container.ID,
				Name = container.Name,
				Location = container.Path
			};
		}
		#endregion Containers


		private static TargetResolution Failed(ResolveOutcome outcome, string reason)
		{
			return new TargetResolution { Outcome = outcome, Method = ResolveMethod.None, Reason = reason };
		}


		// nothing matched; say whether that is because its notebook is closed or because it is gone
		private TargetResolution NothingFits(TargetQuery query)
		{
			var parts = (query.Location ?? string.Empty)
				.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length > 0 && !notebooks.Any(
				n => string.Equals(n.Name, parts[0], StringComparison.OrdinalIgnoreCase)))
			{
				return Failed(ResolveOutcome.Offline, $"the notebook '{parts[0]}' is not open");
			}

			return Failed(ResolveOutcome.Broken, "it no longer exists, or has been renamed or moved");
		}
	}
}
