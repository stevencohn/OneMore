//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable IDE0039 // Use local function

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	/// <summary>
	/// Background service to collect ##hashtags within content.
	/// This is a polling mechanism with specified throttling limits.
	/// </summary>
	internal class HashtagService : Loggable
	{
		public const int DefaultPollingInterval = 2; // 2 minutes

		private const int Minute = 60000;            // ms in 1 minute
		private const int PollingDelay = 10000;      // 10s polling interval waiting to start
		private const int WaitDelay = 3000;          // 3s optimistic pause before WaitPolling

		private readonly bool disabled;

		protected string[] notebookFilters;
		protected HashtagScheduler scheduler;
		protected int scanInterval;
		protected ThreadPriority threadPriority;
		protected CancellationTokenSource serviceToken;

		private int scanCount;
		private long scanTime;
		protected int hour;

		private int running; // re-entrancy guard, Interlocked access only


		public delegate void HashtagScannedHandler(object sender, HashtagScannedEventArgs e);


		/// <summary>
		/// Initialize a new instance for use by the OneMore add-in
		/// </summary>
		public HashtagService()
		{
			var settings = new SettingsProvider().GetCollection("HashtagSheet");
			scanInterval = settings.Get("interval", DefaultPollingInterval) * Minute;
			disabled = settings.Get<bool>("disabled");

			threadPriority = ThreadPriority.Lowest;
		}


		/// <summary>
		/// Fired upon the completion of each full scan
		/// </summary>
		public event HashtagScannedHandler OnHashtagScanned;


		/// <summary>
		/// Raise the OnHashtagScanned even.
		/// Available for inheritors, who are not allowed to invoke events directly.
		/// </summary>
		/// <param name="args"></param>
		protected void HashtagScanned(HashtagScannedEventArgs args)
		{
			OnHashtagScanned?.Invoke(this, args);
		}


		/// <summary>
		/// Start the service
		/// </summary>
		public void Startup()
		{
			if (disabled)
			{
				logger.WriteLine("hashtag service is disabled");
				return;
			}

			scheduler = new HashtagScheduler();

			var state = scheduler.State == ScanningState.None ? "ready" : scheduler.State.ToString();
			logger.WriteLine($"starting hashtag service, {state}");

			hour = DateTime.Now.Hour;

			serviceToken = new CancellationTokenSource();
			Application.ApplicationExit += OnApplicationExit;

			// new thread to provide a bit of isolation
			var thread = new Thread(async () =>
			{
				await StartupLoop();
			})
			{
				Name = $"{nameof(HashtagService)}Thread"
			};

			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Priority = threadPriority;
			thread.Start();
		}


		private void OnApplicationExit(object sender, EventArgs e)
		{
			logger.WriteLine("cancelling HashtagService on ApplicationExit");
			serviceToken?.Cancel();
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// This base method is executed in the context of the OneMore add-in running in OneNote.
		// Compare against the OneMoreTray override.

		protected virtual async Task StartupLoop()
		{
			logger.Debug("StartupLoop() WaitForReady()");
			if (!await WaitForReady(serviceToken.Token))
			{
				CleanupToken();
				return;
			}

			// execute forever or until there are five consecutive errors...

			var errors = 0;
			while (errors < 5 && !serviceToken.IsCancellationRequested)
			{
				try
				{
					await Scan(serviceToken.Token);
					errors = 0;
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch (Exception exc)
				{
					logger.WriteLine($"hashtag service exception {errors}", exc);
					errors++;
				}

				if (errors < 5 && !serviceToken.IsCancellationRequested)
				{
					try
					{
						await Task.Delay(scanInterval, serviceToken.Token);
					}
					catch (OperationCanceledException)
					{
						break;
					}
				}
			}

			CleanupToken();
			logger.WriteLine("hashtag service has stopped");
		}


		private void CleanupToken()
		{
			Application.ApplicationExit -= OnApplicationExit;
			serviceToken?.Dispose();
			serviceToken = null;
		}


		private async Task<bool> WaitForReady(CancellationToken token)
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
						logger.WriteLine($"hashtag service waiting, {scheduler.State}");
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
				logger.Verbose("WaitForReady canceled");
			}

			return !token.IsCancellationRequested;
		}


		protected async Task Scan(CancellationToken token = default)
		{
			// guard against overlapping scans if the interval is shorter than a scan's duration
			if (Interlocked.CompareExchange(ref running, 1, 0) != 0)
			{
				logger.WriteLine("hashtag service: skipping scan, previous scan still in progress");
				return;
			}

			try
			{
				using var scanner = new HashtagScanner();

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
					logger.WriteLine($"hashtag service scanned {scanCount} times in the last hour, averaging {avg}ms");
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
			finally
			{
				Interlocked.Exchange(ref running, 0);
			}
		}
	}
}
