//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Diagnostics;
	using System.Threading;
	using System.Threading.Tasks;


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
		private readonly bool rebuild;
		private readonly int interval;
		private readonly bool disabled;

		private HashtagScheduler scheduler;
		private int hour;
		private int scanCount;
		private long scanTime;


		public delegate void HashtagScannedHandler(object sender, HashtagScannedEventArgs e);


		public HashtagService()
		{
			embedded = true;
			var settings = new SettingsProvider().GetCollection("HashtagSheet");
			interval = settings.Get("interval", DefaultPollingInterval) * Minute;
			disabled = settings.Get<bool>("disabled");
		}


		public HashtagService(bool rebuild)
			: this()
		{
			embedded = false;
			this.rebuild = rebuild;
		}


		/// <summary>
		/// Fired upon the completion of each full scan
		/// </summary>
		public event HashtagScannedHandler OnHashtagScanned;


		public void Startup()
		{
			if (disabled)
			{
				logger.WriteLine("hashtag service is disabled");
				return;
			}

			logger.WriteLine("starting hashtag service");

			scheduler = new HashtagScheduler();
			logger.Verbose($"HastagService scheduler.State is {scheduler.State}");

			hour = DateTime.Now.Hour;

			var thread = new Thread(async () =>
			{
				if (embedded)
				{
					// let OneMore settle before we start, and wait for scheduler to be ready
					do
					{
						await Task.Delay(Pause);
					}
					while (scheduler.State != ScanningState.Ready);
				}
				else
				{
					PrepareRebuild();
				}


				// errors allows repeated consecutive exceptions but limits that to 5 so we
				// don't fall into an infinite loop. If it somehow miraculously recovers then
				// errors is reset back to zero and normal processing continues...

				var errors = 0;
				while (errors < 5)
				{
					try
					{
						await Scan();

						if (!embedded)
						{
							break;
						}

						errors = 0;
					}
					catch (Exception exc)
					{
						logger.WriteLine($"hashtag service exception {errors}", exc);
						errors++;
					}

					await Task.Delay(interval);
				}

				logger.WriteLine("hashtag service has stopped; check for exceptions above");
			});

			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Priority = rebuild ? ThreadPriority.Normal : ThreadPriority.Lowest;
			thread.Start();
		}


		private void PrepareRebuild()
		{
			// at this point, the service is "active" and the rebuild will commence
			// immediately, so prepare by deleting any old database...
			HashtagProvider.DeleteDatabase();

			var provider = new SettingsProvider();
			var settings = provider.GetCollection("HashtagSheet");
			if (settings.Remove("rebuild"))
			{
				provider.SetCollection(settings);
				provider.Save();
			}
		}


		private async Task Scan()
		{
			var clock = new Stopwatch();
			clock.Start();

			using var scanner = new HashtagScanner();
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
