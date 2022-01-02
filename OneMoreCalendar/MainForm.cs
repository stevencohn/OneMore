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
		private readonly MonthView monthView;
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

			monthView = new MonthView()
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
		}


		protected override async void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			await SetMonthView(0);
		}


		private void ChangeView(object sender, EventArgs e)
		{
			if (sender == monthButton)
			{
			}
			else
			{
			}
		}


		private async void ShowToday(object sender, EventArgs e)
		{
			await SetMonthView(0);
		}



		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Month view...

		private void ShowPageStatus(object sender, CalendarPageEventArgs e)
		{
			if (e.Item != null)
			{
				statusLabel.Text = $"{e.Item.Path} > {e.Item.Title}";
				statusCreatedLabel.Text = $"Created: {e.Item.Created.ToShortFriendlyString()}";
				statusModifiedLabel.Text = $"Modified: {e.Item.Modified.ToShortFriendlyString()}";
			}
			else
			{
				statusLabel.Text = string.Empty;
				statusCreatedLabel.Text = string.Empty;
				statusModifiedLabel.Text = string.Empty;
			}
		}


		private async void GotoPrevious(object sender, EventArgs e)
		{
			await SetMonthView(-1);
		}

		private async void GotoNext(object sender, EventArgs e)
		{
			await SetMonthView(1);
		}

		private async Task SetMonthView(int delta)
		{
			var startDate = delta == 0 
				? DateTime.Now.StartOfMonth() 
				: monthView.StartDate.AddMonths(delta);

			var endDate = startDate.EndOfMonth();
			var settings = new SettingsProvider();

			var pages = await new OneNoteProvider().GetPages(
				startDate.StartOfCalendarMonthView(),
				endDate.EndOfCalendarView(),
				await settings.GetNotebookIDs(),
				settings.ShowCreated, settings.ShowModified, false);

			monthView.SetRange(startDate, endDate, pages);

			dateLabel.Text = startDate.ToString("MMMM yyyy");

			nextButton.Enabled = todayButton.Enabled = !DateTime.Now.EqualsMonth(startDate);
		}


		private void ShowDayView(object sender, CalendarDayEventArgs e)
		{
			MessageBox.Show($"clicked {e.DayDate}");
		}


		private async void NavigateToPage(object sender, CalendarPageEventArgs e)
		{
			await new OneNoteProvider().NavigateTo(e.Item.PageID);
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
				await SetMonthView(0);
			}
		}


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
