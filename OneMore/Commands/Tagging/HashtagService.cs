﻿//************************************************************************************************
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
		private const int Minute = 60000; // ms in 1 minute

		private readonly int pollingInterval;
		private readonly bool disabled;

		private int hour;
		private int scanCount;
		private long scanTime;


		public HashtagService()
		{
			var settings = new SettingsProvider();
			var collection = settings.GetCollection("HashtagSheet");
			pollingInterval = collection.Get("interval", DefaultPollingInterval) * Minute;

			disabled = collection.Get<bool>("disabled");

			var rebuild = collection.Get<bool>("rebuild");
			if (rebuild)
			{
				HashtagProvider.DeleteDatabase();
				collection.Remove("rebuild");
				settings.SetCollection(collection);
				settings.Save();
			}
		}


		public void Startup()
		{
			if (disabled)
			{
				logger.WriteLine("hashtag service is disabled");
				return;
			}

			logger.WriteLine("starting hashtag service");

			hour = DateTime.Now.Hour;
			scanCount = 0;
			scanTime = 0;

			var thread = new Thread(async () =>
			{
				// let OneMore settle before we start
				await Task.Delay(3000);

				// 'errors' allows repeated consecutive exceptions but limits that to 5 so we
				// don't fall into an infinite loop. If it somehow miraculously recovers then
				// errors is reset back to zero and normal processing continues...

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

					await Task.Delay(pollingInterval);
				}

				logger.WriteLine("hashtag service has stopped; check for exceptions above");
			});

			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Priority = ThreadPriority.Lowest;
			thread.Start();
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

			if (hour != DateTime.Now.Hour)
			{
				var avg = scanTime / scanCount;
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
