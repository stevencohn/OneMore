using System;
using System.Drawing;
using System.Windows.Forms;

namespace OneMoreCalendar
{
	internal class NavigationButton : Button
	{
		private readonly Color PressedColor = ColorTranslator.FromHtml("#FFF2C4F2");
		private readonly Color HoverColor = ColorTranslator.FromHtml("#FFE0B1DE");


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

			var text = Forward ? "⏵" : "⏴";
			var size = pevent.Graphics.MeasureString(text, Font);

			using (var brush = new SolidBrush(ForeColor))
			{
				pevent.Graphics.DrawString(text, Font, brush,
					(int)((Width - size.Width) / 2),
					(int)((Height - size.Height) / 2)
					);
			}
		}


		protected override void OnMouseDown(MouseEventArgs mevent)
		{
			BackColor = PressedColor;
			base.OnMouseDown(mevent);
		}

		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			BackColor = HoverColor;
			base.OnMouseUp(mevent);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			BackColor = SystemColors.Control;
			base.OnMouseLeave(e);
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			BackColor = HoverColor;
			base.OnMouseEnter(e);
		}
	}
}
