//************************************************************************************************
// Copyright © 2022 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

#pragma warning disable CS0067  // The event is never used

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Commands;
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;


	internal partial class DetailView : ThemedUserControl, ICalendarView
	{
		private sealed class DayItem
		{
			public Rectangle Bounds { get; set; } = Rectangle.Empty;
			public DateTime Date { get; set; }

			// null for an empty day
			public CalendarPage Page { get; set; }

			// first row of its day; only this row shows the day header
			public bool IsFirst { get; set; }

			// index of the day, used to alternate background colors by day
			public int DayIndex { get; set; }
		}


		// logical (96 DPI) pixel widths, scaled to the control's real DPI at each use so the
		// layout renders at the same physical size regardless of monitor scaling
		private int HeadWidth => this.Scaled(170); // day
		private int BellWidth => this.Scaled(40);  // reminders
		private int PathWidth => this.Scaled(250); // path
		private int DateWidth => this.Scaled(170); // created, modified
		private int VPadding => this.Scaled(6);

		private int lineHeight;
		private DayItem hotrow;
		private CalendarPage hotpage;
		private readonly Font hotFont;
		private readonly Font deletedFont;
		private readonly StringFormat format;


		public DetailView()
		{
			InitializeComponent();

			hotFont = new Font(listbox.Font, FontStyle.Regular | FontStyle.Underline);
			deletedFont = new Font(listbox.Font, FontStyle.Regular | FontStyle.Strikeout);

			format = new StringFormat
			{
				Trimming = StringTrimming.EllipsisCharacter,
				FormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoWrap
			};
		}


		public override void OnThemeChange()
		{
			BackColor = Theme.BackColor;
			listbox.BackColor = BackColor;
		}


		public event CalendarDayHandler ClickedDay;
		public event CalendarHoverHandler HoverPage;
		public event CalendarPageHandler ClickedPage;
		public event CalendarPageMenuHandler PageMenu;

		// DetailView does not support creating pages; declared only to satisfy ICalendarView
		public event CalendarCreatedPageHandler ClickedCreatePage;


		public void SetRange(DateTime startDate, DateTime endDate, CalendarPages pages)
		{
			SuspendLayout();
			listbox.Items.Clear();

			// every item is one page row so scrolling advances one page at a time
			// measure real glyph height, as MonthView does, since Font.Height includes generous leading
			using (var g = listbox.CreateGraphics())
			{
				lineHeight = (int)Math.Ceiling(g.MeasureString("Ap", listbox.Font).Height);
			}

			listbox.ItemHeight = lineHeight + VPadding;

			var settings = SettingsProvider.Current;
			var modified = settings.Modified;
			var created = settings.Created;
			var empty = settings.Empty;

			var date = startDate;
			var dayIndex = 0;
			while (date <= endDate)
			{
				var daypages = new CalendarPages();

				// filtering prioritizes modified over created and prevent pages from being
				// displayed twice in the month if both created and modified in the same month
				daypages.AddRange(pages.Where(p =>
					(modified && p.Modified.Date.Equals(date)) ||
					(created && p.Created.Date.Equals(date))
					));

				if (daypages.Any() || empty)
				{
					if (daypages.Any())
					{
						var first = true;
						foreach (var page in daypages)
						{
							listbox.Items.Add(new ListViewItem
							{
								Tag = new DayItem
								{
									Date = date,
									Page = page,
									IsFirst = first,
									DayIndex = dayIndex
								}
							});

							first = false;
						}
					}
					else
					{
						listbox.Items.Add(new ListViewItem
						{
							Tag = new DayItem
							{
								Date = date,
								IsFirst = true,
								DayIndex = dayIndex
							}
						});
					}

					dayIndex++;
				}

				date = date.AddDays(1);
			}

			Invalidate();
			ResumeLayout();
		}


		/// <summary>
		/// Scrolls the list so the given day's first page row is the first visible row, unless
		/// the end of the list is reached first, in which case the last row is at the bottom.
		/// </summary>
		/// <param name="day">
		/// The day to show; if that day has no row (empty days may be hidden) then the next
		/// day that does is used
		/// </param>
		public void ScrollToDay(DateTime day)
		{
			// defer until after layout, and until the control has a handle the first time
			BeginInvoke(new Action(() =>
			{
				var index = -1;
				for (var i = 0; i < listbox.Items.Count; i++)
				{
					if (listbox.Items[i] is ListViewItem item &&
						item.Tag is DayItem dayItem &&
						dayItem.Date.Date >= day.Date)
					{
						index = i;
						break;
					}
				}

				if (index < 0)
				{
					index = listbox.Items.Count - 1;
				}

				if (index >= 0)
				{
					// don't scroll past the point where the last row is at the bottom
					var visible = Math.Max(1, listbox.ClientSize.Height / listbox.ItemHeight);
					var maxTop = Math.Max(0, listbox.Items.Count - visible);
					listbox.TopIndex = Math.Min(index, maxTop);
					listbox.Invalidate();
				}
			}));
		}


		private void HeaderPanelPaint(object sender, PaintEventArgs e)
		{
			e.Graphics.Clear(BackColor);

			using var font = new Font("Segoe UI Light", 10.0f, FontStyle.Regular);
			headerPanel.Height = font.Height + VPadding;
			var y = (headerPanel.Height - font.Height) / 2;

			var dateHead = Properties.Resources.DetailView_Date;
			var size = e.Graphics.MeasureString(dateHead, font);
			var width = e.ClipRectangle.Width - SystemInformation.VerticalScrollBarWidth;

			using var brush = new SolidBrush(Theme.MonthDayFore);

			e.Graphics.DrawString(dateHead, font, brush, (HeadWidth - size.Width) / 2, y);
			e.Graphics.DrawString(Properties.Resources.DetailView_Section, font, brush, HeadWidth + this.Scaled(20), y);

			var editor = new ImageEditor { Style = ImageEditor.Stylization.GrayScale };
			using var gray = editor.Apply(Properties.Resources.Reminder_01_24_Y);

			e.Graphics.DrawImage(gray,
				HeadWidth + PathWidth + this.Scaled(40) + (BellWidth - this.Scaled(15)),
				y + this.Scaled(3), this.Scaled(12f), this.Scaled(12f));

			e.Graphics.DrawString(Properties.Resources.DetailView_Page, font, brush,
				HeadWidth + PathWidth + BellWidth + this.Scaled(60), y);

			e.Graphics.DrawString(Properties.Resources.DetailView_Created, font, brush,
				width - DateWidth * 2, y);

			e.Graphics.DrawString(Properties.Resources.DetailView_Modified, font, brush,
				width - DateWidth, y);
		}


		private void ListBoxDrawItem(object sender, DrawItemEventArgs e)
		{
			if (e.Index < 0)
			{
				return;
			}

			if (listbox.Items[e.Index] is not ListViewItem item || item.Tag is not DayItem row)
			{
				return;
			}

			// alternate by day so all page rows of a day share a background
			using var fill = new SolidBrush(row.DayIndex % 2 == 1 ? Theme.DetailOddBack : Theme.DetailEvenBack);
			e.Graphics.FillRectangle(fill, e.Bounds);

			using var fore = new SolidBrush(Theme.ForeColor);
			using var gray = new SolidBrush(Color.Gray);

			if (row.IsFirst)
			{
				using var line = new Pen(Theme.MonthGrid);
				e.Graphics.DrawLine(line, e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Top);
			}

			// header, only on the first row of each day
			var top = e.Bounds.Top + (VPadding / 2);
			if (row.IsFirst)
			{
				var head = row.Date.ToString("ddd, MMM d");
				var size = e.Graphics.MeasureString(head, listbox.Font);

				e.Graphics.DrawString(head, listbox.Font, fore, (HeadWidth - size.Width) / 2, top);
			}

			var page = row.Page;
			if (page is null)
			{
				return;
			}

			var color = page.IsDeleted ? gray : fore;

			// section color swatch
			if (page.SectionColor != Color.Empty && SettingsProvider.Current.Markers)
			{
				var swatchRect = new RectangleF(
					HeadWidth + this.Scaled(6), top + this.Scaled(1),
					this.Scaled(4), lineHeight - this.Scaled(2));

				using var swatchBrush = new SolidBrush(page.SectionColor);
				e.Graphics.FillRectangle(swatchBrush, swatchRect);
			}

			// section
			var sectionBounds = new RectangleF(HeadWidth + this.Scaled(20), top, PathWidth, lineHeight);
			e.Graphics.DrawString(page.Path, listbox.Font, color, sectionBounds, format);

			// reminder
			if (page.HasReminders)
			{
				e.Graphics.DrawImage(Properties.Resources.Reminder_01_24_Y,
					HeadWidth + PathWidth + this.Scaled(40) + (BellWidth - this.Scaled(15)),
					top + this.Scaled(3), this.Scaled(12f), this.Scaled(12f));
			}

			// title
			var titleX = HeadWidth + PathWidth + BellWidth + this.Scaled(60);
			var titleWidth = Math.Max(0, e.Bounds.Width - DateWidth * 2 - titleX);
			var bounds = new Rectangle(titleX, top, titleWidth, lineHeight);

			var textSize = TitleRenderer.DrawTitle(e.Graphics, page.Title,
				page.IsDeleted ? deletedFont : listbox.Font,
				color, bounds, format);

			// set every time to handle scrolled view; hit area is only the text, not the column
			bounds.Width = Math.Min(titleWidth, textSize.Width + this.Scaled(2));
			page.Bounds = bounds;

			// created
			e.Graphics.DrawString(page.Created.ToShortFriendlyString(),
				listbox.Font, color, e.Bounds.Width - DateWidth * 2, top);

			// modified
			e.Graphics.DrawString(page.Modified.ToShortFriendlyString(),
				listbox.Font, color, e.Bounds.Width - DateWidth, top);
		}

		private void ListBoxKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Down)
			{
				listbox.ScrollDown();
				//var index = listbox.IndexFromPoint(new Point(listbox.Left + 5, listbox.Bottom - 5));
				//if (index < listbox.Items.Count - 1)
				//{
				//	listbox.TopIndex = index + 1;
				//}
			}
			else if (e.KeyCode == Keys.Up)
			{
				listbox.ScrollUp();
				//var index = listbox.IndexFromPoint(new Point(listbox.Left + 5, listbox.Top + 5));
				//if (index > 0)
				//{
				//	listbox.TopIndex = index - 1;
				//}
			}
		}


		/// <summary>
		/// Expands a page's text hit bounds to the full title column for drawing.
		/// </summary>
		private Rectangle TitleArea(Rectangle hit)
		{
			var width = listbox.ClientSize.Width - DateWidth * 2 - hit.X;
			return new Rectangle(hit.X, hit.Y, Math.Max(0, width), hit.Height);
		}


		private void ListBoxMouseMove(object sender, MouseEventArgs e)
		{
			//Logger.Current.Verbose($"moveto {e.Location}");

			var index = listbox.IndexFromPoint(e.Location);
			if (index < 0 ||
				listbox.Items[index] is not ListViewItem hit ||
				hit.Tag is not DayItem row)
			{
				return;
			}

			var page = row.Page is not null && row.Page.Bounds.Contains(e.Location) ? row.Page : null;

			if (page == hotpage)
			{
				if (hotpage != null)
				{
					Cursor = Cursors.Hand;
				}

				return;
			}

			using var g = listbox.CreateGraphics();

			if (hotpage != null)
			{
				using var fill = new SolidBrush(hotrow.DayIndex % 2 == 1 ? Theme.DetailOddBack : Theme.DetailEvenBack);
				g.FillRectangle(fill, TitleArea(hotpage.Bounds));

				using var fore = new SolidBrush(hotpage.IsDeleted ? Color.Gray : Theme.ForeColor);

				TitleRenderer.DrawTitle(g, hotpage.Title,
					hotpage.IsDeleted ? deletedFont : listbox.Font,
					fore,
					TitleArea(hotpage.Bounds), format);

				HoverPage?.Invoke(this, new CalendarPageEventArgs(null));

				hotrow = null;
				hotpage = null;
				Cursor = Cursors.Default;
			}

			if (page != null)
			{
				using var fill2 = new SolidBrush(row.DayIndex % 2 == 1 ? Theme.DetailOddBack : Theme.DetailEvenBack);
				g.FillRectangle(fill2, TitleArea(page.Bounds));

				using var fore2 = new SolidBrush(Theme.Highlight);
				TitleRenderer.DrawTitle(g, page.Title,
					page.IsDeleted ? deletedFont : hotFont,
					fore2, TitleArea(page.Bounds), format);

				HoverPage?.Invoke(this, new CalendarPageEventArgs(page));

				hotrow = row;
				hotpage = page;
				Cursor = Cursors.Hand;
			}
		}


		private void ListBoxResize(object sender, EventArgs e)
		{
			headerPanel.Invalidate();
			listbox.Invalidate();
		}


		/*
		 * Note that MouseClick event doesn't capture right-clicks but MouseUp does.
		 * Also note that if the control overrides OnMouseClick then that *will* capture both
		 * left and right buttons. Windows Forms is fun!
		 * 
		 */
		private void ListBoxMouseUp(object sender, MouseEventArgs e)
		{
			var index = listbox.IndexFromPoint(e.Location);
			if (index < 0 ||
				listbox.Items[index] is not ListViewItem hit ||
				hit.Tag is not DayItem row)
			{
				return;
			}

			var page = row.Page is not null && row.Page.Bounds.Contains(e.Location) ? row.Page : null;
			if (page == null)
			{
				return;
			}

			if (e.Button == MouseButtons.Right)
			{
				PageMenu?.Invoke(this, new CalendarPageMenuEventArgs(
					page, page.Bounds, listbox.PointToScreen(e.Location)));
			}
			else
			{
				ClickedPage?.Invoke(this, new CalendarPageEventArgs(page));
			}
		}


		private void ListBoxScrolled(object sender, ScrollEventArgs e)
		{
			listbox.Invalidate();
			Logger.Current.WriteLine(
				$"scrolled {e.Type} @ {e.ScrollOrientation}, {e.OldValue} >> {e.NewValue}");
		}
	}
}
