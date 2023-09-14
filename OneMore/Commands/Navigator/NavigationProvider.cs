//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Newtonsoft.Json;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading;
	using System.Threading.Tasks;
	using HistoryRecord = OneNote.HierarchyInfo;


	/// <summary>
	/// Provides thread-safe access to the navigation history file.
	/// This is used by both NavigationService and NavigatorWindow.
	/// </summary>
	internal class NavigationProvider : Loggable, IDisposable
	{
		private static readonly SemaphoreSlim semalock = new(1);
		private static readonly SemaphoreSlim semapub = new(1);

		private readonly string path;
		private FileSystemWatcher watcher;
		private EventHandler<List<HistoryRecord>> navigated;
		private DateTime lastWrite;
		private bool disposedValue;


		public NavigationProvider()
		{
			path = Path.Combine(PathHelper.GetAppDataPath(), "Navigator.json");
			lastWrite = DateTime.MinValue;
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
		public event EventHandler<List<HistoryRecord>> Navigated
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
					navigated?.Invoke(this, await ReadHistory());
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
		/// Returns the list of history items tracking visited pages.
		/// </summary>
		/// <returns></returns>
		public async Task<List<HistoryRecord>> ReadHistory()
		{
			try
			{
				await semalock.WaitAsync();

				var log = await Read();
				return log.History;
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
					record = Resolve(pageID);
					log.History.Insert(0, record);
					updated = true;
				}
				else
				{
					record = log.History[index];
					UpdateTitle(record);

					if (index >= 0)
					{
						log.History.RemoveAt(index);
						log.History.Insert(0, record);
						updated = true;
					}
				}

				if (updated)
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


		private HistoryRecord Resolve(string pageID)
		{
			try
			{
				// might be null if the page no longer exits; exception raised in GetPageInfo
				using var one = new OneNote { FallThrough = true };
				return one.GetPageInfo(pageID);
			}
			catch (System.Runtime.InteropServices.COMException)
			{
				logger.WriteLine($"navigator resolve skipping broken page {pageID}");
			}
			catch (Exception exc)
			{
				logger.WriteLine($"navigator can't resolve page {pageID}", exc);
			}

			return null;
		}


		private void UpdateTitle(HistoryRecord record)
		{
			try
			{
				using var one = new OneNote { FallThrough = true };
				var page = one.GetPage(record.PageId, OneNote.PageDetail.Basic);
				record.Name = page.Title;
			}
			catch (System.Runtime.InteropServices.COMException)
			{
				logger.WriteLine($"navigator update title skipping broken page {record.PageId}");
			}
			catch (Exception exc)
			{
				logger.WriteLine($"navigator can't udpate title for page {record.PageId}", exc);
			}
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
