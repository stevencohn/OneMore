//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;


	internal delegate void CalendarDayHandler(object sender, CalendarDayEventArgs e);
	internal delegate void CalendarPageHandler(object sender, CalendarPageEventArgs e);


	internal class CalendarDayEventArgs : EventArgs
	{
		public CalendarDayEventArgs(DateTime dayDate)
			: base()
		{
			DayDate = dayDate;
		}

		public DateTime DayDate { get; private set; }
	}


	internal class CalendarPageEventArgs : EventArgs
	{
		public CalendarPageEventArgs(string pageID)
			: base()
		{
			PageID = pageID;
		}

		public string PageID { get; private set; }
	}


	internal interface ICalendarView
	{
		event CalendarDayHandler ClickedDay;
		event CalendarPageHandler ClickedPage;

		void SetRange(DateTime startDate, DateTime endDate, CalendarItems items);
	}
}
