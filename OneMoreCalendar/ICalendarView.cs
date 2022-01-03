//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;


	internal delegate void CalendarDayHandler(object sender, CalendarDayEventArgs e);
	internal delegate void CalendarHoverHandler(object sender, CalendarPageEventArgs e);
	internal delegate void CalendarPageHandler(object sender, CalendarPageEventArgs e);


	/// <summary>
	/// 
	/// </summary>
	internal class CalendarDayEventArgs : EventArgs
	{
		public CalendarDayEventArgs(DateTime dayDate)
			: base()
		{
			DayDate = dayDate;
		}

		public DateTime DayDate { get; private set; }
	}


	/// <summary>
	/// 
	/// </summary>
	internal class CalendarPageEventArgs : EventArgs
	{
		public CalendarPageEventArgs(CalendarPage page)
			: base()
		{
			Page = page;
		}

		public CalendarPage Page { get; private set; }
	}


	/// <summary>
	/// 
	/// </summary>
	internal interface ICalendarView
	{
		event CalendarDayHandler ClickedDay;
		event CalendarHoverHandler HoverPage;
		event CalendarPageHandler ClickedPage;

		void SetRange(DateTime startDate, DateTime endDate, CalendarPages pages);
	}
}
