//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Threading;
	using System.Threading.Tasks;


	/// <summary>
	/// Background service to monitor page navigations. This is a polling mechanism with
	/// specified throttling limits.
	/// </summary>
	internal class NavigationService : Loggable
	{

		// SafeWatchWindow must be less than MinimumPollingInterval; used by NavigationProvider
		// to filter out duplicate FileSystemWatcher.Changed events
		public const int MinimumPollingInterval = 1000; // do not change!
		public const int SafeWatchWindow = MinimumPollingInterval - 500;

		// defaults are used by Settings dialog
		public const int DefaultPollingInterval = 1250; // 1.25s
		public const int DefaultHistoryDepth = 10;

		private readonly int pollingInterval;
		private readonly int historyDepth;
		private readonly bool disabled;

		private readonly NavigationProvider provider;
		private string currentId = null;
		private int commitment = 0;


		public NavigationService()
		{
			provider = new NavigationProvider();

			var settings = new SettingsProvider();
			var collection = settings.GetCollection("NavigatorSheet");
			pollingInterval = collection.Get("interval", DefaultPollingInterval);
			historyDepth = collection.Get("depth", DefaultHistoryDepth);
			disabled = collection.Get("disabled", false);
		}


		public void Startup()
		{
			if (disabled)
			{
				logger.WriteLine("navigation service is disabled");
				return;
			}

			logger.WriteLine("starting navigation service");

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
						logger.WriteLine($"navigation service exception {errors}", exc);
						errors++;
					}

					await Task.Delay(pollingInterval);
				}

				logger.WriteLine("navigation service has stopped; check for exceptions above");
			})
			{
				Name = $"{nameof(NavigationService)}Thread"
			};

			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Priority = ThreadPriority.BelowNormal;
			thread.Start();
		}


		private async Task Scan()
		{
			await using var one = new OneNote();
			var pageId = one.CurrentPageId;

			if (pageId != currentId)
			{
				currentId = pageId;
				commitment = 0;
			}
			else
			{
				commitment++;
				if (commitment == 1)
				{
					await provider.RecordHistory(pageId, historyDepth);
				}
			}
		}
	}
}
