//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System.Drawing;
	using System.Windows.Forms;


	internal partial class MonthDayControl : UserControl
	{
		public MonthDayControl()
		{
			InitializeComponent();
		}


		public MonthDayControl(CalendarItem item)
			: this()
		{
			Item = item;
		}


		public CalendarItem Item { get; private set; }


		protected override void OnPaint(PaintEventArgs e)
		{
			//var clip = e.ClipRectangle;
			//var g = e.Graphics;

			e.Graphics.DrawRectangle(Pens.Red, e.ClipRectangle);

			// header...

			var font = new Font("Segoe UI", 10.0f, FontStyle.Regular);
			var box = new Rectangle(e.ClipRectangle.X, 0, e.ClipRectangle.Width - 1, font.Height + 2);
			var bg = new SolidBrush(ColorTranslator.FromHtml("#FFF4E8F3"));

			e.Graphics.FillRectangle(bg, box);
			e.Graphics.DrawRectangle(Pens.Black, box);

			// content...

			if (Item != null)
			{
				e.Graphics.DrawString(Item.Date.Day.ToString(), font, Brushes.Black, box.X + 3, box.Y + 1);


			}

			base.OnPaint(e);
		}
	}
}
