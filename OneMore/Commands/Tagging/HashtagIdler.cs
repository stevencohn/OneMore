//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal class HashtagIdler : Loggable
	{
		private const string ScheduleTimeFormat = "ddd, MMMM d, yyyy h:mm tt";

		private readonly string cuePath;
		private DateTime scanTime;


		public HashtagIdler()
		{
			cuePath = Path.Combine(PathHelper.GetAppDataPath(), Resx.ScanningCueFile);
			scanTime = DateTime.MinValue;
		}


		public bool IsIdling(out DateTime time)
		{
			if (!File.Exists(cuePath))
			{
				time = DateTime.MinValue;
				return false;
			}

			string text;

			try
			{
				text = File.ReadAllText(cuePath).Trim();
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error reading scan time from {cuePath}", exc);
				time = DateTime.MinValue;
				return false;
			}

			if (!DateTime.TryParse(text,
				CultureInfo.CurrentCulture,
				DateTimeStyles.AssumeUniversal,
				out time))
			{
				logger.WriteLine($"error parsing scan time [{text}]");
				return false;
			}

			return true;
		}


		public bool WaitingForScan()
		{
			if (!File.Exists(cuePath))
			{
				return false;
			}

			var idling = IsIdling(out DateTime time);
			if (!idling)
			{
				return false;
			}

			if (time == scanTime)
			{
				// already warned the user so continue idling patiently...

				logger.Verbose(
					$"idling while waiting for scan on {scanTime.ToString(ScheduleTimeFormat)}");

				return true;
			}

			scanTime = time;

			// must be first time here so warn the user...

			var displayTime = scanTime.ToString(ScheduleTimeFormat);
			logger.WriteLine($"idling while waiting for scan on {displayTime}");

			var icon = new NotifyIcon
			{
				Icon = SystemIcons.Information,
				Visible = true
			};

			icon.ShowBalloonTip(0,
				"Hashtags unavailable",
				$"Waiting for Hashtag scanner to run on {displayTime}",
				ToolTipIcon.Info);

			return true;
		}
	}
}
