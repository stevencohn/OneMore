//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class SearchTitleCommand : Command, ICliCommand
	{
		private static bool commandIsActive = false;

		private IEnumerable<CardModel> selectedCards;
		private string query;
		private static SearchTitleDialog dialog;


		public SearchTitleCommand()
		{
		}


		#region CLI Implementation

		public string CommandName => "SearchTitles";

		public string Description =>
			"Search page titles across a notebook and optionally index the results";

		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("query",
				"Search terms; supports \">\" (sort by modified) and \"#hashtag\" filters",
				required: true)
			.AddString("notebook",
				"Name of the notebook to search; * or omit to search all notebooks, " +
				"\\ to search only the current notebook",
				required: false);

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
			dialog = new SearchTitleDialog();
			dialog.RunModeless((sender, e) =>
			{
				var d = sender as SearchTitleDialog;
				var indexRequested = d?.DialogResult == DialogResult.OK;

				if (indexRequested)
				{
					query = d.Query;
					selectedCards = d.SelectedCards;
				}

				// reset re-entry state synchronously, before kicking off SelectLocation below,
				// so it can never be left stranded behind an async continuation that the modeless
				// dialog's message loop exits before scheduling (which would leave commandIsActive
				// stuck true and the dialog unable to reappear until OneNote is restarted)
				commandIsActive = false;
				dialog?.Dispose();
				dialog = null;

				if (indexRequested)
				{
					using var one = new OneNote();
					one.SelectLocation(
						Resx.SearchQF_Title, Resx.SearchQF_DescriptionIndex,
						OneNote.Scope.Sections, Callback);
				}
			},
			20);

			// only reached immediately (dialog still open) when RunModeless didn't block;
			// when it did block, the callback above already ran and cleared dialog
			dialog?.Elevate(true);

			await Task.Yield();
		}


		private async Task Callback(string sectionId)
		{
			if (string.IsNullOrEmpty(sectionId))
			{
				// cancelled
				return;
			}

			logger.Start("..indexing title search results");

			try
			{
				await IndexSearchResults(sectionId);
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
			string parentId;

			using (var progress = new UI.ProgressDialog())
			{
				progress.SetMaximum(selectedCards.Count());
				progress.Show();

				one.CreatePage(sectionId, out parentId);
				var parent = await one.GetPage(parentId);

				var ns = parent.Namespace;
				PageNamespace.Set(ns);

				parent.Title = Resx.SearchTitleCommand_indexTitle;
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
			cliParams.TryGet("query", out string queryText);
			cliParams.TryGet("notebook", out string notebookOverride);

			var parsed = TitleQueryParser.Parse(queryText);
			var notebookFilter = string.IsNullOrEmpty(notebookOverride)
				? parsed.NotebookFilter
				: notebookOverride;

			Regex finder = null;
			if (!string.IsNullOrEmpty(parsed.TitleText))
			{
				finder = new TextMatchBuilder(false, false).BuildRegex(parsed.TitleText);
			}

			await using var one = new OneNote();

			var notebooksEl = await one.GetNotebooks(OneNote.Scope.Notebooks);
			for (int attempt = 1; attempt < 4 && (notebooksEl == null || !notebooksEl.HasElements); attempt++)
			{
				await Task.Delay(500 * attempt);
				notebooksEl = await one.GetNotebooks(OneNote.Scope.Notebooks);
			}

			if (notebooksEl == null)
			{
				CliOutput = "Cannot connect to OneNote";
				return;
			}

			var ns = one.GetNamespace(notebooksEl);
			var candidates = notebooksEl.Elements(ns + "Notebook")
				.Where(e => e.Attribute("isRecycleBin") is null)
				.Select(e => (
					Id: e.Attribute("ID")?.Value,
					Name: e.Attribute("name")?.Value ?? string.Empty))
				.Where(n => n.Id != null)
				.ToList();

			List<(string Id, string Name)> targets;
			if (string.IsNullOrEmpty(notebookFilter))
			{
				targets = candidates;
			}
			else if (notebookFilter == "*")
			{
				targets = candidates;
			}
			else if (notebookFilter == "\\")
			{
				targets = candidates.Where(n => n.Id == one.CurrentNotebookId).ToList();
			}
			else
			{
				targets = candidates.Where(n =>
					n.Name.IndexOf(notebookFilter, StringComparison.OrdinalIgnoreCase) >= 0)
					.ToList();
			}

			if (targets.Count == 0)
			{
				CliOutput = $"Notebook not found: {notebookFilter}";
				return;
			}

			HashSet<string> hashtagPageIds = null;
			if (parsed.Hashtags.Count > 0)
			{
				if (!HashtagProvider.CatalogExists())
				{
					CliOutput = "Hashtag catalog not found. Run a hashtag scan first.";
					return;
				}

				hashtagPageIds = new HashSet<string>();
				var hashtagQuery = string.Join(" ", parsed.Hashtags);
				using var provider = new HashtagProvider();

				if (targets.Count == 1)
				{
					var tags = provider.SearchTags(
						hashtagQuery, false, false, out _, notebookID: targets[0].Id);

					foreach (var tag in tags) { hashtagPageIds.Add(tag.PageID); }
				}
				else
				{
					var ids = new HashSet<string>(targets.Select(t => t.Id));
					var tags = provider.SearchTags(hashtagQuery, false, false, out _);

					foreach (var tag in tags)
					{
						if (ids.Contains(tag.NotebookID)) { hashtagPageIds.Add(tag.PageID); }
					}
				}
			}

			var results = new List<TitleSearchResult>();
			foreach (var (id, name) in targets)
			{
				var tree = await one.GetNotebook(id, OneNote.Scope.Pages);
				if (tree == null) { continue; }

				results.AddRange(SearchTitleEngine.SearchNotebook(tree, name, finder, hashtagPageIds));
			}

			SearchTitleEngine.Sort(results, parsed.SortByModified);

			var root = new XElement("Results",
				new XAttribute("query", queryText),
				new XAttribute("count", results.Count));

			foreach (var result in results)
			{
				root.Add(new XElement("Page",
					new XAttribute("id", result.PageId),
					new XAttribute("path", result.Path),
					new XAttribute("modified", result.Modified.ToString("o"))));
			}

			CliOutput = root.ToString();
		}
	}
}
