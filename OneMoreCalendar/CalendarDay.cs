//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;

	internal class CalendarDays : List<CalendarDay> { }


	internal class CalendarDay
	{
		public DateTime Date { get; set; }


		public bool InMonth { get; set; }


		public Rectangle Bounds { get; set; }


		public CalendarPages Pages { get; set; } = new CalendarPages();


		public int ScrollOffset { get; set; } = 0;


		public MoreButton UpButton;


		public MoreButton DownButton;
	}
}
