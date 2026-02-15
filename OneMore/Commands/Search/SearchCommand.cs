//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal class SearchCommand : Command
	{
		private static bool commandIsActive = false;

		private bool copying;
		private List<string> pageIds;
		private SearchDialog dialog;


		public SearchCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (dialog is not null)
			{
				dialog.Elevate();
				return;
			}

			if (commandIsActive) { return; }
			commandIsActive = true;

			try
			{
				copying = false;

				dialog = new SearchDialog();
				dialog.RunModeless(async (sender, e) =>
				{
					if (sender is SearchDialog d && d.DialogResult == DialogResult.OK)
					{
						copying = d.CopySelections;
						pageIds = d.SelectedPages;

						var desc = copying
							? Resx.SearchQF_DescriptionCopy
							: Resx.SearchQF_DescriptionMove;

						await using var one = new OneNote();
						one.SelectLocation(Resx.SearchQF_Title, desc, OneNote.Scope.Sections, Callback);
					}
				},
				20);

				dialog.Elevate(true);

				await Task.Yield();
			}
			finally
			{
				commandIsActive = false;
				dialog.Dispose();
				dialog = null;
			}
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
