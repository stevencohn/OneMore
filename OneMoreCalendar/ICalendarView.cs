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
		public CalendarPageEventArgs(CalendarItem item)
			: base()
		{
			Item = item;
		}

		public CalendarItem Item { get; private set; }
	}


	/// <summary>
	/// 
	/// </summary>
	internal interface ICalendarView
	{
		event CalendarDayHandler ClickedDay;
		event CalendarHoverHandler HoverPage;
		event CalendarPageHandler ClickedPage;

		DateTime StartDate { get; }

		DateTime EndDate { get; }

		void SetRange(DateTime startDate, DateTime endDate, CalendarItems items);
	}
}
