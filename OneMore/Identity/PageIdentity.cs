//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Identity
{
	using System.Collections.Generic;


	/// <summary>
	/// A page as currently reported by the OneNote hierarchy. Built from hierarchy data only,
	/// so it can be produced for every page without loading any page content.
	/// </summary>
	/// <remarks>
	/// OneNote regenerates every page, section and notebook ID when a notebook is closed and
	/// reopened, so none of them identify a page for long. The notebook and section keys are
	/// therefore derived from names and paths; see <see cref="PageIdentityKeys"/>.
	/// </remarks>
	internal sealed class PageRef
	{
		public PageRef(string pageID, string notebookKey, string sectionKey,
			string title, string created, string modified, int level = 1)
		{
			PageID = pageID;
			NotebookKey = notebookKey ?? string.Empty;
			SectionKey = sectionKey ?? string.Empty;
			Title = title ?? string.Empty;
			Created = created ?? string.Empty;
			Modified = modified ?? string.Empty;
			Level = level;
		}


		/// <summary>The OneNote page ID, valid only until the notebook is next reopened.</summary>
		public string PageID { get; }

		/// <summary>Identifies the notebook by path or name, never by its OneNote ID.</summary>
		public string NotebookKey { get; }

		/// <summary>Identifies the section by section-group path and name.</summary>
		public string SectionKey { get; }

		public string Title { get; }

		/// <summary>The page creation time. Users can edit it, so it is only a hint.</summary>
		public string Created { get; }

		public string Modified { get; }

		public int Level { get; }

		/// <summary>
		/// The page GUID of a hyperlink to the page, or null when it has not been read. It is not
		/// part of the hierarchy: reading it costs a call to OneNote per page, so the identity pass
		/// sets it only where it is needed to match a page, or when filling it in.
		/// </summary>
		public string PageGuid { get; internal set; }
	}


	/// <summary>
	/// A stored page identity.
	/// </summary>
	internal sealed class IdentityRow
	{
		/// <summary>The database-side identity. Never reused for another page.</summary>
		public long PageKey { get; set; }
		public string PageID { get; set; }
		public string NotebookKey { get; set; }
		public string SectionKey { get; set; }
		public string Title { get; set; }
		public string Created { get; set; }
		public string Modified { get; set; }
		public int Level { get; set; }

		/// <summary>
		/// Gets or sets when the page was first noticed missing from the hierarchy, or null
		/// if it is present. A missing page is kept, not deleted, so that it can be found
		/// again; a notebook that was just reopened fills in gradually.
		/// </summary>
		public string MissingSince { get; set; }

		/// <summary>
		/// Gets or sets the page GUID of a hyperlink to the page, or null if it has not been read
		/// yet; see <see cref="LinkGuids"/>.
		/// </summary>
		public string PageGuid { get; set; }

		public bool IsMissing => MissingSince is not null;
	}


	/// <summary>
	/// How a page was matched to a stored identity, strongest evidence first.
	/// </summary>
	internal enum ResolutionKind
	{
		/// <summary>Same OneNote page ID as stored.</summary>
		Known,

		/// <summary>
		/// ID changed (typically a notebook reopen) but notebook, section, title and
		/// creation time are all unchanged.
		/// </summary>
		SameLocation,

		/// <summary>Moved to another section; title and creation time are unchanged.</summary>
		MovedSection,

		/// <summary>Moved to another notebook; title and creation time are unchanged.</summary>
		MovedNotebook,

		/// <summary>
		/// Nothing about the page is the same, such as a rename together with a move and an edited
		/// creation time, but its hyperlink GUID is the same as exactly one stored identity.
		/// </summary>
		SameGuid,

		/// <summary>
		/// Same notebook, section and title but a different creation time, which means the
		/// creation time was edited. The weakest hierarchy match, so content must be
		/// re-examined.
		/// </summary>
		SameTitle,

		/// <summary>No stored identity matched; this is a new page.</summary>
		New
	}


	/// <summary>
	/// The outcome of resolving one page.
	/// </summary>
	internal sealed class PageResolution
	{
		public PageResolution(PageRef page, IdentityRow row, ResolutionKind kind)
		{
			Page = page;
			Row = row;
			Kind = kind;
			PageKey = row?.PageKey ?? 0;
		}


		public PageRef Page { get; }

		/// <summary>Gets the stored identity matched, or null for a new page.</summary>
		public IdentityRow Row { get; }

		public ResolutionKind Kind { get; }

		/// <summary>Gets the page key; for a new page, set once it has been stored.</summary>
		public long PageKey { get; internal set; }

		/// <summary>
		/// Gets whether the page content must be reprocessed even if its modified time
		/// matches what was last processed, because the match is not certain.
		/// </summary>
		public bool NeedsRefresh => Kind == ResolutionKind.SameTitle;
	}


	/// <summary>
	/// The result of matching a set of pages against stored identities.
	/// </summary>
	internal sealed class PageMatchResult
	{
		public PageMatchResult(IReadOnlyList<PageResolution> pages, IReadOnlyList<IdentityRow> orphans)
		{
			Pages = pages;
			Orphans = orphans;
		}


		/// <summary>One resolution per input page, in input order.</summary>
		public IReadOnlyList<PageResolution> Pages { get; }

		/// <summary>Stored identities that no current page claimed.</summary>
		public IReadOnlyList<IdentityRow> Orphans { get; }
	}


	/// <summary>
	/// Builds the notebook and section keys that identify containers across notebook
	/// reopens, when OneNote regenerates every container ID.
	/// </summary>
	internal static class PageIdentityKeys
	{
		/// <summary>
		/// Gets the key of a notebook from its path (a cloud URL or a local folder), or from
		/// its name when it has no path. Case and a trailing separator are ignored.
		/// </summary>
		public static string NotebookKey(string path, string name)
		{
			var key = string.IsNullOrWhiteSpace(path) ? name : path;
			return (key ?? string.Empty).Trim().TrimEnd('/', '\\').ToLowerInvariant();
		}


		/// <summary>
		/// Gets a single key for a section within a notebook, for sets of sections.
		/// </summary>
		public static string SectionScope(string notebookKey, string sectionKey)
		{
			return notebookKey + "\u001f" + sectionKey;
		}


		/// <summary>
		/// Gets the key of a section from the names of the section groups that contain it,
		/// outermost first, and its own name.
		/// </summary>
		public static string SectionKey(IEnumerable<string> groups, string section)
		{
			var key = string.Empty;
			if (groups is not null)
			{
				foreach (var group in groups)
				{
					key += "/" + group;
				}
			}

			return key + "/" + section;
		}
	}
}
