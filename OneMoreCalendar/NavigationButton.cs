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


		public NavigationButton()
		{
			FlatAppearance.BorderSize = 0;
			FlatStyle = FlatStyle.Flat;
			Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point, 0);
			Size = new Size(40, 60);
			TextAlign = ContentAlignment.MiddleCenter;
			UseVisualStyleBackColor = true;

			Forward = true;
		}

		public bool Forward { get; set; }


		protected override void OnPaint(PaintEventArgs pevent)
		{
			pevent.Graphics.Clear(BackColor);

			if (Enabled && BackColor != SystemColors.Control)
			{
				using (var pen = new Pen(MouseButtons == MouseButtons.Left ? PressedBorder : HoverBorder, 1))
				{
					pevent.Graphics.DrawRectangle(pen,
						pevent.ClipRectangle.X, pevent.ClipRectangle.Y,
						pevent.ClipRectangle.Width - 1, pevent.ClipRectangle.Height - 1);
				}
			}

			var text = Forward ? "⏵" : "⏴";
			var size = pevent.Graphics.MeasureString(text, Font);

			using (var brush = new SolidBrush(Enabled ? ForeColor : Color.DarkGray))
			{
				pevent.Graphics.DrawString(text, Font, brush,
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
				BackColor = SystemColors.Control;
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
