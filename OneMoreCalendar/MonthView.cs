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

			var headFont = new Font("Segoe UI", 10.0f, FontStyle.Regular);
			var bodyFont = new Font("Segoe UI", 9.0f, FontStyle.Regular);
			var headBack = new SolidBrush(ColorTranslator.FromHtml("#FFF4E8F3"));
			var headPen = new Pen(Color.DarkGray, 0.1f);

			foreach (var day in Days)
			{
				// header...

				var box = new Rectangle(
					col * dayWidth, row * dayHeight,
					dayWidth, headFont.Height + 2);

				e.Graphics.FillRectangle(headBack, box);
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

					var format = new StringFormat(
						StringFormatFlags.LineLimit | StringFormatFlags.NoWrap, 1003)
					{
						Trimming = StringTrimming.EllipsisCharacter
					};

					int i = 0;
					foreach (var item in day.Items)
					{
						var clip = new Rectangle(
							box.Left, box.Top + (bodyFont.Height * i),
							box.Width, bodyFont.Height);

						e.Graphics.DrawString(item.Title, bodyFont, Brushes.Black, clip, format);
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
	}
}
