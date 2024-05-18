//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
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
			if (!await ConfirmReady())
			{
				return;
			}

			if (dialog != null)
			{
				// single instance
				dialog.Elevate();
				return;
			}

			var converter = new LegacyTaggingConverter();
			await converter.UpgradeLegacyTags(owner);

			dialog = new HashtagDialog();
			dialog.FormClosed += Dialog_FormClosed;

			dialog.RunModeless(async (sender, e) =>
			{
				var d = sender as HashtagDialog;
				if (d.DialogResult == DialogResult.OK)
				{
					command = d.Command;
					pageIds = d.SelectedPages.ToList();

					var msg = command switch
					{
						HashtagDialog.Commands.Copy => Resx.SearchQF_DescriptionCopy,
						HashtagDialog.Commands.Move => Resx.SearchQF_DescriptionMove,
						_ => Resx.SearchQF_DescriptionIndex
					};

					await using var one = new OneNote();
					one.SelectLocation(Resx.SearchQF_Title, msg, OneNote.Scope.Sections, Callback);
				}
			},
			20);
		}


		private async Task<bool> ConfirmReady()
		{
			var scheduler = new HashtagScheduler();

			if (!HashtagProvider.DatabaseExists())
			{
				using var sdialog = scheduler.State == ScanningState.None
					? new ScheduleScanDialog(false)
					: new ScheduleScanDialog(false, scheduler.StartTime);

				var result = sdialog.ShowDialog(owner);
				if (result == DialogResult.OK)
				{
					scheduler.Notebooks = sdialog.GetSelectedNotebooks();
					scheduler.StartTime = sdialog.StartTime;
					scheduler.State = ScanningState.PendingRebuild;
					await scheduler.Activate();
				}

				return false;
			}

			//if (scheduler.State != ScanningState.Ready)
			//{
			//	var msg = scheduler.State == ScanningState.Scanning
			//		? Resx.HashtagCommand_scanning
			//		: string.Format(Resx.HashtagCommand_waiting, scheduler.StartTime.ToFriendlyString());

			//	MoreMessageBox.Show(owner, msg);
			//	return false;
			//}

			return true;
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
