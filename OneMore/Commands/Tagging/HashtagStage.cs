//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Identity;
	using River.OneMoreAddIn.Pipeline;
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Diagnostics;
	using System.Threading;
	using System.Threading.Tasks;


	/// <summary>
	/// The hashtag stage of the background pipeline: its settings, the scheduler that says
	/// when the hashtag catalog is ready, the provider and scanner, and the results it reports.
	/// This is a polling mechanism with specified throttling limits.
	/// </summary>
	internal class HashtagStage : Loggable, IPipelineStage
	{
		public const int DefaultPollingInterval = 2; // 2 minutes

		private const int Minute = 60000;            // ms in 1 minute

		private HashtagScheduler scheduler;
		private HashtagProvider provider;
		private string[] notebookFilters;
		private int scanCount;
		private long scanTime;
		private int hour;
		private bool initialized;
		private bool activationChecked;
		private DateTime? lastWaitLog;
		private bool reportedNoSnapshot;
		private bool disposed;


		public delegate void HashtagScannedHandler(object sender, HashtagScannedEventArgs e);


		/// <summary>
		/// Initialize a new instance, reading how often the user wants hashtags scanned
		/// </summary>
		public HashtagStage()
		{
			var settings = new SettingsProvider().GetCollection("HashtagSheet");
			Interval = settings.Get("interval", DefaultPollingInterval) * Minute;
		}


		/// <summary>
		/// Fired upon the completion of each full scan
		/// </summary>
		public event HashtagScannedHandler OnHashtagScanned;


		public string Name => "hashtags";


		/// <summary>
		/// Gets the delay, in milliseconds, between scans
		/// </summary>
		public int Interval { get; }


		/// <summary>
		/// Gets whether the user has left hashtag scanning enabled. This reads the setting
		/// each time, so it reflects a change made while OneNote is running.
		/// </summary>
		public bool IsEnabled =>
			!new SettingsProvider().GetCollection("HashtagSheet").Get("disabled", false);


		/// <summary>
		/// Gets or sets whether this is a scan the user scheduled, run by the tray. The tray is
		/// itself building or updating the catalog, so there is nothing to wait for.
		/// </summary>
		public bool Scheduled { get; set; }


		/// <summary>
		/// Gets or sets whether to drop the existing hashtag catalog before the first scan
		/// and build it again, as the tray does for a scheduled rebuild.
		/// </summary>
		public bool Rebuild { get; set; }


		/// <summary>
		/// Sets a list of notebookIDs to target during scan/rebuild.
		/// </summary>
		public void SetNotebookFilters(string[] filters)
		{
			notebookFilters = filters;
		}


		public void Initialize()
		{
			if (!IsEnabled)
			{
				logger.WriteLine("Startup: hashtag service is disabled");
				return;
			}

			EnsureInitialized();
		}


		// the scheduler is created only once the stage is enabled, so a user who has hashtags
		// off is not given one, and a user who turns them on later is
		private void EnsureInitialized()
		{
			if (initialized)
			{
				return;
			}

			scheduler = new HashtagScheduler();

			var state = scheduler.State == ScanningState.None ? "ready" : scheduler.State.ToString();
			logger.WriteLine($"Startup: starting hashtag service, {state}");

			hour = DateTime.Now.Hour;
			initialized = true;
		}


		/// <summary>
		/// Determines whether the hashtag catalog is ready to be scanned, asking the tray to
		/// build it if need be.
		/// </summary>
		public async Task<bool> IsReady(CancellationToken token)
		{
			EnsureInitialized();

			if (Scheduled)
			{
				return true;
			}

			// Activating stops any tray that is running and starts a new one, so the decision is made
			// once, on the first check when the state was just read, as at startup. It is never made
			// again: a later check sees a tray that has simply finished its scan and would wrongly
			// start another.
			var activate = ShouldActivate(activationChecked, scheduler.State, scheduler.Active);
			activationChecked = true;

			if (activate)
			{
				await scheduler.Activate();
			}

			scheduler.Refresh();

			if (scheduler.State == ScanningState.Ready)
			{
				lastWaitLog = null;
				return true;
			}

			// say so once a minute, not every time we are asked
			var now = DateTime.Now;
			if (lastWaitLog is null || now - lastWaitLog.Value >= TimeSpan.FromMilliseconds(Minute))
			{
				logger.WriteLine($"Startup: hashtag service waiting, {scheduler.State}");
				lastWaitLog = now;
			}

			return false;
		}


		/// <summary>
		/// Determines whether to ask the tray to build the hashtag catalog. This is true only on the
		/// first check, and only if the catalog is waiting to be built or scanned and no tray is
		/// already working on it.
		/// </summary>
		internal static bool ShouldActivate(bool alreadyChecked, ScanningState state, bool trayActive)
		{
			return !alreadyChecked
				&& state != ScanningState.None
				&& state != ScanningState.Ready
				&& !trayActive;
		}


		/// <summary>
		/// Scans all notebooks for hashtags once and reports the result.
		/// </summary>
		/// <remarks>
		/// The pages to scan come from the snapshot the identity stage published into the
		/// context. Without one, such as when OneNote could not be read this cycle, there is
		/// nothing to scan and the next cycle tries again.
		/// </remarks>
		public async Task Run(PipelineContext context, CancellationToken token)
		{
			if (!context.TryGet<IdentitySnapshot>(out var snapshot))
			{
				if (!reportedNoSnapshot)
				{
					logger.WriteLine("hashtag service has no page list this cycle, waiting");
					reportedNoSnapshot = true;
				}

				if (Scheduled)
				{
					// the tray has only this one chance, so tell it to give up rather than wait
					OnHashtagScanned?.Invoke(this, new HashtagScannedEventArgs(
						"HashtagService could not read the notebooks"));
				}

				return;
			}

			reportedNoSnapshot = false;

			if (Rebuild)
			{
				// at this point the tray is "active" and the rebuild will commence immediately,
				// so prepare by dropping existing hashtag entities...
				if (!PrepareRebuild())
				{
					// the failure was reported through the event, which ends the tray
					return;
				}

				Rebuild = false;
			}

			provider ??= new HashtagProvider();

			using var scanner = new HashtagScanner(provider);

			if (notebookFilters is not null && notebookFilters.Length > 0)
			{
				scanner.SetNotebookFilters(notebookFilters);
			}

			await scanner.Scan(snapshot, token);

			var s = scanner.Stats;
			scanCount++;
			scanTime += scanner.Stats.Time;

			var avg = scanTime / scanCount;

			OnHashtagScanned?.Invoke(this,
				new HashtagScannedEventArgs(s.TotalPages, s.DirtyPages, s.Time, scanCount, avg));

			if (hour != DateTime.Now.Hour)
			{
				var ws = Process.GetCurrentProcess().WorkingSet64 / 1_048_576;
				var heap = GC.GetTotalMemory(false) / 1_048_576;
				logger.WriteLine($"hashtag service scanned {scanCount} times in the last hour, " +
					$"averaging {avg}ms, workingSet {ws}MB, managedHeap {heap}MB");
				hour = DateTime.Now.Hour;
				scanCount = 0;
				scanTime = 0;
			}
			else if (s.DirtyPages > 0 || s.Time > 1000)
			{
				scanner.Report("hashtag SERVICE");
			}
			else if (logger.IsDebug)
			{
				scanner.Report("hashtag service");
			}
		}


		// drops the hashtag catalog so the scan that follows builds it afresh
		private bool PrepareRebuild()
		{
			using var db = new HashtagProvider();
			if (!db.DropCatalog())
			{
				OnHashtagScanned?.Invoke(this,
					new HashtagScannedEventArgs("HashtagService reporting DropDatabase failure"));

				return false;
			}

			var settingsProvider = new SettingsProvider();
			var settings = settingsProvider.GetCollection("HashtagSheet");
			if (settings.Remove("rebuild"))
			{
				settingsProvider.SetCollection(settings);
				settingsProvider.Save();
			}

			return true;
		}


		/// <summary>
		/// Releases the database connection held between scans.
		/// </summary>
		public void Release()
		{
			provider?.Dispose();
			provider = null;
		}


		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					Release();
				}

				disposed = true;
			}
		}


		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
