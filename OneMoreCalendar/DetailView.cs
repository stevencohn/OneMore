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


	internal partial class DetailView : UserControl, ICalendarView
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
					listbox.Items.Add(new DayItem
					{
						Date = date,
						Pages = daypages
					});
				}

				date = date.AddDays(1);
			}

			Invalidate();
		}


		private void HeaderPanelPaint(object sender, PaintEventArgs e)
		{
			using (var font = new Font("Segoe UI Light", 10.0f, FontStyle.Regular))
			{
				headerPanel.Height = font.Height + VPadding;
				var y = (headerPanel.Height - font.Height) / 2;

				var size = e.Graphics.MeasureString("DATE", font);
				e.Graphics.DrawString("DATE", font, Brushes.SlateGray,
					(HeadWidth - size.Width) / 2, y);

				var width = e.ClipRectangle.Width - SystemInformation.VerticalScrollBarWidth;

				e.Graphics.DrawString("SECTION", font, Brushes.SlateGray, HeadWidth + 20, y);
				e.Graphics.DrawString("PAGE", font, Brushes.SlateGray, HeadWidth + PathWidth + 40, y);
				e.Graphics.DrawString("CREATED", font, Brushes.SlateGray, width - DateWidth * 2, y);
				e.Graphics.DrawString("MODIFIED", font, Brushes.SlateGray, width - DateWidth, y);
			}
		}


		private void ListBoxMeasureItem(object sender, MeasureItemEventArgs e)
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


		private void ListBoxDrawItem(object sender, DrawItemEventArgs e)
		{
			if (e.Index < 0)
			{
				return;
			}

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
					// predict width of page title
					size = e.Graphics.MeasureString(page.Title, listbox.Font);

					var color = page.IsDeleted ? Brushes.Gray : Brushes.Black;

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

					g.DrawString(hotpage.Title,
						hotpage.IsDeleted ? deletedFont : listbox.Font,
						hotpage.IsDeleted ? Brushes.Gray : Brushes.Black,
						hotpage.Bounds, format);
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
			var day = listbox.Items.OfType<DayItem>()
				.FirstOrDefault(d => d.Bounds.Contains(e.Location));

			if (day != null)
			{
				var page = day.Pages.FirstOrDefault(p => p.Bounds.Contains(e.Location));
				if (page != null)
				{
					if (e.Button == MouseButtons.Right)
					{
						SnappedPage?.Invoke(this, new CalendarSnapshotEventArgs(page, page.Bounds));
					}
					else
					{
						ClickedPage?.Invoke(this, new CalendarPageEventArgs(page));
					}
				}
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
