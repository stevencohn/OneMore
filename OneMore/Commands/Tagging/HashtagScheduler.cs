//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;
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
		private readonly Schedule schedule;


		#region Supporting classes
		private sealed class Schedule
		{
			// The scheduled time to build/rebuild the hashtag database.
			// Could be past or future; if past then run immediately.
			[JsonProperty("startTime")]
			public DateTime StartTime { get; set; } = DateTime.Today.AddDays(1);


			// Indicates whether the operation is in progress; is not reset until after
			// operation is confirmed to be completed, so can be used to recognize 
			// interruptions and infer continuation
			[JsonProperty("state")]
			[JsonConverter(typeof(StringEnumConverter))]
			public ScanningState State { get; set; } = ScanningState.None;


			// Indicates whether to shutdown the tray after operation completes
			[JsonProperty("shutdown")]
			public bool Shutdown { get; set; } = true;
		}
		#endregion Supporting classes


		public HashtagScheduler()
		{
			filePath = Path.Combine(PathHelper.GetAppDataPath(), Resx.ScanningCueFile);

			if (File.Exists(filePath))
			{
				schedule = ReadSchedule();
			}
			else
			{
				schedule = new Schedule
				{
					State = HashtagProvider.DatabaseExists()
						? ScanningState.Ready
						: ScanningState.PendingRebuild
				};
			}
		}


		public bool Active => File.Exists(filePath) && Process.GetProcessesByName(TrayName).Any();


		public bool ScheduleExists => File.Exists(filePath);


		public DateTime StartTime
		{
			get { return schedule.StartTime; }
			set { schedule.StartTime = value; }
		}


		public ScanningState State
		{
			get { return schedule.State; }
			set { schedule.State = value; }
		}


		/// <summary>
		/// Called by HashtagCommand to schedule a full rebuild or to rescan newly added notebooks
		/// </summary>
		/// <param name="startTime">The time to schedule the scan</param>
		/// <returns></returns>
		public async Task Activate()
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

			SaveSchedule();

			var dir = new Uri(
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)).LocalPath;

			if (dir.Contains("Debug"))
			{
				// if dev environment, apps won't be in same directory so add extra hop
				dir = Path.Combine(
					Path.GetDirectoryName(Path.GetDirectoryName(
						Path.GetDirectoryName(Path.GetDirectoryName(dir)))),
					@$"{TrayName}\bin\Debug");
			}

			var exe = Path.Combine(dir, $"{TrayName}.exe");
			logger.WriteLine($"starting {exe} @{schedule.StartTime.ToZuluString()}");

			var proc = Process.Start(new ProcessStartInfo
			{
				FileName = exe,
				LoadUserProfile = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				WorkingDirectory = dir
			});

			logger.WriteLine($"started {TrayName} process {proc.Id}");
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


		public void Refresh()
		{
			var update = ReadSchedule(false);
			if (update is not null)
			{
				schedule.State = update.State;
				schedule.StartTime = update.StartTime;
				schedule.Shutdown = update.Shutdown;
			}
		}


		private Schedule ReadSchedule(bool chatty = true)
		{
			if (!File.Exists(filePath))
			{
				if (chatty)
				{
					logger.WriteLine($"could not find schedule file {filePath}");
				}

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
