//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	/// <summary>
	/// A single page-title match.
	/// </summary>
	internal sealed class TitleSearchResult
	{
		public string PageId { get; set; }
		public string Name { get; set; }
		public string Path { get; set; }
		public string Color { get; set; }
		public DateTime Modified { get; set; }
	}


	/// <summary>
	/// Matches page titles directly off an already-fetched notebook hierarchy
	/// (OneNote.Scope.Pages), with no additional COM calls per page or section, so it can run
	/// synchronously against a cached document for both a one-shot search and live type-ahead
	/// re-filtering. Shared by SearchTitleDialog and SearchTitleCommand's CLI path so GUI and
	/// CLI results are always consistent.
	/// </summary>
	internal static class SearchTitleEngine
	{
		/// <summary>
		/// Searches a single notebook's cached hierarchy tree.
		/// </summary>
		/// <param name="notebook">The notebook element, fetched with OneNote.Scope.Pages</param>
		/// <param name="notebookName">Display name of the notebook, used as the path prefix</param>
		/// <param name="finder">Compiled title-matching regex; every page matches if null</param>
		/// <param name="hashtagPageIds">
		/// If non-null, only pages whose ID appears in this set are included, implementing the
		/// AND-filter between hashtag terms and the title text match
		/// </param>
		/// <param name="excludedHashtagPageIds">
		/// If non-null, pages whose ID appears in this set are skipped, implementing "-#hashtag"
		/// exclusion
		/// </param>
		public static List<TitleSearchResult> SearchNotebook(
			XElement notebook,
			string notebookName,
			Regex finder,
			ISet<string> hashtagPageIds = null,
			ISet<string> excludedHashtagPageIds = null)
		{
			var results = new List<TitleSearchResult>();
			var ns = notebook.GetNamespaceOfPrefix(OneNote.Prefix);

			void TraverseSections(XElement parent, string path)
			{
				foreach (var section in parent.Elements(ns + "Section"))
				{
					if (section.Attribute("isRecycleBin") != null ||
						section.Attribute("isInRecycleBin") != null)
					{
						continue;
					}

					var sectionName = section.Attribute("name")?.Value ?? string.Empty;
					var sectionPath = $"{path}/{sectionName}";
					var color = section.Attribute("color")?.Value;

					foreach (var page in section.Elements(ns + "Page"))
					{
						var id = page.Attribute("ID")?.Value;
						if (id == null)
						{
							continue;
						}

						var name = page.Attribute("name")?.Value ?? string.Empty;

						if (finder != null && !finder.IsMatch(name))
						{
							continue;
						}

						if (hashtagPageIds != null && !hashtagPageIds.Contains(id))
						{
							continue;
						}

						if (excludedHashtagPageIds != null && excludedHashtagPageIds.Contains(id))
						{
							continue;
						}

						var modified = DateTime.MinValue;
						var attr = page.Attribute("lastModifiedTime")?.Value;
						if (attr != null)
						{
							DateTime.TryParse(
								attr, CultureInfo.InvariantCulture, DateTimeStyles.None, out modified);
						}

						results.Add(new TitleSearchResult
						{
							PageId = id,
							Name = name,
							Path = $"{sectionPath}/{name}",
							Color = color,
							Modified = modified
						});
					}
				}

				foreach (var group in parent.Elements(ns + "SectionGroup"))
				{
					if (group.Attribute("isRecycleBin") != null)
					{
						continue;
					}

					var groupName = group.Attribute("name")?.Value ?? string.Empty;
					TraverseSections(group, $"{path}/{groupName}");
				}
			}

			TraverseSections(notebook, notebookName);

			return results;
		}


		/// <summary>
		/// Sorts results in place: alphabetically by name (default) or by last-modified,
		/// most recent first, when sortByModified is true.
		/// </summary>
		public static void Sort(List<TitleSearchResult> results, bool sortByModified)
		{
			if (sortByModified)
			{
				results.Sort((a, b) => b.Modified.CompareTo(a.Modified));
			}
			else
			{
				results.Sort((a, b) =>
					string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase));
			}
		}


		/// <summary>
		/// Resolves the sets of page IDs to include and exclude based on the given hashtag
		/// criteria, restricted to the given notebooks. Shared by SearchTitleDialog and
		/// SearchTitleCommand's CLI path so GUI and CLI results are always consistent.
		/// </summary>
		/// <param name="includeHashtags">
		/// Hashtags a page must carry every one of (implicit AND); Included is null if empty
		/// </param>
		/// <param name="excludeHashtags">
		/// Hashtags that exclude a page if it carries any one of them; Excluded is null if empty
		/// </param>
		/// <param name="notebookIds">The notebook IDs to restrict the search to</param>
		public static (HashSet<string> Included, HashSet<string> Excluded) ResolveHashtagFilters(
			List<string> includeHashtags, List<string> excludeHashtags, List<string> notebookIds)
		{
			using var provider = new HashtagProvider();

			// allTags:true matches against the page's full aggregated tag set rather than one
			// tag row at a time, which is required for implicit AND across distinct hashtags to
			// work correctly (a single row's tag column can't equal two different values at once)
			var included = includeHashtags.Count > 0
				? SearchTagPageIds(provider, string.Join(" ", includeHashtags), allTags: true, notebookIds)
				: null;

			// OR-of-rows is sufficient for exclusion: we only need to know whether a page
			// carries any one of the excluded hashtags, so no aggregation is needed. Pages with
			// no tags at all simply never appear here, so they're never excluded.
			var excluded = excludeHashtags.Count > 0
				? SearchTagPageIds(provider, string.Join(" OR ", excludeHashtags), allTags: false, notebookIds)
				: null;

			return (included, excluded);
		}


		private static HashSet<string> SearchTagPageIds(
			HashtagProvider provider, string hashtagQuery, bool allTags, List<string> notebookIds)
		{
			var pageIds = new HashSet<string>();

			if (notebookIds.Count == 1)
			{
				var tags = provider.SearchTags(
					hashtagQuery, caseSensitive: false, allTags: allTags,
					parsed: out _, notebookID: notebookIds[0]);

				foreach (var tag in tags)
				{
					pageIds.Add(tag.PageID);
				}
			}
			else
			{
				var ids = new HashSet<string>(notebookIds);
				var tags = provider.SearchTags(
					hashtagQuery, caseSensitive: false, allTags: allTags, parsed: out _);

				foreach (var tag in tags)
				{
					if (ids.Contains(tag.NotebookID))
					{
						pageIds.Add(tag.PageID);
					}
				}
			}

			return pageIds;
		}
	}
}
