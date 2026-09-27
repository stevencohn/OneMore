//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Drawing;

	internal delegate void CalendarDayHandler(object sender, CalendarDayEventArgs e);
	internal delegate void CalendarHoverHandler(object sender, CalendarPageEventArgs e);
	internal delegate void CalendarPageHandler(object sender, CalendarPageEventArgs e);
	internal delegate void CalendarPageMenuHandler(object sender, CalendarPageMenuEventArgs e);
	internal delegate void CalendarCreatedPageHandler(object sender, CalendarCreatedPageEventArgs e);


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
	/// Describes a right-clicked page and where, in screen coordinates, the click occurred
	/// </summary>
	internal class CalendarPageMenuEventArgs : EventArgs
	{
		public CalendarPageMenuEventArgs(CalendarPage page, Rectangle bounds, Point screenLocation)
			: base()
		{
			Page = page;
			Bounds = bounds;
			ScreenLocation = screenLocation;
		}

		public CalendarPage Page { get; private set; }
		public Rectangle Bounds { get; private set; }
		public Point ScreenLocation { get; private set; }
	}


	/// <summary>
	/// Describes a newly created page and the day it was created for
	/// </summary>
	internal class CalendarCreatedPageEventArgs : EventArgs
	{
		public CalendarCreatedPageEventArgs(DateTime dayDate, string pageId)
			: base()
		{
			DayDate = dayDate;
			PageID = pageId;
		}

		public DateTime DayDate { get; private set; }
		public string PageID { get; private set; }
	}


	/// <summary>
	///
	/// </summary>
	internal interface ICalendarView
	{
		event CalendarDayHandler ClickedDay;
		event CalendarHoverHandler HoverPage;
		event CalendarPageHandler ClickedPage;
		event CalendarPageMenuHandler PageMenu;
		event CalendarCreatedPageHandler ClickedCreatePage;

		void SetRange(DateTime startDate, DateTime endDate, CalendarPages pages);
	}
}
