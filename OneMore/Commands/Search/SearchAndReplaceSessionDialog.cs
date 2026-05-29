//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Resx = Properties.Resources;


	internal partial class SearchAndReplaceSessionDialog : MoreForm
	{
		private readonly Page page;
		private readonly SearchAndReplaceEditor editor;
		private readonly string pageId;

		private List<MatchInfo> matches;
		private readonly HashSet<(int tElementHash, int startIndex)> skipped;
		private readonly HashSet<(int tElementHash, int startIndex)> replaced;
		private int replacedCount;


		public SearchAndReplaceSessionDialog(
			Page page, List<MatchInfo> matches, SearchAndReplaceEditor editor, string pageId)
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.SearchAndReplaceSessionDialog_Text;

				Localize(new string[]
				{
					"replaceButton=word_Replace",
					"skipButton=word_Skip",
					"replaceAllButton",
					"closeButton=word_Close"
				});
			}

			this.page = page;
			this.editor = editor;
			this.pageId = pageId;
			this.matches = matches;
			skipped = new HashSet<(int, int)>();
			replaced = new HashSet<(int, int)>();
			replacedCount = 0;

			UpdateStatus();
		}


		private void UpdateStatus()
		{
			var current = matches.FirstOrDefault(IsPending);
			if (current is null)
			{
				var done = replacedCount > 0
					? string.Format(Resx.SearchAndReplaceCommand_Results, replacedCount, 1)
					: Resx.SearchAndReplaceCommand_NoMatches;

				statusLabel.Text = done;
				replaceButton.Enabled = false;
				skipButton.Enabled = false;
				replaceAllButton.Enabled = false;
				return;
			}

			var remaining = matches.Count(IsPending);
			var total = remaining + replacedCount;
			var matchNum = total - remaining + 1;

			var snippet = current.MatchedText.Length > 20
				? current.MatchedText.Substring(0, 20) + "…"
				: current.MatchedText;

			statusLabel.Text = string.Format(
				Resx.SearchAndReplaceSessionDialog_statusLabel_Text,
				matchNum, total, snippet);

			replaceButton.Enabled = true;
			skipButton.Enabled = true;
			replaceAllButton.Enabled = true;
		}


		private bool IsSkipped(MatchInfo m) =>
			skipped.Contains((m.TElement.GetHashCode(), m.StartIndex));

		private bool IsReplaced(MatchInfo m) =>
			replaced.Contains((m.TElement.GetHashCode(), m.StartIndex));

		private bool IsPending(MatchInfo m) => !IsSkipped(m) && !IsReplaced(m);


		private async void ReplaceOne(object sender, EventArgs e)
		{
			var current = matches.FirstOrDefault(IsPending);
			if (current is null) return;

			try
			{
				replaced.Add((current.TElement.GetHashCode(), current.StartIndex));
				editor.ReplaceMatch(current);
				replacedCount++;

				await using var one = new OneNote();
				await one.Update(page);

				matches = editor.FindAllMatches(page);

				// prune skipped entries that no longer exist in updated matches
				skipped.RemoveWhere(k =>
					!matches.Any(m =>
						m.TElement.GetHashCode() == k.tElementHash &&
						m.StartIndex == k.startIndex));

				UpdateStatus();

				var next = matches.FirstOrDefault(IsPending);
				if (next != null)
				{
					await NavigateTo(next.ParagraphObjectId);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error replacing match", exc);
			}
		}


		private async void SkipOne(object sender, EventArgs e)
		{
			var current = matches.FirstOrDefault(IsPending);
			if (current is null) return;

			skipped.Add((current.TElement.GetHashCode(), current.StartIndex));
			UpdateStatus();

			var next = matches.FirstOrDefault(IsPending);
			if (next != null)
			{
				await NavigateTo(next.ParagraphObjectId);
			}
		}


		private async void ReplaceAll(object sender, EventArgs e)
		{
			try
			{
				// drive the loop from freshly-rescanned matches each iteration so that
				// position offsets stay correct even when replacement length differs
				MatchInfo next;
				while ((next = matches.FirstOrDefault(IsPending)) != null)
				{
					replaced.Add((next.TElement.GetHashCode(), next.StartIndex));
					editor.ReplaceMatch(next);
					replacedCount++;
					matches = editor.FindAllMatches(page);
				}

				await using var one = new OneNote();
				await one.Update(page);
			}
			catch (Exception exc)
			{
				logger.WriteLine("error replacing all matches", exc);
			}

			Close();
		}


		private void CloseSession(object sender, EventArgs e)
		{
			Close();
		}


		private async Task NavigateTo(string objectId)
		{
			try
			{
				await using var one = new OneNote();
				await one.NavigateTo(pageId, objectId);
			}
			catch (Exception exc)
			{
				logger.WriteLine("error navigating to match", exc);
			}
		}
	}
}
