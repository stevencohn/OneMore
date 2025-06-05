//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Newtonsoft.Json;
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading;
	using System.Threading.Tasks;
	using HistoryRecord = OneNote.HierarchyInfo;
	using Resx = Properties.Resources;


	/// <summary>
	/// Provides thread-safe access to the navigation history file.
	/// This is used by both NavigationService and NavigatorWindow.
	/// </summary>
	internal class NavigationProvider : Loggable, IDisposable
	{
		private static readonly SemaphoreSlim semalock = new(1);
		private static readonly SemaphoreSlim semapub = new(1);

		private readonly string path;
		private readonly bool quickNotes;
		private FileSystemWatcher watcher;
		private EventHandler<HistoryLog> navigated;
		private DateTime lastWrite;
		private bool disposedValue;


		public NavigationProvider()
		{
			path = Path.Combine(PathHelper.GetAppDataPath(), "Navigator.json");
			lastWrite = DateTime.MinValue;

			var settings = new SettingsProvider();
			var collection = settings.GetCollection("NavigatorSheet");
			quickNotes = collection.Get("quickNotes", false);
		}


		#region Dispose
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					if (watcher != null)
					{
						watcher.EnableRaisingEvents = false;
						watcher.Changed -= NavigationHandler;
						watcher.Dispose();
						watcher = null;
					}
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion Dispose


		/// <summary>
		/// Adds or removes an event handler to signal that the user has navigated to a page
		/// and stayed long enough to be recorded as "read"
		/// </summary>
		public event EventHandler<HistoryLog> Navigated
		{
			add
			{
				var dir = Path.GetDirectoryName(path);
				var nam = Path.GetFileNameWithoutExtension(path);
				var ext = Path.GetExtension(path);
				watcher = new FileSystemWatcher(dir, $"{nam}*{ext}")
				{
					NotifyFilter = NotifyFilters.LastWrite
				};
				watcher.Changed += NavigationHandler;
				watcher.EnableRaisingEvents = true;
				navigated += value;
			}

			remove
			{
				navigated -= value;
				watcher.EnableRaisingEvents = false;
				watcher.Changed -= NavigationHandler;
				watcher.Dispose();
				watcher = null;
			}
		}


		private async void NavigationHandler(object sender, FileSystemEventArgs e)
		{
			try
			{
				await semapub.WaitAsync();

				// time-check will prevent known bug/feature where FileSystemWatcher.Changed
				// seems to raise duplicate events within just a couple of ticks of each other;
				// throttle should be less than NavigationService.PollingInterval

				var time = File.GetLastWriteTime(e.FullPath);
				if (time.Subtract(lastWrite).TotalMilliseconds > NavigationService.SafeWatchWindow)
				{
					navigated?.Invoke(this, await ReadHistoryLog());
					lastWrite = time;
				}
			}
			finally
			{
				semapub.Release();
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// History...

		/// <summary>
		/// Deletes the given records from the history log.
		/// </summary>
		/// <param name="records">Records to delete</param>
		/// <returns></returns>
		public async Task DeleteHistory(List<HistoryRecord> records)
		{
			await semalock.WaitAsync();

			try
			{
				var log = await Read();

				var updated = false;

				foreach (var record in records)
				{
					var index = log.History.FindIndex(r => r.PageId == record.PageId);
					if (index >= 0)
					{
						log.History.RemoveAt(index);
						updated = true;
					}
				}

				if (updated)
				{
					await Save(log);
				}
			}
			finally
			{
				semalock.Release();
			}
		}


		/// <summary>
		/// Returns the list of history items tracking visited pages.
		/// </summary>
		/// <returns></returns>
		public async Task<HistoryLog> ReadHistoryLog()
		{
			try
			{
				await semalock.WaitAsync();

				var log = await Read();
				return log;
			}
			finally
			{
				semalock.Release();
			}
		}


		/// <summary>
		/// Records the given page ID as a visited page, marking it with the current time
		/// to record the "last visited" time.
		/// </summary>
		/// <param name="pageID">The ID of the visited page</param>
		/// <param name="depth">The maximum number of history records to maintain</param>
		/// <returns></returns>
		public async Task<bool> RecordHistory(string pageID, int depth)
		{
			if (string.IsNullOrWhiteSpace(pageID))
			{
				return false;
			}

			await semalock.WaitAsync();

			try
			{
				var log = await Read();

				var updated = false;

				HistoryRecord record;
				var index = log.History.FindIndex(r => r.PageId == pageID);
				if (index < 0)
				{
					record = await Resolve(pageID);
					if (record != null)
					{
						// tracking Quick Notes?
						if (record.TitleId == null)
						{
							if (quickNotes)
							{
								record.Name = Resx.phrase_QuickNote;
								log.History.Insert(0, record);
								updated = true;
							}
						}
						else
						{
							log.History.Insert(0, record);
							updated = true;
						}
					}
				}
				else
				{
					record = log.History[index];
					if (await UpdateTitle(record))
					{
						log.History.RemoveAt(index);
						log.History.Insert(0, record);
						updated = true;
					}
				}

				// record might be null if page is still loading after previously
				// clearing the notebook cache...

				if (updated && (record != null))
				{
					record.Visited = DateTime.Now.GetTickSeconds();

					if (log.History.Count > depth)
					{
						log.History.RemoveRange(depth, log.History.Count - depth);
					}

					await Save(log);
				}

				return updated;
			}
			finally
			{
				semalock.Release();
			}
		}


		private async Task<HistoryRecord> Resolve(string pageID)
		{
			try
			{
				// might be null if the page no longer exits; exception raised in GetPageInfo
				await using var one = new OneNote { FallThrough = true };
				return await one.GetPageInfo(pageID);
			}
			catch (System.Runtime.InteropServices.COMException exc)
			{
				logger.WriteLine($"navigator resolve skipping broken or unloaded page {pageID}", exc);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"navigator can't resolve page {pageID}", exc);
			}

			return null;
		}


		private async Task<bool> UpdateTitle(HistoryRecord record)
		{
			try
			{
				await using var one = new OneNote { FallThrough = true };
				var page = await one.GetPage(record.PageId, OneNote.PageDetail.Basic);

				if (page.TitleID != null)
				{
					record.Name = page.Title;
				}

				return true;
			}
			catch (System.Runtime.InteropServices.COMException exc)
			{
				logger.WriteLine($"navigator update title skipping broken or unloaded page {record.PageId}", exc);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"navigator can't udpate title for page {record.PageId}", exc);
			}

			return false;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Pinned...

		/// <summary>
		/// Returns the list of pinned items saved by the user.
		/// </summary>
		/// <returns></returns>
		public async Task<List<HistoryRecord>> ReadPinned()
		{
			await semalock.WaitAsync();

			try
			{
				var log = await Read();
				return log.Pinned;
			}
			finally
			{
				semalock.Release();
			}
		}


		/// <summary>
		/// Records the given list of page IDs as "pinned" items
		/// </summary>
		/// <param name="records">A list of page IDs</param>
		/// <returns>True if the pinned list is updated; false if no changes needed</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Bug",
			"S2583:Conditionally executed code should be reachable",
			Justification = "Sonar can't see into predicate")]
		public async Task<bool> AddPinned(List<HistoryRecord> records)
		{
			await semalock.WaitAsync();

			try
			{
				var log = await Read();

				var updated = false;
				records.ForEach(record =>
				{
					var index = log.Pinned.FindIndex(p => p.PageId == record.PageId);
					if (index < 0)
					{
						log.Pinned.Add(record);
						updated = true;
					}
				});


				if (updated)
				{
					await Save(log);
				}

				return updated;
			}
			finally
			{
				semalock.Release();
			}
		}


		/// <summary>
		/// Saves the pinned list; used when reordering pages
		/// </summary>
		/// <param name="records">Reordered list of page IDs</param>
		/// <returns></returns>
		public async Task SavePinned(List<HistoryRecord> records)
		{
			await semalock.WaitAsync();

			try
			{
				var log = await Read();
				log.Pinned.Clear();
				log.Pinned.AddRange(records);
				await Save(log);
			}
			finally
			{
				semalock.Release();
			}
		}


		/// <summary>
		/// Removes the list of page IDs from the pinned list.
		/// </summary>
		/// <param name="records"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Bug",
			"S2583:Conditionally executed code should be reachable",
			Justification = "Sonar can't see into predicate")]
		public async Task<bool> UnpinPages(List<HistoryRecord> records)
		{
			await semalock.WaitAsync();

			try
			{
				var log = await Read();

				var updated = false;
				records.ForEach(record =>
				{
					var index = log.Pinned.FindIndex(p => p.PageId == record.PageId);
					if (index >= 0)
					{
						log.Pinned.RemoveAt(index);
						updated = true;
					}
				});


				if (updated)
				{
					await Save(log);
				}

				return updated;
			}
			finally
			{
				semalock.Release();
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Helpers...

		private async Task<HistoryLog> Read()
		{
			HistoryLog log = null;

			if (File.Exists(path))
			{
				try
				{
					// ensure we have ReadWrite sharing enabled so we don't block access
					// between NavigationService and NavigationDialog
					using var stream = new FileStream(path,
						FileMode.Open,
						FileAccess.Read,
						FileShare.ReadWrite);

					using var reader = new StreamReader(stream);

					log = JsonConvert.DeserializeObject<HistoryLog>(await reader.ReadToEndAsync());
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error reading {path}", exc);
					log = null;
				}
			}

			return log ?? new HistoryLog();
		}


		private async Task Save(HistoryLog log)
		{
			try
			{
				var json = JsonConvert.SerializeObject(log, Formatting.Indented);

				var dir = Path.GetDirectoryName(path);
				PathHelper.EnsurePathExists(dir);

				// ensure we have ReadWrite sharing enabled so we don't block access
				// between NavigationService and NavigationDialog
				using var stream = new FileStream(path,
					FileMode.OpenOrCreate,
					FileAccess.Write,
					FileShare.ReadWrite);

				// clear contents if writing less bytes than file contains
				stream.SetLength(0);

				using var writer = new StreamWriter(stream);
				await writer.WriteAsync(json);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error saving {path}", exc);
			}
		}
	}
}
