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
	using System.Windows.Forms;


	internal partial class MonthView : UserControl
	{
		private sealed class Hotspot
		{
			public CalendarItem Item;
			public Rectangle Clip;
		}

		private const string HeadBackColor = "#FFF4E8F3";
		private const string TodayHeadColor = "#FFD6A6D3";

		private readonly IntPtr hcursor;
		private readonly Font itemFont;
		private readonly Font hotFont;
		private readonly StringFormat format;
		private readonly List<Hotspot> hotspots = new List<Hotspot>();
		private Hotspot hotspot;
		private int month;


		public MonthView()
		{
			InitializeComponent();

			hcursor = Native.LoadCursor(IntPtr.Zero, Native.IDC_HAND);
			itemFont = new Font("Segoe UI", 9.0f, FontStyle.Regular);
			hotFont = new Font("Segoe UI", 9.0f, FontStyle.Regular | FontStyle.Underline);

			format = new StringFormat
			{
				Trimming = StringTrimming.EllipsisCharacter,
				FormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoWrap
			};
		}


		public MonthView(DateTime time, CalendarDays days)
			: this()
		{
			month = time.Month;
			Days = days;
		}


		public CalendarDays Days { get; set; }


		protected void SuspendDrawing(Action action)
		{
			Native.SendMessage(Handle, Native.WM_SETREDRAW, false, 0);
			action();
			Native.SendMessage(Handle, Native.WM_SETREDRAW, true, 0);
			Refresh();
		}


		protected override async void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);
			var spot = hotspots.FirstOrDefault(h => h.Clip.Contains(e.Location));
			if (spot != null)
			{
				using (var one = new OneNote())
				{
					var url = one.GetHyperlink(spot.Item.PageID, string.Empty);
					if (url != null)
					{
						await one.NavigateTo(url);
					}
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
					Native.SetCursor(hcursor);
				}
				return;
			}

			if (hotspot != null)
			{
				using (var g = CreateGraphics())
				{
					var brush = hotspot.Item.Modified.Month == month ? Brushes.White : Brushes.WhiteSmoke;
					g.FillRectangle(brush, hotspot.Clip);
					g.DrawString(hotspot.Item.Title, itemFont, Brushes.Black, hotspot.Clip, format);
				}

				hotspot = null;
				Native.SetCursor(Cursors.Default.Handle);
			}

			if (spot != null)
			{
				using (var g = CreateGraphics())
				{
					g.FillRectangle(Brushes.White, spot.Clip);
					g.DrawString(spot.Item.Title, hotFont, Brushes.Blue, spot.Clip, format);
				}

				hotspot = spot;
				Native.SetCursor(hcursor);
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

			var pen = new Pen(Color.DarkGray, 0.1f);

			var dayWidth = Width / 7;
			for (int i = 1; i < 7; i++)
			{
				e.Graphics.DrawLine(pen, i * dayWidth, 0, i * dayWidth, e.ClipRectangle.Height);
			}

			var dayHeight = Height / 5;
			for (int i = 1; i < 5; i++)
			{
				e.Graphics.DrawLine(pen, 0, i * dayHeight, e.ClipRectangle.Width, i * dayHeight);
			}
		}


		private void PaintDays(PaintEventArgs e)
		{
			var dayWidth = Width / 7;
			var dayHeight = Height / 5;
			var row = 0;
			var col = 0;

			var headFont = new Font("Segoe UI", 10.0f, FontStyle.Regular);
			var headBack = new SolidBrush(ColorTranslator.FromHtml(HeadBackColor));
			var headPen = new Pen(Color.DarkGray, 0.1f);

			var now = DateTime.Now;

			foreach (var day in Days)
			{
				// header...

				var box = new Rectangle(
					col * dayWidth, row * dayHeight,
					dayWidth, headFont.Height + 2);

				if (day.Date.Year == now.Year && day.Date.Month == now.Month && day.Date.Day == now.Day)
				{
					e.Graphics.FillRectangle(new SolidBrush(ColorTranslator.FromHtml(TodayHeadColor)), box);
				}
				else
				{
					e.Graphics.FillRectangle(headBack, box);
				}

				e.Graphics.DrawRectangle(headPen, box);

				e.Graphics.DrawString(day.Date.Day.ToString(), headFont,
					day.InMonth ? Brushes.Black : Brushes.Gray,
					box.X + 3, box.Y + 1);

				// body...

				if (!day.InMonth)
				{
					box = new Rectangle(
						col * dayWidth + 1, row * dayHeight + headFont.Height + 3,
						dayWidth - 2, dayHeight - headFont.Height - 2
						);

					e.Graphics.FillRectangle(Brushes.WhiteSmoke, box);
				}

				if (day.Items.Count > 0)
				{
					box = new Rectangle(
						col * dayWidth + 3, row * dayHeight + headFont.Height + 6,
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
							Item = item,
							Clip = clip
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
		}


		protected override void OnResize(System.EventArgs e)
		{
			base.OnResize(e);
			Invalidate();
		}


		//protected override void WndProc(ref Message msg)
		//{
		//	if (msg.Msg == Native.WM_SETCURSOR && hcursor != IntPtr.Zero)
		//	{
		//		Native.SetCursor(hcursor);
		//		msg.Result = IntPtr.Zero; // indicate handled
		//		return;
		//	}

		//	base.WndProc(ref msg);
		//}
	}
}
