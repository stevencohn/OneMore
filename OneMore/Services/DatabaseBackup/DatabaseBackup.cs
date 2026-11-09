//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Backups
{
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Data.SQLite;
	using System.Globalization;
	using System.IO;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// How often the database should be backed up.
	/// </summary>
	internal enum BackupInterval
	{
		Daily = 0,
		Weekly = 1,
		Monthly = 2
	}


	internal enum BackupOutcome
	{
		/// <summary>Nothing was done: disabled, not due, no database, or already running</summary>
		Skipped,

		Success,

		/// <summary>The backup location does not exist or cannot be written to</summary>
		Inaccessible,

		Failed,

		/// <summary>The user canceled the progress dialog</summary>
		Cancelled
	}


	/// <summary>
	/// The user-configurable backup options, as stored by the BackupSheet.
	/// </summary>
	internal sealed class BackupOptions
	{
		public const int DefaultKeep = 7;
		public const int MinKeep = 1;
		public const int MaxKeep = 40;

		public bool Enabled { get; set; }

		/// <summary>
		/// Gets or sets whether to use the folder currently configured in OneNote; otherwise
		/// <see cref="Folder"/> is used
		/// </summary>
		public bool UseOneNote { get; set; } = true;

		public string Folder { get; set; }

		public BackupInterval Interval { get; set; }

		public int Keep { get; set; } = DefaultKeep;

		/// <summary>
		/// Gets or sets the UTC time of the last successful backup, or null if none
		/// </summary>
		public DateTime? LastBackup { get; set; }


		public static BackupOptions Load()
		{
			var settings = new SettingsProvider().GetCollection(nameof(BackupSheet));
			return Load(settings);
		}


		public static BackupOptions Load(SettingsCollection settings)
		{
			var interval = settings.Get("interval", 0);
			if (interval < 0 || interval > (int)BackupInterval.Monthly)
			{
				interval = 0;
			}

			var keep = settings.Get("keep", DefaultKeep);
			if (keep < MinKeep)
			{
				keep = MinKeep;
			}
			else if (keep > MaxKeep)
			{
				keep = MaxKeep;
			}

			var options = new BackupOptions
			{
				Enabled = settings.Get("enabled", false),
				UseOneNote = settings.Get("useOneNote", true),
				Folder = settings.Get<string>("folder"),
				Interval = (BackupInterval)interval,
				Keep = keep
			};

			var last = settings.Get<string>("lastBackup");
			if (!string.IsNullOrEmpty(last) &&
				DateTime.TryParse(last, CultureInfo.InvariantCulture,
					DateTimeStyles.RoundtripKind, out var when))
			{
				// stored as a round-trip UTC string
				options.LastBackup = when.ToUniversalTime();
			}

			return options;
		}
	}


	/// <summary>
	/// Backs up OneMore.db to the OneNote backup folder or to a custom folder, on a schedule,
	/// keeping only the most recent copies.
	/// </summary>
	internal static class DatabaseBackup
	{
		private const string Prefix = "OneMore-";
		private const string Extension = ".db";
		private const string TempExtension = ".tmp";
		private const int PagesPerStep = 100;

		private static int running;
		private static bool warned;


		/// <summary>
		/// Determines whether a backup is due, that is, whether the schedule's interval has
		/// elapsed since the last successful backup.
		/// </summary>
		public static bool IsDue(BackupOptions options, DateTime nowUtc)
		{
			if (options.LastBackup is null)
			{
				return true;
			}

			var last = options.LastBackup.Value;
			var next = options.Interval switch
			{
				BackupInterval.Weekly => last.AddDays(7),
				BackupInterval.Monthly => last.AddMonths(1),
				_ => last.AddDays(1)
			};

			return nowUtc >= next;
		}


		/// <summary>
		/// Gets the folder in which to store backups. The OneNote folder is read from OneNote
		/// each time because the user can change it there at any time.
		/// </summary>
		/// <returns>The path or null if it cannot be determined</returns>
		public static async Task<string> GetFolder(BackupOptions options)
		{
			if (!options.UseOneNote)
			{
				return string.IsNullOrWhiteSpace(options.Folder) ? null : options.Folder.Trim();
			}

			return await GetOneNoteFolder();
		}


		/// <summary>
		/// Gets the backup folder currently configured in OneNote.
		/// </summary>
		/// <returns>The path or null if it cannot be determined</returns>
		public static async Task<string> GetOneNoteFolder()
		{
			try
			{
				return await Task.Run(async () =>
				{
					await using var one = new OneNote();
					var (backupFolder, _, _) = one.GetFolders();
					Logger.Current.WriteLine($"OneNote backup folder: [{backupFolder}]");
					return string.IsNullOrWhiteSpace(backupFolder) ? null : backupFolder;
				});
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error reading the OneNote backup folder", exc);
				return null;
			}
		}


		/// <summary>
		/// Determines whether the given folder exists and files can be created in it.
		/// </summary>
		public static bool IsAccessible(string folder)
		{
			if (string.IsNullOrWhiteSpace(folder))
			{
				return false;
			}

			try
			{
				if (!Directory.Exists(folder))
				{
					return false;
				}

				var probe = Path.Combine(folder, $".onemore-{Guid.NewGuid():N}.tmp");
				File.WriteAllText(probe, string.Empty);
				File.Delete(probe);
				return true;
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"backup folder is not accessible: {folder}", exc);
				return false;
			}
		}


		/// <summary>
		/// Runs a backup if enabled and due (or forced), displaying a progress dialog while
		/// copying and a warning if the backup location is not accessible.
		/// </summary>
		/// <param name="options">The options to use</param>
		/// <param name="owner">
		/// The window that owns any warning, when called from the UI thread. If null the
		/// caller is a background thread and the dialogs are shown on a new UI thread.
		/// </param>
		/// <param name="force">True to ignore the schedule, as for Backup Now</param>
		public static async Task<BackupOutcome> Execute(
			BackupOptions options, IWin32Window owner, bool force)
		{
			if (!force && (!options.Enabled || !IsDue(options, DateTime.UtcNow)))
			{
				return BackupOutcome.Skipped;
			}

			if (!File.Exists(DatabaseProvider.DatabasePath))
			{
				return BackupOutcome.Skipped;
			}

			// only one backup at a time, e.g. Backup Now while the pipeline stage is running
			if (Interlocked.CompareExchange(ref running, 1, 0) != 0)
			{
				return BackupOutcome.Skipped;
			}

			try
			{
				var folder = await GetFolder(options);
				if (!IsAccessible(folder))
				{
					Logger.Current.WriteLine($"backup folder is not accessible: [{folder}]");
					Warn(owner, force);
					return BackupOutcome.Inaccessible;
				}

				var outcome = owner is null
					? await RunOnUIThread(folder, options)
					: ShowProgress(folder, options);

				if (outcome == BackupOutcome.Success)
				{
					RecordSuccess(DateTime.UtcNow);
				}
				else if (outcome == BackupOutcome.Failed)
				{
					MoreMessageBox.ShowWarning(owner, Resx.BackupSheet_Failed);
				}

				return outcome;
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error running database backup", exc);
				return BackupOutcome.Failed;
			}
			finally
			{
				Interlocked.Exchange(ref running, 0);
			}
		}


		// the background pipeline has no UI thread, so make one for the progress dialog
		private static Task<BackupOutcome> RunOnUIThread(string folder, BackupOptions options)
		{
			var source = new TaskCompletionSource<BackupOutcome>();

			var thread = new Thread(() =>
			{
				try
				{
					source.SetResult(ShowProgress(folder, options));
				}
				catch (Exception exc)
				{
					source.SetException(exc);
				}
			})
			{
				Name = $"{nameof(DatabaseBackup)}Thread"
			};

			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Start();

			return source.Task;
		}


		private static BackupOutcome ShowProgress(string folder, BackupOptions options)
		{
			var outcome = BackupOutcome.Cancelled;

			using var dialog = new ProgressDialog();

			dialog.ShowDialogWithCancel((progress, token) =>
			{
				progress.SetMessage(Resx.BackupSheet_Progress);
				outcome = Backup(folder, options.Keep, progress, token);
				return Task.FromResult(outcome == BackupOutcome.Success);
			});

			return outcome;
		}


		private static void Warn(IWin32Window owner, bool force)
		{
			// the pipeline runs hourly; don't nag more than once per session
			if (force || !warned)
			{
				warned = true;

				if (owner is null)
				{
					var thread = new Thread(() =>
						MoreMessageBox.ShowWarning(null, Resx.BackupSheet_LocationWarning))
					{
						Name = $"{nameof(DatabaseBackup)}WarnThread"
					};

					thread.SetApartmentState(ApartmentState.STA);
					thread.IsBackground = true;
					thread.Start();
					thread.Join();
				}
				else
				{
					MoreMessageBox.ShowWarning(owner, Resx.BackupSheet_LocationWarning);
				}
			}
		}


		/// <summary>
		/// Copies the database into the folder and removes surplus older copies.
		/// </summary>
		private static BackupOutcome Backup(
			string folder, int keep, ProgressDialog progress, CancellationToken token)
		{
			try
			{
				var sized = false;
				var path = Copy(DatabaseProvider.DatabasePath, folder, token, (steps) =>
				{
					// SetMaximum resets the bar so only do it once
					if (!sized)
					{
						progress?.SetMaximum(steps);
						sized = true;
					}

					progress?.Increment();
				});

				if (path is null)
				{
					return BackupOutcome.Cancelled;
				}

				Logger.Current.WriteLine($"backed up database to {path}");

				Prune(folder, keep);
				return BackupOutcome.Success;
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error backing up database", exc);
				return BackupOutcome.Failed;
			}
		}


		/// <summary>
		/// Creates a consistent copy of a SQLite database using SQLite's online backup API,
		/// which, unlike File.Copy, accounts for the write-ahead log and for other processes
		/// writing to the database.
		/// </summary>
		/// <param name="databasePath">The database to copy</param>
		/// <param name="folder">The folder in which to create the copy</param>
		/// <param name="token">Cancels the copy</param>
		/// <param name="step">Called for each step with the total number of steps</param>
		/// <returns>The path of the copy, or null if canceled</returns>
		public static string Copy(
			string databasePath, string folder, CancellationToken token, Action<int> step = null)
		{
			if (token.IsCancellationRequested)
			{
				return null;
			}

			var name = $"{Prefix}{DateTime.Now:yyyyMMdd-HHmmss}{Extension}";
			var target = Path.Combine(folder, name);
			var temp = target + TempExtension;

			if (File.Exists(temp))
			{
				File.Delete(temp);
			}

			var canceled = false;

			try
			{
				using (var source = new SQLiteConnection($"Data source={databasePath}"))
				using (var dest = new SQLiteConnection($"Data source={temp}"))
				{
					source.Open();
					dest.Open();

					source.BackupDatabase(dest, "main", "main", PagesPerStep,
						(_, _, _, _, _, _, total, _) =>
						{
							if (token.IsCancellationRequested)
							{
								canceled = true;
								return false;
							}

							step?.Invoke((total + PagesPerStep - 1) / PagesPerStep);
							return true;
						}, 100);

					if (!canceled)
					{
						// leave a single self-contained file rather than file+wal+shm
						using var cmd = dest.CreateCommand();
						cmd.CommandText = "PRAGMA journal_mode=DELETE";
						cmd.ExecuteNonQuery();
					}
				}

				// the callback isn't invoked for a copy that completes in a single step
				if (canceled || token.IsCancellationRequested)
				{
					return null;
				}

				if (File.Exists(target))
				{
					File.Delete(target);
				}

				File.Move(temp, target);
				return target;
			}
			finally
			{
				if (File.Exists(temp))
				{
					try
					{
						File.Delete(temp);
					}
					catch (Exception exc)
					{
						Logger.Current.WriteLine($"could not delete {temp}", exc);
					}
				}
			}
		}


		/// <summary>
		/// Determines whether the folder contains any backup files made by this feature.
		/// </summary>
		public static bool HasBackups(string folder)
		{
			try
			{
				return !string.IsNullOrWhiteSpace(folder) &&
					Directory.Exists(folder) &&
					Directory.GetFiles(folder, $"{Prefix}*{Extension}").Length > 0;
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"could not look for backups in {folder}", exc);
				return false;
			}
		}


		/// <summary>
		/// Deletes all but the newest backups, as determined by their timestamped names. Only
		/// files named by this feature are touched.
		/// </summary>
		public static void Prune(string folder, int keep)
		{
			var files = Directory.GetFiles(folder, $"{Prefix}*{Extension}");

			// names embed a sortable timestamp so newest sort last
			Array.Sort(files, StringComparer.OrdinalIgnoreCase);

			for (var i = 0; i < files.Length - keep; i++)
			{
				try
				{
					File.Delete(files[i]);
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"could not delete old backup {files[i]}", exc);
				}
			}
		}


		/// <summary>
		/// Records the time of the last successful backup. Done with a fresh provider, and
		/// saved immediately, because saving rewrites the entire settings file.
		/// </summary>
		public static void RecordSuccess(DateTime utc)
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection(nameof(BackupSheet));
			settings.Add("lastBackup", utc.ToString("o", CultureInfo.InvariantCulture));
			provider.SetCollection(settings);
			provider.Save();
		}
	}
}
