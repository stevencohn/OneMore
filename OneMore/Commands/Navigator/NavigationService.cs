//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;


	/// <summary>
	/// 
	/// </summary>
	internal class NavigationService : Loggable
	{
		/// <summary>
		/// Used by Settings to limit polling interval so we don't affect overall performance
		/// </summary>
		public const int MinimumPollingInterval = 1000;

		/// <summary>
		/// Used by NavigationProvider to filter out duplicate FileSystemWatcher.Changed events.
		/// </summary>
		public const int MinWatch = MinimumPollingInterval - 500;


		private const int PollingInterval = 1250; // 1.25s
		private const int HistoryDepth = 10;

		private readonly NavigationProvider provider;
		private string currentId = null;
		private int commitment = 0;


		public NavigationService()
		{
			provider = new NavigationProvider();
		}


		public void Startup()
		{
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

					await Task.Delay(PollingInterval);
				}

				logger.WriteLine("navigation service has stopped; check for exceptions above");
			});

			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Priority = ThreadPriority.BelowNormal;
			thread.Start();
		}


		public async Task Scan()
		{
			using var one = new OneNote();
			var pageId = one.CurrentPageId;

			if (pageId != currentId)
			{
				logger.Verbose($"nav- {pageId}");
				currentId = pageId;
				commitment = 0;
			}
			else
			{
				commitment++;
				if (commitment == 1)
				{
					logger.Verbose($"nav+ {currentId} .. {commitment}");
					await provider.RecordHistory(pageId, HistoryDepth);
				}
			}
		}
	}
}
