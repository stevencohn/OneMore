//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
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


		public HashtagService()
		{
			var settings = new SettingsProvider();
			var collection = settings.GetCollection("HashtagSheet");
			pollingInterval = collection.Get("interval", DefaultPollingInterval) * Minute;

			disabled = collection.Get("disabled", false);
		}


		public void Startup()
		{
			if (disabled)
			{
				logger.WriteLine("hashtag service is disabled");
				return;
			}

			logger.WriteLine("starting hashtag service");

			var thread = new Thread(async () =>
			{
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
			thread.Priority = ThreadPriority.BelowNormal;
			thread.Start();
		}


		private async Task Scan()
		{
			var clock = new Stopwatch();
			clock.Start();

			using var scanner = new HashtagScanner();
			var totalPages = await scanner.Scan();

			clock.Stop();
			var time = clock.ElapsedMilliseconds;
			logger.WriteLine($"hashtag service scanned {totalPages} pages in {time}ms");
		}
	}
}
