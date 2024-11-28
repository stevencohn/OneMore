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

		private int scanCount;
		private long scanTime;
		protected int hour;


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


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// This base method is executed in the context of the OneMore add-in running in OneNote.
		// Compare against the OneMoreTray override.

		protected virtual async Task StartupLoop()
		{
			logger.Debug("StartupLoop() WaitForReady()");
			if (!await WaitForReady())
			{
				return;
			}

			// execute forever or until there are five consecutive errors...

			var errors = 0;
			while (errors < 5)
			{
				try
				{
					await Scan();

					errors = 0;
				}
				catch (Exception exc)
				{
					logger.WriteLine($"hashtag service exception {errors}", exc);
					errors++;
				}

				if (errors < 5)
				{
					await Task.Delay(scanInterval);
				}
			}

			logger.WriteLine("hashtag service has stopped; check for exceptions above");
		}


		private async Task<bool> WaitForReady()
		{
			if (scheduler.State != ScanningState.None &&
				scheduler.State != ScanningState.Ready &&
				!scheduler.Active)
			{
				await scheduler.Activate();
			}

			using var source = new CancellationTokenSource();

			EventHandler handler = (object sender, EventArgs e) =>
			{
				logger.WriteLine("cancelling HashtagService on ApplicationExit");
				source.Cancel();
			};

			// must cancel Task.Delay upon exit or OneNote will hang until Delay completes
			Application.ApplicationExit += handler;

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

					await Task.Delay(delay, source.Token);
					if (!source.IsCancellationRequested)
					{
						scheduler.Refresh();
						count++;

						// resume normal 10s interval
						delay = PollingDelay;
					}
				}
				while (scheduler.State != ScanningState.Ready && !source.IsCancellationRequested);
			}
			catch (TaskCanceledException)
			{
				logger.Verbose("WaitForReady canceled");
			}
			finally
			{
				Application.ApplicationExit -= handler;
			}

			return !source.IsCancellationRequested;
		}


		protected async Task Scan()
		{
			using var scanner = new HashtagScanner();

			if (notebookFilters is not null && notebookFilters.Length > 0)
			{
				scanner.SetNotebookFilters(notebookFilters);
			} else
			{
				return;
			}

			await scanner.Scan();

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
	}
}
