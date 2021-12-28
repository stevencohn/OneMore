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
			public Rectangle Clip;
			public DateTime Date;
			public CalendarItem Item;
		}

		private const string HeadBackColor = "#FFF4E8F3";
		private const string TodayHeadColor = "#FFD6A6D3";

		private readonly IntPtr hand;
		private readonly Font itemFont;
		private readonly Font hotFont;
		private readonly StringFormat format;
		private readonly List<Hotspot> hotspots;
		private Hotspot hotspot;
		private int dowOffset;

		private DateTime date;
		private CalendarDays days;
		private DayOfWeek firstDow;

		public event CalendarDayHandler ClickedDay;
		public event CalendarPageHandler ClickedPage;


		public MonthView()
		{
			InitializeComponent();

			hand = Native.LoadCursor(IntPtr.Zero, Native.IDC_HAND);
			itemFont = new Font("Segoe UI", 9.0f, FontStyle.Regular);
			hotFont = new Font("Segoe UI", 9.0f, FontStyle.Regular | FontStyle.Underline);

			hotspots = new List<Hotspot>();

			format = new StringFormat
			{
				Trimming = StringTrimming.EllipsisCharacter,
				FormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoWrap
			};
		}


		public MonthView(DateTime date, CalendarItems items)
			: this()
		{
			SetRange(date, date, items);
		}


		public DateTime Date => date;


		public void SetRange(DateTime startDate, DateTime endDate, CalendarItems items)
		{
			date = new DateTime(startDate.Year, startDate.Month, 1).Date;

			firstDow = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.FirstDayOfWeek;
			MakeDayList(items);

			Invalidate();
		}


		private void MakeDayList(CalendarItems items)
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
					days.Add(new CalendarDay { Date = runner });
					runner = runner.AddDays(1.0);
				}
			}

			// month

			for (int i = 1; i <= last; i++)
			{
				var day = new CalendarDay { Date = runner, InMonth = true };

				var pp = items.Where(p => p.Modified.Date.Equals(runner));
				if (pp.Any())
				{
					foreach (var p in pp)
					{
						day.Items.Add(p);
					}
				}

				days.Add(day);
				runner = runner.AddDays(1.0);
			}

			// next month

			var rest = 7 - days.Count % 7;
			for (int i = 0; i < rest; i++)
			{
				days.Add(new CalendarDay { Date = runner });
				runner = runner.AddDays(1.0);
			}
		}


		protected void SuspendDrawing(Action action)
		{
			Native.SendMessage(Handle, Native.WM_SETREDRAW, false, 0);
			action();
			Native.SendMessage(Handle, Native.WM_SETREDRAW, true, 0);
			Refresh();
		}


		protected override void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);
			var spot = hotspots.FirstOrDefault(h => h.Clip.Contains(e.Location));
			if (spot != null)
			{
				if (spot.Item != null)
				{
					ClickedPage?.Invoke(this, new CalendarPageEventArgs(spot.Item.PageID));
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

			var spot = hotspots.FirstOrDefault(h => h.Clip.Contains(e.Location));
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
				if (hotspot.Item != null)
				{
					using (var g = CreateGraphics())
					{
						g.FillRectangle(
							hotspot.Item.Modified.Month == date.Month ? Brushes.White : Brushes.WhiteSmoke,
							hotspot.Clip);

						g.DrawString(hotspot.Item.Title, itemFont, Brushes.Black, hotspot.Clip, format);
					}
				}

				hotspot = null;
				Native.SetCursor(Cursors.Default.Handle);
			}

			if (spot != null)
			{
				if (spot.Item != null)
				{
					using (var g = CreateGraphics())
					{
						g.FillRectangle(Brushes.White, spot.Clip);
						g.DrawString(spot.Item.Title, hotFont, Brushes.Blue, spot.Clip, format);
					}
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

			var dayHeight = (Height - dowOffset) / 5;
			for (int i = 1; i < 5; i++)
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
			var dayHeight = (Height - dowOffset) / 5;
			var row = 0;
			var col = 0;

			var headFont = new Font("Segoe UI", 10.0f, FontStyle.Regular);
			var headFore = new SolidBrush(ColorTranslator.FromHtml(TodayHeadColor));
			var headBack = new SolidBrush(ColorTranslator.FromHtml(HeadBackColor));
			var headPen = new Pen(Color.DarkGray, 0.1f);

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
					Clip = box,
					Date = day.Date
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

				if (day.Items.Count > 0)
				{
					box = new Rectangle(
						col * dayWidth + 3, row * dayHeight + headFont.Height + 6 + dowOffset,
						dayWidth - 8, dayHeight - headFont.Height - 8
						);

					int i = 0;
					foreach (var item in day.Items)
					{
						var clip = new Rectangle(
							box.Left, box.Top + (itemFont.Height * i),
							box.Width, itemFont.Height);

						e.Graphics.DrawString(item.Title, itemFont, Brushes.Black, clip, format);

						hotspots.Add(new Hotspot
						{
							Clip = clip,
							Item = item
						});

						i++;
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
