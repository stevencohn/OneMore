//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System.Drawing;
	using System.Windows.Forms;


	internal partial class MonthView : UserControl
	{
		public MonthView()
		{
			InitializeComponent();
		}


		public MonthView(CalendarDays days)
			: this()
		{
			Days = days;
		}


		public CalendarDays Days { get; private set; }


		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

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

			var font = new Font("Segoe UI", 10.0f, FontStyle.Regular);
			var bg = new SolidBrush(ColorTranslator.FromHtml("#FFF4E8F3"));
			var pen = new Pen(Color.DarkGray, 0.1f);

			foreach (var day in Days)
			{
				// header...

				var box = new Rectangle(
					col * dayWidth, row * dayHeight,
					dayWidth, font.Height + 2);

				e.Graphics.FillRectangle(bg, box);
				e.Graphics.DrawRectangle(pen, box);

				e.Graphics.DrawString(day.Date.Day.ToString(), font,
					day.InMonth ? Brushes.Black : Brushes.Gray,
					box.X + 3, box.Y + 1);

				// body...

				box = new Rectangle(
					col * dayWidth + 1, row * dayHeight + font.Height + 3,
					dayWidth - 2, dayHeight - font.Height - 2
					);

				if (!day.InMonth)
				{
					e.Graphics.FillRectangle(Brushes.WhiteSmoke, box);
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
	}
}
