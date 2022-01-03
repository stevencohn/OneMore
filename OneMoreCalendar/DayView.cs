//************************************************************************************************
// Copyright © 2022 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Windows.Forms;


	internal partial class DayView : UserControl, ICalendarView
	{
		public DayView()
		{
			InitializeComponent();
		}


		public DateTime StartDate => throw new NotImplementedException();

		public DateTime EndDate => throw new NotImplementedException();


		public event CalendarDayHandler ClickedDay;
		public event CalendarHoverHandler HoverPage;
		public event CalendarPageHandler ClickedPage;


		public void SetRange(DateTime startDate, DateTime endDate, CalendarItems items)
		{
			throw new NotImplementedException();
		}
	}
}
