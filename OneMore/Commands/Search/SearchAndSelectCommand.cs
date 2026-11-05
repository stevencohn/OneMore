//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Finds text on the current page and selects the first match in each paragraph, leaving
	/// the user to apply any OneNote formatting to the selection. OneNote supports only one
	/// contiguous selection range per paragraph, so later matches in a paragraph are ignored.
	/// </summary>
	internal class SearchAndSelectCommand : Command
	{
		public SearchAndSelectCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var guard = EnterOnce();
			if (guard is null) { return; }

			string pageId;
			string text;

			await using (var one = new OneNote(out var page, out _))
			{
				if (page is null)
				{
					return;
				}

				pageId = page.PageId;
				text = new PageEditor(page).GetSelectedText();
			}

			using var dialog = new SearchAndSelectDialog();

			if (!string.IsNullOrEmpty(text) && !text.Contains("\n"))
			{
				dialog.WhatText = text;
			}

			if (dialog.ShowDialog(owner) != DialogResult.OK)
			{
				IsCancelled = true;
				return;
			}

			await SelectMatches(pageId, dialog.WhatText, dialog.UseRegex, dialog.MatchCase);
		}


		private async Task SelectMatches(
			string pageId, string pattern, bool useRegex, bool matchCase)
		{
			await using var one = new OneNote();
			var page = await one.GetPage(pageId, OneNote.PageDetail.Basic);
			if (page is null)
			{
				return;
			}

			// clear any current selection so the whole page is searched and only the
			// new matches end up selected; OneNote normalizes the rest of the XML
			new Models.SelectionRange(page).Deselect();

			var editor = new SearchAndReplaceEditor(pattern, useRegex, matchCase);
			var selected = FirstMatchSelector.Select(editor.FindAllMatches(page));

			if (selected.Count == 0)
			{
				ShowInfo(Resx.SearchAndReplaceCommand_NoMatches);
				return;
			}

			// scroll to the first match first; the update then sets the selection
			await one.NavigateTo(pageId, selected[0].ParagraphObjectId);
			await one.Update(page);
		}
	}
}
