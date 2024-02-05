//************************************************************************************************
// Copyright © 2002 Steven M Cohn. All Rights Reserved.
// Originally from my own Orqa project
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.UI;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Windows.Forms;


	internal class FadingLabel : Label
	{

		public FadingLabel()
			: base()
		{
			AutoSize = false;
			TextAlign = ContentAlignment.MiddleLeft;

			ForeColor = Color.White;
			Font = new Font("Tahoma", 9F); //, FontStyle.Bold);
		}


		protected override void OnPaint(PaintEventArgs e)
		{
			var manager = ThemeManager.Instance;
			var startColor = manager.GetColor("GradientActiveCaption");
			var endColor = manager.GetColor("GradientInactiveCaption");

			// linear gradient brush for background fill
			var gradient = new LinearGradientBrush(
				new Point(0, 0),
				new Point(Width, 0),
				startColor,
				endColor);

			// fill with gradient 
			e.Graphics.FillRectangle(gradient, new Rectangle(0, 0, Width, Height));

			// overlay rectangle with text
			e.Graphics.DrawString(
				Text,
				Font,
				new SolidBrush(ForeColor),
				new Rectangle(0, Height / 2 - Font.Height / 2, Width, Height),
				new StringFormat { Alignment = StringAlignment.Near }
				);
		}
	}
}
