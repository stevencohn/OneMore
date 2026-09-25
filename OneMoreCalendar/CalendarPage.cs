//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Text.RegularExpressions;


	/// <summary>
	/// 
	/// </summary>
	internal class CalendarPages : List<CalendarPage>
	{
		public CalendarPages()
			: base()
		{
		}

		public CalendarPages(IEnumerable<CalendarPage> pages)
			: base(pages)
		{
		}
	}


	/// <summary>
	/// 
	/// </summary>
	internal class CalendarPage
	{
		public string PageID { get; set; }


		public string Path { get; set; }


		public string Title { get; set; }


		public DateTime Created { get; set; }


		public DateTime Modified { get; set; }


		public bool IsDeleted { get; set; }


		public Rectangle Bounds { get; set; } = Rectangle.Empty;


		public bool HasReminders { get; set; }


		public string Hyperlink { get; set; }


		/// <summary>
		/// The onenote: hyperlink exactly as OneNote resolves it (GetHyperlinkToObject); unlike
		/// Hyperlink, the "onenote:" scheme is never stripped, so it always opens in OneNote.
		/// </summary>
		public string OneNoteHyperlink { get; set; }


		public string WebHyperlink { get; set; }


		/// <summary>
		/// Determines whether the filter matches this page's notebook, section group, section,
		/// or page title, and whether the page carries the requested hashtags; Path already
		/// holds the notebook, section group and section names. Every specified criterion must
		/// be satisfied, the same as Search Titles.
		/// </summary>
		/// <param name="finder">The text filter, or null if the filter has no text</param>
		/// <param name="included">
		/// IDs of pages carrying all the requested hashtags, or null if none were requested
		/// </param>
		/// <param name="excluded">
		/// IDs of pages carrying any of the excluded hashtags, or null if none were specified
		/// </param>
		public bool Matches(Regex finder, HashSet<string> included, HashSet<string> excluded)
		{
			if (finder is not null && !finder.IsMatch($"{Path} > {Title}"))
			{
				return false;
			}

			if (included is not null && !included.Contains(PageID))
			{
				return false;
			}

			return excluded is null || !excluded.Contains(PageID);
		}
	}
}
