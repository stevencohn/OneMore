//************************************************************************************************
// Copyright © 2026 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using HistoryRecord = OneNote.HierarchyInfo;
	using Resx = Properties.Resources;


	#region Wrappers
	internal class CopyPageToReadingListCommand : CopyToReadingListCommand
	{
		public CopyPageToReadingListCommand() : base() { }
		public override Task Execute(params object[] args) => base.Execute(false);
	}


	internal class CopyParagraphToReadingListCommand : CopyToReadingListCommand
	{
		public CopyParagraphToReadingListCommand() : base() { }
		public override Task Execute(params object[] args) => base.Execute(true);
	}
	#endregion Wrappers


	/// <summary>
	/// Adds the current page or paragraph to the My Reading List (pinned items)
	/// </summary>
	internal class CopyToReadingListCommand : Command
	{
		public CopyToReadingListCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var specific = (bool)args[0];

			await using var one = new OneNote(out var page, out _);

			HistoryRecord record;

			if (specific)
			{
				if (!await ConfirmSingleWindow(one, page.PageId))
				{
					return;
				}

				var run = new SelectionRange(page).GetSelection(true);
				var selected = run?.Parent;

				if (selected is null || !selected.GetAttributeValue("objectID", out var objectID))
				{
					ShowError(Resx.Error_BodyContext);
					return;
				}

				var paragraphText = selected.TextValue(stripHtml: true).Trim();
				if (paragraphText.Length > 40)
				{
					paragraphText = $"{paragraphText.Substring(0, 40)}...";
				}

				record = await one.GetPageInfo(page.PageId);
				if (record is null)
				{
					return;
				}

				var pageName = record.Name;
				record.ObjectId = objectID;
				record.Name = $"{pageName} \u2014 {paragraphText}";
				record.Path = $"{record.Path} > {paragraphText}";
				record.Link = one.GetHyperlink(page.PageId, objectID);
			}
			else
			{
				record = await one.GetPageInfo(page.PageId);
				if (record is null)
				{
					return;
				}
			}

			var provider = new NavigationProvider();
			await provider.AddPinned(new List<HistoryRecord> { record });
		}
	}
}
