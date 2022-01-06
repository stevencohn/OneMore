//************************************************************************************************
// Copyright © 2022 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using System;
	using System.Data;
	using System.Linq;
	using System.Windows.Forms;


	internal partial class DetailView : UserControl, ICalendarView
	{

		public DetailView()
		{
			InitializeComponent();
		}

		public event CalendarDayHandler ClickedDay;
		public event CalendarHoverHandler HoverPage;
		public event CalendarPageHandler ClickedPage;
		public event CalendarSnapshotHandler SnappedPage;


		public void SetRange(DateTime startDate, DateTime endDate, CalendarPages pages)
		{
			grid.Rows.Clear();
			var date = startDate;
			while (date <= endDate)
			{
				var daypages = new CalendarPages();
				daypages.AddRange(pages.Where(p =>
					p.Created.Date.Equals(date) || p.Modified.Date.Equals(date)));

				if (daypages.Any())
				{
					var sectionLines = daypages.Select(p => p.Path).Aggregate((a, b) => $"{b}\n{a}");
					var createdLines = daypages.Select(p => p.Created.ToShortFriendlyString()).Aggregate((a, b) => $"{b}\n{a}");
					var modifiedLines = daypages.Select(p => p.Modified.ToShortFriendlyString()).Aggregate((a, b) => $"{b}\n{a}");

					var index = grid.Rows.Add(date.ToString("ddd, MMM d"),
						sectionLines, string.Empty, createdLines, modifiedLines);

					var panel = new DetailPagePanel(daypages);
					grid.Controls.Add(panel);

					panel.Location = grid.GetCellDisplayRectangle(2, index, true).Location;
					panel.Size = grid.GetCellDisplayRectangle(2, index, true).Size;

					grid.Rows[index].Tag = date;
				}
				else
				{
					var index = grid.Rows.Add(date.ToString("ddd, MMM d"),
						string.Empty, string.Empty, string.Empty, string.Empty);

					grid.Rows[index].Tag = date;
				}

				date = date.AddDays(1);
			}
		}
	}
}
