//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Search;
	using System;
	using System.Collections.Generic;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class SearchCommand : Command
	{
		private bool copying;
		private List<string> pageIds;


		public SearchCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			copying = false;

			var dialog = new SearchDialog();
			dialog.RunModeless((sender, e) =>
			{
				var d = sender as SearchDialog;
				if (d.DialogResult == DialogResult.OK)
				{
					copying = dialog.CopySelections;
					pageIds = dialog.SelectedPages;

					var desc = copying
						? Resx.SearchQF_DescriptionCopy
						: Resx.SearchQF_DescriptionMove;

					using (var one = new OneNote())
					{
						one.SelectLocation(Resx.SearchQF_Title, desc, OneNote.Scope.Sections, Callback);
					}
				}
			},
			20);
		}


		private void Callback(string sectionId)
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
				using (var one = new OneNote())
				{
					var service = new SearchServices(owner, one, sectionId);

					if (copying)
					{
						service.CopyPages(pageIds);
					}
					else
					{
						service.MovePages(pageIds);
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
	}
}
