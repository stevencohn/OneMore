//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using OneMoreCalendar.Properties;
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Commands;
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Text.RegularExpressions;
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

		private const int EM_SETCUEBANNER = 0x1501;
		private const int EM_SETMARGINS = 0xD3;
		private const int EC_RIGHTMARGIN = 2;
		private const int MinFilterLength = 3;

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		private DateTime date;
		private CalendarPages pages;
		private int monthDelta;
		private DateTime? pendingDay;
		private Regex filter;
		private bool loading;
		private int monthLabelWidth;
		private Font filterRegularFont;
		private Font filterItalicFont;

		private MonthView monthView;
		private DetailView detailView;
		private YearsForm yearsForm;
		private SettingsForm settingsForm;
		private HelpForm helpForm;


		public CalendarForm(int userMonthDelta)
		{
			InitializeComponent();

			Translator.Localize(this, new[]
			{
				"this",
				"clearLabel",
				"prevButton",
				"nextButton",
				"statusCreatedLabel",
				"statusModifiedLabel"
			});

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

			SendMessage(filterBox.Handle, EM_SETCUEBANNER, (IntPtr)1, Resources.CalendarForm_FilterCue);

			filterRegularFont = filterBox.Font;
			filterItalicFont = new Font(filterRegularFont, FontStyle.Italic);
			UpdateFilterFont();

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

			// widest "<month> yyyy" text; measured once so the filter box stays put as the
			// month text changes width
			foreach (var name in DateTimeFormatInfo.CurrentInfo.MonthNames)
			{
				var width = TextRenderer.MeasureText($"{name} 0000", dateLabel.Font).Width;
				if (width > monthLabelWidth)
				{
					monthLabelWidth = width;
				}
			}

			LayoutFilterBox();
		}


		/// <summary>
		/// Centers the filter box in the space between the month label and the Today button
		/// </summary>
		private void LayoutFilterBox()
		{
			// Resize fires during construction, before ScaleTopPanel has measured anything
			if (monthLabelWidth == 0)
			{
				return;
			}

			var gap = this.Scaled(16);
			var left = dateLabel.Left + monthLabelWidth + gap;
			var right = todayButton.Left - gap;
			var available = right - left;

			var width = Math.Min(this.Scaled(320), available);
			if (width < this.Scaled(100))
			{
				width = this.Scaled(100);
			}

			clearLabel.Width = this.Scaled(22);
			SetFilterMargin();

			filterBox.Width = width;
			filterBox.Location = new System.Drawing.Point(
				left + ((available - width) / 2),
				(topPanel.Height - filterBox.Height) / 2);

			// nestle the clear button inside the box, clear of its border
			var inset = this.Scaled(3);
			clearLabel.Height = filterBox.ClientSize.Height - (inset * 2);
			clearLabel.Location = new System.Drawing.Point(
				filterBox.ClientSize.Width - clearLabel.Width - inset, inset);
		}


		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			base.OnFormClosed(e);
			filterItalicFont?.Dispose();
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


		/// <summary>
		/// Loads the pages for the calendar grid of the current month, showing a busy status
		/// since the first load may need to start OneNote and read all selected notebooks.
		/// </summary>
		private async Task<CalendarPages> LoadPages(SettingsProvider settings)
		{
			statusLabel.Text = Resources.CalendarForm_Loading;
			UseWaitCursor = true;
			loading = true;

			try
			{
				return await new OneNoteProvider().GetPages(
					date.StartOfCalendarMonthView(),
					date.EndOfCalendarView(),
					await settings.GetNotebookIDs(),
					settings.Created, settings.Modified, settings.Deleted);
			}
			finally
			{
				loading = false;
				UseWaitCursor = false;
				statusLabel.Text = BlankStatus;
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
			var settings = SettingsProvider.Current;

			Logger.Current.Debug($"{reason}: loading pages for {date:yyyy-MM} " +
				$"(created:{settings.Created}, modified:{settings.Modified}, deleted:{settings.Deleted})");

			Logger.Current.StartClock();
			pages = await LoadPages(settings);

			Logger.Current.WriteTime($"{reason}: loaded {pages.Count} pages for {date:yyyy-MM}");

			if (monthButton.Checked)
			{
				monthView.SetRange(date, endDate, FilteredPages());
			}
			else
			{
				detailView.SetRange(date, endDate, FilteredPages());
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
			// CheckedChanged fires for both the button being checked and its sibling being
			// unchecked; only respond to the one that became checked, otherwise the stale
			// sibling event rebuilds the previous view on top of the new one
			if (sender is RadioButton radio && !radio.Checked)
			{
				return;
			}

			Logger.Current.Debug($"changed view to {(sender == monthButton ? "month" : "day")}");

			if (sender == monthButton)
			{
				contentPanel.Controls.Clear();
				contentPanel.Controls.Add(monthView);

				monthView.SetRange(date, date.EndOfMonth(), FilteredPages());
			}
			else
			{
				ShowDayView(sender, new CalendarDayEventArgs(pendingDay ?? date));
				pendingDay = null;
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

			// carried through ChangeView to ShowDayView so the day view can scroll to the day
			pendingDay = e.DayDate;
			dayButton.Checked = true;
			pendingDay = null;
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
			var settings = SettingsProvider.Current;
			const string reason = "view: day";

			Logger.Current.Debug($"{reason}: loading pages for {date:yyyy-MM} " +
				$"(created:{settings.Created}, modified:{settings.Modified}, deleted:{settings.Deleted})");

			Logger.Current.StartClock();
			pages = await LoadPages(settings);

			Logger.Current.WriteTime($"{reason}: loaded {pages.Count} pages for {date:yyyy-MM}");

			detailView.SetRange(date, endDate, FilteredPages());

			contentPanel.Controls.Add(detailView);

			// the month's first day means no specific day; leave the list at the top
			if (e.DayDate.Date != date.StartOfMonth().Date)
			{
				detailView.ScrollToDay(e.DayDate);
			}
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
				statusCreatedLabel.Text = string.Format(
					Resources.CalendarForm_Created, e.Page.Created.ToShortFriendlyString());

				statusModifiedLabel.Text = string.Format(
					Resources.CalendarForm_Modified, e.Page.Modified.ToShortFriendlyString());
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

			// Home and Ctrl+Left/Right are text-editing keys while typing a filter
			var typing = filterBox.Focused;

			if (typing && e.KeyCode == Keys.Escape)
			{
				filterBox.Clear();
			}
			else if (e.KeyCode == Keys.PageUp || (!typing && e.Control && e.KeyCode == Keys.Left))
			{
				GotoPrevious(this, e);
			}
			else if (e.KeyCode == Keys.PageDown || (!typing && e.Control && e.KeyCode == Keys.Right))
			{
				if (nextButton.Enabled)
				{
					GotoNext(this, e);
				}
			}
			else if (e.KeyCode == Keys.F5)
			{
				OneNoteProvider.Invalidate();
				await SetMonth(date.Year, "refresh (F5)");
			}
			else if (!typing && e.KeyCode == Keys.Home)
			{
				await SetMonth(0, "today (Home)");
			}
			else if (e.KeyCode == Keys.F1)
			{
				ShowHelp();
			}
		}


		/// <summary>
		/// Handle Ctrl+Tab here, before the dialog-navigation logic sees it. Tab is a dialog
		/// key so, by the time OnKeyDown runs, the first press has already been consumed
		/// moving focus to the next control.
		/// </summary>
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.Tab))
			{
				if (monthButton.Checked)
				{
					dayButton.Checked = true;
				}
				else
				{
					monthButton.Checked = true;
				}

				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
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
				var settings = SettingsProvider.Current;
				Logger.Current.Debug($"settings changed, theme:{settings.Theme}, " +
					$"created:{settings.Created}, modified:{settings.Modified}, " +
					$"deleted:{settings.Deleted}, empty:{settings.Empty}");

				Theme.InitializeTheme(this);
				OneNoteProvider.Invalidate();
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


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Help form...

		private void ShowHelp()
		{
			if (helpForm is not null)
			{
				return;
			}

			Logger.Current.Debug("opened help");

			helpForm = new HelpForm();
			helpForm.FormClosed += ClosedHelp;
			helpForm.Show(this);
		}


		private void ClosedHelp(object sender, FormClosedEventArgs e)
		{
			helpForm.FormClosed -= ClosedHelp;
			helpForm.Dispose();
			helpForm = null;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Page filter...

		/// <summary>
		/// Gets the pages to display: all of them, or only those matching the active filter.
		/// </summary>
		private CalendarPages FilteredPages()
		{
			return filter is null
				? pages
				: new CalendarPages(pages.Where(p => p.Matches(filter)));
		}


		/// <summary>
		/// Keeps typed text from running under the clear button. Must be re-applied whenever
		/// the font changes because setting an edit control's font resets its margins.
		/// </summary>
		private void SetFilterMargin()
		{
			SendMessage(filterBox.Handle, EM_SETMARGINS, (IntPtr)EC_RIGHTMARGIN,
				(IntPtr)((clearLabel.Width + this.Scaled(3)) << 16));
		}


		/// <summary>
		/// The cue banner is drawn in the box's own font, so the box is italic while empty
		/// and regular once the user has typed something.
		/// </summary>
		private void UpdateFilterFont()
		{
			var empty = filterBox.TextLength == 0;
			if (filterBox.Font.Italic != empty)
			{
				filterBox.Font = empty ? filterItalicFont : filterRegularFont;
				SetFilterMargin();
			}
		}


		private void ChangeFilter(object sender, EventArgs e)
		{
			clearLabel.Visible = filterBox.TextLength > 0;
			UpdateFilterFont();

			filterTimer.Stop();
			filterTimer.Start();
		}


		private void ClearFilter(object sender, EventArgs e)
		{
			filterBox.Clear();
			filterBox.Focus();

			// don't wait for the debounce
			filterTimer.Stop();
			ApplyFilter();
		}


		private void HoverClear(object sender, EventArgs e)
		{
			clearLabel.ForeColor = clearLabel.ClientRectangle.Contains(clearLabel.PointToClient(Cursor.Position))
				? Theme.HoverColor
				: Theme.ForeColor;
		}


		private void FilterTick(object sender, EventArgs e)
		{
			filterTimer.Stop();
			ApplyFilter();
		}


		private void FilterKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				filterTimer.Stop();
				ApplyFilter();
				e.SuppressKeyPress = true;
			}
		}


		private void ApplyFilter()
		{
			var text = filterBox.Text.Trim();
			Regex finder = null;

			if (text.Length >= MinFilterLength)
			{
				try
				{
					// same query syntax as OneMore's Title Search
					finder = new TextMatchBuilder(false, false).BuildRegex(text);
				}
				catch (Exception exc)
				{
					// incomplete or unsupported query, e.g. "NOT (a b)"; keep the current filter
					Logger.Current.Debug($"ignoring filter [{text}]: {exc.Message}");
					return;
				}
			}

			filter = finder;

			// if pages are still loading then SetMonth will apply the new filter when done
			if (loading || pages is null)
			{
				return;
			}

			if (monthButton.Checked)
			{
				monthView.SetRange(date, date.EndOfMonth(), FilteredPages());
			}
			else
			{
				detailView?.SetRange(date, date.EndOfMonth(), FilteredPages());
			}

			ShowPageStatus(this, new CalendarPageEventArgs(null));
		}


		private void ResizeTopPanel(object sender, EventArgs e)
		{
			LayoutFilterBox();

			prevButton.Invalidate();
			nextButton.Invalidate();
			todayButton.Invalidate();
			monthButton.Invalidate();
			dayButton.Invalidate();
			settingsButton.Invalidate();
		}
	}
}
