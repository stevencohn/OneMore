using System;
using System.Drawing;
using System.Windows.Forms;

namespace OneMoreCalendar
{
	internal class NavigationButton : Button
	{
		private readonly Color PressedColor = ColorTranslator.FromHtml("#FFF0DAEE");
		private readonly Color PressedBorder = ColorTranslator.FromHtml("#FF9E5499");
		private readonly Color HoverColor = ColorTranslator.FromHtml("#FFF7EDF7");
		private readonly Color HoverBorder = ColorTranslator.FromHtml("#FFF0DAEE");

		private readonly Color backColor;


		public NavigationButton()
		{
			FlatAppearance.BorderSize = 0;
			FlatStyle = FlatStyle.Flat;
			Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point, 0);
			Size = new Size(40, 60);
			TextAlign = ContentAlignment.MiddleCenter;
			UseVisualStyleBackColor = true;

			Direction = ArrowDirection.Right;
			backColor = BackColor;
		}

		public ArrowDirection Direction { get; set; }


		protected override void OnPaint(PaintEventArgs pevent)
		{
			pevent.Graphics.Clear(BackColor);

			if (Enabled && BackColor != backColor)
			{
				using (var pen = new Pen(MouseButtons == MouseButtons.Left ? PressedBorder : HoverBorder, 1))
				{
					pevent.Graphics.DrawRectangle(pen,
						pevent.ClipRectangle.X, pevent.ClipRectangle.Y,
						pevent.ClipRectangle.Width - 1, pevent.ClipRectangle.Height - 1);
				}
			}

			string arrow;
			switch (Direction)
			{
				case ArrowDirection.Right: arrow = "⏵"; break; // \u23F5
				case ArrowDirection.Up: arrow = "⏶"; break; // \u23F6
				case ArrowDirection.Down: arrow = "⏷"; break; // \u23F7
				default: arrow = "⏴"; break; // \u23F4
			}

			var size = pevent.Graphics.MeasureString(arrow, Font);

			using (var brush = new SolidBrush(Enabled ? ForeColor : Color.DarkGray))
			{
				pevent.Graphics.DrawString(arrow, Font, brush,
					(int)((Width - size.Width) / 2),
					(int)((Height - size.Height) / 2)
					);
			}
		}


		protected override void OnMouseDown(MouseEventArgs mevent)
		{
			if (Enabled)
			{
				BackColor = PressedColor;
				base.OnMouseDown(mevent);
			}
		}

		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			if (Enabled)
			{
				BackColor = HoverColor;
				base.OnMouseUp(mevent);
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			if (Enabled)
			{
				BackColor = backColor;
				base.OnMouseLeave(e);
			}
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			if (Enabled)
			{
				BackColor = HoverColor;
				base.OnMouseEnter(e);
			}
		}
	}
}
