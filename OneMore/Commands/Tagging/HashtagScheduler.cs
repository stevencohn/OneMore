//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Threading.Tasks;
	using Resx = Properties.Resources;


	/// <summary>
	/// Determines whether there is a pending or active scheduled scan and activates the
	/// OneMoreTray app if a scan has been scheduled. The schedule is kept in the database,
	/// along with a lease that identifies the tray process working on it, so that a tray that
	/// has died can be told apart from one that is still scanning.
	/// </summary>
	internal class HashtagScheduler : Loggable
	{
		/// <summary>
		/// The number of failed attempts after which a schedule is abandoned
		/// </summary>
		public const int MaxAttempts = 3;

		/// <summary>
		/// How long the tray has to claim a schedule after it is started
		/// </summary>
		internal static readonly TimeSpan StartupGrace = TimeSpan.FromSeconds(60);

		/// <summary>
		/// How long a heartbeat is trusted
		/// </summary>
		internal static readonly TimeSpan HeartbeatTimeout = TimeSpan.FromSeconds(90);

		private const string TrayName = "OneMoreTray";

		private readonly HashtagScheduleRecord schedule;
		private bool exists;


		public HashtagScheduler()
		{
			DeleteLegacyFile();

			var record = ReadRecord();
			exists = record is not null;
			schedule = record ?? NewDefault();
		}


		/// <summary>
		/// Gets whether a tray process is working on, or is about to take, the schedule.
		/// </summary>
		public bool Active
		{
			get
			{
				try
				{
					return exists
						&& State != ScanningState.None
						&& IsLeaseAlive(schedule, DateTime.UtcNow, ProcessMatches);
				}
				catch (Exception exc)
				{
					logger.WriteLine("error checking for active tray process", exc);
					return false;
				}
			}
		}


		/// <summary>
		/// Gets the number of times a tray has failed to complete the schedule
		/// </summary>
		public int Attempts => schedule.Attempts;


		public string[] Notebooks
		{
			get { return schedule.Notebooks; }
			set { schedule.Notebooks = value ?? Array.Empty<string>(); }
		}


		public bool ScheduleExists => exists;


		/// <summary>
		/// Gets or sets the scheduled start time, as local time
		/// </summary>
		public DateTime StartTime
		{
			get { return schedule.StartTime.ToLocalTime(); }
			set { schedule.StartTime = value.ToUniversalTime(); }
		}


		public ScanningState State
		{
			get { return schedule.State; }
			set
			{
				// a new request from the user starts over
				if (value == ScanningState.PendingRebuild || value == ScanningState.PendingScan)
				{
					schedule.Attempts = 0;
				}

				schedule.State = value;
			}
		}


		/// <summary>
		/// Determines whether a lease belongs to a live tray. The process check is injected
		/// so this can be tested.
		/// </summary>
		internal static bool IsLeaseAlive(
			HashtagScheduleRecord record, DateTime now, Func<int, DateTime, bool> processMatches)
		{
			if (record is null)
			{
				return false;
			}

			if (record.OwnerPid is null || record.OwnerStart is null)
			{
				// the tray was just started and has not claimed the schedule yet
				return now - record.Updated < StartupGrace;
			}

			var last = record.Heartbeat ?? record.Updated;
			if (now - last > HeartbeatTimeout)
			{
				return false;
			}

			return processMatches(record.OwnerPid.Value, record.OwnerStart.Value);
		}


		// the process must have the recorded id and start time, otherwise the id was reused
		private static bool ProcessMatches(int pid, DateTime startUtc)
		{
			try
			{
				using var process = Process.GetProcessById(pid);
				return Math.Abs((process.StartTime.ToUniversalTime() - startUtc).TotalSeconds) < 2;
			}
			catch (Exception)
			{
				// no such process, or not allowed to look at it
				return false;
			}
		}


		// only the trays of this user session; another session's tray is not ours to stop
		private static Process[] GetTrays()
		{
			var session = Process.GetCurrentProcess().SessionId;
			return Process.GetProcessesByName(TrayName)
				.Where(p =>
				{
					try { return p.SessionId == session; }
					catch (Exception) { return false; }
				})
				.ToArray();
		}


		/// <summary>
		/// Called by HashtagCommand to schedule a full rebuild or to rescan newly added notebooks
		/// </summary>
		public async Task Activate()
		{
			const int maxAttempts = 3;
			for (var attempt = 1; attempt <= maxAttempts; attempt++)
			{
				Process process = null;
				try
				{
					process = GetTrays().FirstOrDefault();
					if (process is null)
					{
						break;
					}

					logger.WriteLine($"stopping {TrayName} (attempt {attempt}/{maxAttempts})");
					process.Kill();
					using var cts = new System.Threading.CancellationTokenSource(5000);
					await process.WaitForExitAsync(cts.Token);
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error stopping {TrayName}", exc);
				}
				finally
				{
					process?.Dispose();
				}
			}

			try
			{
				if (GetTrays().Any())
				{
					logger.WriteLine($"could not stop {TrayName}; aborting activation");
					return;
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error checking for {TrayName} process; aborting activation", exc);
				return;
			}

			// the previous tray, if any, is gone so its lease is void
			schedule.OwnerPid = null;
			schedule.OwnerStart = null;
			schedule.Heartbeat = null;

			if (!SaveSchedule())
			{
				return;
			}

			var dir = new Uri(
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)).LocalPath;

			var exe = Path.Combine(dir, $"{TrayName}.exe");
			if (!File.Exists(exe))
			{
				logger.WriteLine($"could not find {exe}");
				return;
			}

			logger.WriteLine($"starting {exe} @{schedule.StartTime.ToZuluString()}");

			var proc = Process.Start(new ProcessStartInfo
			{
				FileName = exe,
				LoadUserProfile = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				WorkingDirectory = dir
			});

			if (proc is null)
			{
				logger.WriteLine($"failed to start {TrayName} process");
				return;
			}

			logger.WriteLine($"started {TrayName} process {proc.Id}");
		}


		/// <summary>
		/// Called by the tray when it begins work, to record that this process owns the schedule
		/// </summary>
		public void Claim()
		{
			try
			{
				using var process = Process.GetCurrentProcess();
				using var db = new HashtagScheduleProvider();
				if (db.Claim(process.Id, process.StartTime.ToUniversalTime()))
				{
					schedule.OwnerPid = process.Id;
					schedule.OwnerStart = process.StartTime.ToUniversalTime();
					schedule.Heartbeat = DateTime.UtcNow;
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error claiming schedule", exc);
			}
		}


		/// <summary>
		/// Called periodically by the tray to show it is still alive
		/// </summary>
		public void Heartbeat()
		{
			try
			{
				if (schedule.OwnerPid is int pid)
				{
					using var db = new HashtagScheduleProvider();
					db.Heartbeat(pid);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error updating heartbeat", exc);
			}
		}


		/// <summary>
		/// Moves a pending schedule to Scanning, unless something else already changed it
		/// </summary>
		public bool TryBeginScan()
		{
			try
			{
				using var db = new HashtagScheduleProvider();
				if (db.TryChangeState(schedule.State, ScanningState.Scanning))
				{
					schedule.State = ScanningState.Scanning;
					return true;
				}

				return false;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error starting scan", exc);
				return false;
			}
		}


		/// <summary>
		/// Records that the tray could not complete the schedule. After too many failures the
		/// schedule is abandoned so it is not retried every time OneNote starts.
		/// </summary>
		/// <returns>True if the schedule was abandoned</returns>
		public bool RecordFailure()
		{
			try
			{
				using var db = new HashtagScheduleProvider();
				var attempts = db.RecordFailure();
				if (attempts < 0)
				{
					return false;
				}

				schedule.Attempts = attempts;
				if (attempts >= MaxAttempts)
				{
					logger.WriteLine($"giving up on scheduled scan after {attempts} failures");
					ClearSchedule();
					return true;
				}

				// leave it pending so the next OneNote session tries again
				if (schedule.State == ScanningState.Scanning)
				{
					db.TryChangeState(ScanningState.Scanning, ScanningState.PendingScan);
				}

				return false;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error recording failure", exc);
				return false;
			}
		}


		public void ClearSchedule()
		{
			try
			{
				using var db = new HashtagScheduleProvider();
				if (db.Clear())
				{
					exists = false;
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error clearing schedule", exc);
			}
		}


		public void Refresh()
		{
			var update = ReadRecord();
			exists = update is not null;

			if (update is null)
			{
				schedule.State = HashtagProvider.CatalogExists()
					? ScanningState.Ready
					: ScanningState.PendingRebuild;

				schedule.OwnerPid = null;
				schedule.OwnerStart = null;
				schedule.Heartbeat = null;
			}
			else
			{
				schedule.Notebooks = update.Notebooks;
				schedule.State = update.State;
				schedule.StartTime = update.StartTime;
				schedule.OwnerPid = update.OwnerPid;
				schedule.OwnerStart = update.OwnerStart;
				schedule.Heartbeat = update.Heartbeat;
				schedule.Attempts = update.Attempts;
				schedule.Updated = update.Updated;
			}
		}


		public bool SaveSchedule()
		{
			try
			{
				using var db = new HashtagScheduleProvider();
				if (db.Save(schedule))
				{
					exists = true;
					schedule.Updated = DateTime.UtcNow;
					return true;
				}

				return false;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error writing scan schedule", exc);
				return false;
			}
		}


		private HashtagScheduleRecord ReadRecord()
		{
			try
			{
				using var db = new HashtagScheduleProvider();
				return db.Read();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error reading scan schedule", exc);
				return null;
			}
		}


		private static HashtagScheduleRecord NewDefault()
		{
			return new HashtagScheduleRecord
			{
				StartTime = DateTime.Today.AddDays(1).ToUniversalTime(),
				State = HashtagProvider.CatalogExists()
					? ScanningState.Ready
					: ScanningState.PendingRebuild
			};
		}


		// the schedule used to be a json file; it is no longer read, so a pending schedule left
		// by an older version is discarded and the catalog's own state decides what happens next
		private void DeleteLegacyFile()
		{
			try
			{
				var file = Path.Combine(PathHelper.GetAppDataPath(), Resx.ScanningCueFile);
				if (File.Exists(file))
				{
					logger.WriteLine($"deleting obsolete schedule file {file}");
					File.Delete(file);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error deleting obsolete schedule file", exc);
			}
		}
	}
}
