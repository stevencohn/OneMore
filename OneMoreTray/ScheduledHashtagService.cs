//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreTray
{
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Threading.Tasks;


	/// <summary>
	/// A one-shot, get in, get it, get out, kinda deal. Performs a schedule scan, either
	/// now or in the futre, and optionally builds or rebuilds the catalog.
	/// 
	/// Subsequent scanning will be done by the HashtagService running within OneMore itself.
	/// </summary>
	internal class ScheduledHashtagService : HashtagService
	{
		private readonly bool rebuildCatalog;


		/// <summary>
		/// Initialize a new instance for use by the OneMoreTray app
		/// </summary>
		/// <param name="rebuild"></param>
		public ScheduledHashtagService(bool rebuild)
			: base()
		{
			rebuildCatalog = rebuild;
			if (rebuildCatalog)
			{
				threadPriority = System.Threading.ThreadPriority.BelowNormal;
			}
		}


		/// <summary>
		/// Sets a list of notebookIDs to target during scan/rebuild.
		/// </summary>
		/// <param name="filters"></param>
		public void SetNotebookFilters(string[] filters)
		{
			notebookFilters = filters;
		}


		protected override async Task StartupLoop()
		{
			if (rebuildCatalog)
			{
				// at this point, the service is "active" and the rebuild will commence
				// immediately, so prepare by dropping existing hashtag entities...
				using var db = new HashtagProvider();
				if (!db.DropCatalog())
				{
					HashtagScanned(new HashtagScannedEventArgs(
						"HashtagService reporting DropDatabase failure"));

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
					await Task.Delay(scanInterval);
				}
			}

			if (errors > 0)
			{
				logger.WriteLine("hashtag service has stopped; check for exceptions above");
			}
		}
	}
}
