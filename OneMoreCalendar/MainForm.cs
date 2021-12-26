//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Collections.Generic;
	using System.Windows.Forms;


	public partial class MainForm : Form
	{
		private MonthView monthView;


		public MainForm()
		{
			InitializeComponent();


			var now = DateTime.Now;
			var days = MakeDayList(now.Year, now.Month, DayOfWeek.Sunday);


			monthView = new MonthView(days)
			{
				BackColor = System.Drawing.Color.White,
				Dock = DockStyle.Fill,
				Location = new System.Drawing.Point(0, 0),
				Margin = new Padding(0),
				Name = "monthView",
				Size = new System.Drawing.Size(978, 506),
				TabIndex = 0
			};

			contentPanel.Controls.Add(monthView);


			/*
var item = new CalendarItem
{
	Date = DateTime.Now,
	PageID = "page-id",
	Title = "Title!"
};

var day = new MonthDayControl(item)
{
	Dock = DockStyle.Fill
};

table.Controls.Add(day, 0, 0);
*/
		}


		private CalendarDays MakeDayList(int year, int month, DayOfWeek firstDay)
		{
			var days = new CalendarDays();

			var date = new DateTime(year, month, 1);

			var first = date.DayOfWeek;
			var last = DateTime.DaysInMonth(date.Year, date.Month);

			var dow = firstDay == DayOfWeek.Sunday
				? (int)first
				: first == DayOfWeek.Sunday ? 6 : (int)first - 1;

			// previous month

			if (dow > 0)
			{
				var prev = date.AddDays(-dow);
				for (int i = 0; i < dow; i++)
				{
					days.Add(new CalendarDay { Date = prev });
					prev = prev.AddDays(1.0);
				}
			}

			// month

			for (int day = 1; day <= last; day++)
			{
				days.Add(new CalendarDay { Date = date, InMonth = true });
				date = date.AddDays(1.0);
			}

			// next month

			var rest = 7 - days.Count % 7;
			for (int i = 0; i < rest; i++)
			{
				days.Add(new CalendarDay { Date = date });
				date = date.AddDays(1.0);
			}

			return days;
		}
	}
}
