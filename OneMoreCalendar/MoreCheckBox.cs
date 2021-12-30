//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// Custom CheckBox with OneMore purple checkbox and text that might take on
	/// ellipse if the width of the control dynamically shrinks too much
	/// </summary>
	internal class MoreCheckBox : CheckBox
	{
		private const string BoxColor = "#FF73356E";


		public MoreCheckBox()
		{
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
		}


		protected override void OnPaint(PaintEventArgs pevent)
		{
			var g = pevent.Graphics;

			g.Clear(BackColor);

			var boxColor = ColorTranslator.FromHtml(BoxColor);
			using (var pen = new Pen(boxColor))
			{
				g.DrawRectangle(pen, 0, 1, 14, 14);
			}

			if (Checked)
			{
				using (var brush = new SolidBrush(boxColor))
				{
					g.FillRectangle(brush, 2, 3, 11, 11);
				}
			}

			var size = g.MeasureString(Text, Font);
			using (var brush = new SolidBrush(ForeColor))
			{
				g.DrawString(Text, Font, brush,
					new Rectangle(16, // standard icon size
						(pevent.ClipRectangle.Height - (int)size.Height) / 2,
						pevent.ClipRectangle.Width - 16,
						(int)size.Height),
					new StringFormat
					{
						Trimming = StringTrimming.EllipsisCharacter,
						FormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoWrap
					});
			}
		}
	}
}
