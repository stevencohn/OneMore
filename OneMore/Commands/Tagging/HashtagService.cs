//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable IDE0039 // Use local function

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Diagnostics;
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
		private const int Pause = 3000; // 3s delay before starting
		private const int Minute = 60000; // ms in 1 minute

		private readonly bool embedded;
		private readonly bool fullRebuild;
		private readonly int interval;
		private readonly bool disabled;

		private string[] notebookFilters;
		private HashtagScheduler scheduler;
		private int hour;
		private int scanCount;
		private long scanTime;


		public delegate void HashtagScannedHandler(object sender, HashtagScannedEventArgs e);


		/// <summary>
		/// Initialize a new instance for use by the OneMore add-in
		/// </summary>
		public HashtagService()
		{
			embedded = true;
			var settings = new SettingsProvider().GetCollection("HashtagSheet");
			interval = settings.Get("interval", DefaultPollingInterval) * Minute;
			disabled = settings.Get<bool>("disabled");
		}


		/// <summary>
		/// Initialize a new instance for use by the OneMoreTray app
		/// </summary>
		/// <param name="rebuild"></param>
		public HashtagService(bool rebuild)
			: this()
		{
			embedded = false;
			fullRebuild = rebuild;
		}


		/// <summary>
		/// Fired upon the completion of each full scan
		/// </summary>
		public event HashtagScannedHandler OnHashtagScanned;


		/// <summary>
		/// Sets a list of notebookIDs to target during scan/rebuild.
		/// </summary>
		/// <param name="filters"></param>
		public void SetNotebookFilters(string[] filters)
		{
			notebookFilters = filters;
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
				if (embedded)
				{
					await StartupLoop();
				}
				else
				{
					await StartupRebuild();
				}
			})
			{
				Name = $"{nameof(HashtagService)}Thread"
			};

			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Priority = fullRebuild ? ThreadPriority.Normal : ThreadPriority.Lowest;
			thread.Start();
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// This method is executed in the context of the OneMore add-in running in OneNote

		private async Task StartupLoop()
		{
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
					await Task.Delay(interval);
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
				// then wait for scheduler to be ready...

				var count = 0;
				do
				{
					if (count % (Minute / Pause) == 0) // every minute
					{
						logger.WriteLine($"hashtag service waiting, {scheduler.State}");
					}

					await Task.Delay(Pause, source.Token);
					if (!source.IsCancellationRequested)
					{
						scheduler.Refresh();
						count++;
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


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// This method is executed in the context of the OneMoreTray app

		private async Task StartupRebuild()
		{
			if (fullRebuild)
			{
				// at this point, the service is "active" and the rebuild will commence
				// immediately, so prepare by dropping existing hashtag entities...
				using var db = new HashtagProvider();
				if (!db.DropDatabase())
				{
					return;
				}

				var provider = new SettingsProvider();
				var settings = provider.GetCollection("HashtagSheet");
				if (settings.Remove("rebuild"))
				{
					provider.SetCollection(settings);
					provider.Save();
				}
			}

			// we want this to complete exactly once and then exit but allow
			// for up to five consecutive errors during execution...

			var errors = 0;
			var done = false;

			while (errors < 5 && !done)
			{
				try
				{
					await Scan();

					errors = 0;
					done = true;
				}
				catch (Exception exc)
				{
					logger.WriteLine($"hashtag service exception {errors}", exc);
					errors++;
				}

				if (!done)
				{
					await Task.Delay(interval);
				}
			}

			if (errors > 0)
			{
				logger.WriteLine("hashtag service has stopped; check for exceptions above");
			}
		}


		private async Task Scan()
		{
			var clock = new Stopwatch();
			clock.Start();

			using var scanner = new HashtagScanner();

			if (notebookFilters is not null && notebookFilters.Length > 0)
			{
				scanner.SetNotebookFilters(notebookFilters);
			}

			var (dirtyPages, totalPages) = await scanner.Scan();

			clock.Stop();
			var time = clock.ElapsedMilliseconds;

			scanCount++;
			scanTime += time;

			var avg = scanTime / scanCount;

			OnHashtagScanned?.Invoke(this,
				new HashtagScannedEventArgs(totalPages, dirtyPages, time, scanCount, avg));

			if (hour != DateTime.Now.Hour)
			{
				logger.WriteLine($"hashtag service scanned {scanCount} times in the last hour, averaging {avg}ms");
				hour = DateTime.Now.Hour;
				scanCount = 0;
				scanTime = 0;
			}

			if (dirtyPages > 0 || time > 1000)
			{
				logger.WriteLine(
					$"hashtag service scanned {totalPages} pages, updating {dirtyPages}, in {time}ms");
			}
		}
	}
}
