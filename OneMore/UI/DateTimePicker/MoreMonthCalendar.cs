//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S3881 // "IDisposable" should be implemented correctly

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Globalization;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// An owner-drawn replacement for MonthCalendar that follows the OneMore theme. Supports
	/// day, month, year and decade views, drilling in and out by clicking the title.
	/// </summary>
	[DefaultEvent(nameof(DateChanged))]
	[DefaultProperty(nameof(SelectionStart))]
	internal class MoreMonthCalendar : Control, IThemedControl, ILoadControl
	{
		private enum Hit
		{
			None,
			Title,
			Previous,
			Next,
			Cell,
			Today
		}

		private struct CellMetrics
		{
			public Rectangle Title;
			public Rectangle Previous;
			public Rectangle Next;
			public Rectangle Header;
			public Rectangle Grid;
			public Rectangle Today;
			public int CellWidth;
			public int CellHeight;
			public int WeekWidth;
		}

		private const int Radius = 3;

		private readonly ThemeManager manager;

		private CalendarView view = CalendarView.Days;
		private DateTime displayMonth;
		private DateTime focusDate;
		private DateTime selectionStart;
		private DateTime selectionEnd;
		private DateTime anchor;
		private DateTime minDate = new DateTime(1753, 1, 1);
		private DateTime maxDate = new DateTime(9998, 12, 31);
		private DateTime todayDate;
		private bool todayDateSet;
		private DayOfWeek? firstDayOfWeek;
		private int maxSelectionCount = 7;
		private bool showToday = true;
		private bool showTodayCircle = true;
		private bool showWeekNumbers;
		private Hit hotHit;
		private int hotRow = -1;
		private int hotCol = -1;
		private Hit pressedHit;
		private string todayText;


		public MoreMonthCalendar()
		{
			SetStyle(
				ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
				ControlStyles.Selectable | ControlStyles.StandardClick |
				ControlStyles.UserMouse,
				true);

			manager = ThemeManager.Instance;

			var today = DateTime.Today;
			selectionStart = selectionEnd = focusDate = anchor = today;
			displayMonth = new DateTime(today.Year, today.Month, 1);

			TabStop = true;
			ClientSize = GetPreferredSize(Size.Empty);
		}


		#region Events
		[Category("Action"), Description("Raised when the selected date or range changes")]
		public event DateRangeEventHandler DateChanged;

		[Category("Action"), Description("Raised when the user explicitly picks a date")]
		public event DateRangeEventHandler DateSelected;

		[Category("More"), Description("Raised when the displayed view or month changes")]
		public event EventHandler DisplayChanged;
		#endregion Events


		#region Properties
		public string ThemedBack { get; set; }


		public string ThemedFore { get; set; }


		[Category("Behavior"), Description("The text shown before today's date in the footer")]
		public string TodayText
		{
			get => todayText ?? Resx.word_Today;
			set
			{
				todayText = value;
				Invalidate();
			}
		}


		private bool ShouldSerializeTodayText() => todayText is not null;


		private void ResetTodayText() => todayText = null;


		/// <summary>
		/// Gets or sets the background of the day grid; empty uses the theme.
		/// </summary>
		[Category("Appearance")]
		public Color MonthBackColor { get; set; } = Color.Empty;


		/// <summary>
		/// Gets or sets the text color of the day grid; empty uses the theme.
		/// </summary>
		[Category("Appearance")]
		public Color MonthForeColor { get; set; } = Color.Empty;


		/// <summary>
		/// Gets or sets the background of the title bar; empty uses the theme.
		/// </summary>
		[Category("Appearance")]
		public Color TitleBackColor { get; set; } = Color.Empty;


		/// <summary>
		/// Gets or sets the text color of the title bar; empty uses the theme.
		/// </summary>
		[Category("Appearance")]
		public Color TitleForeColor { get; set; } = Color.Empty;


		/// <summary>
		/// Gets or sets the text color of days outside the displayed month; empty uses
		/// the theme.
		/// </summary>
		[Category("Appearance")]
		public Color TrailingForeColor { get; set; } = Color.Empty;


		[Category("Behavior")]
		public DateTime MinDate
		{
			get => minDate;
			set
			{
				if (value > maxDate)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				minDate = value.Date;
				SetSelection(selectionStart, selectionEnd, false);
				Invalidate();
			}
		}


		[Category("Behavior")]
		public DateTime MaxDate
		{
			get => maxDate;
			set
			{
				if (value < minDate)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				maxDate = value.Date;
				SetSelection(selectionStart, selectionEnd, false);
				Invalidate();
			}
		}


		[Category("Behavior")]
		public DateTime SelectionStart
		{
			get => selectionStart;
			set
			{
				SetSelection(value, value > selectionEnd ? value : selectionEnd, true);
				anchor = selectionStart;
			}
		}


		[Category("Behavior")]
		public DateTime SelectionEnd
		{
			get => selectionEnd;
			set
			{
				SetSelection(value < selectionStart ? value : selectionStart, value, true);
				anchor = selectionStart;
			}
		}


		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public SelectionRange SelectionRange
		{
			get => new SelectionRange(selectionStart, selectionEnd);
			set
			{
				SetSelection(value.Start, value.End, true);
				anchor = selectionStart;
			}
		}


		[Category("Behavior"), DefaultValue(7)]
		public int MaxSelectionCount
		{
			get => maxSelectionCount;
			set
			{
				if (value < 1)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				maxSelectionCount = value;
			}
		}


		[Category("Behavior")]
		public DateTime TodayDate
		{
			get => todayDateSet ? todayDate : DateTime.Today;
			set
			{
				todayDate = value.Date;
				todayDateSet = true;
				Invalidate();
			}
		}


		[Category("Behavior"), DefaultValue(true)]
		public bool ShowToday
		{
			get => showToday;
			set
			{
				showToday = value;
				ClientSize = GetPreferredSize(Size.Empty);
				Invalidate();
			}
		}


		[Category("Behavior"), DefaultValue(true)]
		public bool ShowTodayCircle
		{
			get => showTodayCircle;
			set
			{
				showTodayCircle = value;
				Invalidate();
			}
		}


		[Category("Behavior"), DefaultValue(false)]
		public bool ShowWeekNumbers
		{
			get => showWeekNumbers;
			set
			{
				showWeekNumbers = value;
				ClientSize = GetPreferredSize(Size.Empty);
				Invalidate();
			}
		}


		[Category("Behavior")]
		public DayOfWeek FirstDayOfWeek
		{
			get => firstDayOfWeek ?? Culture.DateTimeFormat.FirstDayOfWeek;
			set
			{
				firstDayOfWeek = value;
				Invalidate();
			}
		}


		private bool ShouldSerializeFirstDayOfWeek() => firstDayOfWeek.HasValue;


		private void ResetFirstDayOfWeek() => firstDayOfWeek = null;


		/// <summary>
		/// Gets the currently displayed granularity.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public CalendarView View
		{
			get => view;
			set
			{
				if (view != value)
				{
					view = value;
					OnDisplayChanged();
				}
			}
		}


		/// <summary>
		/// Gets the date that has keyboard focus; arrow keys move this.
		/// </summary>
		[Browsable(false)]
		public DateTime FocusDate => focusDate;


		protected override Size DefaultSize => new Size(200, 200);


		private static CultureInfo Culture => AddIn.Locale;
		#endregion Properties


		#region Theme
		void ILoadControl.OnLoad()
		{
			ApplyTheme(manager);
		}


		public void ApplyTheme(ThemeManager themeManager)
		{
			if (ThemeManager.IsDesignTime)
			{
				return;
			}

			BackColor = themeManager.GetColor("Window", ThemedBack);
			ForeColor = Enabled
				? themeManager.GetColor("WindowText", ThemedFore)
				: themeManager.GetColor("GrayText");

			Invalidate();
		}


		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			ApplyTheme(manager);
		}


		private Color Back => !MonthBackColor.IsEmpty
			? MonthBackColor
			: Enabled ? manager.GetColor("Window", ThemedBack) : manager.GetColor("InactiveWindow");

		private Color Fore => !MonthForeColor.IsEmpty
			? MonthForeColor
			: Enabled ? manager.GetColor("WindowText", ThemedFore) : manager.GetColor("GrayText");
		private Color Gray => manager.GetColor("GrayText");
		private Color TitleBack => TitleBackColor.IsEmpty ? manager.GetColor("Control") : TitleBackColor;
		private Color TitleFore => TitleForeColor.IsEmpty ? manager.GetColor("ControlText") : TitleForeColor;
		private Color Trailing => TrailingForeColor.IsEmpty ? Gray : TrailingForeColor;
		#endregion Theme


		#region Metrics
		private int Scale(int value)
		{
			return (int)Math.Round(value * DeviceDpi / 96f);
		}


		private static Size Measure(string text, Font font)
		{
			return TextRenderer.MeasureText(text, font, Size.Empty,
				TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
		}


		public override Size GetPreferredSize(Size proposedSize)
		{
			var info = Culture.DateTimeFormat;

			var cell = Measure("88", Font).Width;
			foreach (var name in info.ShortestDayNames)
			{
				cell = Math.Max(cell, Measure(name, Font).Width);
			}

			var cellW = cell + Scale(12);
			var cellH = Font.Height + Scale(8);
			var pad = Scale(4);
			var titleH = Font.Height + Scale(12);
			var cols = showWeekNumbers ? 8 : 7;

			var width = (cols * cellW) + (2 * pad);
			var height = pad + titleH + (7 * cellH) + pad;
			if (showToday)
			{
				height += cellH + pad;
			}

			return new Size(width, height);
		}


		private CellMetrics GetLayout()
		{
			var pad = Scale(4);
			var cellH = Font.Height + Scale(8);
			var titleH = Font.Height + Scale(12);

			var layout = new CellMetrics();
			var width = ClientSize.Width - (2 * pad);

			layout.Title = new Rectangle(0, 0, ClientSize.Width, titleH + pad);

			var arrow = titleH;
			layout.Previous = new Rectangle(pad, pad / 2, arrow, titleH);
			layout.Next = new Rectangle(ClientSize.Width - pad - arrow, pad / 2, arrow, titleH);

			var top = layout.Title.Bottom + pad;

			if (view == CalendarView.Days)
			{
				var cols = showWeekNumbers ? 8 : 7;
				var cellW = width / cols;

				layout.CellWidth = cellW;
				layout.CellHeight = cellH;
				layout.WeekWidth = showWeekNumbers ? cellW : 0;

				layout.Header = new Rectangle(pad, top, cols * cellW, cellH);
				layout.Grid = new Rectangle(
					pad + layout.WeekWidth, top + cellH, 7 * cellW, 6 * cellH);
			}
			else
			{
				// 3 columns by 4 rows spanning the same area as header + 6 weeks
				layout.CellWidth = width / 3;
				layout.CellHeight = (7 * cellH) / 4;
				layout.Header = Rectangle.Empty;
				layout.Grid = new Rectangle(
					pad, top, 3 * layout.CellWidth, 4 * layout.CellHeight);
			}

			layout.Today = showToday
				? new Rectangle(pad, top + (7 * cellH) + pad, width, cellH)
				: Rectangle.Empty;

			return layout;
		}
		#endregion Metrics


		#region Navigation and selection
		private void OnDisplayChanged()
		{
			Invalidate();
			DisplayChanged?.Invoke(this, EventArgs.Empty);
		}


		private DateTime ClampToRange(DateTime date)
		{
			date = date.Date;
			return date < minDate ? minDate : date > maxDate ? maxDate : date;
		}


		private void SetSelection(DateTime start, DateTime end, bool notify)
		{
			start = ClampToRange(start);
			end = ClampToRange(end);

			if (end < start)
			{
				end = start;
			}

			if ((end - start).Days + 1 > maxSelectionCount)
			{
				end = start.AddDays(maxSelectionCount - 1);
			}

			var changed = start != selectionStart || end != selectionEnd;
			selectionStart = start;
			selectionEnd = end;

			if (changed)
			{
				Invalidate();

				if (notify)
				{
					DateChanged?.Invoke(this, new DateRangeEventArgs(start, end));
				}
			}
		}


		/// <summary>
		/// Moves the display to show the month of the given date and resets to the days view.
		/// </summary>
		public void SetDisplayDate(DateTime date)
		{
			date = ClampToRange(date);
			focusDate = date;
			displayMonth = new DateTime(date.Year, date.Month, 1);
			view = CalendarView.Days;
			OnDisplayChanged();
		}


		/// <summary>
		/// Sets the selection to the single date and displays its month without raising
		/// DateChanged; used to seed the calendar before showing it.
		/// </summary>
		public void SeedDate(DateTime date)
		{
			date = ClampToRange(date);
			selectionStart = selectionEnd = anchor = date;
			SetDisplayDate(date);
		}


		private void MoveFocus(DateTime target, bool extend)
		{
			target = ClampToRange(target);
			focusDate = target;
			displayMonth = new DateTime(target.Year, target.Month, 1);

			if (extend)
			{
				SetSelection(
					target < anchor ? target : anchor,
					target < anchor ? anchor : target,
					true);
			}
			else
			{
				anchor = target;
				SetSelection(target, target, true);
			}

			Invalidate();
		}


		private void Page(int direction)
		{
			switch (view)
			{
				case CalendarView.Days:
					ShiftMonths(direction);
					break;

				case CalendarView.Months:
					ShiftMonths(direction * 12);
					break;

				case CalendarView.Years:
					ShiftMonths(direction * 12 * 10);
					break;

				case CalendarView.Decades:
					ShiftMonths(direction * 12 * 100);
					break;
			}
		}


		private void ShiftMonths(int months)
		{
			// month index counted from year 0 so that arithmetic and clamping are simple
			var index = ((long)displayMonth.Year * 12) + (displayMonth.Month - 1) + months;
			var lo = ((long)minDate.Year * 12) + (minDate.Month - 1);
			var hi = ((long)maxDate.Year * 12) + (maxDate.Month - 1);
			index = Math.Max(lo, Math.Min(hi, index));

			var year = (int)(index / 12);
			var month = (int)(index % 12) + 1;

			if (year == displayMonth.Year && month == displayMonth.Month)
			{
				return;
			}

			displayMonth = new DateTime(year, month, 1);
			var day = Math.Min(focusDate.Day, DateTime.DaysInMonth(year, month));
			focusDate = ClampToRange(new DateTime(year, month, day));
			OnDisplayChanged();
		}


		private void DrillOut()
		{
			if (view != CalendarView.Decades)
			{
				view = (CalendarView)((int)view + 1);
				OnDisplayChanged();
			}
		}


		private void DrillIn(int row, int column)
		{
			var index = (row * 3) + column;

			if (view == CalendarView.Decades)
			{
				var start = CalendarLayout.DecadePageStart(displayMonth.Year) + (index * 10);
				if (CalendarLayout.IsDecadeInRange(start, minDate, maxDate))
				{
					SetDisplayYear(Math.Max(1, start));
					view = CalendarView.Years;
					OnDisplayChanged();
				}
			}
			else if (view == CalendarView.Years)
			{
				var year = CalendarLayout.YearPageStart(displayMonth.Year) + index;
				if (CalendarLayout.IsYearInRange(year, minDate, maxDate))
				{
					SetDisplayYear(year);
					view = CalendarView.Months;
					OnDisplayChanged();
				}
			}
			else if (view == CalendarView.Months)
			{
				var month = index + 1;
				if (CalendarLayout.IsMonthInRange(displayMonth.Year, month, minDate, maxDate))
				{
					displayMonth = new DateTime(displayMonth.Year, month, 1);
					var day = Math.Min(focusDate.Day,
						DateTime.DaysInMonth(displayMonth.Year, month));
					focusDate = ClampToRange(new DateTime(displayMonth.Year, month, day));
					view = CalendarView.Days;
					OnDisplayChanged();
				}
			}
		}


		private void SetDisplayYear(int year)
		{
			year = Math.Max(1, Math.Min(9999, year));
			var day = Math.Min(focusDate.Day, DateTime.DaysInMonth(year, displayMonth.Month));
			displayMonth = new DateTime(year, displayMonth.Month, 1);
			focusDate = ClampToRange(new DateTime(year, displayMonth.Month, day));
		}


		private void SelectDate(DateTime date, bool extend)
		{
			if (!CalendarLayout.IsInRange(date, minDate, maxDate))
			{
				return;
			}

			MoveFocus(date, extend);

			DateSelected?.Invoke(this, new DateRangeEventArgs(selectionStart, selectionEnd));
		}
		#endregion Navigation and selection


		#region Hit testing
		private Hit HitTest(Point point, out int row, out int column)
		{
			row = column = -1;
			var layout = GetLayout();

			if (layout.Previous.Contains(point))
			{
				return Hit.Previous;
			}

			if (layout.Next.Contains(point))
			{
				return Hit.Next;
			}

			if (layout.Title.Contains(point))
			{
				return Hit.Title;
			}

			if (showToday && layout.Today.Contains(point))
			{
				return Hit.Today;
			}

			if (layout.Grid.Contains(point) && layout.CellWidth > 0 && layout.CellHeight > 0)
			{
				column = (point.X - layout.Grid.X) / layout.CellWidth;
				row = (point.Y - layout.Grid.Y) / layout.CellHeight;
				return Hit.Cell;
			}

			return Hit.None;
		}


		/// <summary>
		/// Gets the date at the client point when in the days view, or null.
		/// </summary>
		public DateTime? DateAt(Point point)
		{
			if (view == CalendarView.Days &&
				HitTest(point, out var row, out var column) == Hit.Cell)
			{
				return CalendarLayout.CellDate(
					CalendarLayout.FirstVisibleDate(displayMonth, FirstDayOfWeek), row, column);
			}

			return null;
		}
		#endregion Hit testing


		#region Mouse
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			var hit = HitTest(e.Location, out var row, out var column);
			if (hit != hotHit || row != hotRow || column != hotCol)
			{
				hotHit = hit;
				hotRow = row;
				hotCol = column;
				Invalidate();
			}
		}


		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			if (hotHit != Hit.None)
			{
				hotHit = Hit.None;
				hotRow = hotCol = -1;
				Invalidate();
			}
		}


		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (Enabled && e.Button == MouseButtons.Left)
			{
				Focus();
				pressedHit = HitTest(e.Location, out _, out _);
				Invalidate();
			}
		}


		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			var pressed = pressedHit;
			pressedHit = Hit.None;
			Invalidate();

			if (!Enabled || e.Button != MouseButtons.Left)
			{
				return;
			}

			var hit = HitTest(e.Location, out var row, out var column);
			if (hit != pressed)
			{
				return;
			}

			switch (hit)
			{
				case Hit.Previous:
					Page(-1);
					break;

				case Hit.Next:
					Page(1);
					break;

				case Hit.Title:
					DrillOut();
					break;

				case Hit.Today:
					SelectDate(TodayDate, false);
					break;

				case Hit.Cell:
					if (view == CalendarView.Days)
					{
						var first = CalendarLayout.FirstVisibleDate(displayMonth, FirstDayOfWeek);
						var date = CalendarLayout.CellDate(first, row, column);
						SelectDate(date, (ModifierKeys & Keys.Shift) == Keys.Shift);
					}
					else
					{
						DrillIn(row, column);
					}
					break;
			}
		}


		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if (Enabled && e.Delta != 0)
			{
				Page(e.Delta > 0 ? -1 : 1);
			}
		}
		#endregion Mouse


		#region Keyboard
		protected override bool IsInputKey(Keys keyData)
		{
			switch (keyData & Keys.KeyCode)
			{
				case Keys.Left:
				case Keys.Right:
				case Keys.Up:
				case Keys.Down:
				case Keys.PageUp:
				case Keys.PageDown:
				case Keys.Home:
				case Keys.End:
				case Keys.Enter:
					return true;

				case Keys.Escape:
					// in the days view let the host (dialog or popup) handle Escape
					return view != CalendarView.Days;
			}

			return base.IsInputKey(keyData);
		}


		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (!Enabled)
			{
				return;
			}

			var shift = e.Shift;
			var ctrl = e.Control;

			if (ctrl && e.KeyCode == Keys.Up)
			{
				DrillOut();
				e.Handled = true;
				return;
			}

			if (ctrl && e.KeyCode == Keys.Down)
			{
				DrillInFocus();
				e.Handled = true;
				return;
			}

			if (view == CalendarView.Days)
			{
				if (OnDaysKey(e.KeyCode, shift, ctrl))
				{
					e.Handled = true;
				}
			}
			else if (OnPagedKey(e.KeyCode))
			{
				e.Handled = true;
			}
		}


		private bool OnDaysKey(Keys key, bool shift, bool ctrl)
		{
			var d = focusDate;
			try
			{
				switch (key)
				{
					case Keys.Left:
						MoveFocus(d.AddDays(-1), shift);
						return true;

					case Keys.Right:
						MoveFocus(d.AddDays(1), shift);
						return true;

					case Keys.Up:
						MoveFocus(d.AddDays(-7), shift);
						return true;

					case Keys.Down:
						MoveFocus(d.AddDays(7), shift);
						return true;

					case Keys.PageUp:
						MoveFocus(ctrl ? d.AddYears(-1) : d.AddMonths(-1), shift);
						return true;

					case Keys.PageDown:
						MoveFocus(ctrl ? d.AddYears(1) : d.AddMonths(1), shift);
						return true;

					case Keys.Home:
						MoveFocus(new DateTime(d.Year, d.Month, 1), shift);
						return true;

					case Keys.End:
						MoveFocus(
							new DateTime(d.Year, d.Month, DateTime.DaysInMonth(d.Year, d.Month)),
							shift);
						return true;

					case Keys.Enter:
						SelectDate(focusDate, false);
						return true;
				}
			}
			catch (ArgumentOutOfRangeException)
			{
				// moved past DateTime.MinValue/MaxValue; ignore
			}

			return false;
		}


		private bool OnPagedKey(Keys key)
		{
			var step = view == CalendarView.Months ? 1 : view == CalendarView.Years ? 1 : 10;

			switch (key)
			{
				case Keys.Left:
					MoveFocusUnit(-step);
					return true;

				case Keys.Right:
					MoveFocusUnit(step);
					return true;

				case Keys.Up:
					MoveFocusUnit(-3 * step);
					return true;

				case Keys.Down:
					MoveFocusUnit(3 * step);
					return true;

				case Keys.PageUp:
					Page(-1);
					return true;

				case Keys.PageDown:
					Page(1);
					return true;

				case Keys.Enter:
					DrillInFocus();
					return true;

				case Keys.Escape:
					view = (CalendarView)((int)view - 1);
					OnDisplayChanged();
					return true;
			}

			return false;
		}


		/// <summary>
		/// Moves the keyboard focus date by a number of months (Months view) or years
		/// (Years and Decades views).
		/// </summary>
		private void MoveFocusUnit(int units)
		{
			try
			{
				var date = view == CalendarView.Months
					? focusDate.AddMonths(units)
					: focusDate.AddYears(units);

				focusDate = ClampToRange(date);
				displayMonth = new DateTime(focusDate.Year, focusDate.Month, 1);
				Invalidate();
			}
			catch (ArgumentOutOfRangeException)
			{
				// ignore
			}
		}


		private void DrillInFocus()
		{
			switch (view)
			{
				case CalendarView.Decades:
					view = CalendarView.Years;
					break;

				case CalendarView.Years:
					view = CalendarView.Months;
					break;

				case CalendarView.Months:
					view = CalendarView.Days;
					break;

				default:
					return;
			}

			OnDisplayChanged();
		}


		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			Invalidate();
		}


		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			Invalidate();
		}
		#endregion Keyboard


		#region Painting
		protected override void OnPaint(PaintEventArgs e)
		{
			var g = e.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias;

			var layout = GetLayout();

			using (var brush = new SolidBrush(Back))
			{
				g.FillRectangle(brush, ClientRectangle);
			}

			PaintTitle(g, layout);

			switch (view)
			{
				case CalendarView.Days:
					PaintDays(g, layout);
					break;

				case CalendarView.Months:
					PaintMonths(g, layout);
					break;

				case CalendarView.Years:
					PaintYears(g, layout);
					break;

				default:
					PaintDecades(g, layout);
					break;
			}

			if (showToday)
			{
				PaintToday(g, layout);
			}

			using var pen = new Pen(manager.ButtonBorder);
			g.SmoothingMode = SmoothingMode.None;
			g.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
		}


		private string GetTitle()
		{
			switch (view)
			{
				case CalendarView.Days:
					return displayMonth.ToString("MMMM yyyy", Culture);

				case CalendarView.Months:
					return displayMonth.Year.ToString(Culture);

				case CalendarView.Years:
					{
						var start = displayMonth.Year - (displayMonth.Year % 10);
						return $"{start} - {start + 9}";
					}

				default:
					{
						var start = displayMonth.Year - (displayMonth.Year % 100);
						return $"{start} - {start + 99}";
					}
			}
		}


		private void PaintTitle(Graphics g, CellMetrics layout)
		{
			using (var brush = new SolidBrush(TitleBack))
			{
				g.FillRectangle(brush, layout.Title);
			}

			var fore = Enabled ? TitleFore : Gray;

			if (Enabled && hotHit == Hit.Title)
			{
				using var hot = new SolidBrush(manager.ButtonHotBack);
				var text = Measure(GetTitle(), Font);
				var box = new Rectangle(
					layout.Title.Left + ((layout.Title.Width - text.Width) / 2) - Scale(6),
					layout.Previous.Top,
					text.Width + Scale(12),
					layout.Previous.Height);
				g.FillRoundedRectangle(hot, box, Radius);
			}

			TextRenderer.DrawText(g, GetTitle(), Font,
				new Rectangle(
					layout.Previous.Right, layout.Previous.Top,
					layout.Next.Left - layout.Previous.Right, layout.Previous.Height),
				fore,
				TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
				TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);

			PaintArrow(g, layout.Previous, true, hotHit == Hit.Previous, fore,
				PrevEnabled());

			PaintArrow(g, layout.Next, false, hotHit == Hit.Next, fore,
				NextEnabled());
		}


		private bool PrevEnabled()
		{
			switch (view)
			{
				case CalendarView.Days:
					return displayMonth > new DateTime(minDate.Year, minDate.Month, 1);

				case CalendarView.Months:
					return displayMonth.Year > minDate.Year;

				case CalendarView.Years:
					return displayMonth.Year - (displayMonth.Year % 10) > minDate.Year;

				default:
					return displayMonth.Year - (displayMonth.Year % 100) > minDate.Year;
			}
		}


		private bool NextEnabled()
		{
			switch (view)
			{
				case CalendarView.Days:
					return displayMonth < new DateTime(maxDate.Year, maxDate.Month, 1);

				case CalendarView.Months:
					return displayMonth.Year < maxDate.Year;

				case CalendarView.Years:
					return displayMonth.Year - (displayMonth.Year % 10) + 9 < maxDate.Year;

				default:
					return displayMonth.Year - (displayMonth.Year % 100) + 99 < maxDate.Year;
			}
		}


		private void PaintArrow(
			Graphics g, Rectangle bounds, bool left, bool hot, Color fore, bool available)
		{
			if (Enabled && available && hot)
			{
				using var back = new SolidBrush(
					pressedHit != Hit.None ? manager.ButtonPressBorder : manager.ButtonHotBack);

				g.FillRoundedRectangle(back, bounds, Radius);
			}

			var w = Scale(4);
			var h = Scale(8);
			var cx = bounds.Left + (bounds.Width / 2);
			var cy = bounds.Top + (bounds.Height / 2);

			var points = left
				? new[]
				{
					new Point(cx + (w / 2), cy - (h / 2)),
					new Point(cx + (w / 2), cy + (h / 2)),
					new Point(cx - (w / 2), cy)
				}
				: new[]
				{
					new Point(cx - (w / 2), cy - (h / 2)),
					new Point(cx - (w / 2), cy + (h / 2)),
					new Point(cx + (w / 2), cy)
				};

			using var arrow = new SolidBrush(available ? fore : Gray);
			g.FillPolygon(arrow, points);
		}


		private void PaintDays(Graphics g, CellMetrics layout)
		{
			var info = Culture.DateTimeFormat;
			var first = FirstDayOfWeek;
			var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
				TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

			// weekday header
			for (var c = 0; c < 7; c++)
			{
				var day = (DayOfWeek)((((int)first) + c) % 7);
				var rect = new Rectangle(
					layout.Grid.X + (c * layout.CellWidth), layout.Header.Y,
					layout.CellWidth, layout.CellHeight);

				TextRenderer.DrawText(g, info.GetShortestDayName(day), Font, rect, Gray, flags);
			}

			using (var pen = new Pen(manager.ButtonBorder))
			{
				var y = layout.Header.Bottom - 1;
				g.DrawLine(pen, layout.Header.Left, y, layout.Header.Right, y);
			}

			var firstVisible = CalendarLayout.FirstVisibleDate(displayMonth, first);
			var today = TodayDate;
			var focused = Focused && Enabled;

			for (var r = 0; r < CalendarLayout.Rows; r++)
			{
				if (showWeekNumbers)
				{
					var weekDate = CalendarLayout.CellDate(firstVisible, r, 3);
					var rect = new Rectangle(
						layout.Header.X, layout.Grid.Y + (r * layout.CellHeight),
						layout.WeekWidth, layout.CellHeight);

					TextRenderer.DrawText(g,
						CalendarLayout.WeekNumber(weekDate, Culture, first).ToString(Culture),
						Font, rect, Gray, flags);
				}

				for (var c = 0; c < CalendarLayout.Columns; c++)
				{
					var date = CalendarLayout.CellDate(firstVisible, r, c);
					var rect = new Rectangle(
						layout.Grid.X + (c * layout.CellWidth),
						layout.Grid.Y + (r * layout.CellHeight),
						layout.CellWidth, layout.CellHeight);

					var inRange = CalendarLayout.IsInRange(date, minDate, maxDate);
					var selected = date >= selectionStart && date <= selectionEnd;
					var isHot = hotHit == Hit.Cell && hotRow == r && hotCol == c;
					var color = date.Month != displayMonth.Month ? Trailing : Fore;

					var cell = Rectangle.Inflate(rect, -1, -1);

					if (selected && Enabled)
					{
						using var brush = new SolidBrush(manager.GetColor("Highlight"));
						g.FillRoundedRectangle(brush, cell, Radius);
						color = manager.GetColor("HighlightText");
					}
					else if (isHot && inRange && Enabled)
					{
						using var brush = new SolidBrush(
							pressedHit == Hit.Cell ? manager.ButtonPressBorder : manager.ButtonHotBack);
						g.FillRoundedRectangle(brush, cell, Radius);
					}

					if (!inRange)
					{
						color = Gray;
					}

					TextRenderer.DrawText(g, date.Day.ToString(Culture), Font, rect, color, flags);

					if (showTodayCircle && date == today)
					{
						using var pen = new Pen(manager.GetColor("HotTrack"));
						g.DrawRoundedRectangle(pen, new Rectangle(
							cell.X, cell.Y, cell.Width - 1, cell.Height - 1), Radius);
					}

					if (focused && date == focusDate)
					{
						using var pen = new Pen(manager.GetColor("HotTrack"))
						{
							DashStyle = DashStyle.Dot
						};

						var inner = Rectangle.Inflate(cell, -2, -2);
						g.DrawRectangle(pen, inner.X, inner.Y, inner.Width - 1, inner.Height - 1);
					}
				}
			}
		}


		private void PaintCell(
			Graphics g, CellMetrics layout, int row, int column,
			string text, bool selected, bool available, bool trailing, bool focus)
		{
			var rect = new Rectangle(
				layout.Grid.X + (column * layout.CellWidth),
				layout.Grid.Y + (row * layout.CellHeight),
				layout.CellWidth, layout.CellHeight);

			var cell = Rectangle.Inflate(rect, -Scale(3), -Scale(3));
			var color = trailing ? Trailing : Fore;

			if (selected && Enabled && available)
			{
				using var brush = new SolidBrush(manager.GetColor("Highlight"));
				g.FillRoundedRectangle(brush, cell, Radius);
				color = manager.GetColor("HighlightText");
			}
			else if (hotHit == Hit.Cell && hotRow == row && hotCol == column &&
				available && Enabled)
			{
				using var brush = new SolidBrush(
					pressedHit == Hit.Cell ? manager.ButtonPressBorder : manager.ButtonHotBack);
				g.FillRoundedRectangle(brush, cell, Radius);
			}

			if (!available)
			{
				color = Gray;
			}

			TextRenderer.DrawText(g, text, Font, rect, color,
				TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
				TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);

			if (focus && Focused && Enabled)
			{
				using var pen = new Pen(manager.GetColor("HotTrack")) { DashStyle = DashStyle.Dot };
				var inner = Rectangle.Inflate(cell, -2, -2);
				g.DrawRectangle(pen, inner.X, inner.Y, inner.Width - 1, inner.Height - 1);
			}
		}


		private void PaintMonths(Graphics g, CellMetrics layout)
		{
			var names = Culture.DateTimeFormat.AbbreviatedMonthNames;
			var year = displayMonth.Year;

			for (var i = 0; i < 12; i++)
			{
				PaintCell(g, layout, i / 3, i % 3,
					names[i],
					selectionStart.Year == year && selectionStart.Month == i + 1,
					CalendarLayout.IsMonthInRange(year, i + 1, minDate, maxDate),
					false,
					focusDate.Year == year && focusDate.Month == i + 1);
			}
		}


		private void PaintYears(Graphics g, CellMetrics layout)
		{
			var start = CalendarLayout.YearPageStart(displayMonth.Year);
			var decade = displayMonth.Year - (displayMonth.Year % 10);

			for (var i = 0; i < 12; i++)
			{
				var year = start + i;
				PaintCell(g, layout, i / 3, i % 3,
					year >= 1 && year <= 9999 ? year.ToString(Culture) : string.Empty,
					selectionStart.Year == year,
					CalendarLayout.IsYearInRange(year, minDate, maxDate),
					year < decade || year > decade + 9,
					focusDate.Year == year);
			}
		}


		private void PaintDecades(Graphics g, CellMetrics layout)
		{
			var start = CalendarLayout.DecadePageStart(displayMonth.Year);
			var century = displayMonth.Year - (displayMonth.Year % 100);

			for (var i = 0; i < 12; i++)
			{
				var year = start + (i * 10);
				var label = year >= 0 && year + 9 <= 9999
					? $"{year}-{(year + 9) % 100:00}"
					: string.Empty;

				PaintCell(g, layout, i / 3, i % 3,
					label,
					selectionStart.Year >= year && selectionStart.Year <= year + 9,
					CalendarLayout.IsDecadeInRange(year, minDate, maxDate),
					year < century || year > century + 99,
					focusDate.Year >= year && focusDate.Year <= year + 9);
			}
		}


		private void PaintToday(Graphics g, CellMetrics layout)
		{
			var today = TodayDate;
			var label = TodayText;
			var text = string.IsNullOrEmpty(label)
				? today.ToString("d", Culture)
				: $"{label}: {today.ToString("d", Culture)}";

			if (Enabled && hotHit == Hit.Today)
			{
				using var brush = new SolidBrush(manager.ButtonHotBack);
				g.FillRoundedRectangle(brush, layout.Today, Radius);
			}

			var available = CalendarLayout.IsInRange(today, minDate, maxDate);

			TextRenderer.DrawText(g, text, Font, layout.Today,
				available && Enabled ? Fore : Gray,
				TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
				TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
		}
		#endregion Painting
	}
}
