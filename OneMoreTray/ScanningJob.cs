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


	internal class ScanningJob : ApplicationContext
	{
		private const string ScanningCue = "Scanning.cue";

		private readonly ILogger logger;
		private readonly NotifyIcon trayIcon;
		private readonly string cue;
		private readonly CancellationTokenSource source;
		private DateTime scheduledScan;


		public ScanningJob()
		{
			logger = Logger.Current;

			trayIcon = MakeNotifyIcon();

			scheduledScan = ParseArguments();
			if (scheduledScan > DateTime.MinValue) // internally, an efficient comparison of ticks
			{
				cue = GetCuePath(scheduledScan);
				if (cue is not null)
				{
					source = new CancellationTokenSource();
					Task.Run(async () =>
					{
						if (scheduledScan > DateTime.Now)
						{
							var delay = scheduledScan - DateTime.Now;
							await Task.Delay(delay, source.Token);
						}

						Execute();

					}, source.Token);
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
				new("Reschedule", DoExit),
				new("Exit", DoExit)
			});

			menu.Popup += (sender, e) =>
			{
				var menu = sender as ContextMenu;
				if (scheduledScan > DateTime.Now)
				{
					menu.MenuItems[0].Text = $"Scheduled: {scheduledScan.ToFriendlyString()}";
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
			if (args.Length > 0)
			{
				if (DateTime.TryParse(args[0],
					CultureInfo.CurrentCulture,
					DateTimeStyles.AssumeUniversal,
					out var date))
				{
					return date;
				}
			}

			return DateTime.MinValue;
		}


		private string GetCuePath(DateTime date)
		{
			var path = Path.Combine(PathHelper.GetAppDataPath(), ScanningCue);
			if (!File.Exists(path))
			{
				try
				{
					File.WriteAllText(path, date.ToZuluString());
				}
				catch (Exception exc)
				{
					logger.WriteLine("cannot write to cue file", exc);
					return null;
				}
			}

			return path;
		}


		private void Execute()
		{
			var forceRebuild = !File.Exists(cue);
			var scanner = new HashtagService(forceRebuild);
			scanner.OnHashtagScanned += DoScanned;
			scanner.Startup();

			source.Dispose();
		}


		private void DoScanned(object sender, HashtagScannedEventArgs args)
		{
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


		private void DoExit(object sender, EventArgs e)
		{
			if (River.OneMoreAddIn.UI.MoreMessageBox.ShowQuestion(null, "Shutdown?") == DialogResult.Yes)
			{
				logger.WriteLine("shutting down tray");
				// hide tray icon, otherwise it will remain shown until user mouses over it
				trayIcon.Visible = false;
				Application.Exit();
			}
		}
	}

}
