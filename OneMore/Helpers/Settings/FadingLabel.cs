//************************************************************************************************
// Copyright © 2002 Steven M. Cohn. All Rights Reserved.
// Originally from my own Orqa project
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Settings
{
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Windows.Forms;


	internal class FadingLabel : Label
	{
		private Color startColor;
		private Color endColor;


		public FadingLabel()
			: base()
		{
			AutoSize = false;
			TextAlign = ContentAlignment.MiddleLeft;

			startColor = SystemColors.GradientActiveCaption;
			endColor = SystemColors.ControlLightLight;
			ForeColor = SystemColors.ActiveCaptionText;

			Font = new Font("Tahoma", 9F, FontStyle.Bold);
		}


		protected override void OnPaint(PaintEventArgs e)
		{
			// declare linear gradient brush for fill background of label
			LinearGradientBrush GBrush = new LinearGradientBrush(
				new Point(0, 0),
				new Point(Width, 0),
				startColor,
				endColor);

			Rectangle rect = new Rectangle(0, 0, Width, Height);

			// Fill with gradient 
			e.Graphics.FillRectangle(GBrush, rect);

			// draw text on label
			SolidBrush drawBrush = new SolidBrush(ForeColor);

			StringFormat sf = new StringFormat();

			// align with center
			sf.Alignment = StringAlignment.Near;

			// set rectangle bound text
			RectangleF recf = new RectangleF(
				0, Height / 2 - Font.Height / 2, Width, Height);

			// output string
			e.Graphics.DrawString(Text, Font, drawBrush, recf, sf);
		}
	}
}
