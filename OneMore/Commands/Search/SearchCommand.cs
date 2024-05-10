//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Search;
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal class SearchCommand : Command
	{
		private bool copying;
		private List<string> pageIds;


		public SearchCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			copying = false;

			var dialog = new SearchDialog();
			dialog.RunModeless(async (sender, e) =>
			{
				var d = sender as SearchDialog;
				if (d.DialogResult == DialogResult.OK)
				{
					copying = dialog.CopySelections;
					pageIds = dialog.SelectedPages;

					var desc = copying
						? Resx.SearchQF_DescriptionCopy
						: Resx.SearchQF_DescriptionMove;

					await using var one = new OneNote();
					one.SelectLocation(Resx.SearchQF_Title, desc, OneNote.Scope.Sections, Callback);
				}
			},
			20);

			await Task.Yield();
		}


		private async Task Callback(string sectionId)
		{
			if (string.IsNullOrEmpty(sectionId))
			{
				// cancelled
				return;
			}

			var action = copying ? "copying" : "moving";
			logger.Start($"..{action} {pageIds.Count} pages");

			try
			{
				await using var one = new OneNote();
				var service = new SearchServices(one, sectionId);

				if (copying)
				{
					await service.CopyPages(pageIds);
				}
				else
				{
					await service.MovePages(pageIds);
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
	}
}
