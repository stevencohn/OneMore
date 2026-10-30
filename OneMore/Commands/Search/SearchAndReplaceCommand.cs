//************************************************************************************************
// Copyright © 2018 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Web;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Opens a modeless dialog to find, replace or highlight text. On This Page the dialog
	/// steps through matches with Find Next and Replace; every scope can Replace All.
	/// </summary>
	internal class SearchAndReplaceCommand : Command
	{
		private Page currentPage;
		private OneNote.Scope scope;
		private SearchAndReplaceDialog searchDialog;
		private SearchAndReplaceSession session;
		private SearchAndReplaceEditor editor;
		private string signature;
		private bool searched;

		// the color of the session is chosen once, so every match gets the same color
		private int highlightChoice;
		private int highlightIndex;
		private string highlightColor;


		public SearchAndReplaceCommand()
		{
		}


		/// <summary>
		/// Gets a value indicating whether to highlight matches instead of replacing them.
		/// </summary>
		protected virtual bool Highlighting => false;


		public override async Task Execute(params object[] args)
		{
			using var guard = EnterOnce();
			if (guard is null) { return; }

			string text;
			await using (var one = new OneNote(out currentPage, out _))
			{
				if (currentPage is null)
				{
					return;
				}

				text = new PageEditor(currentPage).GetSelectedText();
			}

			session = new SearchAndReplaceSession(currentPage.PageId, StampPage);
			searchDialog = new SearchAndReplaceDialog(Highlighting);

			if (text.Length > 0)
			{
				searchDialog.WhatText = text;
			}

			if (Highlighting)
			{
				searchDialog.SetPalette(HighlightRotator.GetPalette(currentPage));
			}

			searchDialog.FindNextClicked += FindNext;
			searchDialog.ReplaceClicked += ReplaceOne;
			searchDialog.ReplaceAllClicked += ReplaceAll;

			searchDialog.RunModeless(Closed);
		}


		// remember the position in the highlighter rotation on every page that is highlighted
		private void StampPage(Page page)
		{
			if (Highlighting)
			{
				HighlightRotator.SaveIndex(page, highlightIndex);
			}
		}


		private async void FindNext(object sender, EventArgs e)
		{
			await Step(s => session.FindNext(s));
		}


		private async void ReplaceOne(object sender, EventArgs e)
		{
			await Step(s => Highlighting ? session.Highlight(s) : session.Replace(s));
		}


		private async Task Step(Func<SearchAndReplaceEditor, Task<SessionResult>> action)
		{
			searchDialog.SetBusy(true);
			try
			{
				editor = await MakeEditor();
				var result = await action(editor);
				searched = true;

				searchDialog.SetStatus(Describe(result));
			}
			catch (Exception exc)
			{
				logger.WriteLine("error stepping through matches", exc);
				searchDialog.SetStatus(exc.Message);
			}
			finally
			{
				searchDialog.SetBusy(false);
			}
		}


		private string Describe(SessionResult result)
		{
			if (result.Found)
			{
				return string.Format(
					result.Wrapped
						? Resx.SearchAndReplaceDialog_matchWrapped
						: Resx.SearchAndReplaceDialog_matchStatus,
					result.Ordinal, result.Total);
			}

			return result.Changed > 0
				? string.Format(Results, result.Changed, 1)
				: Resx.SearchAndReplaceCommand_NoMatches;
		}


		private string Results => Highlighting
			? Resx.SearchAndHighlightCommand_Results
			: Resx.SearchAndReplaceCommand_Results;


		private async void ReplaceAll(object sender, EventArgs e)
		{
			searchDialog.SetBusy(true);
			try
			{
				editor = await MakeEditor();
				scope = searchDialog.Scope;

				if (scope == OneNote.Scope.Self)
				{
					var count = await session.ApplyAll(editor);
					logger.WriteLine($"found {count} matches on {currentPage.Title}");

					searchDialog.SetStatus(count > 0
						? string.Format(Results, count, 1)
						: Resx.SearchAndReplaceCommand_NoMatches);
				}
				else
				{
					// put the current page back the way it was before searching all pages
					await session.Release();
					session.Reset();

					await SearchHierarchy();
					searchDialog.SetStatus(string.Empty);
				}

				searched = true;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error replacing all matches", exc);
				searchDialog.SetStatus(exc.Message);
			}
			finally
			{
				searchDialog.SetBusy(false);
			}
		}


		private async void Closed(object sender, EventArgs e)
		{
			try
			{
				await session.Release();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error restoring page", exc);
			}

			if (searched && editor is not null)
			{
				SaveSettings();
			}
		}


		// Build an editor from the current state of the dialog
		private async Task<SearchAndReplaceEditor> MakeEditor()
		{
			// searching for something new starts again from the top of the page
			var current = $"{searchDialog.WhatText}\u0001{searchDialog.UseRegex}\u0001{searchDialog.MatchCase}";
			if (current != signature)
			{
				signature = current;
				session.Reset();
			}

			if (Highlighting)
			{
				return new SearchAndReplaceEditor(
					searchDialog.WhatText,
					searchDialog.UseRegex,
					searchDialog.MatchCase,
					await ResolveColor());
			}

			// let user insert a newline char
			var withText = searchDialog.WithText.Replace("\\n", "\n");

			return new SearchAndReplaceEditor(
				searchDialog.WhatText,
				withText,
				searchDialog.UseRegex,
				searchDialog.MatchCase);
		}


		// Resolve the color, either the one chosen or the next in the rotating sequence
		private async Task<string> ResolveColor()
		{
			var choice = searchDialog.HighlightIndex;
			if (highlightColor is not null && choice == highlightChoice)
			{
				return highlightColor;
			}

			var index = choice;
			string color;

			if (choice < 0)
			{
				await using var one = new OneNote();
				var page = await one.GetPage(currentPage.PageId, OneNote.PageDetail.Basic);
				(index, color) = HighlightRotator.GetColor(page, advance: true);
			}
			else
			{
				color = searchDialog.HighlightColor;
			}

			highlightChoice = choice;
			highlightIndex = index;
			highlightColor = color;
			return color;
		}


		public async Task<bool> SearchHierarchy()
		{
			using var one = new OneNote();
			var hierarchy = scope switch
			{
				OneNote.Scope.Pages => await one.GetSection(),
				OneNote.Scope.Sections => await one.GetNotebook(OneNote.Scope.Pages),
				_ => await one.GetNotebooks(OneNote.Scope.Pages)
			};

			var ns = hierarchy.GetNamespaceOfPrefix(OneNote.Prefix);

			// OneNote flags the recycle bin on its section group (isRecycleBin) and on the
			// sections within it (isInRecycleBin), not on the pages, so test each page's ancestors;
			// materialize once since the query is enumerated repeatedly below
			var pagerefs = hierarchy.Descendants(ns + "Page")
				.Where(e => e.Attribute("isInRecycleBin") is null &&
					!e.Ancestors().Any(a =>
						a.Attribute("isRecycleBin") is not null ||
						a.Attribute("isInRecycleBin") is not null))
				.ToList();

			if (pagerefs.Count == 0)
			{
				logger.WriteLine("no pages found in scope");
				return false;
			}

			var totalPages = 0;
			var totalMatches = 0;
			using var progress = new ProgressDialog();
			progress.SetMaximum(pagerefs.Count);

			var result = progress.ShowDialogWithCancel(async (dialog, token) =>
			{
				foreach (var pageref in pagerefs)
				{
					if (token.IsCancellationRequested)
					{
						break;
					}

					// build full page path, including section, section groups, and notebook
					var path = pageref.Attribute("name").Value;
					var parent = pageref.Parent;
					while (parent is not null && parent.Name.LocalName != "Notebooks")
					{
						path = $"{parent.Attribute("name").Value}/{path}";
						parent = parent.Parent;
					}

					progress.Increment();
					progress.SetMessage(path);

					var page = await one.GetPage(
						pageref.Attribute("ID").Value, OneNote.PageDetail.Basic);

					var matches = editor.SearchAndReplace(page);
					logger.WriteLine($"found {matches} matches on /{path}");

					if (matches > 0)
					{
						await one.Update(page);
						totalMatches += matches;
						totalPages++;
					}
				}

				return true;
			});

			if (result == DialogResult.Cancel)
			{
				logger.WriteLine("search and replace cancelled, " +
					$"{totalPages} of {pagerefs.Count} pages searched");
			}
			else
			{
				if (totalMatches == 0 && totalPages == 0)
				{
					totalPages = pagerefs.Count;
				}

				ShowInfo(string.Format(Results, totalMatches, totalPages));
			}

			if (Highlighting && totalMatches > 0)
			{
				// reload because the current page may have just been updated above
				var current = await one.GetPage(currentPage.PageId, OneNote.PageDetail.Basic);
				HighlightRotator.SaveIndex(current, highlightIndex);
				await one.Update(current);
			}

			return totalPages > 0;
		}


		private void SaveSettings()
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection("SearchReplace");

			// remember WhatText and options...

			var whats = settings.Get<XElement>("whats");
			if (whats == null)
			{
				whats = new XElement("whats");
				settings.Add("whats", whats);
			}

			// only add if not already present
			if (!whats.Elements().Any(e => e.Value == editor.SearchPattern))
			{
				if (whats.Elements().Count() > 8)
				{
					whats.Elements().Last().Remove();
				}

				whats.AddFirst(new XElement("whatText",
					new XAttribute("matchCase", editor.CaseSensitive.ToString()),
					new XAttribute("useRegex", editor.EnableRegex.ToString()),
					HttpUtility.HtmlEncode(editor.SearchPattern)
					));
			}

			if (Highlighting)
			{
				provider.SetCollection(settings);
				provider.Save();
				return;
			}

			// remember WithText...

			var withs = settings.Get<XElement>("withs");
			if (withs == null)
			{
				withs = new XElement("withs");
				settings.Add("withs", withs);
			}

			// only add if not already present
			if (!withs.Elements().Any(e => e.Value == editor.ReplacementString))
			{
				if (withs.Elements().Count() > 8)
				{
					withs.Elements().Last().Remove();
				}

				withs.AddFirst(new XElement("withText",
					HttpUtility.HtmlEncode(editor.ReplacementString)
					));
			}

			provider.SetCollection(settings);
			provider.Save();
		}
	}
}
