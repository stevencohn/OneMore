//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Commands.Snippets.Toc;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Refreshes tables of contents, scoped by the parameters supplied:
	/// <list type="bullet">
	/// <item>notebook, section, page: the page TOC on that page</item>
	/// <item>notebook, section: the page TOC on every page in the section</item>
	/// <item>notebook: the section TOC page of every section in the notebook</item>
	/// <item>none: the notebook TOC page of every notebook</item>
	/// </list>
	/// Page TOCs whose page shows no obvious changes, based on lastModifiedTime, are skipped
	/// unless forced. Section and notebook TOCs are always rebuilt.
	/// </summary>
	internal class RefreshTocCommand : Command, ICliCommand
	{
		private int refreshed;
		private int skipped;
		private int failed;


		public RefreshTocCommand()
		{
		}


		#region CLI Implementation

		public string CommandName => "RefreshToc";


		public string Description => "Refresh tables of contents in a page, section, notebook, or all notebooks";


		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of notebook; when omitted, refreshes the notebook TOC page of every notebook",
				required: false)
			.AddString("section", "Path of section; when omitted, refreshes the section TOC page of every section in the notebook",
				required: false)
			.AddString("page", "Name of page; when omitted, refreshes the page TOC of every page in the section",
				required: false)
			.AddBoolean("force", "Refresh page TOCs even if they appear to be up to date",
				required: false, defaultValue: false);

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			// only runs from the CLI; the interactive Refresh link is handled by InsertTocCommand
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;
			if (cliParams is null)
			{
				return;
			}

			cliParams.TryGet("notebook", out string notebook);
			cliParams.TryGet("section", out string section);
			cliParams.TryGet("page", out string page);
			cliParams.TryGet("force", out bool force);

			var hasNotebook = !string.IsNullOrWhiteSpace(notebook);
			var hasSection = !string.IsNullOrWhiteSpace(section);
			var hasPage = !string.IsNullOrWhiteSpace(page);

			if ((hasSection && !hasNotebook) || (hasPage && !hasSection))
			{
				CliOutput = "ERR: a section requires a notebook and a page requires a section";
				return;
			}

			await using var one = new OneNote();

			if (hasSection)
			{
				await RefreshPageTocs(one, notebook, section, hasPage ? page : "*", force);
			}
			else if (hasNotebook)
			{
				await RefreshHierarchyTocs(one, new[] { await FindNotebookId(one, notebook) }, "section");
			}
			else
			{
				await RefreshHierarchyTocs(one, await GetNotebookIds(one), "notebook");
			}

			var summary = $"refreshed:{refreshed} skipped:{skipped} failed:{failed}";
			logger.WriteLine($"refresh toc: {summary}");

			// keep any error message set while processing
			if (string.IsNullOrEmpty(CliOutput))
			{
				CliOutput = summary;
			}
		}


		// Page TOCs...

		private async Task RefreshPageTocs(
			OneNote one, string notebook, string section, string page, bool force)
		{
			var pageIds = await one.FindPagesByPath(notebook, section, page);
			if (pageIds.Length == 0)
			{
				CliOutput = $"ERR: no pages found at path {notebook}/{section}/{page}";
				return;
			}

			foreach (var pageId in pageIds)
			{
				if (Cancellation.IsCancellationRequested)
				{
					break;
				}

				await RefreshPageToc(pageId, force);
			}
		}


		private async Task RefreshPageToc(string pageId, bool force)
		{
			var parameters = await InsertTocCommand.CollectParameterDefaults(pageId);

			// skip pages without a TOC and pages hosting section or notebook TOCs
			if (parameters is null || parameters.Count == 0 ||
				parameters.Contains("section") || parameters.Contains("notebook") ||
				parameters.Contains(SectionTocGenerator.RefreshSectionCmd) ||
				parameters.Contains(NotebookTocGenerator.RefreshNotebookCmd))
			{
				return;
			}

			// refreshing preserves the user's formatting of existing entries
			if (!parameters.Any(p => p.StartsWith("refresh")))
			{
				parameters.Insert(0, "refresh");
			}

			var generator = new PageTocGenerator(parameters, pageId);

			if (!force)
			{
				var (upToDate, _) = await generator.CheckUpToDate();
				if (upToDate)
				{
					skipped++;
					logger.WriteLine($"table of contents is up to date, skipping page {pageId}");
					return;
				}
			}

			Tally(await generator.Refresh(), pageId);
		}


		// Section and notebook TOCs...

		private async Task RefreshHierarchyTocs(OneNote one, IEnumerable<string> notebookIds, string kind)
		{
			foreach (var notebookId in notebookIds)
			{
				if (notebookId is null)
				{
					CliOutput = "ERR: notebook not found";
					return;
				}

				if (Cancellation.IsCancellationRequested)
				{
					break;
				}

				var hierarchy = await one.GetNotebook(notebookId, OneNote.Scope.Pages);
				if (hierarchy is null)
				{
					continue;
				}

				var ns = one.GetNamespace(hierarchy);

				foreach (var tocPage in FindTocPages(hierarchy, ns, kind))
				{
					if (Cancellation.IsCancellationRequested)
					{
						break;
					}

					var pageId = tocPage.Attribute("ID").Value;
					var sectionId = tocPage.Ancestors(ns + "Section").First().Attribute("ID").Value;

					var parameters = await InsertTocCommand.CollectParameterDefaults(pageId)
						?? new TocParameters();

					TocGenerator generator = kind == "section"
						? new SectionTocGenerator(parameters, pageId, sectionId)
						: new NotebookTocGenerator(parameters, pageId, sectionId, notebookId);

					Tally(await generator.Refresh(), pageId);
				}
			}
		}


		private static List<XElement> FindTocPages(XElement hierarchy, XNamespace ns, string kind)
		{
			return hierarchy.Descendants(ns + "Page")
				.Where(p => p.Attribute("isInRecycleBin") is null &&
					!p.Ancestors().Any(a => a.Attribute("isRecycleBin") is not null) &&
					p.Elements(ns + "Meta").Any(m =>
						m.Attribute("name")?.Value == MetaNames.TableOfContents &&
						m.Attribute("content")?.Value == kind))
				.ToList();
		}


		private static async Task<string> FindNotebookId(OneNote one, string name)
		{
			var notebooks = await one.GetNotebooks(OneNote.Scope.Notebooks);
			if (notebooks is null)
			{
				return null;
			}

			var ns = one.GetNamespace(notebooks);
			return notebooks.Elements(ns + "Notebook")
				.FirstOrDefault(n => string.Equals(
					n.Attribute("name")?.Value, name, StringComparison.InvariantCultureIgnoreCase))
				?.Attribute("ID")?.Value;
		}


		private static async Task<List<string>> GetNotebookIds(OneNote one)
		{
			var notebooks = await one.GetNotebooks(OneNote.Scope.Notebooks);
			if (notebooks is null)
			{
				return new List<string>();
			}

			var ns = one.GetNamespace(notebooks);
			return notebooks.Elements(ns + "Notebook")
				.Select(n => n.Attribute("ID")?.Value)
				.Where(id => !string.IsNullOrEmpty(id))
				.ToList();
		}


		private void Tally(bool success, string pageId)
		{
			if (success)
			{
				refreshed++;
			}
			else
			{
				failed++;
				logger.WriteLine($"could not refresh table of contents on page {pageId}");
			}
		}
	}
}
