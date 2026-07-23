//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class SearchCommand : Command, ICliCommand
	{
		private static bool commandIsActive = false;

		private SearchDialog.Commands command;
		private List<string> pageIds;
		private IEnumerable<CardModel> selectedCards;
		private string query;
		private SearchDialog dialog;


		public SearchCommand()
		{
		}


		#region CLI Implementation

		public string CommandName => "Search";

		public string Description => "Search for pages matching a query across a notebook, section, or page";

		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of the notebook to search", required: true)
			.AddString("section", "Path of section to search; omit to search the entire notebook",
				required: false)
			.AddString("page", "Name of page to search; requires section parameter",
				required: false)
			.AddString("query", "Search terms (supports AND, OR, NOT, * wildcard)", required: true);

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			if (runningFromCli)
			{
				await ExecuteCli(args[0] as CliParameterSet);
				return;
			}

			if (dialog is not null)
			{
				dialog.Elevate();
				return;
			}

			if (commandIsActive) { return; }
			commandIsActive = true;

			// Cleanup runs from the RunModeless close callback rather than a finally block
			// here, because RunModeless only blocks until the dialog closes when the calling
			// thread has no message loop yet (e.g. a plain ribbon click, dispatched through
			// CommandFactory's Task.Run wrapper). When invoked with a message loop already
			// running on this thread (e.g. Replay, which calls Execute directly), RunModeless
			// just Show()s the dialog and returns immediately — disposing it right here would
			// close it before the user ever sees it.
			dialog = new SearchDialog();
			dialog.RunModeless(async (sender, e) =>
			{
				try
				{
					if (sender is SearchDialog d && d.DialogResult == DialogResult.OK)
					{
						command = d.Command;
						query = d.Query;

						if (command == SearchDialog.Commands.Index)
						{
							selectedCards = d.SelectedCards;
						}
						else
						{
							pageIds = d.SelectedPages;
						}

						var desc = command switch
						{
							SearchDialog.Commands.Copy => Resx.SearchQF_DescriptionCopy,
							SearchDialog.Commands.Move => Resx.SearchQF_DescriptionMove,
							_ => Resx.SearchQF_DescriptionIndex
						};

						await using var one = new OneNote();
						one.SelectLocation(Resx.SearchQF_Title, desc, OneNote.Scope.Sections, Callback);
					}
				}
				finally
				{
					commandIsActive = false;
					dialog?.Dispose();
					dialog = null;
				}
			},
			20);

			// only reached immediately (dialog still open) when RunModeless didn't block;
			// when it did block, the callback above already ran and cleared dialog
			if (dialog != null)
			{
				dialog.Elevate(true);
			}

			await Task.Yield();
		}


		private async Task Callback(string sectionId)
		{
			if (string.IsNullOrEmpty(sectionId))
			{
				// cancelled
				return;
			}

			logger.Start($"..{command} pages");

			try
			{
				if (command == SearchDialog.Commands.Index)
				{
					await IndexSearchResults(sectionId);
				}
				else
				{
					await using var one = new OneNote();
					var service = new SearchServices(one, sectionId);

					if (command == SearchDialog.Commands.Copy)
					{
						await service.CopyPages(pageIds);
					}
					else
					{
						await service.MovePages(pageIds);
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}
			finally
			{
				logger.End();
			}
		}


		private async Task IndexSearchResults(string sectionId)
		{
			await using var one = new OneNote();
			string parentId = null;

			using (var progress = new UI.ProgressDialog())
			{
				progress.SetMaximum(selectedCards.Count());
				progress.Show();

				one.CreatePage(sectionId, out parentId);
				var parent = await one.GetPage(parentId);

				var ns = parent.Namespace;
				PageNamespace.Set(ns);

				parent.Title = Resx.SearchCommand_indexTitle;
				parent.SetMeta(MetaNames.SearchIndex, "true");

				var container = parent.EnsureContentContainer();

				var h1Index = parent.GetQuickStyle(Styles.StandardStyles.Heading1).Index;
				var todoIndex = parent.AddTagDef("3", "To Do", 4);

				container.Add(new Paragraph(query).SetQuickStyle(h1Index));

				var content = new XElement(ns + "OEChildren");
				container.Add(new Paragraph(content));

				foreach (var card in selectedCards.OrderBy(c => c.Title))
				{
					progress.SetMessage(card.Title);
					progress.Increment();

					var link = one.GetHyperlink(card.PageId, string.Empty);

					content.Add(new Paragraph(
						new Tag(todoIndex, false),
						new XElement(ns + "T",
							new XCData($"<a href=\"{link}\">{WebUtility.HtmlEncode(card.Title)}</a>"))
						));

					var bullets = new ContentList(ns);
					foreach (var hit in card.Hits)
					{
						link = one.GetHyperlink(card.PageId, hit.ObjectId);
						bullets.Add(new Bullet($"<a href=\"{link}\">{WebUtility.HtmlEncode(hit.PlainText)}</a>"));
					}

					content.Add(new Paragraph(bullets));
					content.Add(new Paragraph(string.Empty));
				}

				await one.Update(parent);
				parentId = parent.PageId;
			}

			// navigate after progress dialog is closed otherwise it will hang!
			await one.NavigateTo(parentId);
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// CLI path

		private async Task ExecuteCli(CliParameterSet cliParams)
		{
			cliParams.TryGet("notebook", out string notebookName);
			cliParams.TryGet("section", out string sectionPath);
			cliParams.TryGet("page", out string pageName);
			cliParams.TryGet("query", out string queryText);

			var finder = new TextMatchBuilder(false, false).BuildRegex(queryText);
			if (finder is null)
			{
				CliOutput = $"Invalid query: {queryText}";
				return;
			}

			var results = new List<(string pageId, string path, IList<SearchHit> hits)>();

			var engineOptions = new SearchOptions { IncludeToc = true };
			using var cts = new CancellationTokenSource();

			var engine = new SearchEngine(
				engineOptions,
				cts.Token,
				count => { },
				name => { },
				(pageId, groupTitle, color, hits) =>
					results.Add((pageId,
						groupTitle ?? $"{notebookName}/{sectionPath}/{pageName}",
						hits)));

			await using var one = new OneNote();

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

			var hasSection = !string.IsNullOrEmpty(sectionPath);
			var hasPage = !string.IsNullOrEmpty(pageName);

			if (!hasSection)
			{
				var notebookTree = await one.GetNotebook(
					notebookEl.Attribute("ID").Value, OneNote.Scope.Pages);

				if (notebookTree == null)
				{
					CliOutput = $"Cannot load notebook: {notebookName}";
					return;
				}

				await engine.SearchNotebook(one, notebookTree, finder);
			}
			else
			{
				var notebookTree = await one.GetNotebook(
					notebookEl.Attribute("ID").Value, OneNote.Scope.Sections);

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

				var sectionId = node.Attribute("ID")?.Value;
				if (string.IsNullOrEmpty(sectionId))
				{
					CliOutput = $"Section not found: {sectionPath}";
					return;
				}

				var section = await one.GetSection(sectionId);
				if (section == null)
				{
					CliOutput = $"Cannot load section: {sectionPath}";
					return;
				}

				if (!hasPage)
				{
					await engine.SearchSection(one, section, finder);
				}
				else
				{
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

					var pageId = pageEl.Attribute("ID").Value;
					var page = await one.GetPage(pageId, OneNote.PageDetail.Basic);
					await engine.SearchPage(page, finder);
				}
			}

			var root = new XElement("Results",
				new XAttribute("query", queryText),
				new XAttribute("count", results.Count));

			foreach (var (pageId, path, hits) in results)
			{
				var pageEl = new XElement("Page",
					new XAttribute("id", pageId),
					new XAttribute("path", path ?? string.Empty));

				foreach (var hit in hits)
				{
					pageEl.Add(new XElement("Hit",
						new XAttribute("objectId", hit.ObjectId),
						hit.PlainText));
				}

				root.Add(pageEl);
			}

			CliOutput = root.ToString();
		}
	}
}
