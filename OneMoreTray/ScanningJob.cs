﻿//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreTray
{
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal class ScanningJob : ApplicationContext
	{
		private readonly ILogger logger;
		private readonly NotifyIcon trayIcon;
		private readonly HashtagScheduler scheduler;
		private CancellationTokenSource source;


		public ScanningJob()
		{
			logger = Logger.Current;
			trayIcon = MakeNotifyIcon();

			scheduler = new HashtagScheduler();

			if (!scheduler.ScheduleExists)
			{
				ToastMissingSchedule();
				return;
			}

			ScheduleScan();
		}


		private NotifyIcon MakeNotifyIcon()
		{
			var statusItem = new MenuItem("Scheduled: ?") { Enabled = false };
			var separatorItem = new MenuItem("-") { Enabled = false };

			var menu = new ContextMenu(new MenuItem[]
			{
				statusItem,
				separatorItem,
				new(Resx.word_Reschedule, DoReschedule),
				new(Resx.RunNowMenuItem, DoRunImmediately),
				new(Resx.word_Exit, DoExit)
			});

			menu.Popup += (sender, e) =>
			{
				var menu = sender as ContextMenu;
				if (scheduler.StartTime > DateTime.Now)
				{
					menu.MenuItems[0].Text = string.Format(
						Resx.ScheduledFor, scheduler.StartTime.ToString(Resx.ScheduleTimeFormat));
				}
				else
				{
					menu.MenuItems[0].Text = Resx.phrase_Scanning;
					menu.MenuItems[2].Enabled = false; // reschedule
					menu.MenuItems[3].Enabled = false; // run now
				}
			};

			var icon = new NotifyIcon
			{
				Icon = Resx.Logo,
				ContextMenu = menu,
				Visible = true,
			};

			return icon;
		}


		private void ScheduleScan()
		{
			source = new CancellationTokenSource();
			Task.Run(async () =>
			{
				if (scheduler.StartTime > DateTime.Now)
				{
					var time = scheduler.StartTime.ToString(Resx.ScheduleTimeFormat);
					logger.WriteLine($"waiting until {time}");

					trayIcon.ShowBalloonTip(0,
						Resx.ScannerTitle,
						string.Format(Resx.ScannerScheduled, time),
						ToolTipIcon.Error);

					var delay = scheduler.StartTime - DateTime.Now;
					await Task.Delay(delay, source.Token);
				}

				Execute();

			}, source.Token);
		}


		private void Execute()
		{
			logger.WriteLine("starting HashtagService");

			var scanner = new HashtagService(scheduler.State == ScanningState.PendingRebuild);
			scanner.OnHashtagScanned += DoScanned;
			scanner.Startup();

			scheduler.State = ScanningState.Scanning;
			scheduler.SaveSchedule();

			source.Dispose();
		}


		private void DoScanned(object sender, HashtagScannedEventArgs args)
		{
			logger.WriteLine($"scan completed at {DateTime.Now}");

			logger.WriteLine(
				$"scanned {args.HourCount} times in the last hour, averaging {args.HourAverage}ms");

			logger.WriteLine(
				$"scanned {args.TotalPages} pages, updating {args.DirtyPages}, in {args.Time}ms");

			scheduler.ClearSchedule();

			trayIcon.ShowBalloonTip(0,
				Resx.ScanCompleteTitle,
				Resx.ScanComplete,
				ToolTipIcon.Info);

			Application.Exit();
		}


		private void ToastMissingSchedule()
		{
			logger.WriteLine("missing schedule file, aborting");

			trayIcon.ShowBalloonTip(0,
				Resx.BadArgumentsTitle,
				Resx.BadArguments,
				ToolTipIcon.Error);

			Task.Run(async () =>
			{
				await Task.Delay(2000);
				Application.Exit();
			});
		}


		private void DoReschedule(object sender, EventArgs e)
		{
			using var dialog = new ScheduleScanDialog(scheduler.StartTime);

			var msg = string.Format(
				scheduler.State == ScanningState.PendingScan
					? Resx.Reschedule_Scan
					: Resx.Reschedule_Rebuild,
				scheduler.StartTime.ToString(Resx.ScheduleTimeFormat));

			dialog.SetIntroText(msg);

			var result = dialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				source.Cancel(false);
				scheduler.StartTime = dialog.StartTime;
				scheduler.SaveSchedule();
				ScheduleScan();
			}
		}


		private void DoRunImmediately(object sender, EventArgs e)
		{
			if (MoreMessageBox.ShowQuestion(null, Resx.ScanNowConfirmation) == DialogResult.Yes)
			{
				source.Cancel(false);
				scheduler.ClearSchedule();

				scheduler.StartTime = DateTime.Now.AddSeconds(-1);
				scheduler.SaveSchedule();
				ScheduleScan();
			}
		}


		private void DoExit(object sender, EventArgs e)
		{
			var msg = scheduler.State == ScanningState.Scanning
				? Resx.ScanRunning
				: string.Format(
					Resx.RescheduleDialog_currentLabel_Text,
					scheduler.StartTime.ToString(Resx.ScheduleTimeFormat));

			msg += $"\n{Resx.CloseConfirm}";

			if (MoreMessageBox.ShowQuestion(null, msg) == DialogResult.Yes)
			{
				logger.WriteLine("shutting down tray");
				// hide tray icon, otherwise it will remain shown until user mouses over it
				trayIcon.Visible = false;
				Application.Exit();
			}
		}
	}
}
