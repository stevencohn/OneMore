//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.TextIndex;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Xml.Linq;


	internal sealed class SearchHit
	{
		public string PlainText { get; set; }
		public string ObjectId { get; set; }
	}


	internal enum DateFilterMode
	{
		None = 0,
		CreatedAfter = 1,
		CreatedBefore = 2,
		UpdatedAfter = 3,
		UpdatedBefore = 4
	}


	internal sealed class SearchOptions
	{
		public bool IncludeToc { get; set; }
		public DateFilterMode DateFilter { get; set; }
		public DateTime DateValue { get; set; }
	}


	internal class SearchEngine
	{
		private readonly SearchOptions options;
		private readonly CancellationToken cancellationToken;
		private readonly Action<int> onTotalPageCount;
		private readonly Action<string> onPageSearching;
		private readonly Action<string, string, string, IList<SearchHit>> onPageFound;


		public SearchEngine(
			SearchOptions options,
			CancellationToken cancellationToken,
			Action<int> onTotalPageCount,
			Action<string> onPageSearching,
			Action<string, string, string, IList<SearchHit>> onPageFound)
		{
			this.options = options;
			this.cancellationToken = cancellationToken;
			this.onTotalPageCount = onTotalPageCount;
			this.onPageSearching = onPageSearching;
			this.onPageFound = onPageFound;
		}


		/// <summary>
		/// Gets or sets what the text index found, if it is being used. A page the index knows
		/// as it is now is answered from it, without reading the page from OneNote.
		/// </summary>
		public IndexedHits Indexed { get; set; }


		/// <summary>
		/// Gets or sets the ID of a page that is always read from OneNote, even when the index
		/// has it.
		/// </summary>
		public string LivePageID { get; set; }


		/// <summary>
		/// Gets how many pages were answered by the index.
		/// </summary>
		public int IndexedPageCount { get; private set; }


		/// <summary>
		/// Gets how many pages were read from OneNote by searching sections.
		/// </summary>
		public int ReadPageCount { get; private set; }


		public async Task SearchNotebook(OneNote one, Regex finder)
		{
			var notebook = await one.GetNotebook(OneNote.Scope.Pages);
			await SearchNotebook(one, notebook, finder);
		}


		public async Task SearchNotebook(OneNote one, XElement notebook, Regex finder)
		{
			var ns = one.GetNamespace(notebook);

			if (options.DateFilter != DateFilterMode.None)
			{
				FilterPagesByDate(notebook, ns);
			}

			onTotalPageCount(notebook.Descendants(ns + "Page").Count());
			await TraverseSections(one, ns, notebook, string.Empty, finder);
		}


		/// <summary>
		/// Searches every open notebook.
		/// </summary>
		public async Task SearchAllNotebooks(OneNote one, Regex finder)
		{
			var root = await one.GetNotebooks(OneNote.Scope.Pages);
			if (root is null)
			{
				return;
			}

			var ns = one.GetNamespace(root);

			if (options.DateFilter != DateFilterMode.None)
			{
				FilterPagesByDate(root, ns);
			}

			// the notebooks, and the sections that were opened outside of any notebook
			var containers = root.Elements()
				.Where(e => e.Name.LocalName == "Notebook" || e.Name.LocalName == "OpenSections")
				.ToList();

			onTotalPageCount(containers.Sum(c => c.Descendants(ns + "Page").Count()));

			foreach (var container in containers)
			{
				if (cancellationToken.IsCancellationRequested) break;
				await TraverseSections(one, ns, container, string.Empty, finder);
			}
		}


		// Pages are already filtered at notebook level; do not re-filter per section.
		private async Task TraverseSections(
			OneNote one, XNamespace ns, XElement parent, string path, Regex finder)
		{
			var sections = parent.Elements(ns + "Section").Where(e =>
				e.Attribute("isRecycleBin") is null &&
				e.Attribute("isInRecycleBin") is null);

			foreach (var section in sections)
			{
				if (cancellationToken.IsCancellationRequested) break;
				await Task.Yield();
				await SearchSectionPages(one, section, finder);
			}

			var sectionGroups = parent.Elements(ns + "SectionGroup")
				.Where(e => e.Attribute("isRecycleBin") is null);

			foreach (var group in sectionGroups)
			{
				if (cancellationToken.IsCancellationRequested) break;
				await Task.Yield();

				var groupName = group.Attribute("name").Value;
				path = path.Length == 0 ? groupName : $"{path}/{groupName}";

				await TraverseSections(one, ns, group, path, finder);
			}
		}


		public async Task SearchSection(OneNote one, XElement section, Regex finder)
		{
			var ns = one.GetNamespace(section);

			if (options.DateFilter != DateFilterMode.None)
			{
				FilterPagesByDate(section, ns);
			}

			onTotalPageCount(section.Descendants(ns + "Page").Count());
			await SearchSectionPages(one, section, finder);
		}


		/// <summary>
		/// Finds the page group containing the specified page: the nearest level-1 page at or
		/// before it, plus all of the subpages (pageLevel &gt; 1) that immediately follow.
		/// </summary>
		/// <param name="section">A section element with its Page children</param>
		/// <param name="ns">The OneNote namespace of the section</param>
		/// <param name="pageId">The ID of a page in the group, typically the current page</param>
		/// <returns>
		/// The Page elements of the group in hierarchy order, or null if the page is not found
		/// or the page is a lone level-1 page with no subpages
		/// </returns>
		public static IList<XElement> GetPageGroup(XElement section, XNamespace ns, string pageId)
		{
			var pages = section.Elements(ns + "Page").ToList();

			var index = pages.FindIndex(e => e.Attribute("ID")?.Value == pageId);
			if (index < 0)
			{
				return null;
			}

			var first = index;
			while (first > 0 && GetPageLevel(pages[first]) > 1)
			{
				first--;
			}

			var last = first;
			while (last + 1 < pages.Count && GetPageLevel(pages[last + 1]) > 1)
			{
				last++;
			}

			return last > first ? pages.GetRange(first, last - first + 1) : null;

			static int GetPageLevel(XElement page)
			{
				page.GetAttributeValue("pageLevel", out int level, 1);
				return level;
			}
		}


		/// <summary>
		/// Searches only the pages in the page group containing the specified page.
		/// </summary>
		/// <param name="one">The OneNote instance</param>
		/// <param name="section">A section element with its Page children; it is pruned in place</param>
		/// <param name="pageId">The ID of a page in the group, typically the current page</param>
		/// <param name="finder">The compiled search expression</param>
		public async Task SearchPageGroup(OneNote one, XElement section, string pageId, Regex finder)
		{
			var ns = one.GetNamespace(section);
			var group = GetPageGroup(section, ns, pageId);

			if (group is null)
			{
				// the current page no longer has subpages so degrade to searching just the page
				var page = await one.GetPage(pageId, OneNote.PageDetail.Basic);
				onTotalPageCount(1);
				await SearchPage(page, finder);
				return;
			}

			// prune the freshly loaded section down to the group so that SearchSection applies
			// date filtering, progress counts, and result titles exactly as it does for a section
			section.Elements(ns + "Page").Where(e => !group.Contains(e)).Remove();

			await SearchSection(one, section, finder);
		}


		// Inner page loop used by both SearchNotebook (already filtered) and SearchSection.
		private async Task SearchSectionPages(OneNote one, XElement section, Regex finder)
		{
			var ns = one.GetNamespace(section);
			var sectionId = section.Attribute("ID").Value;
			var info = await one.GetSectionInfo(sectionId);

			var pageElements = section.Elements(ns + "Page").ToList();

			foreach (var pageElement in pageElements)
			{
				if (cancellationToken.IsCancellationRequested) break;
				await Task.Yield();

				var pageId = pageElement.Attribute("ID").Value;

				// A page whose index entry is current has already been searched. The page the user
				// is on is always read, as it may have edits that OneNote has not yet recorded.
				if (Indexed is not null &&
					pageId != LivePageID &&
					Indexed.TryGet(pageId,
						pageElement.Attribute("lastModifiedTime")?.Value, out var indexedHits))
				{
					var indexedName = pageElement.Attribute("name")?.Value ?? string.Empty;
					onPageSearching(indexedName);
					IndexedPageCount++;

					if (indexedHits.Count > 0)
					{
						onPageFound(pageId, $"{info.Path}/{indexedName}", info.Color, indexedHits);
					}

					continue;
				}

				var page = await one.GetPage(pageId, OneNote.PageDetail.Basic);
				var pageName = page.Root.Attribute("name").Value;

				onPageSearching(pageName);
				ReadPageCount++;

				var hits = await SearchPageBody(page, finder);

				if (cancellationToken.IsCancellationRequested) break;

				if (hits.Any())
				{
					onPageFound(pageId, $"{info.Path}/{pageName}", info.Color, hits);
				}
			}
		}


		public async Task SearchPage(Page page, Regex finder)
		{
			var hits = await SearchPageBody(page, finder);
			if (hits.Any())
			{
				onPageFound(page.PageId, null, null, hits);
			}
		}


		private async Task<IList<SearchHit>> SearchPageBody(Page page, Regex finder)
		{
			var ns = page.Namespace;

			var paragraphs = PageTextExtractor.SelectParagraphs(page, options.IncludeToc);

			if (paragraphs.Count == 0)
			{
				return Array.Empty<SearchHit>();
			}

			var matched = await Task.Run(() =>
			{
				var hits = new List<(string objectId, string text)>();
				foreach (var paragraph in paragraphs)
				{
					if (cancellationToken.IsCancellationRequested) break;

					var text = PageTextExtractor.GetRawText(paragraph, ns);
					if (text.Length > 0 && finder.IsMatch(text))
					{
						hits.Add((paragraph.Attribute("objectID").Value, text));
					}
				}
				return hits;
			});

			return matched.Select(m => new SearchHit
			{
				PlainText = HttpUtility.HtmlDecode(m.text),
				ObjectId = m.objectId
			}).ToList();
		}


		private void FilterPagesByDate(XElement parent, XNamespace ns)
		{
			var field = "dateTime";
			var after = true;

			switch (options.DateFilter)
			{
				case DateFilterMode.CreatedBefore:
					after = false;
					break;

				case DateFilterMode.UpdatedAfter:
					field = "lastModifiedTime";
					break;

				case DateFilterMode.UpdatedBefore:
					field = "lastModifiedTime";
					after = false;
					break;
			}

			var date = options.DateValue.Date;

			var pages = parent.Descendants(ns + "Page").ToList();
			foreach (var page in pages)
			{
				var value = DateTime.Parse(
					page.Attribute(field).Value, CultureInfo.InvariantCulture).ToLocalTime().Date;

				if ((after && value < date) || (!after && value > date))
				{
					page.Remove();
				}
			}
		}
	}
}
