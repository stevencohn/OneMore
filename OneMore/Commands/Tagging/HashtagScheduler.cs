//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Newtonsoft.Json;
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Threading.Tasks;
	using Resx = Properties.Resources;


	/// <summary>
	/// Determines whether there is a pending or active scheduled scan and activates the
	/// OneMoreTray app if a scan has been scheduled.
	/// </summary>
	internal class HashtagScheduler : Loggable
	{
		private const string TrayName = "OneMoreTray";

		private readonly string filePath;
		private Schedule schedule;


		#region Supporting classes
		public enum SchedulePhase
		{
			Pending,		// user-requested scan to import large data (e.g. notebooks, sections)
			PendingRebuild,	// rebuild database and scan all notebooks
			Rebuilding,		// in process of rebuilding database; this should be quick!
			Scanning		// scanning data
		}

		public sealed class Schedule
		{
			// The scheduled time to build/rebuild the hashtag database.
			// Could be past or future; if past then run immediately.
			[JsonProperty("startTime")]
			public DateTime StartTime { get; set; } = DateTime.Today.AddDays(1);


			// Indicates whether the operation is in progress; is not reset until after
			// operation is confirmed to be completed, so can be used to recognize 
			// interruptions and infer continuation
			[JsonProperty("phase")]
			public SchedulePhase Phase { get; set; } = SchedulePhase.Pending;


			// Indicates whether to shutdown the tray after operation completes
			[JsonProperty("shutdown")]
			public bool Shutdown { get; set; } = true;
		}
		#endregion Supporting classes


		public HashtagScheduler()
		{
			filePath = Path.Combine(PathHelper.GetAppDataPath(), Resx.ScanningCueFile);
			schedule = ReadSchedule() ?? new Schedule();
		}


		public bool ScheduleExists => File.Exists(filePath);


		public DateTime StartTime
		{
			get { return schedule is null ? DateTime.MinValue : schedule.StartTime; }
			set { schedule.StartTime = value; }
		}


		public SchedulePhase Phase => schedule.Phase;


		/// <summary>
		/// Entry point for UI to determine if a scan is pending.
		/// </summary>
		/// <param name="time">
		/// The DateTime of the pending scan or MinValue (0) if no scan is scheduled
		/// </param>
		/// <returns>True if a waiting for a pending scan</returns>
		public bool IsWaiting(out Schedule schedule)
		{
			if (!File.Exists(filePath))
			{
				schedule = null;
				return false;
			}

			schedule = ReadSchedule();
			return schedule is not null;
		}


		/// <summary>
		/// Called by HashtagCommand to schedule a full rebuild or to rescan newly added notebooks
		/// </summary>
		/// <param name="startTime">The time to schedule the scan</param>
		/// <returns></returns>
		public async Task ScheduleScan(DateTime startTime)
		{
			var process = Process.GetProcessesByName(TrayName).FirstOrDefault();
			if (process != null)
			{
				try
				{
					logger.WriteLine($"stopping {TrayName}");
					process.Kill();
					await process.WaitForExitAsync();
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error stopping {TrayName}", exc);
				}
				finally
				{
					process.Dispose();
				}
			}

			schedule = new Schedule { StartTime = startTime };
			SaveSchedule();

			var dir = new Uri(
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)).LocalPath;

			if (dir.Contains("Debug"))
			{
				// special redirect for dev environment
				dir = Path.Combine(
					Path.GetDirectoryName(Path.GetDirectoryName(
						Path.GetDirectoryName(Path.GetDirectoryName(dir)))),
					@$"{TrayName}\bin\Debug");
			}

			var exe = Path.Combine(dir, $"{TrayName}.exe");
			logger.WriteLine($"starting {exe} @{startTime.ToZuluString()}");

			var proc = Process.Start(new ProcessStartInfo
			{
				FileName = exe,
				LoadUserProfile = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				WorkingDirectory = dir
			});

			logger.WriteLine($"started {TrayName} process {proc.Id}");
		}


		/// <summary>
		/// Entry point for the HashtagService to poll for an active scan.
		/// If a scan has been scheduled, ensures the OneMoreTray app is running.
		/// </summary>
		/// <returns>True if a scan is pending or active.</returns>
		public async Task<bool> WaitingForScan()
		{
			if (File.Exists(filePath))
			{
				schedule = ReadSchedule();
			}
			else if (!HashtagProvider.DatabaseExists())
			{
				schedule = null;
			}
			else
			{
				return false;
			}

			if (schedule is not null)
			{
				// must have already warned the user so continue idling patiently...
				logger.Verbose($"waiting for hashtag scan on {schedule.StartTime}");
				return true;
			}

			// at this point, it must be the first time, so start up OneMoreTray if needed which
			// is only done the first time incase the user intentionally shut down the tray app...
			// note that it may have already been started if OneNote was since restarted.

			schedule = new Schedule();
			await ScheduleScan(schedule.StartTime);

			return true;
		}


		public void ClearSchedule()
		{
			if (File.Exists(filePath))
			{
				try
				{
					File.Delete(filePath);
				}
				catch (Exception exc)
				{
					logger.WriteLine("error deleting schedule file", exc);
				}
			}
		}


		public Schedule ReadSchedule()
		{
			if (!File.Exists(filePath))
			{
				logger.WriteLine($"could not find schedule file {filePath}");
				return null;
			}

			try
			{
				var json = File.ReadAllText(filePath);

				return JsonConvert.DeserializeObject<Schedule>(json, new JsonSerializerSettings
				{
					// convert the UTC timestamp to a local time
					DateTimeZoneHandling = DateTimeZoneHandling.Local
				});
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error reading scan schedule from {filePath}", exc);
				return null;
			}
		}


		public void SaveSchedule()
		{
			try
			{
				var json = JsonConvert.SerializeObject(schedule,
					new JsonSerializerSettings
					{
						DateTimeZoneHandling = DateTimeZoneHandling.Utc,
						Formatting = Formatting.Indented
					});

				File.WriteAllText(filePath, json);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error writing scan schedule to {filePath}", exc);
			}
		}
	}
}
