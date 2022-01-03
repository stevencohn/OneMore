//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using System;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	public partial class MainForm : Form
	{
		private DateTime date;
		private CalendarPages pages;

		private MonthView monthView;
		private DayView dayView;
		private SettingsForm settingsForm;
		private FormWindowState? winstate = null;


		public MainForm()
		{
			InitializeComponent();

			statusLabel.Text = string.Empty;
			statusCreatedLabel.Text = string.Empty;
			statusModifiedLabel.Text = string.Empty;

			Width = 1500;
			Height = 1000;
		}


		protected override async void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			monthView = new MonthView
			{
				Dock = DockStyle.Fill,
				Location = new System.Drawing.Point(0, 0),
				Name = "monthView",
				TabIndex = 0
			};

			monthView.ClickedPage += NavigateToPage;
			monthView.ClickedDay += ShowDayView;
			monthView.HoverPage += ShowPageStatus;

			contentPanel.Controls.Add(monthView);

			await SetMonth(0);
		}


		private async Task SetMonth(int delta)
		{
			date = delta == 0
				? DateTime.Now.StartOfMonth()
				: date.AddMonths(delta);

			var endDate = date.EndOfMonth();

			DateTime viewStart, viewEnd;
			if (dayButton.Checked)
			{
				viewStart = date;
				viewEnd = date.EndOfMonth();
			}
			else
			{
				viewStart = date.StartOfCalendarMonthView();
				viewEnd = date.EndOfCalendarView();
			}

			var settings = new SettingsProvider();

			pages = await new OneNoteProvider().GetPages(
				viewStart,
				viewEnd,
				await settings.GetNotebookIDs(),
				settings.Created, settings.Modified, false);

			if (monthButton.Checked)
			{
				monthView.SetRange(date, endDate, pages);
			}
			else
			{
				dayView.SetRange(date, endDate, pages);
			}

			dateLabel.Text = date.ToString("MMMM yyyy");

			nextButton.Enabled = todayButton.Enabled = !DateTime.Now.EqualsMonth(date);
		}


		/// <summary>
		/// Respond to the day/month view buttons
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ChangeView(object sender, EventArgs e)
		{
			if (sender == monthButton)
			{
				contentPanel.Controls.Remove(dayView);
				contentPanel.Controls.Add(monthView);

				monthView.SetRange(date.StartOfCalendarMonthView(), date.EndOfCalendarView(), pages);
			}
			else
			{
				ShowDayView(sender, new CalendarDayEventArgs(date));
			}
		}


		/// <summary>
		/// Respond to the monthView Day header to show daily details
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void ShowDayView(object sender, CalendarDayEventArgs e)
		{
			contentPanel.Controls.Remove(monthView);

			if (dayView == null)
			{
				dayView = new DayView
				{
					Dock = DockStyle.Fill,
					Location = new System.Drawing.Point(0, 0),
					Name = "dayView",
					TabIndex = 0
				};

				dayView.HoverPage += ShowPageStatus;
			}

			var endDate = date.EndOfMonth();
			var settings = new SettingsProvider();

			pages = await new OneNoteProvider().GetPages(
				date, endDate,
				await settings.GetNotebookIDs(),
				settings.Created, settings.Modified, false);

			dayView.SetRange(date, endDate, pages);
			contentPanel.Controls.Add(dayView);
		}


		/// <summary>
		/// Respond to the previous button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void GotoPrevious(object sender, EventArgs e)
		{
			await SetMonth(-1);
		}


		/// <summary>
		/// Respond to the next button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void GotoNext(object sender, EventArgs e)
		{
			await SetMonth(1);
		}


		/// <summary>
		/// Respond to the Today button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void ShowToday(object sender, EventArgs e)
		{
			await SetMonth(0);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Month view...

		private void ShowPageStatus(object sender, CalendarPageEventArgs e)
		{
			if (e.Page != null)
			{
				statusLabel.Text = $"{e.Page.Path} > {e.Page.Title}";
				statusCreatedLabel.Text = $"Created: {e.Page.Created.ToShortFriendlyString()}";
				statusModifiedLabel.Text = $"Modified: {e.Page.Modified.ToShortFriendlyString()}";
			}
			else
			{
				statusLabel.Text = string.Empty;
				statusCreatedLabel.Text = string.Empty;
				statusModifiedLabel.Text = string.Empty;
			}
		}


		private async void NavigateToPage(object sender, CalendarPageEventArgs e)
		{
			await new OneNoteProvider().NavigateTo(e.Page.PageID);
		}


		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (monthButton.Checked)
			{
				if (e.KeyCode == Keys.PageUp)
				{
					GotoPrevious(this, e);
				}
				else if (e.KeyCode == Keys.PageDown)
				{
					if (nextButton.Enabled)
					{
						GotoNext(this, e);
					}
				}
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Dealing with the settings form...

		private void ToggleSettings(object sender, EventArgs e)
		{
			if (settingsButton.Checked)
			{
				settingsForm = new SettingsForm();
				var location = PointToScreen(settingsButton.Location);
				location.Offset(-(settingsForm.Width - settingsButton.Width), settingsButton.Height);

				settingsForm.Location = location;
				settingsForm.FormClosing += ClosingSettings;
				settingsForm.FormClosed += ClosedSettings;
				settingsForm.Show(this);
			}
			else
			{
				settingsForm.FormClosing -= ClosingSettings;
				settingsForm.FormClosed -= ClosedSettings;
				settingsForm.Close();
			}
		}

		private void ClosedSettings(object sender, FormClosedEventArgs e)
		{
			settingsForm.FormClosed -= ClosedSettings;
			settingsForm.FormClosing -= ClosingSettings;
			settingsForm.Dispose();
			settingsForm = null;
		}

		private async void ClosingSettings(object sender, FormClosingEventArgs e)
		{
			settingsButton.Checked = false;

			if (settingsForm.DialogResult == DialogResult.OK)
			{
				await SetMonth(0);
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Basic window management...

		protected override void OnMove(EventArgs e)
		{
			base.OnMove(e);
			if (settingsForm?.Visible == true)
			{
				var location = PointToScreen(settingsButton.Location);
				location.Offset(-(settingsForm.Width - settingsButton.Width), settingsButton.Height);
				settingsForm.Location = location;
			}
		}


		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			if (WindowState == winstate)
			{
				if (settingsForm?.Visible == true)
				{
					var location = PointToScreen(settingsButton.Location);
					location.Offset(-(settingsForm.Width - settingsButton.Width), settingsButton.Height);
					settingsForm.Location = location;
				}

				winstate = WindowState;
			}
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			if (settingsForm?.Visible == true)
			{
				var location = PointToScreen(settingsButton.Location);
				location.Offset(-(settingsForm.Width - settingsButton.Width), settingsButton.Height);
				settingsForm.Location = location;
			}
		}
	}
}
