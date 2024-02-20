//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;

	internal class HashtagCommand : Command
	{
		private HashtagDialog.Commands command;
		private IEnumerable<string> pageIds;

		private static HashtagDialog dialog;

		public HashtagCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			if (dialog != null)
			{
				// single instance
				dialog.ForceTopMost();
				dialog.Activate();
				dialog.TopMost = false;
				return;
			}

			dialog = new HashtagDialog();
			dialog.FormClosed += Dialog_FormClosed;

			await dialog.RunModeless(async (sender, e) =>
			{
				var d = sender as HashtagDialog;
				if (d.DialogResult == DialogResult.OK)
				{
					command = d.Command;
					pageIds = d.SelectedPages.ToList();

					var desc = command == HashtagDialog.Commands.Copy
						? Resx.SearchQF_DescriptionCopy
						: Resx.SearchQF_DescriptionMove;

					await using var one = new OneNote();
					one.SelectLocation(Resx.SearchQF_Title, desc, OneNote.Scope.Sections, Callback);
				}
			},
			20);
		}

		private void Dialog_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (dialog != null)
			{
				dialog.FormClosed -= Dialog_FormClosed;
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

			logger.Start($"..{command} {pageIds.Count()} pages");

			try
			{
				await using var one = new OneNote();
				var service = new SearchServices(one, sectionId);

				switch (command)
				{
					case HashtagDialog.Commands.Index:
						await service.IndexPages(pageIds);
						break;

					case HashtagDialog.Commands.Copy:
						await service.CopyPages(pageIds);
						break;

					case HashtagDialog.Commands.Move:
						await service.MovePages(pageIds);
						break;
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
