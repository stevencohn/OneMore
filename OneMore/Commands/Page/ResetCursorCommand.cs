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
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	/// <summary>
	/// Resets the text cursor on pages within a scope to the start of the page title. Pages
	/// whose cursor is already in the title are left alone.
	/// Available only from the command palette and the CLI.
	/// </summary>
	/// <remarks>
	/// This is a plain CLI command, not an ICliPageCommand, so that it controls its own page
	/// traversal and can report totals per section and a grand total rather than per page.
	/// </remarks>
	internal class ResetCursorCommand : Command, ICliCommand
	{
		// time for OneNote to finish navigating so the page cursor is realized
		private const int NavigationDelay = 300;

		// time for the page to activate its cursor after OneNote regains focus
		private const int FocusDelay = 400;


		private enum Outcome
		{
			Touched,
			AlreadyInTitle,
			Skipped
		}


		private sealed class SectionPages
		{
			public string Name;
			public List<string> PageIds = new();
		}


		private sealed class Totals
		{
			public int Seen;
			public int Touched;
			public int InTitle;
			public int Skipped;
		}


		public ResetCursorCommand()
		{
		}


		#region CLI Implementation

		public string CommandName => "ResetCursor";

		public string Description => "Reset the text cursor to the page title";

		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of notebook", required: true)
			.AddString("section", "Path of section", required: false)
			.AddString("page", "Name of page", required: false);

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;
			if (cliParams is not null)
			{
				await ExecuteCli(cliParams);
				return;
			}

			await ExecuteInteractive();
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// CLI

		private async Task ExecuteCli(CliParameterSet parameters)
		{
			parameters.TryGet("notebook", out string notebook);
			parameters.TryGet("section", out string section);
			parameters.TryGet("page", out string pageName);

			if (string.IsNullOrWhiteSpace(notebook))
			{
				CliOutput = "ERR:ResetCursor requires a 'notebook' parameter";
				return;
			}

			try
			{
				await using var one = new OneNote();

				var sections = await FindCliSections(one, notebook, section, pageName);
				if (sections.Count == 0)
				{
					CliOutput = "No pages found";
					return;
				}

				// first step: activate OneNote, otherwise it may not honor the navigation
				// or realize the cursor position of the pages
				Native.SetForegroundWindow(one.WindowHandle);
				await Task.Delay(FocusDelay);

				var initialPageId = one.CurrentPageId;
				var grand = new Totals();

				try
				{
					foreach (var sectionPages in sections)
					{
						if (Cancellation.IsCancellationRequested)
						{
							break;
						}

						var totals = await ProcessSection(
							one, sectionPages, null, Cancellation);

						grand.Seen += totals.Seen;
						grand.Touched += totals.Touched;
						grand.InTitle += totals.InTitle;
						grand.Skipped += totals.Skipped;

						// section totals only; individual pages are not reported
						await ReportProgress(
							$"section '{sectionPages.Name}': touched {totals.Touched} " +
							$"of {totals.Seen} pages");
					}
				}
				finally
				{
					// return to the original page once, after the entire batch
					if (!string.IsNullOrEmpty(initialPageId))
					{
						await one.NavigateTo(initialPageId);
					}
				}

				CliOutput =
					$"total: touched {grand.Touched} of {grand.Seen} pages " +
					$"in {sections.Count} section{(sections.Count == 1 ? string.Empty : "s")}";

				logger?.WriteLine($"{nameof(ResetCursorCommand)} {CliOutput}");
			}
			catch (Exception exc) when (exc is not OperationCanceledException)
			{
				logger?.WriteLine($"error running {nameof(ResetCursorCommand)} CLI", exc);
				CliOutput = $"ERR:{exc.Message}";
			}
		}


		private static async Task<List<SectionPages>> FindCliSections(
			OneNote one, string notebook, string section, string pageName)
		{
			if (!string.IsNullOrWhiteSpace(section))
			{
				var pageIds = await one.FindPagesByPath(
					notebook, section,
					string.IsNullOrWhiteSpace(pageName) ? "*" : pageName);

				return pageIds.Length == 0
					? new List<SectionPages>()
					: new List<SectionPages>
					{
						new SectionPages { Name = section, PageIds = pageIds.ToList() }
					};
			}

			// entire notebook
			var notebooks = await one.GetNotebooks();
			var ns = one.GetNamespace(notebooks);

			var id = notebooks?.Elements(ns + "Notebook")
				.FirstOrDefault(n => string.Equals(
					n.Attribute("name")?.Value, notebook.Trim(),
					StringComparison.InvariantCultureIgnoreCase))
				?.Attribute("ID")?.Value;

			if (string.IsNullOrEmpty(id))
			{
				return new List<SectionPages>();
			}

			var root = await one.GetNotebook(id, OneNote.Scope.Pages);
			return ToSections(one, root);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Interactive

		private async Task ExecuteInteractive()
		{
			using var guard = EnterOnce();
			if (guard is null)
			{
				logger?.WriteLine($"{nameof(ResetCursorCommand)} is already active");
				return;
			}

			try
			{
				using var dialog = new ResetCursorDialog();
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				logger?.WriteLine($"{nameof(ResetCursorCommand)} scope={dialog.Scope}");

				await using var one = new OneNote();
				var sections = await GetSections(one, dialog.Scope);
				await ResetSections(one, sections);
			}
			catch (Exception exc) when (exc is not OperationCanceledException)
			{
				logger?.WriteLine($"error running {nameof(ResetCursorCommand)}", exc);
			}
		}


		private async Task ResetSections(OneNote one, List<SectionPages> sections)
		{
			var total = sections.Sum(s => s.PageIds.Count);
			logger?.WriteLine($"{nameof(ResetCursorCommand)} {total} pages in {sections.Count} sections");
			if (total == 0)
			{
				return;
			}

			var initialPageId = one.CurrentPageId;
			var grand = new Totals();

			using var progress = new UI.ProgressDialog();
			progress.SetMaximum(total);

			// modal dialog with a Cancel button; the work runs on a background STA thread
			// while the dialog pumps messages, so OneNote navigation can be serviced
			var result = progress.ShowDialogWithCancel(async (dialog, token) =>
			{
				foreach (var section in sections)
				{
					if (token.IsCancellationRequested)
					{
						return false;
					}

					var totals = await ProcessSection(one, section, progress, token);

					grand.Seen += totals.Seen;
					grand.Touched += totals.Touched;
					grand.InTitle += totals.InTitle;
					grand.Skipped += totals.Skipped;
				}

				return true;
			});

			if (result == DialogResult.Cancel || result == DialogResult.Abort)
			{
				logger?.WriteLine($"{nameof(ResetCursorCommand)} cancelled");
			}

			// always leave the user on the page they started on, even if cancelled
			if (!string.IsNullOrEmpty(initialPageId))
			{
				await one.NavigateTo(initialPageId);
			}

			ShowInfo(string.Format(Resx.ResetCursorCommand_Result, grand.Touched, grand.Seen));
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Shared

		private async Task<Totals> ProcessSection(
			OneNote one, SectionPages section, UI.ProgressDialog progress, CancellationToken token)
		{
			var totals = new Totals();

			foreach (var pageId in section.PageIds)
			{
				if (token.IsCancellationRequested)
				{
					break;
				}

				if (progress is not null)
				{
					progress.SetMessage(section.Name);
					progress.Increment();
				}

				totals.Seen++;

				switch (await ResetPage(one, pageId))
				{
					case Outcome.Touched: totals.Touched++; break;
					case Outcome.AlreadyInTitle: totals.InTitle++; break;
					default: totals.Skipped++; break;
				}
			}

			logger?.WriteLine(
				$"section '{section.Name}': touched {totals.Touched} of {totals.Seen} pages " +
				$"({totals.InTitle} already in title, {totals.Skipped} skipped)");

			return totals;
		}


		private static async Task<List<SectionPages>> GetSections(
			OneNote one, UI.SelectorScope scope)
		{
			XElement root;
			switch (scope)
			{
				case UI.SelectorScope.Section:
					root = await one.GetSection();
					break;

				case UI.SelectorScope.Notebook:
					root = await one.GetNotebook(OneNote.Scope.Pages);
					break;

				case UI.SelectorScope.Notebooks:
					root = await one.GetNotebooks(OneNote.Scope.Pages);
					break;

				default:
					return new List<SectionPages>
					{
						new SectionPages
						{
							Name = "(current page)",
							PageIds = new List<string> { one.CurrentPageId }
						}
					};
			}

			return ToSections(one, root);
		}


		private static List<SectionPages> ToSections(OneNote one, XElement root)
		{
			var sections = new List<SectionPages>();
			if (root is null)
			{
				return sections;
			}

			var ns = one.GetNamespace(root);

			// a Section scope returns the Section element itself
			var elements = root.Name == ns + "Section"
				? new[] { root }
				: root.Descendants(ns + "Section").ToArray();

			foreach (var element in elements)
			{
				// skip anything in the recycle bin
				if (element.Attribute("isRecycleBin") is not null ||
					element.Attribute("isInRecycleBin") is not null ||
					element.Ancestors().Any(a =>
						a.Attribute("isRecycleBin") is not null ||
						a.Attribute("isInRecycleBin") is not null))
				{
					continue;
				}

				var pageIds = element.Elements(ns + "Page")
					.Select(p => p.Attribute("ID")?.Value)
					.Where(id => !string.IsNullOrEmpty(id))
					.ToList();

				if (pageIds.Count > 0)
				{
					sections.Add(new SectionPages
					{
						Name = element.Attribute("name")?.Value ?? "(unnamed)",
						PageIds = pageIds
					});
				}
			}

			return sections;
		}


		private static string DescribeSelection(Page page)
		{
			var items = page.Root.Descendants()
				.Where(e => e.Attribute("selected") is not null)
				.Select(e =>
				{
					var path = string.Join("/", e.AncestorsAndSelf()
						.Reverse()
						.Skip(1)
						.Select(a => a.Name.LocalName));

					return $"{path}={e.Attribute("selected").Value} (len {e.Value.Length})";
				})
				.ToList();

			return items.Count == 0 ? "none" : string.Join("; ", items);
		}


		private async Task<Outcome> ResetPage(OneNote one, string pageId)
		{
			// OneNote only reports the cursor for the page being viewed, so go there first
			// and give the UI a moment to settle before reading the selection
			if (!await one.NavigateTo(pageId))
			{
				logger?.WriteLine($"could not navigate to page {pageId}");
				return Outcome.Skipped;
			}

			await Task.Delay(NavigationDelay);

			// the progress dialog (or the CLI host) may hold the foreground, so the page would
			// not activate its cursor; refocus OneNote so the cursor position is realized,
			// then let it settle
			Native.SetForegroundWindow(one.WindowHandle);
			await Task.Delay(FocusDelay);

			var page = await one.GetPage(pageId, OneNote.PageDetail.All);
			if (page is null)
			{
				return Outcome.Skipped;
			}

			PageNamespace.Set(page.Namespace);

			var editor = new PageEditor(page);

			// leave pages alone if the cursor is already in the title
			if (editor.IsSelectionOnlyInTitle())
			{
				return Outcome.AlreadyInTitle;
			}

			// diagnostics: show exactly what OneNote reported as selected so we can learn what
			// a cursor in the title looks like when it is not recognized
			logger?.WriteLine($"page '{page.Title}' selection: {DescribeSelection(page)}");

			if (!editor.SetCursorAtTitle())
			{
				return Outcome.Skipped;
			}

			await one.Update(page);
			return Outcome.Touched;
		}
	}
}
