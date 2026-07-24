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
		public static List<TitleSearchResult> SearchNotebook(
			XElement notebook,
			string notebookName,
			Regex finder,
			ISet<string> hashtagPageIds = null)
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
	}
}
