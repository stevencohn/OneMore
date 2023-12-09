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

		public HashtagCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote();
			using var dialog = new HashtagDialog(one.CurrentNotebookId, one.CurrentSectionId);

			await dialog.RunModeless((sender, e) =>
			{
				var d = sender as HashtagDialog;
				if (d.DialogResult == DialogResult.OK)
				{
					command = d.Command;
					pageIds = d.SelectedPages;

					var desc = command == HashtagDialog.Commands.Copy
						? Resx.SearchQF_DescriptionCopy
						: Resx.SearchQF_DescriptionMove;

					using var one = new OneNote();
					one.SelectLocation(Resx.SearchQF_Title, desc, OneNote.Scope.Sections, Callback);
				}
			},
			20);
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
				using var one = new OneNote();
				var service = new SearchServices(owner, one, sectionId);

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
