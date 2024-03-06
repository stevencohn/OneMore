//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreTray
{
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Commands;
	using System;
	using System.Globalization;
	using System.IO;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal class ScanningJob : ApplicationContext
	{
		private const string ScanningCue = "Scanning.cue";

		private readonly ILogger logger;
		private readonly NotifyIcon trayIcon;
		private string cue;
		private CancellationTokenSource source;
		private DateTime scanTime;


		public ScanningJob()
		{
			logger = Logger.Current;

			trayIcon = MakeNotifyIcon();

			scanTime = ParseArguments();
			if (scanTime > DateTime.MinValue) // internally, an efficient comparison of ticks
			{
				cue = GetCuePath(scanTime);
				if (cue is not null)
				{
					ScheduleScan();
				}
			}
			else
			{
				ToastBadArguments();
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
				new("Reschedule", DoReschedule),
				new("Exit", DoExit)
			});

			menu.Popup += (sender, e) =>
			{
				var menu = sender as ContextMenu;
				if (scanTime > DateTime.Now)
				{
					menu.MenuItems[0].Text = $"Scheduled for {scanTime.ToString(Resx.ScheduleTimeFormat)}";
				}
				else
				{
					menu.MenuItems[0].Text = "Scanning...";
					menu.MenuItems[2].Enabled = false;
				}
			};

			var icon = new NotifyIcon
			{
				Icon = Properties.Resources.Logo,
				ContextMenu = menu,
				Visible = true,
			};

			return icon;
		}


		private DateTime ParseArguments()
		{
			// theoretically, this method might expand to parse more arguments for additional
			// commands and use cases...

			var args = Environment.GetCommandLineArgs();

			// args[0] is executable path so skip to args[1]
			if (args.Length > 1)
			{
				logger.WriteLine($"parsing arg [{args[1]}]");

				if (DateTime.TryParse(args[1],
					CultureInfo.CurrentCulture,
					DateTimeStyles.AssumeUniversal,
					out var time))
				{
					return time;
				}
			}

			return DateTime.MinValue;
		}


		private string GetCuePath(DateTime time)
		{
			var path = Path.Combine(PathHelper.GetAppDataPath(), ScanningCue);
			try
			{
				// create or overwrite, allowing schedule to be updated
				File.WriteAllText(path, time.ToZuluString());
			}
			catch (Exception exc)
			{
				logger.WriteLine("cannot write to cue file", exc);
				path = null;
			}

			return path;
		}


		private void ScheduleScan()
		{
			source = new CancellationTokenSource();
			Task.Run(async () =>
			{
				if (scanTime > DateTime.Now)
				{
					logger.WriteLine($"waiting until {scanTime.ToString(Resx.ScheduleTimeFormat)}");
					var delay = scanTime - DateTime.Now;
					await Task.Delay(delay, source.Token);
				}

				Execute();

			}, source.Token);
		}


		private void Execute()
		{
			logger.WriteLine("starting HashtagService");

			var forceRebuild = !File.Exists(cue);

			var scanner = new HashtagService(forceRebuild);
			scanner.OnHashtagScanned += DoScanned;
			scanner.Startup();

			source.Dispose();
		}


		private void DoScanned(object sender, HashtagScannedEventArgs args)
		{
			logger.WriteLine($"scan completed at {DateTime.Now}");

			logger.WriteLine(
				$"scanned {args.HourCount} times in the last hour, averaging {args.HourAverage}ms");

			logger.WriteLine(
				$"scanned {args.TotalPages} pages, updating {args.DirtyPages}, in {args.Time}ms");

			if (File.Exists(cue))
			{
				try
				{
					File.Delete(cue);
				}
				catch (Exception exc)
				{
					logger.WriteLine("error deleting cue file", exc);
				}
			}

			trayIcon.ShowBalloonTip(0,
				"Scan Complete",
				"OneMore hashtag scanning is complete and ready to use",
				ToolTipIcon.Info);

			Application.Exit();
		}


		private void ToastBadArguments()
		{
			logger.WriteLine("bad arguments, aborting");

			trayIcon.ShowBalloonTip(0,
				"Bad Arguments",
				"OneMoreTray was not started correctly and will close now",
				ToolTipIcon.Error);

			Task.Run(async () =>
			{
				await Task.Delay(2000);
				Application.Exit();
			});
		}


		private void DoReschedule(object sender, EventArgs e)
		{
			using var dialog = new RescheduleDialog(scanTime);
			var result = dialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				source.Cancel(false);
				scanTime = dialog.Time;
				cue = GetCuePath(scanTime);
				ScheduleScan();
			}
		}


		private void DoExit(object sender, EventArgs e)
		{
			var msg = (File.Exists(cue)
				? $"A scan is schedule to run on {scanTime.ToString(Resx.ScheduleTimeFormat)}"
				: "A scan is currently running") +
				"\nAre you sure you want to close the tray application?";

			if (River.OneMoreAddIn.UI.MoreMessageBox.ShowQuestion(null, msg) == DialogResult.Yes)
			{
				logger.WriteLine("shutting down tray");
				// hide tray icon, otherwise it will remain shown until user mouses over it
				trayIcon.Visible = false;
				Application.Exit();
			}
		}
	}
}
