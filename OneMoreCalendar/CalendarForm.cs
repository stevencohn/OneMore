//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using OneMoreCalendar.Properties;
	using River.OneMoreAddIn;
	using System;
	using System.IO;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	/// <summary>
	/// Main OneMoreCalendar form
	/// </summary>
	internal partial class CalendarForm : ThemedForm
	{
		private const int ManualDelta = 1000;

		// a single space rather than string.Empty: an empty ToolStripStatusLabel measures
		// shorter than one with real text, so clearing to "" shrinks statusStrip's auto height,
		// which shrinks contentPanel (Dock=Fill), which shifts hotspot bounds enough to toggle
		// the hover state right back on - an endless resize/repaint feedback loop
		private const string BlankStatus = " ";

		private DateTime date;
		private CalendarPages pages;
		private int monthDelta;

		private MonthView monthView;
		private DetailView detailView;
		private YearsForm yearsForm;
		private SettingsForm settingsForm;


		public CalendarForm(int userMonthDelta)
		{
			InitializeComponent();

			monthDelta = userMonthDelta;
			date = DateTime.Now.StartOfMonth();

			statusLabel.Text = BlankStatus;
			statusCreatedLabel.Text = BlankStatus;
			statusModifiedLabel.Text = BlankStatus;
		}


		protected override async void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// autoscale must be set prior to setting minsize otherwise it isn't applied
			AutoScaleMode = AutoScaleMode.None;

			// DeviceDpi isn't valid until the window handle exists, so size the form here
			// rather than in the constructor, scaling to render at the same physical size
			// regardless of the monitor's DPI
			Width = this.Scaled(1500); // TODO: save as settings?
			Height = this.Scaled(1000);
			MinimumSize = new System.Drawing.Size(this.Scaled(935), this.Scaled(625));

			ScaleTopPanel();

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

			await SetMonth(monthDelta, "startup");

			// when started from OneNote, need to force window to top
			TopMost = true;
			TopMost = false;
		}


		/// <summary>
		/// topPanel and its children were originally authored/tuned by eye directly against
		/// a 150% (144 DPI) display with no DPI-scaling applied at all, so those literal
		/// pixel values only look right at that one DPI. This backs out their 96-DPI base
		/// values and scales them properly, reproducing the current 150%-DPI appearance
		/// exactly while rendering proportionally smaller at 100% DPI instead of oversized.
		/// </summary>
		private void ScaleTopPanel()
		{
			topPanel.Height = this.Scaled(53);

			void PlaceRightAnchored(Control control, int width, int height, int rightMargin, int top)
			{
				control.Size = new System.Drawing.Size(this.Scaled(width), this.Scaled(height));
				control.Location = new System.Drawing.Point(
					topPanel.ClientSize.Width - this.Scaled(rightMargin) - this.Scaled(width),
					this.Scaled(top));
			}

			PlaceRightAnchored(dayButton, 43, 43, 76, 8);
			PlaceRightAnchored(monthButton, 43, 43, 123, 8);
			PlaceRightAnchored(todayButton, 43, 43, 201, 8);
			PlaceRightAnchored(settingsButton, 43, 43, 8, 8);

			nextButton.Size = new System.Drawing.Size(this.Scaled(21), this.Scaled(36));
			nextButton.Location = new System.Drawing.Point(this.Scaled(33), this.Scaled(8));

			prevButton.Size = new System.Drawing.Size(this.Scaled(21), this.Scaled(36));
			prevButton.Location = new System.Drawing.Point(this.Scaled(8), this.Scaled(8));

			dateLabel.Location = new System.Drawing.Point(this.Scaled(59), this.Scaled(8));
		}


		public override void OnThemeChange()
		{
			if (Theme.DarkMode)
			{
				todayButton.Image = Resources.today_32.MapColor(Theme.IconColor);
				monthButton.Image = Resources.month_32.MapColor(Theme.IconColor);
				dayButton.Image = Resources.day_32.MapColor(Theme.IconColor);
				settingsButton.Image = Resources.settings_32.MapColor(Theme.IconColor);
			}
			else
			{
				todayButton.Image = Resources.today_32;
				monthButton.Image = Resources.month_32;
				dayButton.Image = Resources.day_32;
				settingsButton.Image = Resources.settings_32;
			}

			nextButton.PreferredFore = Theme.LinkColor;
			nextButton.PreferredBack = Theme.BackColor;
			prevButton.PreferredFore = Theme.LinkColor;
			prevButton.PreferredBack = Theme.BackColor;
			todayButton.PreferredBack = Theme.BackColor;

			if (contentPanel.Controls.Contains(monthView))
			{
				detailView?.OnThemeChange();
			}
		}


		private async Task SetMonth(int delta, string reason)
		{
			if (delta < ManualDelta)
			{
				date = delta == 0
					? DateTime.Now.StartOfMonth()
					: date.AddMonths(delta);
			}

			if (date.StartOfMonth() > DateTime.Now.StartOfMonth())
			{
				date = DateTime.Now.StartOfMonth();
				return;
			}

			var endDate = date.EndOfMonth();
			var settings = new SettingsProvider();

			Logger.Current.Debug($"{reason}: loading pages for {date:yyyy-MM} " +
				$"(created:{settings.Created}, modified:{settings.Modified}, deleted:{settings.Deleted})");

			Logger.Current.StartClock();
			pages = await new OneNoteProvider().GetPages(
				date.StartOfCalendarMonthView(),
				date.EndOfCalendarView(),
				await settings.GetNotebookIDs(),
				settings.Created, settings.Modified, settings.Deleted);

			Logger.Current.WriteTime($"{reason}: loaded {pages.Count} pages for {date:yyyy-MM}");

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
			Logger.Current.Debug($"changed view to {(sender == monthButton ? "month" : "day")}");

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


		private async void ClickDayView(object sender, CalendarDayEventArgs e)
		{
			Logger.Current.Debug($"clicked day {e.DayDate:yyyy-MM-dd}");

			if (e.DayDate.Month != date.Month)
			{
				SuspendLayout();
				date = e.DayDate.StartOfMonth();
				await SetMonth(ManualDelta, $"day click {e.DayDate:yyyy-MM-dd}");
				ResumeLayout();
			}

			dayButton.Checked = true;
		}


		/// <summary>
		/// Respond to the monthView Day header to show daily details
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void ShowDayView(object sender, CalendarDayEventArgs e)
		{
			Logger.Current.Debug($"showing day view for {e.DayDate:yyyy-MM-dd}");

			contentPanel.Controls.Clear();

			if (detailView is null)
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
			const string reason = "view: day";

			Logger.Current.Debug($"{reason}: loading pages for {date:yyyy-MM} " +
				$"(created:{settings.Created}, modified:{settings.Modified}, deleted:{settings.Deleted})");

			Logger.Current.StartClock();
			pages = await new OneNoteProvider().GetPages(
				date.StartOfCalendarMonthView(),
				date.EndOfCalendarView(),
				await settings.GetNotebookIDs(),
				settings.Created, settings.Modified, settings.Deleted);

			Logger.Current.WriteTime($"{reason}: loaded {pages.Count} pages for {date:yyyy-MM}");

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
			await SetMonth(-1, "previous month");
		}


		/// <summary>
		/// Respond to the next button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void GotoNext(object sender, EventArgs e)
		{
			await SetMonth(1, "next month");
		}


		/// <summary>
		/// Respond to the Today button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void ShowToday(object sender, EventArgs e)
		{
			await SetMonth(0, "today");
		}


		private SnapshotForm snapForm;
		private async void SnappedPage(object sender, CalendarSnapshotEventArgs e)
		{
			Logger.Current.Debug($"previewing page '{e.Page.Title}' ({e.Page.PageID})");

			var path = await new OneNoteProvider().Export(e.Page.PageID);

			Logger.Current.WriteLine($"exported page '{e.Page.Title}' to {path}");

			var location = PointToScreen(e.Bounds.Location);
			location.Offset(50, 70);

			snapForm = new SnapshotForm(e.Page, path)
			{
				Location = location
			};

			snapForm.Deactivate += DeactivateSnap;
			snapForm.Show(this);
		}

		private void DeactivateSnap(object sender, EventArgs e)
		{
			if (snapForm is not null)
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
			if (e.Page is not null)
			{
				statusLabel.Text = $"{e.Page.Path} > {e.Page.Title}";
				statusCreatedLabel.Text = $"Created: {e.Page.Created.ToShortFriendlyString()}";
				statusModifiedLabel.Text = $"Modified: {e.Page.Modified.ToShortFriendlyString()}";
			}
			else
			{
				statusLabel.Text = BlankStatus;
				statusCreatedLabel.Text = BlankStatus;
				statusModifiedLabel.Text = BlankStatus;
			}
		}


		private async void NavigateToPage(object sender, CalendarPageEventArgs e)
		{
			Logger.Current.WriteLine($"navigating to page '{e.Page.Title}' ({e.Page.PageID})");
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
				await SetMonth(date.Year, "refresh (F5)");
			}
			else if (e.KeyCode == Keys.Home)
			{
				await SetMonth(0, "today (Home)");
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
			Logger.Current.Debug("opened years picker");

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
				Logger.Current.Debug($"selected year {yearsForm.Year}");

				date = new DateTime(yearsForm.Year, date.Month, 1);
				if (date.CompareTo(DateTime.Now.Date) > 0)
				{
					date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
				}

				await SetMonth(date.Year, $"year {yearsForm.Year}");
			}

			yearsForm.Dispose();
			yearsForm = null;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Settings form...

		private void ToggleSettings(object sender, EventArgs e)
		{
			Logger.Current.Debug(settingsButton.Checked ? "opened settings" : "closed settings");

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
				var settings = new SettingsProvider();
				Logger.Current.Debug($"settings changed, theme:{settings.Theme}, " +
					$"created:{settings.Created}, modified:{settings.Modified}, " +
					$"deleted:{settings.Deleted}, empty:{settings.Empty}");

				Theme.InitializeTheme(this);
				await SetMonth(date.Year, "settings applied");
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
