//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using System;
	using System.IO;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	/// <summary>
	/// Main OneMoreCalendar form
	/// </summary>
	public partial class CalendarForm : Form
	{
		private DateTime date;
		private CalendarPages pages;

		private MonthView monthView;
		private DetailView detailView;
		private YearsForm yearsForm;
		private SettingsForm settingsForm;


		public CalendarForm()
		{
			InitializeComponent();

			statusLabel.Text = string.Empty;
			statusCreatedLabel.Text = string.Empty;
			statusModifiedLabel.Text = string.Empty;

			Width = 1500; // TODO: save as settings?
			Height = 1000;

			// TODO: beta
			Text = $"{Text} (BETA)";
		}


		protected override async void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// autoscale must be set prior to setting minsize otherwise it isn't applied
			AutoScaleMode = AutoScaleMode.Font;
			MinimumSize = new System.Drawing.Size(935, 625);

			monthView = new MonthView
			{
				Dock = DockStyle.Fill,
				Location = new System.Drawing.Point(0, 0),
				Name = "monthView",
				TabIndex = 0
			};

			monthView.ClickedPage += NavigateToPage;
			monthView.ClickedDay += ClickDayView;
			monthView.HoverPage += ShowPageStatus;
			monthView.SnappedPage += SnappedPage;

			contentPanel.Controls.Add(monthView);

			await SetMonth(0);

			// when started from OneNote, need to force window to top
			TopMost = true;
			TopMost = false;
		}


		private async Task SetMonth(int delta)
		{
			if (delta < 1000)
			{
				date = delta == 0
					? DateTime.Now.StartOfMonth()
					: date.AddMonths(delta);
			}

			var endDate = date.EndOfMonth();
			var settings = new SettingsProvider();

			pages = await new OneNoteProvider().GetPages(
				date.StartOfCalendarMonthView(),
				date.EndOfCalendarView(),
				await settings.GetNotebookIDs(),
				settings.Created, settings.Modified, settings.Deleted);

			if (monthButton.Checked)
			{
				monthView.SetRange(date, endDate, pages);
			}
			else
			{
				detailView.SetRange(date, endDate, pages);
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
				contentPanel.Controls.Clear();
				contentPanel.Controls.Add(monthView);

				monthView.SetRange(date, date.EndOfMonth(), pages);
			}
			else
			{
				ShowDayView(sender, new CalendarDayEventArgs(date));
			}
		}


		private void ClickDayView(object sender, CalendarDayEventArgs e)
		{
			dayButton.Checked = true;
		}


		/// <summary>
		/// Respond to the monthView Day header to show daily details
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void ShowDayView(object sender, CalendarDayEventArgs e)
		{
			contentPanel.Controls.Clear();

			if (detailView == null)
			{
				detailView = new DetailView
				{
					Dock = DockStyle.Fill,
					Location = new System.Drawing.Point(0, 0),
					Name = "dayView",
					TabIndex = 0
				};

				detailView.HoverPage += ShowPageStatus;
				detailView.ClickedPage += NavigateToPage;
				detailView.SnappedPage += SnappedPage;
			}

			var endDate = date.EndOfMonth();
			var settings = new SettingsProvider();

			pages = await new OneNoteProvider().GetPages(
				date.StartOfCalendarMonthView(),
				date.EndOfCalendarView(),
				await settings.GetNotebookIDs(),
				settings.Created, settings.Modified, settings.Deleted);

			detailView.SetRange(date, endDate, pages);
			contentPanel.Controls.Add(detailView);
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


		private SnapshotForm snapForm;
		private void SnappedPage(object sender, CalendarSnapshotEventArgs e)
		{
			var path = new OneNoteProvider().Export(e.Page.PageID);
			Logger.Current.WriteLine($"exported page '{e.Page.Title}' to {path}");

			var location = PointToScreen(e.Bounds.Location);
			location.Offset(50, 70);

			snapForm = new SnapshotForm(e.Page, path);
			snapForm.Location = location;
			snapForm.Deactivate += DeactivateSnap;
			snapForm.Show(this);
		}

		private void DeactivateSnap(object sender, EventArgs e)
		{
			if (snapForm != null)
			{
				var path = snapForm.Path;
				snapForm.Dispose();
				snapForm = null;

				if (File.Exists(path))
				{
					try
					{
						File.Delete(path);
					}
					catch (Exception exc)
					{
						Logger.Current.WriteLine("error deleting temp metafile", exc);
					}
				}
			}
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


		protected override async void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.KeyCode == Keys.PageUp || (e.Control && e.KeyCode == Keys.Right))
			{
				GotoPrevious(this, e);
			}
			else if (e.KeyCode == Keys.PageDown || (e.Control && e.KeyCode == Keys.Left))
			{
				if (nextButton.Enabled)
				{
					GotoNext(this, e);
				}
			}
			else if (e.KeyCode == Keys.F5)
			{
				await SetMonth(date.Year);
			}
			else if (e.KeyCode == Keys.Home)
			{
				await SetMonth(0);
			}
			else if (e.Control && (e.KeyCode == Keys.Tab))
			{
				if (monthButton.Checked)
				{
					dayButton.Checked = true;
					monthButton.Checked = false;
				}
				else
				{
					monthButton.Checked = true;
					dayButton.Checked = false;
				}
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Years form...

		private void DropDownYears(object sender, LinkLabelLinkClickedEventArgs e)
		{
			yearsForm = new YearsForm(date.Year);
			var location = PointToScreen(dateLabel.Location);
			location.Offset(0, dateLabel.Height);

			yearsForm.Location = location;
			yearsForm.Deactivate += DeactivateYears;
			yearsForm.Show(this);
		}

		private async void DeactivateYears(object sender, EventArgs e)
		{
			TopMost = false;
			TopMost = true;
			TopMost = false;

			if (yearsForm.Year > 0)
			{
				date = new DateTime(yearsForm.Year, date.Month, 1);
				if (date.CompareTo(DateTime.Now.Date) > 0)
				{
					date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
				}

				await SetMonth(date.Year);
			}

			yearsForm.Dispose();
			yearsForm = null;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Settings form...

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
				settingsForm.Deactivate += DeactivateSettings;
				settingsForm.Show(this);
			}
			else
			{
				settingsForm.FormClosing -= ClosingSettings;
				settingsForm.FormClosed -= ClosedSettings;
				settingsForm.Deactivate -= DeactivateSettings;
				settingsForm.Close();
			}
		}


		private async void ClosingSettings(object sender, FormClosingEventArgs e)
		{
			settingsButton.Checked = false;

			if (settingsForm.DialogResult == DialogResult.OK)
			{
				await SetMonth(date.Year);
			}
		}


		private void ClosedSettings(object sender, FormClosedEventArgs e)
		{
			settingsForm.FormClosed -= ClosedSettings;
			settingsForm.FormClosing -= ClosingSettings;
			settingsForm.Dispose();
			settingsForm = null;
		}


		private void DeactivateSettings(object sender, EventArgs e)
		{
			if (!settingsForm.Busy)
			{
				settingsButton.Checked = false;
				ClosedSettings(sender, null);
			}
		}

		private void ResizeTopPanel(object sender, EventArgs e)
		{
			prevButton.Invalidate();
			nextButton.Invalidate();
			todayButton.Invalidate();
			monthButton.Invalidate();
			dayButton.Invalidate();
			settingsButton.Invalidate();
		}
	}
}
