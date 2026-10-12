//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S3881 // "IDisposable" should be implemented correctly

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Globalization;
	using System.Windows.Forms;


	/// <summary>
	/// An owner-drawn replacement for DateTimePicker that follows the OneMore theme and
	/// offers the same public API: Value, MinDate, MaxDate, Format, CustomFormat,
	/// ShowCheckBox, Checked, ShowUpDown, UpDownAlign, DropDownAlign and Calendar* colors.
	/// </summary>
	[DefaultProperty(nameof(Value))]
	[DefaultEvent(nameof(ValueChanged))]
	[ToolboxItem(true)]
	internal class MoreDateTimePicker : Control, IThemedControl, ILoadControl
	{
		private enum Part
		{
			None,
			CheckBox,
			Segment,
			DropButton,
			UpButton,
			DownButton
		}

		private const int CheckStop = -2;
		private const int NoStop = -1;
		private const int EntryTimeoutMs = 1000;

		/// <summary>
		/// The minimum date supported, same as DateTimePicker.MinimumDateTime.
		/// </summary>
		public static readonly DateTime MinimumDateTime = new DateTime(1753, 1, 1);

		/// <summary>
		/// The maximum date supported, same as DateTimePicker.MaximumDateTime.
		/// </summary>
		public static readonly DateTime MaximumDateTime = new DateTime(9998, 12, 31);

		private readonly ThemeManager manager;
		private readonly List<DateSegment> segments = new List<DateSegment>();

		private DateTime value;
		private DateTime minDate = MinimumDateTime;
		private DateTime maxDate = MaximumDateTime;
		private DateTimePickerFormat format = DateTimePickerFormat.Long;
		private string customFormat;
		private bool showCheckBox;
		private bool isChecked = true;
		private bool showUpDown;
		private LeftRightAlignment upDownAlign = LeftRightAlignment.Right;
		private LeftRightAlignment dropDownAlign = LeftRightAlignment.Left;

		private Color calendarForeColor = Color.Empty;
		private Color calendarMonthBackground = Color.Empty;
		private Color calendarTitleBackColor = Color.Empty;
		private Color calendarTitleForeColor = Color.Empty;
		private Color calendarTrailingForeColor = Color.Empty;
		private Font calendarFont;

		private int active = NoStop;
		private string entry = string.Empty;
		private int entryTick;

		private bool layoutDirty = true;
		private Rectangle checkRect;
		private Rectangle upRect;
		private Rectangle downRect;
		private Rectangle dropRect;
		private Rectangle[] segmentBounds = Array.Empty<Rectangle>();

		private Part hotPart;
		private int hotIndex = NoStop;
		private Part pressedPart;

		private MoreCalendarDropDown dropDown;
		private bool returningFromDropDown;


		public MoreDateTimePicker()
		{
			SetStyle(
				ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
				ControlStyles.Selectable | ControlStyles.StandardClick |
				ControlStyles.UserMouse | ControlStyles.FixedHeight,
				true);

			manager = ThemeManager.Instance;
			value = Clamp(DateTime.Now);

			TabStop = true;
			Rebuild();
		}


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				dropDown?.Dispose();
				dropDown = null;
			}

			base.Dispose(disposing);
		}


		#region Events
		[Category("Action"), Description("Raised when the Value or Checked state changes")]
		public event EventHandler ValueChanged;

		[Category("Behavior"), Description("Raised when the Format changes")]
		public event EventHandler FormatChanged;

		[Category("Behavior"), Description("Raised when the calendar drop-down is shown")]
		public event EventHandler DropDown;

		[Category("Behavior"), Description("Raised when the calendar drop-down is closed")]
		public event EventHandler CloseUp;
		#endregion Events


		#region Properties
		public string ThemedBack { get; set; }


		public string ThemedFore { get; set; }


		[Bindable(true)]
		[Category("Behavior"), Description("The date and time value of the control")]
		public DateTime Value
		{
			get => value;
			set
			{
				if (this.value == value)
				{
					return;
				}

				if (value < minDate || value > maxDate)
				{
					throw new ArgumentOutOfRangeException(
						nameof(value), "Value is not within MinDate and MaxDate");
				}

				this.value = value;
				Invalidate();
				OnValueChanged(EventArgs.Empty);
			}
		}


		private bool ShouldSerializeValue() => value.Date != DateTime.Today;


		private void ResetValue() => Value = Clamp(DateTime.Now);


		[Category("Behavior")]
		public DateTime MinDate
		{
			get => minDate;
			set
			{
				if (value < MinimumDateTime || value > MaximumDateTime || value > maxDate)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				minDate = value;
				ClampValue();
			}
		}


		private bool ShouldSerializeMinDate() => minDate != MinimumDateTime;


		private void ResetMinDate() => MinDate = MinimumDateTime;


		[Category("Behavior")]
		public DateTime MaxDate
		{
			get => maxDate;
			set
			{
				if (value < MinimumDateTime || value > MaximumDateTime || value < minDate)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				maxDate = value;
				ClampValue();
			}
		}


		private bool ShouldSerializeMaxDate() => maxDate != MaximumDateTime;


		private void ResetMaxDate() => MaxDate = MaximumDateTime;


		[Category("Appearance"), DefaultValue(DateTimePickerFormat.Long)]
		[RefreshProperties(RefreshProperties.Repaint)]
		public DateTimePickerFormat Format
		{
			get => format;
			set
			{
				if (format != value)
				{
					format = value;
					Rebuild();
					FormatChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}


		[Category("Behavior"), DefaultValue(null)]
		[RefreshProperties(RefreshProperties.Repaint)]
		public string CustomFormat
		{
			get => customFormat;
			set
			{
				if (customFormat != value)
				{
					customFormat = value;
					if (format == DateTimePickerFormat.Custom)
					{
						Rebuild();
						FormatChanged?.Invoke(this, EventArgs.Empty);
					}
				}
			}
		}


		[Category("Behavior"), DefaultValue(false)]
		public bool ShowCheckBox
		{
			get => showCheckBox;
			set
			{
				if (showCheckBox != value)
				{
					showCheckBox = value;
					Rebuild();
				}
			}
		}


		[Bindable(true)]
		[Category("Behavior"), DefaultValue(true)]
		public bool Checked
		{
			get => isChecked;
			set
			{
				if (isChecked != value)
				{
					isChecked = value;
					if (!isChecked)
					{
						CloseDropDown();
					}

					Invalidate();
					OnValueChanged(EventArgs.Empty);
				}
			}
		}


		[Category("Behavior"), DefaultValue(false)]
		public bool ShowUpDown
		{
			get => showUpDown;
			set
			{
				if (showUpDown != value)
				{
					showUpDown = value;
					CloseDropDown();
					Rebuild();
				}
			}
		}


		[Category("Appearance"), DefaultValue(LeftRightAlignment.Right)]
		public LeftRightAlignment UpDownAlign
		{
			get => upDownAlign;
			set
			{
				if (upDownAlign != value)
				{
					upDownAlign = value;
					layoutDirty = true;
					Invalidate();
				}
			}
		}


		[Category("Appearance"), DefaultValue(LeftRightAlignment.Left)]
		public LeftRightAlignment DropDownAlign
		{
			get => dropDownAlign;
			set => dropDownAlign = value;
		}


		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool DroppedDown => dropDown?.Visible == true;


		[Category("Appearance"), DefaultValue(typeof(Color), "")]
		public Color CalendarForeColor
		{
			get => calendarForeColor;
			set => calendarForeColor = value;
		}


		[Category("Appearance"), DefaultValue(typeof(Color), "")]
		public Color CalendarMonthBackground
		{
			get => calendarMonthBackground;
			set => calendarMonthBackground = value;
		}


		[Category("Appearance"), DefaultValue(typeof(Color), "")]
		public Color CalendarTitleBackColor
		{
			get => calendarTitleBackColor;
			set => calendarTitleBackColor = value;
		}


		[Category("Appearance"), DefaultValue(typeof(Color), "")]
		public Color CalendarTitleForeColor
		{
			get => calendarTitleForeColor;
			set => calendarTitleForeColor = value;
		}


		[Category("Appearance"), DefaultValue(typeof(Color), "")]
		public Color CalendarTrailingForeColor
		{
			get => calendarTrailingForeColor;
			set => calendarTrailingForeColor = value;
		}


		[Category("Appearance")]
		public Font CalendarFont
		{
			get => calendarFont ?? Font;
			set => calendarFont = value;
		}


		private bool ShouldSerializeCalendarFont() => calendarFont is not null;


		private void ResetCalendarFont() => calendarFont = null;


		[Browsable(false)]
		public int PreferredHeight => Font.Height + Scale(8);


		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string Text
		{
			get
			{
				if (segments is null)
				{
					return string.Empty;
				}

				var culture = Culture;
				var builder = new System.Text.StringBuilder();
				foreach (var segment in segments)
				{
					builder.Append(segment.Format(value, culture));
				}

				return builder.ToString();
			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					return;
				}

				if (!DateTime.TryParse(value, Culture, DateTimeStyles.None, out var date))
				{
					throw new FormatException("Text is not a valid date and time");
				}

				Value = date;
			}
		}


		protected override Size DefaultSize => new Size(200, 24);


		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			// like the stock control, height is determined by the font
			base.SetBoundsCore(x, y, width, PreferredHeight, specified);
		}


		private static CultureInfo Culture => AddIn.Locale;


		private bool Editable => Enabled && (!showCheckBox || isChecked);
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
			ForeColor = themeManager.GetColor("WindowText", ThemedFore);
			Invalidate();
		}


		private Color BackThemed => Enabled
			? manager.GetColor("Window", ThemedBack)
			: manager.GetColor("InactiveWindow");

		private Color ForeThemed => Editable
			? manager.GetColor("WindowText", ThemedFore)
			: manager.GetColor("GrayText");
		#endregion Theme


		#region Value helpers
		private DateTime Clamp(DateTime date)
		{
			return DatePartMath.Clamp(date, minDate, maxDate);
		}


		private void ClampValue()
		{
			var clamped = Clamp(value);
			if (clamped != value)
			{
				value = clamped;
				OnValueChanged(EventArgs.Empty);
			}

			Invalidate();
		}


		protected virtual void OnValueChanged(EventArgs e)
		{
			AccessibilityNotifyClients(AccessibleEvents.ValueChange, -1);
			ValueChanged?.Invoke(this, e);
		}


		private bool TrySetValue(DateTime candidate)
		{
			if (candidate < minDate || candidate > maxDate)
			{
				return false;
			}

			if (candidate != value)
			{
				value = candidate;
				Invalidate();
				OnValueChanged(EventArgs.Empty);
			}

			return true;
		}
		#endregion Value helpers


		#region Layout
		private int Scale(int px)
		{
			return (int)Math.Round(px * DeviceDpi / 96f);
		}


		private void Rebuild()
		{
			segments.Clear();
			segments.AddRange(DateFormatParser.Parse(
				DateFormatParser.GetPattern(format, customFormat, Culture), Culture));

			var stops = GetStops();
			if (!stops.Contains(active))
			{
				active = stops.Count > 0 ? stops[0] : NoStop;
			}

			entry = string.Empty;
			layoutDirty = true;
			Invalidate();
		}


		private List<int> GetStops()
		{
			var stops = new List<int>();

			if (showCheckBox)
			{
				stops.Add(CheckStop);
			}

			for (var i = 0; i < segments.Count; i++)
			{
				if (segments[i].IsEditable)
				{
					stops.Add(i);
				}
			}

			return stops;
		}


		private static int MeasureWidth(string text, Font font)
		{
			return TextRenderer.MeasureText(text, font, Size.Empty,
				TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine).Width;
		}


		private void EnsureLayout()
		{
			if (!layoutDirty)
			{
				return;
			}

			layoutDirty = false;

			var pad = Scale(3);
			var buttonWidth = Scale(17);
			var height = ClientSize.Height;

			var x = pad;
			var right = ClientSize.Width - 1;

			checkRect = upRect = downRect = dropRect = Rectangle.Empty;

			if (showUpDown)
			{
				var left = upDownAlign == LeftRightAlignment.Left;
				var bounds = new Rectangle(
					left ? 1 : right - buttonWidth, 1, buttonWidth, height - 2);

				upRect = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height / 2);
				downRect = new Rectangle(
					bounds.X, upRect.Bottom, bounds.Width, bounds.Bottom - upRect.Bottom);

				if (left)
				{
					x = bounds.Right + pad;
				}
				else
				{
					right = bounds.Left;
				}
			}
			else
			{
				dropRect = new Rectangle(right - buttonWidth, 1, buttonWidth, height - 2);
				right = dropRect.Left;
			}

			if (showCheckBox)
			{
				var size = Scale(13);
				checkRect = new Rectangle(x, (height - size) / 2, size, size);
				x = checkRect.Right + (pad * 2);
			}

			var culture = Culture;
			segmentBounds = new Rectangle[segments.Count];

			for (var i = 0; i < segments.Count; i++)
			{
				var width = MeasureWidth(segments[i].WidestText(culture), Font);
				segmentBounds[i] = new Rectangle(x, 1, width, height - 2);
				x += width;
			}
		}


		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			layoutDirty = true;
			Invalidate();
		}


		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			Height = PreferredHeight;
			layoutDirty = true;
			Invalidate();
		}


		protected override void OnDpiChangedAfterParent(EventArgs e)
		{
			base.OnDpiChangedAfterParent(e);
			Height = PreferredHeight;
			layoutDirty = true;
			Invalidate();
		}


		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			Invalidate();
		}


		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			if (!Enabled)
			{
				CloseDropDown();
			}

			Invalidate();
		}
		#endregion Layout


		#region Hit testing
		private Part HitTestPart(Point point, out int index)
		{
			EnsureLayout();
			index = NoStop;

			if (checkRect.Contains(point) ||
				(showCheckBox && point.X < checkRect.Right && !upRect.Contains(point) &&
				!downRect.Contains(point)))
			{
				return showCheckBox ? Part.CheckBox : Part.None;
			}

			if (upRect.Contains(point))
			{
				return Part.UpButton;
			}

			if (downRect.Contains(point))
			{
				return Part.DownButton;
			}

			if (dropRect.Contains(point))
			{
				return Part.DropButton;
			}

			var first = NoStop;
			var last = NoStop;
			for (var i = 0; i < segments.Count; i++)
			{
				if (!segments[i].IsEditable)
				{
					continue;
				}

				if (first < 0)
				{
					first = i;
				}

				last = i;

				// the span of a segment includes the literal that follows it
				var span = segmentBounds[i];
				var next = i + 1 < segments.Count && segments[i + 1].IsLiteral
					? segmentBounds[i + 1].Width
					: 0;

				if (point.X >= span.Left && point.X < span.Right + next)
				{
					index = i;
					return Part.Segment;
				}
			}

			if (first >= 0)
			{
				// left of the first field selects the first; blank area to the right of the
				// last field selects the last
				index = point.X < segmentBounds[first].Left ? first : last;
				return Part.Segment;
			}

			return Part.None;
		}
		#endregion Hit testing


		#region Mouse
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			var part = HitTestPart(e.Location, out var index);
			if (part != hotPart || index != hotIndex)
			{
				hotPart = part;
				hotIndex = index;
				Invalidate();
			}
		}


		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			if (hotPart != Part.None)
			{
				hotPart = Part.None;
				hotIndex = NoStop;
				Invalidate();
			}
		}


		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (!Enabled || e.Button != MouseButtons.Left)
			{
				return;
			}

			var wasOpen = dropDown?.JustClosedByClick == true;

			Focus();

			var part = HitTestPart(e.Location, out var index);
			pressedPart = part;

			switch (part)
			{
				case Part.CheckBox:
					active = CheckStop;
					Checked = !isChecked;
					break;

				case Part.Segment:
					if (index >= 0)
					{
						active = index;
						entry = string.Empty;
					}
					break;

				case Part.DropButton:
					if (Editable && !wasOpen)
					{
						active = active == NoStop ? FirstSegmentStop() : active;
						OpenDropDown();
					}
					break;

				case Part.UpButton:
					AdjustActive(1, true);
					break;

				case Part.DownButton:
					AdjustActive(-1, true);
					break;
			}

			Invalidate();
		}


		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			pressedPart = Part.None;
			Invalidate();
		}


		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);

			if (Focused && Editable && active >= 0 && e.Delta != 0)
			{
				AdjustActive(e.Delta > 0 ? 1 : -1);

				if (e is HandledMouseEventArgs handled)
				{
					handled.Handled = true;
				}
			}
		}
		#endregion Mouse


		#region Keyboard
		private int FirstSegmentStop()
		{
			var stops = GetStops();
			foreach (var stop in stops)
			{
				if (stop >= 0)
				{
					return stop;
				}
			}

			return NoStop;
		}


		private void AdjustActive(int delta, bool fromButton = false)
		{
			if (!Editable)
			{
				return;
			}

			if (active < 0)
			{
				if (active == CheckStop && !fromButton)
				{
					// arrows do nothing while the check box has the focus
					return;
				}

				active = FirstSegmentStop();
			}

			if (active < 0 || active >= segments.Count)
			{
				return;
			}

			entry = string.Empty;
			TrySetValue(DatePartMath.Adjust(
				value, segments[active].Part, delta, minDate, maxDate));
		}


		/// <summary>
		/// Moves the active stop by one in the given direction; false if at the end.
		/// </summary>
		private bool MoveStop(int direction)
		{
			var stops = GetStops();
			var position = stops.IndexOf(active);
			var next = position + direction;

			if (position < 0 || next < 0 || next >= stops.Count)
			{
				return false;
			}

			active = stops[next];
			entry = string.Empty;
			Invalidate();
			return true;
		}


		protected override bool IsInputKey(Keys keyData)
		{
			switch (keyData & Keys.KeyCode)
			{
				case Keys.Left:
				case Keys.Right:
				case Keys.Up:
				case Keys.Down:
				case Keys.Home:
				case Keys.End:
					return true;
			}

			return base.IsInputKey(keyData);
		}


		protected override bool ProcessDialogKey(Keys keyData)
		{
			if ((keyData & (Keys.Control | Keys.Alt)) == 0 &&
				(keyData & Keys.KeyCode) == Keys.Tab && Focused)
			{
				var direction = (keyData & Keys.Shift) == Keys.Shift ? -1 : 1;
				if (MoveStop(direction))
				{
					return true;
				}
			}

			return base.ProcessDialogKey(keyData);
		}


		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (!Enabled)
			{
				return;
			}

			if (e.KeyCode == Keys.F4 || (e.Alt && e.KeyCode == Keys.Down))
			{
				if (DroppedDown)
				{
					CloseDropDown();
				}
				else if (Editable && !showUpDown)
				{
					OpenDropDown();
				}

				e.Handled = true;
				return;
			}

			switch (e.KeyCode)
			{
				case Keys.Up:
					AdjustActive(1);
					e.Handled = true;
					break;

				case Keys.Down:
					AdjustActive(-1);
					e.Handled = true;
					break;

				case Keys.Left:
					MoveStop(-1);
					e.Handled = true;
					break;

				case Keys.Right:
					MoveStop(1);
					e.Handled = true;
					break;

				case Keys.Home:
					{
						var stops = GetStops();
						if (stops.Count > 0)
						{
							active = stops[0];
							entry = string.Empty;
							Invalidate();
						}
					}
					e.Handled = true;
					break;

				case Keys.End:
					{
						var stops = GetStops();
						if (stops.Count > 0)
						{
							active = stops[stops.Count - 1];
							entry = string.Empty;
							Invalidate();
						}
					}
					e.Handled = true;
					break;

				case Keys.Space:
					if (active == CheckStop)
					{
						Checked = !isChecked;
						e.Handled = true;
					}
					break;
			}
		}


		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);

			if (!Editable || active < 0 || active >= segments.Count || char.IsControl(e.KeyChar))
			{
				return;
			}

			var segment = segments[active];
			if (unchecked(Environment.TickCount - entryTick) > EntryTimeoutMs)
			{
				entry = string.Empty;
			}

			entryTick = Environment.TickCount;

			if (segment.MaxDigits > 0)
			{
				EnterDigit(segment, e.KeyChar);
				e.Handled = true;
			}
			else if (segment.Part == DatePart.Month)
			{
				EnterMonthName(segment, e.KeyChar);
				e.Handled = true;
			}
			else if (segment.Part == DatePart.AmPm)
			{
				EnterAmPm(e.KeyChar);
				e.Handled = true;
			}
		}


		private void EnterDigit(DateSegment segment, char key)
		{
			if (!DatePartMath.TryEnter(
				entry, key, segment.Part, value, segment.MaxDigits,
				out var buffer, out var complete))
			{
				return;
			}

			entry = buffer;
			var number = int.Parse(buffer, CultureInfo.InvariantCulture);

			if (segment.Part == DatePart.Year && segment.MaxDigits <= 2)
			{
				number = DatePartMath.ExpandYear(number, buffer.Length, Culture.Calendar);
			}

			var candidate = DatePartMath.SetPart(value, segment.Part, number);
			if (candidate.HasValue)
			{
				TrySetValue(candidate.Value);
			}

			if (complete)
			{
				entry = string.Empty;
				MoveStop(1);
			}

			Invalidate();
		}


		private void EnterMonthName(DateSegment segment, char key)
		{
			var info = Culture.DateTimeFormat;
			var names = segment.Count >= 4 ? info.MonthNames : info.AbbreviatedMonthNames;

			int Find(string prefix)
			{
				for (var i = 0; i < 12; i++)
				{
					if (Culture.CompareInfo.IsPrefix(names[i], prefix, CompareOptions.IgnoreCase))
					{
						return i + 1;
					}
				}

				return 0;
			}

			var month = Find(entry + key);
			if (month == 0)
			{
				entry = string.Empty;
				month = Find(key.ToString());
			}

			if (month > 0)
			{
				entry += key;
				var candidate = DatePartMath.SetPart(value, DatePart.Month, month);
				if (candidate.HasValue)
				{
					TrySetValue(candidate.Value);
				}
			}
		}


		private void EnterAmPm(char key)
		{
			var info = Culture.DateTimeFormat;
			var c = char.ToLower(key, Culture);

			var am = char.ToLower(info.AMDesignator.Length > 0 ? info.AMDesignator[0] : 'a', Culture);
			var pm = char.ToLower(info.PMDesignator.Length > 0 ? info.PMDesignator[0] : 'p', Culture);

			int? number = null;
			if (c == am)
			{
				number = 0;
			}
			else if (c == pm)
			{
				number = 1;
			}

			if (number.HasValue)
			{
				var candidate = DatePartMath.SetPart(value, DatePart.AmPm, number.Value);
				if (candidate.HasValue)
				{
					TrySetValue(candidate.Value);
				}
			}
		}


		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);

			var stops = GetStops();
			if (returningFromDropDown)
			{
				// keep the active field when focus comes back from the calendar
				returningFromDropDown = false;
			}
			else if (stops.Count > 0)
			{
				var keyboard = MouseButtons == MouseButtons.None;
				if (!stops.Contains(active))
				{
					active = stops[0];
				}
				else if (keyboard && (ModifierKeys & Keys.Shift) == Keys.Shift)
				{
					// reverse tab into the control
					active = stops[stops.Count - 1];
				}
				else if (keyboard)
				{
					active = stops[0];
				}
			}

			entry = string.Empty;
			Invalidate();
		}


		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			entry = string.Empty;
			Invalidate();
		}
		#endregion Keyboard


		#region Drop-down
		private void OpenDropDown()
		{
			if (!Editable || showUpDown || DroppedDown)
			{
				return;
			}

			if (dropDown is null)
			{
				dropDown = new MoreCalendarDropDown();
				dropDown.Calendar.DateSelected += OnCalendarSelected;
				dropDown.Closed += OnDropDownClosed;
			}

			var calendar = dropDown.Calendar;
			calendar.MonthBackColor = calendarMonthBackground;
			calendar.MonthForeColor = calendarForeColor;
			calendar.TitleBackColor = calendarTitleBackColor;
			calendar.TitleForeColor = calendarTitleForeColor;
			calendar.TrailingForeColor = calendarTrailingForeColor;
			calendar.Font = CalendarFont;

			DropDown?.Invoke(this, EventArgs.Empty);

			dropDown.Show(this, value.Date, minDate, maxDate, dropDownAlign);
			Invalidate();
		}


		private void CloseDropDown()
		{
			dropDown?.Close();
		}


		private void OnCalendarSelected(object sender, DateRangeEventArgs e)
		{
			// keep the time of day and Kind, replace only the date
			var date = e.Start;
			var candidate = DatePartMath.With(
				value, date.Year, date.Month, date.Day, value.Hour, value.Minute, value.Second);

			TrySetValue(candidate);
			dropDown.Close();
		}


		private void OnDropDownClosed(object sender, ToolStripDropDownClosedEventArgs e)
		{
			CloseUp?.Invoke(this, EventArgs.Empty);
			Invalidate();

			if (!IsDisposed && Enabled)
			{
				returningFromDropDown = !Focused;
				Focus();
			}
		}
		#endregion Drop-down


		#region Painting
		protected override void OnPaint(PaintEventArgs e)
		{
			EnsureLayout();

			var g = e.Graphics;
			var enabled = Enabled;
			var focused = Focused && enabled;

			using (var brush = new SolidBrush(BackThemed))
			{
				g.FillRectangle(brush, ClientRectangle);
			}

			if (showCheckBox)
			{
				PaintCheckBox(g);
			}

			PaintSegments(g, focused);

			if (showUpDown)
			{
				PaintUpDown(g);
			}
			else
			{
				PaintDropButton(g);
			}

			var border = manager.ButtonBorder;
			if (focused)
			{
				border = manager.GetColor("HotTrack");
			}
			else if (enabled && hotPart != Part.None)
			{
				border = manager.ButtonHotBorder;
			}

			using var pen = new Pen(border);
			g.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
		}


		private void PaintSegments(Graphics g, bool focused)
		{
			var culture = Culture;
			var fore = ForeThemed;
			var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
				TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;

			for (var i = 0; i < segments.Count; i++)
			{
				var bounds = segmentBounds[i];
				var color = fore;

				if (i == active && focused && Editable)
				{
					using var brush = new SolidBrush(manager.GetColor("Highlight"));
					g.FillRectangle(brush, Rectangle.Inflate(bounds, 0, -Scale(2)));
					color = manager.GetColor("HighlightText");
				}

				TextRenderer.DrawText(
					g, segments[i].Format(value, culture), Font, bounds, color, flags);
			}
		}


		private void PaintCheckBox(Graphics g)
		{
			var enabled = Enabled;
			var hot = enabled && hotPart == Part.CheckBox;

			using (var brush = new SolidBrush(BackThemed))
			{
				g.FillRectangle(brush, checkRect);
			}

			var border = !enabled
				? manager.GetColor("GrayText")
				: hot ? manager.ButtonHotBorder : manager.ButtonBorder;

			using (var pen = new Pen(border))
			{
				g.DrawRectangle(pen, checkRect.X, checkRect.Y,
					checkRect.Width - 1, checkRect.Height - 1);
			}

			if (isChecked)
			{
				var s = checkRect.Width / 13f;
				var points = new[]
				{
					new PointF(checkRect.X + (3 * s), checkRect.Y + (6.5f * s)),
					new PointF(checkRect.X + (5.5f * s), checkRect.Y + (9 * s)),
					new PointF(checkRect.X + (10 * s), checkRect.Y + (3.5f * s))
				};

				var mode = g.SmoothingMode;
				g.SmoothingMode = SmoothingMode.AntiAlias;

				using var pen = new Pen(enabled
					? manager.GetColor("WindowText")
					: manager.GetColor("GrayText"), Math.Max(1.5f, s * 1.5f));

				g.DrawLines(pen, points);
				g.SmoothingMode = mode;
			}

			if (Focused && active == CheckStop && enabled)
			{
				using var focus = new Pen(manager.GetColor("HotTrack")) { DashStyle = DashStyle.Dot };
				var r = Rectangle.Inflate(checkRect, 2, 2);
				g.DrawRectangle(focus, r.X, r.Y, r.Width - 1, r.Height - 1);
			}
		}


		private void PaintButton(Graphics g, Rectangle bounds, Part part, bool up)
		{
			var enabled = Editable;
			var pressed = enabled && ((pressedPart == part && hotPart == part) ||
				(part == Part.DropButton && DroppedDown));
			var hot = enabled && hotPart == part;

			var back = pressed
				? manager.ButtonPressBorder
				: hot ? manager.ButtonHotBack : manager.ButtonBack;

			using (var brush = new SolidBrush(back))
			{
				g.FillRectangle(brush, bounds);
			}

			var color = enabled ? manager.GetColor("WindowText") : manager.GetColor("GrayText");

			var w = Scale(8);
			var h = Scale(4);
			var cx = bounds.Left + (bounds.Width / 2);
			var cy = bounds.Top + (bounds.Height / 2);

			var points = up
				? new[]
				{
					new Point(cx - (w / 2), cy + (h / 2)),
					new Point(cx + (w / 2), cy + (h / 2)),
					new Point(cx, cy - (h / 2))
				}
				: new[]
				{
					new Point(cx - (w / 2), cy - (h / 2)),
					new Point(cx + (w / 2), cy - (h / 2)),
					new Point(cx, cy + (h / 2))
				};

			var mode = g.SmoothingMode;
			g.SmoothingMode = SmoothingMode.AntiAlias;

			using (var brush = new SolidBrush(color))
			{
				g.FillPolygon(brush, points);
			}

			g.SmoothingMode = mode;
		}


		private void PaintDropButton(Graphics g)
		{
			PaintButton(g, dropRect, Part.DropButton, false);

			using var pen = new Pen(manager.ButtonBorder);
			g.DrawLine(pen, dropRect.Left, dropRect.Top, dropRect.Left, dropRect.Bottom - 1);
		}


		private void PaintUpDown(Graphics g)
		{
			PaintButton(g, upRect, Part.UpButton, true);
			PaintButton(g, downRect, Part.DownButton, false);

			using var pen = new Pen(manager.ButtonBorder);
			var left = upDownAlign == LeftRightAlignment.Left;
			var x = left ? upRect.Right - 1 : upRect.Left;

			g.DrawLine(pen, x, upRect.Top, x, downRect.Bottom - 1);
			g.DrawLine(pen, upRect.Left, upRect.Bottom - 1, upRect.Right - 1, upRect.Bottom - 1);
		}
		#endregion Painting


		#region Accessibility
		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new PickerAccessibleObject(this);
		}


		private sealed class PickerAccessibleObject : ControlAccessibleObject
		{
			private readonly MoreDateTimePicker picker;


			public PickerAccessibleObject(MoreDateTimePicker owner)
				: base(owner)
			{
				picker = owner;
			}


			public override AccessibleRole Role => AccessibleRole.ComboBox;


			public override string Value
			{
				get => picker.Text;
				set => picker.Text = value;
			}


			public override string DefaultAction => picker.DroppedDown ? "Close" : "Open";


			public override void DoDefaultAction()
			{
				if (picker.DroppedDown)
				{
					picker.CloseDropDown();
				}
				else
				{
					picker.OpenDropDown();
				}
			}


			public override AccessibleStates State
			{
				get
				{
					var state = base.State;
					state |= picker.DroppedDown
						? AccessibleStates.Expanded
						: AccessibleStates.Collapsed;

					return state;
				}
			}
		}
		#endregion Accessibility
	}
}
