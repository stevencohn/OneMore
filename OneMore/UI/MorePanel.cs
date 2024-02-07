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
	internal class MorePanel : Panel, ILoadControl
	{

		[Description("Specifies the color of the bottom border")]
		public Color BottomBorderColor { get; set; } = SystemColors.ActiveBorder;


		[Description("Specifies the thickness of the bottom border")]
		public int BottomBorderSize { get; set; } = 2;


		[Description("Specifies the color of the top border")]
		public Color TopBorderColor { get; set; } = SystemColors.Control;


		[Description("Specifies the thickness of the top border")]
		public int TopBorderSize { get; set; } = 0;


		[Description("Specifies the themed background color")]
		public string ThemedBack { get; set; }


		[Description("Specifies the themed foreground color")]
		public string ThemedFore { get; set; }


		void ILoadControl.OnLoad()
		{
			var manager = ThemeManager.Instance;
			BackColor = manager.GetColor("Control", ThemedBack);

			ForeColor = Enabled
				? manager.GetColor("ControlText", ThemedFore)
				: manager.GetColor("GrayText");

			BottomBorderColor = manager.GetColor(BottomBorderColor);
			TopBorderColor = manager.GetColor(TopBorderColor);
		}


		protected override void OnPaintBackground(PaintEventArgs e)
		{
			base.OnPaintBackground(e);

			if (TopBorderSize > 0)
			{
				using var brush = new SolidBrush(TopBorderColor);
				e.Graphics.FillRectangle(brush, 0, 0, Width - 1, TopBorderSize);
			}

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
