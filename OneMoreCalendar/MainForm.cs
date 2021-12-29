//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using System;
	using System.Windows.Forms;


	public partial class MainForm : Form
	{
		private readonly MonthView monthView;


		public MainForm()
		{
			InitializeComponent();

			Width = 1500;
			Height = 1000;

			var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			var endDate = new DateTime(now.Year, now.Month,
				DateTime.DaysInMonth(now.Year, now.Month)).Date;

			var pages = new OneNoteProvider().GetPages(now, endDate);

			monthView = new MonthView(now, pages)
			{
				BackColor = System.Drawing.Color.White,
				Dock = DockStyle.Fill,
				Location = new System.Drawing.Point(0, 0),
				Margin = new Padding(0),
				Name = "monthView",
				Size = new System.Drawing.Size(978, 506),
				TabIndex = 0
			};

			monthView.ClickedPage += MonthView_ClickedPage;
			monthView.ClickedDay += MonthView_ClickedDay;

			contentPanel.Controls.Add(monthView);
		}

		private void MonthView_ClickedDay(object sender, CalendarDayEventArgs e)
		{
			MessageBox.Show($"clicked {e.DayDate}");
		}

		private async void MonthView_ClickedPage(object sender, CalendarPageEventArgs e)
		{
			using (var one = new OneNote())
			{
				var url = one.GetHyperlink(e.PageID, string.Empty);
				if (url != null)
				{
					await one.NavigateTo(url);
				}
			}
		}

		private void GotoPrevious(object sender, EventArgs e)
		{
			var startDate = monthView.Date.AddMonths(-1);
			var endDate = new DateTime(startDate.Year, startDate.Month,
				DateTime.DaysInMonth(startDate.Year, startDate.Month)).Date;

			monthView.SetRange(startDate, endDate, new OneNoteProvider().GetPages(startDate, endDate));

			dateLabel.Text = startDate.ToString("MMMM yyyy");

			nextButton.Enabled = true;
		}

		private void GotoNext(object sender, EventArgs e)
		{
			var startDate = monthView.Date.AddMonths(1);
			var endDate = new DateTime(startDate.Year, startDate.Month,
				DateTime.DaysInMonth(startDate.Year, startDate.Month)).Date;

			monthView.SetRange(startDate, endDate, new OneNoteProvider().GetPages(startDate, endDate));

			dateLabel.Text = startDate.ToString("MMMM yyyy");

			var now = DateTime.Now;
			if (startDate.Year == now.Year && startDate.Month == now.Month)
			{
				nextButton.Enabled = false;
			}
		}


		protected override bool IsInputKey(Keys keyData)
		{
			switch (keyData)
			{
				case Keys.Right:
				case Keys.Left:
				case Keys.Up:
				case Keys.Down:
					return true;
			}
			return base.IsInputKey(keyData);
		}
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.KeyCode == Keys.Left || e.KeyCode == Keys.PageUp)
			{
				GotoPrevious(this, e);
			}
			else if (e.KeyCode == Keys.Right || e.KeyCode == Keys.PageDown)
			{
				if (nextButton.Enabled)
				{
					GotoNext(this, e);
				}
			}
		}
	}
}
