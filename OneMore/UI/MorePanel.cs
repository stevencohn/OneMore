//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.ComponentModel;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// Extends the standard Panel control by adding a bottom border line.
	/// </summary>
	internal class MorePanel : Panel
	{

		[Description("Specifies the color of the bottom border")]
		public Color BottomBorderColor { get; set; } = SystemColors.ActiveBorder;


		[Description("Specifies the thickness of the bottom border")]
		public int BottomBorderSize { get; set; } = 2;


		protected override void OnPaintBackground(PaintEventArgs e)
		{
			base.OnPaintBackground(e);

			if (BottomBorderSize > 0)
			{
				using var brush = new SolidBrush(BottomBorderColor);

				e.Graphics.FillRectangle(brush,
					0, Height - BottomBorderSize - 1,
					Width, Height - 1);
			}
		}
	}
}
