//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Threading;
	using System.Windows.Forms;


	internal partial class MonthView : UserControl, ICalendarView
	{
		private sealed class Hotspot
		{
			public Rectangle Bounds;
			public DateTime Date;
			public CalendarPage Page;
			public bool InMonth;
		}

		private const string HeadBackColor = "#FFF4E8F3";
		private const string TodayHeadColor = "#FFD6A6D3";
		private const string MoreGlyph = "⏷"; // \u23F7

		private readonly IntPtr hand;
		private readonly Font hotFont;
		private readonly Font moreFont;
		private readonly Font deletedFont;
		private readonly StringFormat format;
		private readonly List<Hotspot> hotspots;
		private Hotspot hotspot;
		private int dowOffset;
		private int weeks;

		private DateTime date;
		private CalendarDays days;
		private DayOfWeek firstDow;


		public MonthView()
		{
			InitializeComponent();

			hand = Native.LoadCursor(IntPtr.Zero, Native.IDC_HAND);
			hotFont = new Font(Font, FontStyle.Regular | FontStyle.Underline);
			deletedFont = new Font(Font, FontStyle.Regular | FontStyle.Strikeout);
			moreFont = new Font("Segoe UI", 14.0f, FontStyle.Regular);

			hotspots = new List<Hotspot>();

			format = new StringFormat
			{
				Trimming = StringTrimming.EllipsisCharacter,
				FormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoWrap
			};

			date = DateTime.Now.StartOfMonth();
		}


		public event CalendarDayHandler ClickedDay;

		public event CalendarHoverHandler HoverPage;

		public event CalendarPageHandler ClickedPage;

		public event CalendarSnapshotHandler SnappedPage;


		public void SetRange(DateTime startDate, DateTime endDate, CalendarPages pages)
		{
			date = startDate.StartOfMonth();

			firstDow = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.FirstDayOfWeek;
			MakeDayList(pages);

			Invalidate();
		}


		private void MakeDayList(CalendarPages pages)
		{
			days = new CalendarDays();

			var first = date.DayOfWeek;
			var last = DateTime.DaysInMonth(date.Year, date.Month);

			var dow = firstDow == DayOfWeek.Sunday
				? (int)first
				: first == DayOfWeek.Sunday ? 6 : (int)first - 1;

			var runner = date.Date;

			// previous month

			if (dow > 0)
			{
				runner = runner.AddDays(-dow);
				for (int i = 0; i < dow; i++)
				{
					var day = new CalendarDay { Date = runner };

					var pp = pages.Where(p => p.Modified.Date.Equals(runner));
					pp.ForEach(p => day.Pages.Add(p));

					days.Add(day);
					runner = runner.AddDays(1.0);
				}
			}

			// month

			for (int i = 1; i <= last; i++)
			{
				var day = new CalendarDay { Date = runner, InMonth = true };

				var pp = pages.Where(p => p.Modified.Date.Equals(runner));
				pp.ForEach(p => day.Pages.Add(p));

				days.Add(day);
				runner = runner.AddDays(1.0);
			}

			// next month

			var rest = 7 - days.Count % 7;
			if (rest < 7)
			{
				for (int i = 0; i < rest; i++)
				{
					var day = new CalendarDay { Date = runner };

					var pp = pages.Where(p => p.Modified.Date.Equals(runner));
					pp.ForEach(p => day.Pages.Add(p));

					days.Add(day);
					runner = runner.AddDays(1.0);
				}
			}
		}


		//protected void SuspendDrawing(Action action)
		//{
		//	Native.SendMessage(Handle, Native.WM_SETREDRAW, false, 0);
		//	action();
		//	Native.SendMessage(Handle, Native.WM_SETREDRAW, true, 0);
		//	Refresh();
		//}


		protected override async void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);

			Hotspot spot;

			if (e.Button == MouseButtons.Right)
			{
				spot = hotspots.FirstOrDefault(h => h.Bounds.Contains(e.Location));
				if (spot != null)
				{
					SnappedPage?.Invoke(this,
						new CalendarSnapshotEventArgs(spot.Page, spot.Bounds));
				}

				return;
			}

			spot = hotspots.FirstOrDefault(h => h.Bounds.Contains(e.Location));
			if (spot != null)
			{
				if (spot.Page != null)
				{
					ClickedPage?.Invoke(this, new CalendarPageEventArgs(spot.Page));
				}
				else
				{
					ClickedDay?.Invoke(this, new CalendarDayEventArgs(spot.Date));
				}
			}
		}


		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			var spot = hotspots.FirstOrDefault(h => h.Bounds.Contains(e.Location));
			if (spot == hotspot)
			{
				if (hotspot != null)
				{
					Native.SetCursor(hand);
				}
				return;
			}

			if (hotspot != null)
			{
				if (hotspot.Page != null)
				{
					using (var g = CreateGraphics())
					{
						g.FillRectangle(
							hotspot.Page.Modified.Month == date.Month ? Brushes.White : Brushes.WhiteSmoke,
							hotspot.Bounds);

						if (hotspot.Page.IsDeleted)
						{
							g.DrawString(hotspot.Page.Title, deletedFont, Brushes.Gray, hotspot.Bounds, format);
						}
						else
						{
							var brush = hotspot.InMonth ? Brushes.Black : Brushes.Gray;
							g.DrawString(hotspot.Page.Title, Font, brush, hotspot.Bounds, format);
						}
					}

					HoverPage?.Invoke(this, new CalendarPageEventArgs(null));
				}

				hotspot = null;
				Native.SetCursor(Cursors.Default.Handle);
			}

			if (spot != null)
			{
				if (spot.Page != null)
				{
					using (var g = CreateGraphics())
					{
						var brush = spot.InMonth ? Brushes.White : Brushes.WhiteSmoke;
						g.FillRectangle(brush, spot.Bounds);

						g.DrawString(spot.Page.Title, 
							spot.Page.IsDeleted ? deletedFont : hotFont,
							Brushes.DarkOrchid, spot.Bounds, format);
					}

					HoverPage?.Invoke(this, new CalendarPageEventArgs(spot.Page));
				}

				hotspot = spot;
				Native.SetCursor(hand);
			}
		}


		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			hotspots.Clear();

			PaintGrid(e);
			PaintDays(e);
		}


		private void PaintGrid(PaintEventArgs e)
		{
			e.Graphics.Clear(Color.White);

			// day of week names...

			var dowFont = new Font("Segoe UI Light", 10.0f, FontStyle.Regular);
			var culture = Thread.CurrentThread.CurrentUICulture.DateTimeFormat;
			dowOffset = dowFont.Height + 2;

			var dowFormat = new StringFormat
			{
				Alignment = StringAlignment.Center,
				FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit,
				Trimming = StringTrimming.EllipsisCharacter
			};

			var dayWidth = Width / 7;

			// day names and vertical lines...

			var pen = new Pen(Color.DarkGray, 0.1f);
			var dow = firstDow == DayOfWeek.Sunday ? 0 : 1;

			for (int i = 0; i < 7; i++, dow++)
			{
				var name = culture.GetDayName((DayOfWeek)(dow % 7)).ToUpper();
				var clip = new Rectangle(dayWidth * i, 1, dayWidth, dowFont.Height + 2);
				e.Graphics.DrawString(name, dowFont, Brushes.SlateGray, clip, dowFormat);

				if (i < 6)
				{
					var x = (i + 1) * dayWidth;
					e.Graphics.DrawLine(pen, x, dowOffset, x, e.ClipRectangle.Height);
				}
			}

			// horizontal lines...

			weeks = days.Count / 7;
			var dayHeight = (Height - dowOffset) / weeks;
			for (int i = 1; i < weeks; i++)
			{
				e.Graphics.DrawLine(pen,
					0, i * dayHeight + dowOffset,
					e.ClipRectangle.Width, i * dayHeight + dowOffset);
			}

			dowFormat.Dispose();
			dowFont.Dispose();
			pen.Dispose();
		}


		private void PaintDays(PaintEventArgs e)
		{
			var dayWidth = Width / 7;
			var dayHeight = (Height - dowOffset) / weeks;
			var row = 0;
			var col = 0;

			var headFont = new Font("Segoe UI", 10.0f, FontStyle.Regular);
			var headFore = new SolidBrush(ColorTranslator.FromHtml(TodayHeadColor));
			var headBack = new SolidBrush(ColorTranslator.FromHtml(HeadBackColor));
			var headPen = new Pen(Color.DarkGray, 0.1f);
			var moreSize = e.Graphics.MeasureString(MoreGlyph, Font);

			var now = DateTime.Now.Date;

			foreach (var day in days)
			{
				// header...

				var box = new Rectangle(
					col * dayWidth, row * dayHeight + dowOffset,
					dayWidth, headFont.Height + 2);

				e.Graphics.FillRectangle(
					// compare only date part
					day.Date.Date.Equals(now.Date) ? headFore : headBack,
					box);

				e.Graphics.DrawRectangle(headPen, box);

				e.Graphics.DrawString(day.Date.Day.ToString(), headFont,
					day.InMonth ? Brushes.Black : Brushes.Gray,
					box.X + 3, box.Y + 1);

				hotspots.Add(new Hotspot
				{
					Bounds = box,
					Date = day.Date,
					InMonth = day.InMonth
				});

				// body...

				if (!day.InMonth)
				{
					box = new Rectangle(
						col * dayWidth + 1, row * dayHeight + headFont.Height + 3 + dowOffset,
						dayWidth - 2, dayHeight - headFont.Height - 2
						);

					e.Graphics.FillRectangle(Brushes.WhiteSmoke, box);
				}

				if (day.Pages.Count > 0)
				{
					box = new Rectangle(
						col * dayWidth + 3, row * dayHeight + headFont.Height + 6 + dowOffset,
						dayWidth - 8, dayHeight - headFont.Height - 8
						);

					var maxItems = box.Height / Font.Height;

					for (int i = 0; i < day.Pages.Count && i < maxItems; i++)
					{
						var page = day.Pages[i];

						var clip = new Rectangle(
							box.Left, box.Top + (Font.Height * i),
							i == maxItems - 1 && day.Pages.Count > maxItems ? box.Width - (int)moreSize.Width : box.Width,
							Font.Height);

						e.Graphics.DrawString(page.Title,
							page.IsDeleted ? deletedFont : Font,
							page.IsDeleted || !day.InMonth ? Brushes.Gray : Brushes.Black, clip, format);

						hotspots.Add(new Hotspot
						{
							Bounds = clip,
							Page = page,
							InMonth = day.InMonth
						});
					}

					if (maxItems < day.Pages.Count)
					{
						e.Graphics.DrawString(MoreGlyph, moreFont, Brushes.DarkGray,
							box.Right - moreSize.Width - 1, box.Bottom - moreSize.Height - 4);
					}
				}

				col++;
				if (col > 6)
				{
					col = 0;
					row++;
				}
			}

			headFore.Dispose();
			headFont.Dispose();
			headBack.Dispose();
			headPen.Dispose();
		}


		protected override void OnResize(System.EventArgs e)
		{
			base.OnResize(e);
			Invalidate();
		}
	}
}
