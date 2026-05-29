//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// CLI-only command that adds hashtags to the tag bank at the top of one or more pages.
	/// </summary>
	internal class AddHashtagCommand : Command, ICliPageCommand
	{
		public AddHashtagCommand()
		{
			IsCancelled = true;
		}


		public string CommandName => "AddHashtag";

		public string Description => "Add hashtags to the tag bank on one or more pages";

		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of the notebook to update", required: true)
			.AddString("section",
				"Path of the section to update; omit to update all sections in the notebook",
				required: false)
			.AddString("page", "Name of the page to update; requires the section parameter",
				required: false)
			.AddString("tags",
				"space-separated hashtags to add, e.g. \"#work #project\"",
				required: true);


		public override async Task Execute(params object[] args)
		{
			var cliParams = args[0] as CliParameterSet;

			cliParams.TryGet("tags", out string tags);
			if (string.IsNullOrWhiteSpace(tags))
			{
				logger.WriteLine("AddHashtagCommand: no tags specified");
				return;
			}

			tags = NormalizeTags(tags);

			await using var one = new OneNote();

			// pageId is injected by the CLI framework when section is specified;
			// in that case Execute is called once per resolved page
			if (cliParams.TryGet("pageId", out string pageId) && !string.IsNullOrEmpty(pageId))
			{
				await ProcessPage(one, pageId, tags);
				return;
			}

			cliParams.TryGet("notebook", out string notebookName);
			cliParams.TryGet("section", out string sectionPath);

			if (!string.IsNullOrWhiteSpace(sectionPath))
			{
				await ProcessSection(one, notebookName, sectionPath, tags);
			}
			else
			{
				await ProcessNotebook(one, notebookName, tags);
			}
		}


		private static string NormalizeTags(string tags)
		{
			var tokens = tags
				.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

			return string.Join(" ", tokens.Select(t => t.StartsWith("#") ? t : "#" + t));
		}


		private async Task ProcessPage(OneNote one, string pageId, string tags)
		{
			var page = await one.GetPage(pageId);
			if (page == null) return;

			if (AddTagsToBank(page, tags))
			{
				await one.Update(page);

				logger.WriteLine($"AddHashtagCommand: added tags to page: {page.Title}");
			}
		}


		private async Task ProcessSection(
			OneNote one, string notebookName, string sectionPath, string tags)
		{
			var sectionId = await one.FindSectionIdByPath(notebookName, sectionPath);
			if (string.IsNullOrEmpty(sectionId))
			{
				logger.WriteLine($"AddHashtagCommand: section not found: {sectionPath}");
				return;
			}

			var section = await one.GetSection(sectionId);
			if (section == null) return;

			var sns = one.GetNamespace(section);
			var pageIds = section
				.Descendants(sns + "Page")
				.Select(p => p.Attribute("ID")?.Value)
				.Where(id => !string.IsNullOrEmpty(id))
				.ToList();

			foreach (var pid in pageIds)
			{
				await ProcessPage(one, pid, tags);
			}
		}


		private async Task ProcessNotebook(OneNote one, string notebookName, string tags)
		{
			var notebooks = await one.GetNotebooks(OneNote.Scope.Notebooks);
			for (int attempt = 1; attempt < 4 && (notebooks == null || !notebooks.HasElements); attempt++)
			{
				await Task.Delay(500 * attempt);
				notebooks = await one.GetNotebooks(OneNote.Scope.Notebooks);
			}

			if (notebooks == null)
			{
				logger.WriteLine("AddHashtagCommand: cannot connect to OneNote");
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
				logger.WriteLine($"AddHashtagCommand: notebook not found: {notebookName}");
				return;
			}

			var notebookId = notebookEl.Attribute("ID").Value;
			var hierarchy = await one.GetNotebook(notebookId, OneNote.Scope.Pages);
			if (hierarchy == null) return;

			var hns = one.GetNamespace(hierarchy);
			var pageIds = hierarchy
				.Descendants(hns + "Page")
				.Select(p => p.Attribute("ID")?.Value)
				.Where(id => !string.IsNullOrEmpty(id))
				.ToList();

			foreach (var pid in pageIds)
			{
				await ProcessPage(one, pid, tags);
			}
		}


		private static bool AddTagsToBank(Page page, string tags)
		{
			var ns = page.Namespace;
			var banker = new TagBankCommand();
			banker.MakeWordBank(page, ns);

			if (banker.BankOutline is null) return false;

			var run = banker.BankOutline.Descendants(ns + "T").FirstOrDefault();
			if (run?.GetCData() is XCData cdata)
			{
				cdata.Value = $"{cdata.Value} {tags}";
				return true;
			}

			return false;
		}
	}
}
