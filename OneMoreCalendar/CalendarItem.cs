//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Collections.Generic;


	/// <summary>
	/// 
	/// </summary>
	internal class CalendarItems : List<CalendarItem> { }


	/// <summary>
	/// 
	/// </summary>
	internal class CalendarItem
	{
		public DateTime Date { get; set; }

		public string PageID { get; set; }

		public string Title { get; set; }
	}
}
