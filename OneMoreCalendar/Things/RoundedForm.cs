//************************************************************************************************
// Copyright © 2022 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Drawing;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;


	internal partial class RoundedForm : ThemedForm
	{

		[DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
		private static extern IntPtr CreateRoundRectRgn
		(
			int nLeftRect,     // x-coordinate of upper-left corner
			int nTopRect,      // y-coordinate of upper-left corner
			int nRightRect,    // x-coordinate of lower-right corner
			int nBottomRect,   // y-coordinate of lower-right corner
			int nWidthEllipse, // width of ellipse
			int nHeightEllipse // height of ellipse
		);


		private const int Radius = 8;


		public RoundedForm()
		{
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignMode)
			{
				FormBorderStyle = FormBorderStyle.None;
				var radius = this.Scaled(Radius);
				Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, radius, radius));
			}
		}


		protected override void OnPaintBackground(PaintEventArgs e)
		{
			//base.OnPaintBackground(e);

			var radius = this.Scaled(Radius);
			Rectangle r;

			using var brush = new SolidBrush(BackColor);
			r = new Rectangle(0, 0, e.ClipRectangle.Width, e.ClipRectangle.Height);
			e.Graphics.FillRoundedRectangle(brush, r, radius);

			using var pen = new Pen(Theme.Border);
			r = new Rectangle(0, 0, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 1);
			e.Graphics.DrawRoundedRectangle(pen, r, radius);
		}
	}
}
