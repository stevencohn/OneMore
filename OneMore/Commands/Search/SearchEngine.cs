//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
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
		private readonly Regex cleaner;
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

			cleaner = new Regex(
				@"(?:<\s*(?:span|a)[^>]*?>)|(?:</(?:span|a)>)|(?:&#\d+;)",
				RegexOptions.Compiled);
		}


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
			await TraverseSections(notebook, string.Empty);

			// Pages are already filtered at notebook level; do not re-filter per section.
			async Task TraverseSections(XElement parent, string path)
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

					await TraverseSections(group, path);
				}
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


		// Inner page loop used by both SearchNotebook (already filtered) and SearchSection.
		private async Task SearchSectionPages(OneNote one, XElement section, Regex finder)
		{
			var ns = one.GetNamespace(section);
			var sectionId = section.Attribute("ID").Value;
			var info = await one.GetSectionInfo(sectionId);

			var pageIds = section.Elements(ns + "Page")
				.Select(e => e.Attribute("ID").Value)
				.ToList();

			foreach (var pageId in pageIds)
			{
				if (cancellationToken.IsCancellationRequested) break;
				await Task.Yield();

				var page = await one.GetPage(pageId, OneNote.PageDetail.Basic);
				var pageName = page.Root.Attribute("name").Value;

				onPageSearching(pageName);

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

			var paragraphs = page.BodyOutlines
				.Descendants(ns + "OE")
				.ToList();

			if (!options.IncludeToc)
			{
				paragraphs = paragraphs.Where(e =>
					(e.Elements(ns + "T").Any()
					&& !e.AncestorsAndSelf(ns + "OE")
						.Any(a => a.Elements(ns + "Meta")
							.Any(m => m.Attribute("name")?.Value == "omToc"))
					) ||
					e.Elements(ns + "InsertedFile").Any()
				)
				.ToList();
			}

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

					var text = GetRawText(paragraph, ns);
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


		private string GetRawText(XElement paragraph, XNamespace ns)
		{
			var text = string.Empty;
			paragraph.Elements(ns + "T").ForEach(e =>
			{
				var line = cleaner.Replace(e.Value, string.Empty).Trim();
				if (line.Length > 0)
				{
					text = $"{text}{line} ";
				}
			});

			if (text.Length == 0)
			{
				paragraph.Elements(ns + "InsertedFile").ForEach(e =>
				{
					if (e.Attribute("pathSource")?.Value is string pathSource)
					{
						text = $"{text} {pathSource}";
					}

					if (e.Attribute("preferredName")?.Value is string preferredName)
					{
						text = $"{text} {preferredName}";
					}
				});
			}

			return text.Trim();
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
