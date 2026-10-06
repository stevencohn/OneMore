//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreTray
{
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Identity;
	using River.OneMoreAddIn.Pipeline;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Globalization;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal class ScanningJob : ApplicationContext
	{
		private readonly ILogger logger;
		private readonly NotifyIcon trayIcon;
		private readonly HashtagScheduler scheduler;
		private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(30);

		private CancellationTokenSource source;
		private System.Threading.Timer heartbeat;


		public ScanningJob()
		{
			logger = Logger.Current;

			SetLanguage();

			trayIcon = MakeNotifyIcon();

			scheduler = new HashtagScheduler();

			// for debugging, pass an argument to bypass the schedule-existence check
			var args = Environment.GetCommandLineArgs();
			if (args.Length == 1 && !scheduler.ScheduleExists)
			{
				ToastMissingSchedule();
				return;
			}

			River.OneMoreAddIn.Helpers.SessionLogger.WriteSessionHeader();

			// record that this process owns the schedule and keep proving it is alive, so the
			// add-in can tell a working tray from one that died or hung
			scheduler.Claim();
			heartbeat = new System.Threading.Timer(
				_ => scheduler.Heartbeat(), null, HeartbeatInterval, HeartbeatInterval);

			ScheduleScan();
		}


		private void SetLanguage()
		{
			var settings = new SettingsProvider().GetCollection(nameof(GeneralSheet));
			var lang = settings.Get("language", "en");
			var culture = CultureInfo.GetCultureInfo(lang);

			Thread.CurrentThread.CurrentUICulture = culture;

			if (!settings.Get("keepWorkstationLocale", false))
			{
				Thread.CurrentThread.CurrentCulture = culture;
			}
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
			var token = source.Token;
			Task.Run(async () =>
			{
				try
				{
					while (scheduler.StartTime > DateTime.Now)
					{
						var time = scheduler.StartTime.ToString(Resx.ScheduleTimeFormat);
						logger.WriteLine($"waiting until {time}");

						trayIcon.ShowBalloonTip(0, Resx.ScannerTitle,
							string.Format(Resx.ScannerScheduled, time), ToolTipIcon.Info);

						// Task.Delay cannot wait longer than int.MaxValue ms, about 24 days, so
						// wait in bounded steps and look at the clock again
						var delay = scheduler.StartTime - DateTime.Now;
						var max = TimeSpan.FromDays(1);
						await Task.Delay(delay > max ? max : delay, token);
					}

					Execute();
				}
				catch (OperationCanceledException)
				{
					// rescheduled or run now; a new wait has already been started
				}
				catch (Exception exc)
				{
					logger.WriteLine("scheduled scan failed", exc);
					Abandon();
				}
			}, token);
		}


		// the tray cannot finish the schedule, so record that and close rather than linger
		// as a process that appears to be working
		private void Abandon()
		{
			scheduler.RecordFailure();
			trayIcon.Visible = false;
			Application.Exit();
		}


		private void Execute()
		{
			var settings = new SettingsProvider().GetCollection(nameof(HashtagSheet));
			if (settings.Get("disabled", false))
			{
				logger.WriteLine("hashtag scanning is disabled, aborting scheduler");
				scheduler.ClearSchedule();
				source.Dispose();

				// nothing left to do, so do not linger as a resident process
				trayIcon.Visible = false;
				Application.Exit();
				return;
			}

			// take ownership of the work before starting it, so the schedule says Scanning
			// for as long as the pipeline might be running
			var rebuild = scheduler.State == ScanningState.PendingRebuild;

			if (!scheduler.TryBeginScan())
			{
				logger.WriteLine("scheduled scan was changed by another process, closing OneMoreTray");
				trayIcon.Visible = false;
				Application.Exit();
				return;
			}

			logger.WriteLine("starting HashtagService");

			trayIcon.ShowBalloonTip(0, Resx.ScannerTitle, Resx.ScanStarting, ToolTipIcon.Info);

			// the tray runs the identity stage and then the hashtag stage once, for a scan the user
			// scheduled
			var stage = new HashtagStage { Scheduled = true, Rebuild = rebuild };

			if (scheduler.Notebooks is not null && scheduler.Notebooks.Length > 0)
			{
				stage.SetNotebookFilters(scheduler.Notebooks);
			}

			stage.OnHashtagScanned += DoScanned;

			var service = new PipelineService(new IPipelineStage[] { new IdentityStage(), stage })
			{
				Mode = PipelineMode.OneShot,
				ThreadPriority = rebuild ? ThreadPriority.BelowNormal : ThreadPriority.Lowest
			};

			service.Startup();

			source.Dispose();
		}


		private void DoScanned(object sender, HashtagScannedEventArgs args)
		{
			if (args.Failed)
			{
				if (!string.IsNullOrWhiteSpace(args.ErrorMessage))
				{
					logger.WriteLine(args.ErrorMessage);
					logger.WriteLine("ScanningJob aborted, closing OneMoreTray");
				}

				// counts the failure and abandons the schedule after too many, so a persistent
				// failure is not retried every time OneNote starts
				Abandon();
				return;
			}

			logger.WriteLine($"scan completed at {DateTime.Now}");

			logger.WriteLine(
				$"scanned {args.HourCount} times in the last hour, averaging {args.HourAverage}ms");

			logger.WriteLine(
				$"scanned {args.TotalPages} pages, updating {args.DirtyPages}, in {args.Time}ms");

			scheduler.ClearSchedule();
			trayIcon.ShowBalloonTip(0, Resx.ScanCompleteTitle, Resx.ScanComplete, ToolTipIcon.Info);

			logger.WriteLine("ScanningJob completed, closing OneMoreTray");
			Application.Exit();
		}


		private void ToastMissingSchedule()
		{
			logger.WriteLine("missing schedule file, aborting");

			trayIcon.ShowBalloonTip(0, Resx.BadArgumentsTitle, Resx.BadArguments, ToolTipIcon.Error);

			Task.Run(async () =>
			{
				await Task.Delay(2000);
				Application.Exit();
			});
		}


		private void DoReschedule(object sender, EventArgs e)
		{
			var showBooks = scheduler.Notebooks.Length > 0;
			using var dialog = new ScheduleScanDialog(showBooks, scheduler.StartTime);

			var msg = string.Format(
				scheduler.State == ScanningState.PendingScan
					? Resx.Reschedule_Scan
					: Resx.Reschedule_Rebuild,
				scheduler.StartTime.ToString(Resx.ScheduleTimeFormat));

			dialog.SetIntroText(msg);
			dialog.SetPreferredIDs(scheduler.Notebooks);
			dialog.StartPosition = FormStartPosition.CenterScreen;

			var result = dialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				source.Cancel(false);

				scheduler.Notebooks = dialog.GetSelectedNotebooks();
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

				scheduler.StartTime = DateTime.Now.AddSeconds(-1);
				scheduler.SaveSchedule();
				ScheduleScan();
			}
		}


		private void DoExit(object sender, EventArgs e)
		{
			var scanning = scheduler.State == ScanningState.Scanning;

			var msg = scanning
				? Resx.ScanRunning
				: string.Format(
					Resx.ScanScheduled,
					scheduler.StartTime.ToString(Resx.ScheduleTimeFormat));

			msg += $"\n{Resx.CloseConfirm}";

			if (MoreMessageBox.ShowQuestion(null, msg) == DialogResult.Yes)
			{
				if (!scanning)
				{
					logger.WriteLine("deleting scheduled scan");
					scheduler.ClearSchedule();
				}

				logger.WriteLine("shutting down tray");
				// hide tray icon, otherwise it will remain shown until user mouses over it
				trayIcon.Visible = false;
				Application.Exit();
			}
		}
	}
}
