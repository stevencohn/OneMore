//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using River.OneMoreAddIn.Cli;


	/// <summary>
	/// CLI-only command that searches the hashtag index across notebooks, sections, or pages.
	/// </summary>
	internal class SearchHashtagsCommand : Command, ICliCommand
	{
		public SearchHashtagsCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public string CommandName => "SearchHashtags";

		public string Description => "Search hashtag index for pages matching a query";

		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of the notebook to search; omit to search all notebooks",
				required: false)
			.AddString("section", "Path of section to search; omit to search entire notebook",
				required: false)
			.AddString("page", "Name of page to search; requires section parameter",
				required: false)
			.AddString("query", "Hashtag search criteria (supports AND, OR, NOT, * wildcard)",
				required: true)
			.AddBoolean("allTags", "Include all tags on each page in addition to query-matching tags",
				required: false, defaultValue: false);


		public override async Task Execute(params object[] args)
		{
			var cliParams = args[0] as CliParameterSet;

			cliParams.TryGet("notebook", out string notebookName);
			cliParams.TryGet("section", out string sectionPath);
			cliParams.TryGet("page", out string pageName);
			cliParams.TryGet("query", out string queryText);
			cliParams.TryGet("allTags", out bool allTags);

			if (!HashtagProvider.CatalogExists())
			{
				CliOutput = "Hashtag catalog not found. Run a hashtag scan first.";
				return;
			}

			await using var one = new OneNote();

			var hasNotebook = !string.IsNullOrEmpty(notebookName);
			var hasSection = !string.IsNullOrEmpty(sectionPath);
			var hasPage = !string.IsNullOrEmpty(pageName);

			string notebookID = null;

			if (hasNotebook)
			{
				// resolve notebook name → ID

				var notebooks = await one.GetNotebooks(OneNote.Scope.Notebooks);
				for (int attempt = 1; attempt < 4 && (notebooks == null || !notebooks.HasElements); attempt++)
				{
					await Task.Delay(500 * attempt);
					notebooks = await one.GetNotebooks(OneNote.Scope.Notebooks);
				}

				if (notebooks == null)
				{
					CliOutput = "Cannot connect to OneNote";
					return;
				}

				var nns = one.GetNamespace(notebooks);
				var notebookEl = notebooks
					.Elements(nns + "Notebook")
					.FirstOrDefault(n => string.Equals(
						n.Attribute("name")?.Value, notebookName,
						StringComparison.InvariantCultureIgnoreCase));

				if (notebookEl == null)
				{
					CliOutput = $"Notebook not found: {notebookName}";
					return;
				}

				notebookID = notebookEl.Attribute("ID").Value;
			}

			string sectionID = null;
			string pageID = null;

			if (hasSection)
			{
				if (!hasNotebook)
				{
					CliOutput = "The notebook parameter is required when section is specified";
					return;
				}

				// resolve section path → ID

				var notebookTree = await one.GetNotebook(notebookID, OneNote.Scope.Sections);
				if (notebookTree == null)
				{
					CliOutput = $"Cannot load notebook: {notebookName}";
					return;
				}

				var node = notebookTree;
				var parts = sectionPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var part in parts)
				{
					node = node.Elements().FirstOrDefault(e =>
						(e.Name.LocalName == "Section" || e.Name.LocalName == "SectionGroup") &&
						string.Equals(e.Attribute("name")?.Value, part,
							StringComparison.InvariantCultureIgnoreCase));

					if (node == null)
					{
						CliOutput = $"Section not found: {sectionPath}";
						return;
					}
				}

				sectionID = node.Attribute("ID")?.Value;
				if (string.IsNullOrEmpty(sectionID))
				{
					CliOutput = $"Section not found: {sectionPath}";
					return;
				}

				if (hasPage)
				{
					// resolve page name → OneNote pageID for post-filtering

					var section = await one.GetSection(sectionID);
					if (section == null)
					{
						CliOutput = $"Cannot load section: {sectionPath}";
						return;
					}

					var sns = one.GetNamespace(section);
					var pageEl = section.Descendants(sns + "Page")
						.FirstOrDefault(p => string.Equals(
							p.Attribute("name")?.Value, pageName,
							StringComparison.InvariantCultureIgnoreCase));

					if (pageEl == null)
					{
						CliOutput = $"Page not found: {pageName}";
						return;
					}

					pageID = pageEl.Attribute("ID").Value;
				}
			}

			// query the hashtag database

			var provider = new HashtagProvider();
			var tags = provider.SearchTags(queryText, 
				caseSensitive: false,
				allTags: allTags, 
				parsed: out _,
				notebookID: notebookID,
				sectionID: sectionID);

			// post-filter to a single page when requested
			IEnumerable<Hashtag> filtered = hasPage
				? tags.Where(t => t.PageID == pageID)
				: tags;

			// group hits by page and emit XML

			var grouped = filtered
				.GroupBy(t => t.PageID)
				.ToList();

			var root = new XElement("Results",
				new XAttribute("query", queryText),
				new XAttribute("count", filtered.Count()),
				new XAttribute("pages", grouped.Count));

			foreach (var group in grouped)
			{
				var first = group.First();
				var pageEl = new XElement("Page",
					new XAttribute("id", first.PageID),
					new XAttribute("path", first.HierarchyPath ?? string.Empty),
					new XAttribute("title", first.PageTitle ?? string.Empty));

				foreach (var tag in group.OrderBy(t => t.DocumentOrder))
				{
					pageEl.Add(new XElement("Hit",
						new XAttribute("tag", tag.Tag),
						new XAttribute("objectId", tag.ObjectID ?? string.Empty),
						new XAttribute("directHit", tag.DirectHit),
						new XAttribute("lastModified", tag.LastModified ?? string.Empty),
						tag.Snippet ?? string.Empty));
				}

				root.Add(pageEl);
			}

			CliOutput = root.ToString();
		}
	}
}
