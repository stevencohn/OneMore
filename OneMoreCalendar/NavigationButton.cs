using System.Drawing;
using System.Windows.Forms;

namespace OneMoreCalendar
{
	internal class NavigationButton : Button
	{
		private bool forward;


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


		public bool Forward
		{
			get
			{
				return forward;
			}

			set
			{
				forward = value;
				Text = forward ? "⏵" : "⏴";
			}
		}

		protected override void OnPaint(PaintEventArgs pevent)
		{
			base.OnPaint(pevent);
		}


		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			base.OnPaintBackground(pevent);
		}
	}
}
