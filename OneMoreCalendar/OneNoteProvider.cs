//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Text.RegularExpressions;
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

		private static readonly Regex ReversedLink =
			new(@"onenote:(#.+?&end)&base-path=(https:.+)");

		// serializes loads so concurrent requests result in a single OneNote hierarchy dump
		private static readonly SemaphoreSlim loadLock = new(1, 1);

		// guards the cached index fields below
		private static readonly object indexLock = new();
		private static List<CalendarPage> index;
		private static string indexKey;
		private static DateTime indexLoaded;


		/// <summary>
		/// Constructs a new OneNote wrapper on a background thread. Its constructor calls
		/// into COM to activate OneNote (ApplicationFactory.CreateApplication), which blocks
		/// synchronously and, when OneNote isn't already running, can take several seconds to
		/// cold-start the process plus further blocking Thread.Sleep retries on transient
		/// busy/RPC errors while it finishes initializing. Awaiting this instead of calling
		/// "new OneNote()" directly keeps that blocking work off the UI thread.
		/// </summary>
		private static Task<OneNote> NewOneNote() => Task.Run(() => new OneNote());


		/// <summary>
		/// Returns true if an interactive ONENOTE.EXE - one with a visible main window, not
		/// just a headless COM server - is currently running. Read-only: never touches COM or
		/// launches anything, so it's safe to call even though this app may already hold its
		/// own COM connection to OneNote (from loading the page index). This app must never try
		/// to launch OneNote itself: a headless server started first can prevent a subsequent
		/// interactive launch from ever taking over.
		/// </summary>
		public static bool IsRunningInteractively()
		{
			foreach (var proc in Process.GetProcessesByName("ONENOTE"))
			{
				try
				{
					proc.Refresh();
					if (proc.MainWindowHandle != IntPtr.Zero)
					{
						return true;
					}
				}
				catch
				{
					// process may exit between enumerate and inspect — skip it
				}
			}
			return false;
		}


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
				await using var one = await NewOneNote();
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
		/// Creates a new page in the given section, titled with the given date, with its
		/// dateTime attribute set to noon local time on that date.
		/// </summary>
		/// <param name="date">The calendar day the new page represents</param>
		/// <param name="sectionId">The section, chosen via SelectLocation, to contain the page</param>
		/// <returns>The ID of the newly created page</returns>
		public async Task<string> CreatePage(DateTime date, string sectionId)
		{
			try
			{
				await using var one = await NewOneNote();

				var sectionXml = await one.GetSection(sectionId);
				var sns = sectionXml.GetNamespaceOfPrefix(OneNote.Prefix);
				var existingTitles = new HashSet<string>(
					sectionXml.Elements(sns + "Page").Attributes("name").Select(a => a.Value),
					StringComparer.CurrentCultureIgnoreCase);

				var title = UniqueTitle(string.Format(
					Properties.Resources.MonthView_NewPageTitle, date.ToString("yyyy-MM-dd")),
					existingTitles);

				one.CreatePage(sectionId, out var pageId);
				var newpage = await one.GetPage(pageId);

				newpage.Title = title;

				// use local noon rather than midnight so converting to UTC (and back to the
				// viewer's local time) can never cross a day boundary and land on the wrong day
				var noon = DateTime.SpecifyKind(date.Date.AddHours(12), DateTimeKind.Local);
				newpage.Root.SetAttributeValue("dateTime", noon.ToZuluString());

				await one.Update(newpage);

				return pageId;
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"error creating page for {date:yyyy-MM-dd}", exc);
				throw;
			}
		}


		/// <summary>
		/// Appends a " (1)", " (2)", etc. suffix to the given title, incrementing until the
		/// result no longer collides with an existing title in the section.
		/// </summary>
		private static string UniqueTitle(string title, ICollection<string> existingTitles)
		{
			if (!existingTitles.Contains(title))
			{
				return title;
			}

			var n = 1;
			string candidate;
			do
			{
				candidate = $"{title} ({n})";
				n++;
			}
			while (existingTitles.Contains(candidate));

			return candidate;
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
					var ancestors = e.Ancestors().ToList();

					var path = ancestors
						.Where(n => n.Attribute("name") != null)
						.Select(n => n.Attribute("name").Value)
						.Aggregate((name1, name2) => $"{name2} > {name1}");

					if (path.EndsWith(DeletedPages))
					{
						path = path.Substring(0, path.Length - DeletedPages.Length)
								+ Properties.Resources.word_RecycleBin;
					}

					// nearest ancestor is always the page's own Section
					var sectionColor = ancestors[0].Attribute("color")?.Value;

					return new CalendarPage
					{
						PageID = e.Attribute("ID").Value,
						Path = path,
						SectionColor = string.IsNullOrEmpty(sectionColor)
							? Color.Empty
							: ColorHelper.FromHtml(sectionColor),
						Title = e.Attribute("name").Value,
						Created = DateTime.Parse(
							e.Attribute("dateTime").Value, DateTimeFormatInfo.CurrentInfo),
						Modified = DateTime.Parse(
							e.Attribute("lastModifiedTime").Value, DateTimeFormatInfo.CurrentInfo),
						IsDeleted = e.Attribute("isInRecycleBin") != null,
						ReminderContent = e.Elements(ns + "Meta")
							.FirstOrDefault(m =>
								m.Attribute("name").Value == MetaNames.Reminder &&
								m.Attribute("content").Value.Length > 0)
							?.Attribute("content").Value
					};
				})
				.ToList();
		}


		private async Task<XElement> GetNotebooks(IEnumerable<string> ids)
		{
			try
			{
				// attempt optimal ways to load...

				await using var one = await NewOneNote();

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
				await using var one = await NewOneNote();
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

			await using var one = await NewOneNote();
			foreach (var page in pages)
			{
				if (token.IsCancellationRequested)
				{
					break;
				}

				try
				{
					var link = one.GetHyperlink(page.PageID, string.Empty);
					page.OneNoteHyperlink = FixReversedHyperlink(link);
					page.Hyperlink = NormalizeHyperlink(link);
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
		/// Hyperlinks may be returned from the OneNote API backwards from the expected
		/// format so this swaps the two parts that need to be reversed. The result is
		/// always a onenote: hyperlink.
		/// </summary>
		private static string FixReversedHyperlink(string hyperlink)
		{
			if (hyperlink is null)
			{
				return null;
			}

			var match = ReversedLink.Match(hyperlink);
			return match.Success
				// hyperlink is reversed, so correct it
				? $"onenote:{match.Groups[2].Value}{match.Groups[1].Value}"
				: hyperlink;
		}


		/// <summary>
		/// Fixes a reversed hyperlink and, if it is already correct, strips the onenote:
		/// scheme, as used when copying the links of a whole day.
		/// </summary>
		private static string NormalizeHyperlink(string hyperlink)
		{
			if (hyperlink is not null && hyperlink.StartsWith("onenote:https:"))
			{
				// hyperlink is correct, just strip onenote: part
				return hyperlink.Substring(8);
			}

			return FixReversedHyperlink(hyperlink);
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
		/// <param name="newWindow">True to open the page in a new OneNote window</param>
		/// <returns></returns>
		public async Task NavigateTo(string pageID, bool newWindow = false)
		{
			try
			{
				await using var one = await NewOneNote();
				var url = one.GetHyperlink(pageID, string.Empty);
				if (!string.IsNullOrEmpty(url))
				{
					await one.NavigateTo(url, newWindow);
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
