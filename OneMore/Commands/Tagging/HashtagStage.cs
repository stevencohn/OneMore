//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Diagnostics;
	using System.Threading;
	using System.Threading.Tasks;


	/// <summary>
	/// The hashtag part of the background service: its settings, the scheduler that says when
	/// the hashtag catalog is ready, the provider and scanner, and the results it reports.
	/// This is a polling mechanism with specified throttling limits.
	/// </summary>
	/// <remarks>
	/// Everything here used to live in HashtagService, which is being turned into a generic
	/// host that runs stages. Keeping the hashtag-specific parts together lets the host know
	/// nothing about hashtags.
	/// </remarks>
	internal class HashtagStage : Loggable, IDisposable
	{
		public const int DefaultPollingInterval = 2; // 2 minutes

		private const int Minute = 60000;            // ms in 1 minute
		private const int PollingDelay = 10000;      // 10s polling interval waiting to start
		private const int WaitDelay = 3000;          // 3s optimistic pause before WaitPolling

		private HashtagScheduler scheduler;
		private HashtagProvider provider;
		private string[] notebookFilters;
		private int scanCount;
		private long scanTime;
		private int hour;
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
		/// Sets a list of notebookIDs to target during scan/rebuild.
		/// </summary>
		public void SetNotebookFilters(string[] filters)
		{
			notebookFilters = filters;
		}


		/// <summary>
		/// Prepares the stage to run, noting the state of the hashtag catalog.
		/// </summary>
		public void Initialize()
		{
			scheduler = new HashtagScheduler();

			var state = scheduler.State == ScanningState.None ? "ready" : scheduler.State.ToString();
			logger.WriteLine($"Startup: starting hashtag service, {state}");

			hour = DateTime.Now.Hour;
		}


		/// <summary>
		/// Waits until the hashtag catalog is ready to be scanned, activating the tray to
		/// build it if need be.
		/// </summary>
		/// <returns>False if the wait was canceled</returns>
		public async Task<bool> WaitForReady(CancellationToken token)
		{
			if (scheduler.State != ScanningState.None &&
				scheduler.State != ScanningState.Ready &&
				!scheduler.Active)
			{
				await scheduler.Activate();
			}

			try
			{
				// wait at least once to let OneMore settle before we start
				// then wait for scheduler to be ready, if necessary...

				// start with 3s interval, optimistically hoping we're in a good state to go!
				var delay = WaitDelay;

				var count = 0;
				do
				{
					if (count % (Minute / WaitDelay) == 0) // every minute
					{
						logger.WriteLine($"Startup: hashtag service waiting, {scheduler.State}");
					}

					await Task.Delay(delay, token);
					scheduler.Refresh();
					count++;

					// resume normal 10s interval
					delay = PollingDelay;
				}
				while (scheduler.State != ScanningState.Ready && !token.IsCancellationRequested);
			}
			catch (OperationCanceledException)
			{
				logger.Verbose("HashtagService WaitForReady canceled");
			}

			return !token.IsCancellationRequested;
		}


		/// <summary>
		/// Scans all notebooks for hashtags once and reports the result.
		/// </summary>
		public async Task Run(CancellationToken token = default)
		{
			provider ??= new HashtagProvider();

			using var scanner = new HashtagScanner(provider);

			if (notebookFilters is not null && notebookFilters.Length > 0)
			{
				scanner.SetNotebookFilters(notebookFilters);
			}

			await scanner.Scan(token);

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
