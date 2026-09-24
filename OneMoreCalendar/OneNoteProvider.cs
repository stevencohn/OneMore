//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// An abstraction of the OneNote class, provides a bit of decoupling between
	/// the calendar app and the OneNote addin library
	/// </summary>
	internal class OneNoteProvider
	{
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetForegroundWindow(IntPtr hWnd);


		// how long an index that includes the current month is trusted before reloading
		private static readonly TimeSpan LiveTtl = TimeSpan.FromSeconds(60);

		// serializes loads so concurrent requests result in a single OneNote hierarchy dump
		private static readonly SemaphoreSlim loadLock = new(1, 1);

		// guards the cached index fields below
		private static readonly object indexLock = new();
		private static List<CalendarPage> index;
		private static string indexKey;
		private static DateTime indexLoaded;


		/// <summary>
		/// Export an XPS representation of the specified page to the TEMP folder
		/// </summary>
		/// <param name="pageID"></param>
		/// <returns>The path of the file generated</returns>
		public async Task<string> Export(string pageID)
		{
			var path = Path.Combine(
				Path.GetTempPath(),
				Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".xps");

			try
			{
				await using var one = new OneNote();
				one.Export(pageID, path, OneNote.ExportFormat.XPS);
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"error exporting page {pageID}", exc);
				throw;
			}

			return path;
		}


		/// <summary>
		/// Discards the cached page index so the next request reloads from OneNote.
		/// </summary>
		public static void Invalidate()
		{
			lock (indexLock)
			{
				index = null;
			}
		}


		/// <summary>
		/// Gets the pages created and/or modified within the given date range.
		/// </summary>
		/// <remarks>
		/// OneNote can't filter its hierarchy by date so every load dumps all pages of the
		/// selected notebooks. That dump is cached in memory and the date range is applied
		/// to the cache. A range that touches the current month is "live" and reloads when
		/// the cache is older than LiveTtl; a range wholly before the current month is
		/// static and is served from the cache regardless of age.
		/// </remarks>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <param name="notebookIDs"></param>
		/// <param name="created"></param>
		/// <param name="modified"></param>
		/// <param name="deleted"></param>
		/// <returns></returns>
		public async Task<CalendarPages> GetPages(
			DateTime startDate, DateTime endDate,
			IEnumerable<string> notebookIDs,
			bool created, bool modified, bool deleted)
		{
			var live = endDate >= DateTime.Now.StartOfMonth();
			var all = await GetIndex(notebookIDs, live);

			return new CalendarPages(all
				.Where(p => deleted || !p.IsDeleted)
				// filter by one or both filters
				.Where(p =>
					(created && p.Created.InRange(startDate, endDate)) ||
					(modified && p.Modified.InRange(startDate, endDate)))
				// prefer creation time
				.OrderBy(p => created ? p.Created : p.Modified));
		}


		/// <summary>
		/// Gets the cached index of all pages in the notebooks, loading it if it is missing,
		/// for a different set of notebooks, or live and past its time-to-live.
		/// </summary>
		private async Task<List<CalendarPage>> GetIndex(IEnumerable<string> notebookIDs, bool live)
		{
			var ids = notebookIDs.ToList();
			var key = string.Join("|", ids.OrderBy(i => i, StringComparer.Ordinal));

			await loadLock.WaitAsync();
			try
			{
				bool stale;
				lock (indexLock)
				{
					stale = index is null || key != indexKey ||
						(live && DateTime.UtcNow - indexLoaded > LiveTtl);
				}

				if (stale)
				{
					var loaded = await LoadIndex(ids);
					lock (indexLock)
					{
						index = loaded;
						indexKey = key;
						indexLoaded = DateTime.UtcNow;
					}

					return loaded;
				}

				lock (indexLock)
				{
					return index;
				}
			}
			finally
			{
				loadLock.Release();
			}
		}


		private async Task<List<CalendarPage>> LoadIndex(IEnumerable<string> ids)
		{
			var notebooks = await GetNotebooks(ids);
			var ns = notebooks.GetNamespaceOfPrefix(OneNote.Prefix);

			const string DeletedPages = "OneNote_RecycleBin > Deleted Pages";

			return notebooks.Descendants(ns + "Page")
				.Select(e =>
				{
					var path = e.Ancestors()
						.Where(n => n.Attribute("name") != null)
						.Select(n => n.Attribute("name").Value)
						.Aggregate((name1, name2) => $"{name2} > {name1}");

					if (path.EndsWith(DeletedPages))
					{
						path = path.Substring(0, path.Length - DeletedPages.Length) + "Recycle Bin";
					}

					return new CalendarPage
					{
						PageID = e.Attribute("ID").Value,
						Path = path,
						Title = e.Attribute("name").Value,
						Created = DateTime.Parse(
							e.Attribute("dateTime").Value, DateTimeFormatInfo.CurrentInfo),
						Modified = DateTime.Parse(
							e.Attribute("lastModifiedTime").Value, DateTimeFormatInfo.CurrentInfo),
						IsDeleted = e.Attribute("isInRecycleBin") != null,
						HasReminders = e.Elements(ns + "Meta")
							.Any(m =>
								m.Attribute("name").Value == MetaNames.Reminder &&
								m.Attribute("content").Value.Length > 0)
					};
				})
				.ToList();
		}


		private async Task<XElement> GetNotebooks(IEnumerable<string> ids)
		{
			try
			{
				// attempt optimal ways to load...

				await using var one = new OneNote();

				if (!ids.Any())
				{
					return await one.GetNotebooks(OneNote.Scope.Pages);
				}

				var notebooks = await one.GetNotebooks();
				var ns = notebooks.GetNamespaceOfPrefix(OneNote.Prefix);
				if (ids.Count() == notebooks.Elements(ns + "Notebook").Count())
				{
					var found = ids.Count(i => notebooks
						.Elements(ns + "Notebook")
						.Any(e => e.Attribute("ID").Value == i));

					if (found == ids.Count())
					{
						return await one.GetNotebooks(OneNote.Scope.Pages);
					}
				}

				// filter out unknown notebookIDs to avoid uncatchable exception!
				var nids = notebooks.Elements(ns + "Notebook").Select(e => e.Attribute("ID").Value);
				var knownIDs = ids.Where(i => nids.Contains(i));

				var books = new XElement(ns + "Notebooks",
					new XAttribute(XNamespace.Xmlns + OneNote.Prefix, ns)
					);

				foreach (var id in knownIDs)
				{
					var book = await one.GetNotebook(id, OneNote.Scope.Pages);
					books.Add(book);
				}

				// return filtered list; otherwise return all notebooks
				return books.Elements().Any() ? books : notebooks;
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error fetching notebooks", exc);
				throw;
			}
		}


		/// <summary>
		/// Get a collection of all available notebooks
		/// </summary>
		/// <returns></returns>
		public async Task<IEnumerable<Notebook>> GetNotebooks()
		{
			try
			{
				await using var one = new OneNote();
				var notebooks = await one.GetNotebooks();
				var ns = notebooks.GetNamespaceOfPrefix(OneNote.Prefix);

				return notebooks.Elements(ns + "Notebook")
					.Select(e => new Notebook(e));
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error fetching notebooks", exc);
				throw;
			}
		}


		/// <summary>
		/// Gets the onenote:hyperlink and Web hyperlink for each page.
		/// </summary>
		/// <param name="pages">A collection of CalendarPages</param>
		/// <param name="token">
		/// A token that, when canceled, stops the fetch after the current page completes
		/// </param>
		/// <param name="setMaximum">Called once, up front, with the total number of pages</param>
		/// <param name="stepCallback">
		/// Called after each page is processed, whether it succeeded or failed
		/// </param>
		/// <returns></returns>
		public async Task GetPageLinks(
			List<CalendarPage> pages,
			CancellationToken token = default,
			Action<int> setMaximum = null,
			Func<CalendarPage, Task> stepCallback = null)
		{
			setMaximum?.Invoke(pages.Count);

			await using var one = new OneNote();
			foreach (var page in pages)
			{
				if (token.IsCancellationRequested)
				{
					break;
				}

				try
				{
					page.Hyperlink = one.GetHyperlink(page.PageID, string.Empty);
					page.WebHyperlink = one.GetWebHyperlink(page.PageID, string.Empty);
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine("error getting page hyperlinks", exc);
					page.Hyperlink = null;
				}

				if (stepCallback != null)
				{
					await stepCallback(page);
				}
			}
		}


		/// <summary>
		/// Get a collection of all unique years in the given notebooks
		/// </summary>
		/// <param name="notebookIDs"></param>
		/// <returns></returns>
		public async Task<IEnumerable<int>> GetYears(IEnumerable<string> notebookIDs)
		{
			try
			{
				// years don't need a live index; use whatever is cached
				var pages = await GetIndex(notebookIDs, false);

				var years = pages
					.Select(p => p.Created.Year)
					.Union(pages.Select(p => p.Modified.Year))
					.Distinct()
					.OrderByDescending(y => y)
					.ToList();

				return years;
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error fetching years for calendar", exc);
				throw;
			}
		}


		/// <summary>
		/// Open OneNote and navigate to the specified page
		/// </summary>
		/// <param name="pageID"></param>
		/// <returns></returns>
		public async Task NavigateTo(string pageID)
		{
			try
			{
				await using var one = new OneNote();
				var url = one.GetHyperlink(pageID, string.Empty);
				if (!string.IsNullOrEmpty(url))
				{
					await one.NavigateTo(url);
					SetForegroundWindow(one.WindowHandle);
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"error navigating to page {pageID}", exc);
				throw;
			}
		}
	}
}
