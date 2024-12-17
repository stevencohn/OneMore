//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	#region Wrappers
	internal class ScanHashtagsCommand : HashtagScanCommand
	{
		public ScanHashtagsCommand() : base() { }
		public override async Task Execute(params object[] args)
		{
			using var scanner = new HashtagScanner();
			await scanner.Scan();
			scanner.Report();
		}
	}

	internal class ScanHashtagsOnPageCommand : HashtagScanCommand
	{
		public ScanHashtagsOnPageCommand() : base() { }
		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote();
			using var scanner = new HashtagScanner();

			var section = await one.GetSectionInfo();

			await scanner.ScanPage(one,
				one.CurrentPageId, one.CurrentNotebookId, one.CurrentSectionId,
				section.Path, true);
		}
	}
	#endregion


	internal class HashtagScanCommand : Command
	{

		public HashtagScanCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var scheduler = new HashtagScheduler();

			if (scheduler.State == ScanningState.Rebuilding ||
				scheduler.State == ScanningState.Scanning)
			{
				var msg = scheduler.State == ScanningState.Scanning
					? Resx.HashtagCommand_scanning
					: string.Format(Resx.HashtagCommand_waiting, scheduler.StartTime.ToFriendlyString());

				ShowInfo(msg);
				return;
			}

			// by now, there should at least be an automated scheduled scan if no db
			// but keep this here just incase
			var showNotebooks = HashtagProvider.CatalogExists();

			using var dialog =
				scheduler.State == ScanningState.None ||
				scheduler.State == ScanningState.Ready
					? new ScheduleScanDialog(showNotebooks)
					: new ScheduleScanDialog(showNotebooks, scheduler.StartTime);

			if (scheduler.State != ScanningState.None &&
				scheduler.State != ScanningState.Ready)
			{
				dialog.SetIntroText(string.Format(
					Resx.HashtagSheet_prescheduled,
					scheduler.StartTime.ToString("ddd, MMMM d, yyyy h:mm tt"))
					);
			}
			else
			{
				dialog.SetIntroText(Resx.HashtagSheet_scanNotebooks);
			}

			dialog.SetPreferredIDs(scheduler.Notebooks);

			var result = dialog.ShowDialog(owner);
			if (result == DialogResult.OK)
			{
				scheduler.Notebooks = dialog.GetSelectedNotebooks();
				scheduler.StartTime = dialog.StartTime;
				scheduler.State = ScanningState.PendingScan;
				await scheduler.Activate();
			}
		}
	}
}
