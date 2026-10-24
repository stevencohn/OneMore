//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Microsoft.Office.Core;
	using River.OneMoreAddIn.Settings;
	using System;
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
		private readonly bool scrollPane;
		private IRibbonUI ribbon;
		private readonly NavigationProvider provider;
		private string currentId = null;
		private int commitment = 0;


		public NavigationService(IRibbonUI ribbon)
		{
			this.ribbon = ribbon;
			provider = new NavigationProvider();

			var settings = new SettingsProvider();
			var collection = settings.GetCollection("NavigatorSheet");
			pollingInterval = collection.Get("interval", DefaultPollingInterval);
			historyDepth = collection.Get("depth", DefaultHistoryDepth);
			disabled = collection.Get("disabled", false);

			scrollPane = collection.Get("scrollPane", false);		}


		/// <summary>
		/// Replace the cached ribbon reference, e.g. when OneNote's ribbon COM surrogate
		/// is recycled mid-session and RibbonLoaded fires again with a new proxy.
		/// </summary>
		/// <param name="ribbon">The current OneNote ribbon</param>
		public void RefreshRibbon(IRibbonUI ribbon)
		{
			this.ribbon = ribbon;
		}


		public void Startup()
		{
			// the poller also drives navigation pane scrolling, independent of history
			if (disabled && !scrollPane)
			{
				logger.WriteLine("Startup: navigation service is disabled");
				return;
			}

			logger.WriteLine("Startup: starting navigation service");

			ServiceLoop.Start(
				nameof(NavigationService), logger, () => pollingInterval, _ => Scan());
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
					// the page has been stable for one interval; bring its section into view
					if (scrollPane)
					{
						NavPaneScroller.Scroll(one.WindowHandle);
					}

					if (!disabled && await provider.RecordHistory(pageId, historyDepth))
					{
						ribbon.SafeInvalidateControl("ribNavigatorButton");
					}
				}
			}
		}
	}
}
