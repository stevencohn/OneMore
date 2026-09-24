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


		public string WebHyperlink { get; set; }


		/// <summary>
		/// Determines whether the filter matches this page's notebook, section group, section,
		/// or page title; Path already holds the notebook, section group and section names.
		/// </summary>
		public bool Matches(Regex finder)
		{
			return finder.IsMatch($"{Path} > {Title}");
		}
	}
}
