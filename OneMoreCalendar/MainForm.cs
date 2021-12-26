//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Linq;
	using System.Windows.Forms;


	public partial class MainForm : Form
	{
		private MonthView monthView;


		public MainForm()
		{
			InitializeComponent();

			Width = 1500;
			Height = 1000;


			var provider = new OneNoteProvider();
			var pages = provider.GetPages();


			var now = DateTime.Now;
			var days = MakeDayList(now.Year, now.Month, DayOfWeek.Sunday, pages);


			monthView = new MonthView(now, days)
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
		}


		private CalendarDays MakeDayList(int year, int month, DayOfWeek firstDay, CalendarItems items)
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

			for (int i = 1; i <= last; i++)
			{
				var day = new CalendarDay { Date = date, InMonth = true };

				var pp = items.Where(p => p.Modified.Year == date.Year && p.Modified.Month == date.Month && p.Modified.Day == date.Day);
				if (pp.Any())
				{
					foreach (var p in pp)
					{
						day.Items.Add(p);
					}
				}

				days.Add(day);
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
