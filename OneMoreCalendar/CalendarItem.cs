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
		public string PageID { get; set; }


		public string Notebook { get; set; }


		public string Section { get; set; }


		public string Title { get; set; }


		public DateTime Created { get; set; }


		public DateTime Modified { get; set; }
	}
}
