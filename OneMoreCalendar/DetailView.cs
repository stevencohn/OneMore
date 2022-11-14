//************************************************************************************************
// Copyright © 2022 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

#pragma warning disable CS0067  // The event is never used

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
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
			public CalendarPages Pages { get; set; }
		}


		private const int HeadWidth = 170; // day
		private const int PathWidth = 250; // path
		private const int DateWidth = 170; // created, modified
		private const int VPadding = 6;

		private DayItem hotday;
		private CalendarPage hotpage;
		private readonly IntPtr hand;
		private readonly Font hotFont;
		private readonly Font deletedFont;
		private readonly StringFormat format;


		public DetailView()
		{
			InitializeComponent();

			hand = Native.LoadCursor(IntPtr.Zero, Native.IDC_HAND);
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
		public event CalendarSnapshotHandler SnappedPage;


		public void SetRange(DateTime startDate, DateTime endDate, CalendarPages pages)
		{
			SuspendLayout();
			listbox.Items.Clear();

			var date = startDate;
			while (date <= endDate)
			{
				var settings = new SettingsProvider();

				var daypages = new CalendarPages();

				// filtering prioritizes modified over created and prevent pages from being
				// displayed twice in the month if both created and modified in the same month
				daypages.AddRange(pages.Where(p =>
					(settings.Modified && p.Modified.Date.Equals(date)) ||
					(!settings.Modified && p.Created.Date.Equals(date))
					));

				if (daypages.Any() || settings.Empty)
				{
					var item = new ListViewItem
					{
						Tag = new DayItem
						{
							Date = date,
							Pages = daypages
						}
					};

					listbox.Items.Add(item);
				}

				date = date.AddDays(1);
			}

			Invalidate();
			ResumeLayout();
		}


		private void HeaderPanelPaint(object sender, PaintEventArgs e)
		{
			e.Graphics.Clear(BackColor);

			using var font = new Font("Segoe UI Light", 10.0f, FontStyle.Regular);
			headerPanel.Height = font.Height + VPadding;
			var y = (headerPanel.Height - font.Height) / 2;

			var size = e.Graphics.MeasureString("DATE", font);
			var width = e.ClipRectangle.Width - SystemInformation.VerticalScrollBarWidth;

			using var brush = new SolidBrush(Theme.MonthDayFore);

			e.Graphics.DrawString("DATE", font, brush, (HeadWidth - size.Width) / 2, y);
			e.Graphics.DrawString("SECTION", font, brush, HeadWidth + 20, y);
			e.Graphics.DrawString("PAGE", font, brush, HeadWidth + PathWidth + 40, y);
			e.Graphics.DrawString("CREATED", font, brush, width - DateWidth * 2, y);
			e.Graphics.DrawString("MODIFIED", font, brush, width - DateWidth, y);
		}


		private void ListBoxMeasureItem(object sender, MeasureItemEventArgs e)
		{
			if (listbox.Items[e.Index] is ListViewItem item && item.Tag is DayItem day)
			{
				e.ItemHeight = day.Pages.Count > 1
					? (day.Pages.Count * listbox.Font.Height) + (VPadding * 3)
					: listbox.Font.Height + (VPadding * 2);
			}
			else
			{
				e.ItemHeight = listbox.Font.Height + (VPadding * 2);
			}

			// entire width of grid
			e.ItemWidth = Width;
		}


		private void ListBoxDrawItem(object sender, DrawItemEventArgs e)
		{
			if (e.Index < 0)
			{
				return;
			}

			using var fill = new SolidBrush(e.Index % 2 == 1 ? Theme.DetailOddBack : Theme.DetailEvenBack);
			e.Graphics.FillRectangle(fill, e.Bounds);

			using var line = new Pen(Theme.MonthGrid);
			e.Graphics.DrawLine(line, e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Top);
			//e.Graphics.DrawLine(Pens.LightGray, HeadWidth, e.Bounds.Top, HeadWidth, e.Bounds.Bottom);

			if (listbox.Items[e.Index] is ListViewItem item && item.Tag is DayItem day)
			{
				// set every time to handle scrolled view
				day.Bounds = e.Bounds;

				// header
				var head = day.Date.ToString("ddd, MMM d");
				var size = e.Graphics.MeasureString(head, listbox.Font);

				using var fore = new SolidBrush(Theme.ForeColor);
				using var gray = new SolidBrush(Color.Gray);

				e.Graphics.DrawString(head, listbox.Font, fore,
					(HeadWidth - size.Width) / 2,
					e.Bounds.Top + (e.Bounds.Height - size.Height) / 2);

				// pages
				var top = e.Bounds.Top + VPadding;
				foreach (var page in day.Pages)
				{
					// predict width of page title
					size = e.Graphics.MeasureString(page.Title, listbox.Font);

					var color = page.IsDeleted ? gray : fore;

					// section
					e.Graphics.DrawString(page.Path,
						listbox.Font, color, HeadWidth + 20, top, format);

					// title
					var bounds = new Rectangle(
						HeadWidth + PathWidth + 40, top, (int)size.Width + 2, (int)size.Height);

					e.Graphics.DrawString(page.Title,
						page.IsDeleted ? deletedFont : listbox.Font,
						color, bounds, format);

					page.Bounds = bounds;

					// created
					e.Graphics.DrawString(page.Created.ToShortFriendlyString(),
						listbox.Font, color, e.Bounds.Width - DateWidth * 2, top);

					// modified
					e.Graphics.DrawString(page.Modified.ToShortFriendlyString(),
						listbox.Font, color, e.Bounds.Width - DateWidth, top);

					top += (int)size.Height;
				}
			}
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


		private void ListBoxMouseMove(object sender, MouseEventArgs e)
		{
			//Logger.Current.WriteLine($"moveto {e.Location}");

			if (listbox.Items.OfType<ListViewItem>()
				.FirstOrDefault(d =>
					listbox.GetItemRectangle(listbox.Items.IndexOf(d)).Contains(e.Location))?
				.Tag is not DayItem day)
			{
				return;
			}

			//Logger.Current.WriteLine($"day bounds {day.Bounds}");

			var page = day.Pages.FirstOrDefault(p => p.Bounds.Contains(e.Location));

			//Logger.Current.WriteLine($"page bounds {page.Bounds}");

			if (page == hotpage)
			{
				if (hotpage != null)
				{
					Native.SetCursor(hand);
				}

				return;
			}

			using var g = listbox.CreateGraphics();
			int index;

			if (hotpage != null)
			{
				index = listbox.Items.OfType<ListViewItem>()
					.Where(item => item.Tag == hotday)
					.Select(item => listbox.Items.IndexOf(item))
					.FirstOrDefault();

				using var fill = new SolidBrush(index % 2 == 1 ? Theme.DetailOddBack : Theme.DetailEvenBack);
				g.FillRectangle(fill, hotpage.Bounds);

				using var fore = new SolidBrush(hotpage.IsDeleted ? Color.Gray : Theme.ForeColor);

				g.DrawString(hotpage.Title,
					hotpage.IsDeleted ? deletedFont : listbox.Font,
					fore,
					hotpage.Bounds, format);

				HoverPage?.Invoke(this, new CalendarPageEventArgs(null));

				hotday = null;
				hotpage = null;
			}

			if (page != null)
			{
				index = listbox.Items.OfType<ListViewItem>()
						.Where(item => item.Tag == day)
						.Select(item => listbox.Items.IndexOf(item))
						.FirstOrDefault();

				using var fill2 = new SolidBrush(index % 2 == 1 ? Theme.DetailOddBack : Theme.DetailEvenBack);
				g.FillRectangle(fill2, page.Bounds);

				using var fore2 = new SolidBrush(Theme.Highlight);
				g.DrawString(page.Title,
					page.IsDeleted ? deletedFont : hotFont,
					fore2, page.Bounds, format);

				HoverPage?.Invoke(this, new CalendarPageEventArgs(page));

				hotday = day;
				hotpage = page;
				Native.SetCursor(hand);
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
			if (listbox.Items.OfType<ListViewItem>()
				.FirstOrDefault(d =>
					listbox.GetItemRectangle(listbox.Items.IndexOf(d)).Contains(e.Location))?
				.Tag is not DayItem day)
			{
				return;
			}

			var page = day.Pages.FirstOrDefault(p => p.Bounds.Contains(e.Location));
			if (page == null)
			{
				return;
			}

			if (e.Button == MouseButtons.Right)
			{
				SnappedPage?.Invoke(this, new CalendarSnapshotEventArgs(page, page.Bounds));
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
