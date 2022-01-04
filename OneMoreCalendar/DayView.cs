//************************************************************************************************
// Copyright © 2022 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;


	internal partial class DayView : UserControl, ICalendarView
	{
		private sealed class DayItem
		{
			public Rectangle Bounds { get; set; } = Rectangle.Empty;
			public DateTime Date { get; set; }
			public CalendarPages Pages { get; set; }
		}


		private const int HeadWidth = 170;
		private const int DateWidth = 170;
		private const int VPadding = 6;

		private DayItem hotday;
		private CalendarPage hotpage;
		private readonly IntPtr hand;
		private readonly Font hotFont;
		private readonly Font deletedFont;
		private readonly StringFormat format;


		public DayView()
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


		public event CalendarDayHandler ClickedDay;
		public event CalendarHoverHandler HoverPage;
		public event CalendarPageHandler ClickedPage;
		public event CalendarSnapshotHandler SnappedPage;


		public void SetRange(DateTime startDate, DateTime endDate, CalendarPages pages)
		{
			listbox.Items.Clear();

			var date = startDate;
			while (date <= endDate)
			{
				var daypages = new CalendarPages();
				daypages.AddRange(pages.Where(p =>
					p.Created.Date.Equals(date) || p.Modified.Date.Equals(date)));

				listbox.Items.Add(new DayItem
				{
					Date = date,
					Pages = daypages
				});

				date = date.AddDays(1);
			}

			Invalidate();
		}


		private void MeasureDay(object sender, MeasureItemEventArgs e)
		{
			if (listbox.Items[e.Index] is DayItem day)
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


		private void DrawDay(object sender, DrawItemEventArgs e)
		{
			e.Graphics.FillRectangle(e.Index % 2 == 1 ? AppColors.RowBrush : Brushes.White, e.Bounds);

			e.Graphics.DrawLine(Pens.LightGray, e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Top);
			//e.Graphics.DrawLine(Pens.LightGray, HeadWidth, e.Bounds.Top, HeadWidth, e.Bounds.Bottom);

			if (listbox.Items[e.Index] is DayItem day)
			{
				// set every time to handle scrolled view
				day.Bounds = e.Bounds;

				// header
				var head = day.Date.ToString("ddd, MMM d");
				var size = e.Graphics.MeasureString(head, listbox.Font);
				e.Graphics.DrawString(head, listbox.Font, Brushes.Black,
					(HeadWidth - size.Width) / 2,
					e.Bounds.Top + (e.Bounds.Height - size.Height) / 2);

				// pages
				var top = e.Bounds.Top + VPadding;
				foreach (var page in day.Pages)
				{
					size = e.Graphics.MeasureString(page.Title, listbox.Font);

					var bounds = new Rectangle(
						HeadWidth + 20, top, (int)size.Width + 2, (int)size.Height);

					e.Graphics.DrawString(page.Title,
						page.IsDeleted ? deletedFont : listbox.Font,
						Brushes.Black, bounds);

					page.Bounds = bounds;

					e.Graphics.DrawString(page.Created.ToShortFriendlyString(),
						listbox.Font, Brushes.Black, e.Bounds.Width - DateWidth * 2, top);

					e.Graphics.DrawString(page.Modified.ToShortFriendlyString(),
						listbox.Font, Brushes.Black, e.Bounds.Width - DateWidth, top);

					top += (int)size.Height;
				}
			}
		}

		private void ScrollDays(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Down)
			{
				var index = listbox.IndexFromPoint(new Point(listbox.Left + 5, listbox.Bottom - 5));
				if (index < listbox.Items.Count - 1)
				{
					listbox.TopIndex = index + 1;
				}
			}
			else if (e.KeyCode == Keys.Up)
			{
				var index = listbox.IndexFromPoint(new Point(listbox.Left + 5, listbox.Top + 5));
				if (index > 0)
				{
					listbox.TopIndex = index - 1;
				}
			}
		}


		private void HoverHighlight(object sender, MouseEventArgs e)
		{
			var day = listbox.Items.OfType<DayItem>()
				.FirstOrDefault(d => d.Bounds.Contains(e.Location));

			CalendarPage page = null;
			if (day != null)
			{
				page = day.Pages.FirstOrDefault(p => p.Bounds.Contains(e.Location));
			}

			if (page == hotpage)
			{
				if (hotpage != null)
				{
					Native.SetCursor(hand);
				}
				return;
			}

			if (hotpage != null)
			{
				using (var g = listbox.CreateGraphics())
				{
					var index = listbox.Items.IndexOf(hotday);
					g.FillRectangle(index % 2 == 1 ? AppColors.RowBrush : Brushes.White, hotpage.Bounds);

					g.DrawString(hotpage.Title, listbox.Font, Brushes.Black, hotpage.Bounds, format);
				}

				HoverPage?.Invoke(this, new CalendarPageEventArgs(null));

				hotday = null;
				hotpage = null;
			}

			if (page != null)
			{
				using (var g = listbox.CreateGraphics())
				{
					var index = listbox.Items.IndexOf(day);
					g.FillRectangle(index % 2 == 1 ? AppColors.RowBrush : Brushes.White, page.Bounds);

					g.DrawString(page.Title,
						page.IsDeleted ? deletedFont : hotFont,
						Brushes.DarkOrchid, page.Bounds, format);
				}

				HoverPage?.Invoke(this, new CalendarPageEventArgs(page));

				hotday = day;
				hotpage = page;
				Native.SetCursor(hand);
			}
		}

		private void ReviewView(object sender, EventArgs e)
		{
			listbox.Invalidate();
		}

		private void ClickPage(object sender, MouseEventArgs e)
		{
			var day = listbox.Items.OfType<DayItem>()
				.FirstOrDefault(d => d.Bounds.Contains(e.Location));

			if (day != null)
			{
				var page = day.Pages.FirstOrDefault(p => p.Bounds.Contains(e.Location));
				if (page != null)
				{
					ClickedPage?.Invoke(this, new CalendarPageEventArgs(page));
				}
			}
		}
	}
}
