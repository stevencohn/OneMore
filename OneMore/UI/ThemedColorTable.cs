//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.Drawing;
	using System.Windows.Forms;

	internal class ThemedColorTable : ProfessionalColorTable
	{
		private readonly ThemeManager manager;
		private readonly Color menuColor;
		private readonly Color menuHighlightColor;
		private readonly Color menuMarginColor;

		public ThemedColorTable()
		{
			manager = ThemeManager.Instance;
			menuColor = manager.GetColor("MenuBar");
			menuHighlightColor = manager.GetColor("MenuHighlight");
			menuMarginColor = manager.GetColor("MenuMargin");
		}

		// split button
		public override Color ButtonSelectedGradientBegin => menuHighlightColor;
		public override Color ButtonSelectedGradientEnd => menuHighlightColor;
		public override Color ButtonSelectedGradientMiddle => menuHighlightColor;
		public override Color ImageMarginGradientBegin => menuMarginColor;
		public override Color ImageMarginGradientMiddle => menuMarginColor;
		public override Color ImageMarginGradientEnd => menuMarginColor;
		public override Color MenuBorder => manager.GetColor("WindowFrame");
		public override Color MenuItemBorder => manager.GetColor("MenuItemBorder");
		public override Color MenuItemPressedGradientBegin => menuHighlightColor;
		public override Color MenuItemPressedGradientEnd => menuHighlightColor;
		public override Color MenuItemSelectedGradientBegin => menuHighlightColor;
		public override Color MenuItemSelectedGradientEnd => menuHighlightColor;
		public override Color MenuStripGradientBegin => menuColor;
		public override Color MenuStripGradientEnd => menuColor;
		public override Color SeparatorLight => manager.GetColor("MenuSeparator");
		public override Color SeparatorDark => manager.GetColor("MenuSeparator");
		public override Color ToolStripBorder => manager.GetColor("WindowFrame");
		public override Color ToolStripContentPanelGradientBegin => menuColor;
		public override Color ToolStripContentPanelGradientEnd => menuColor;
		public override Color ToolStripDropDownBackground => menuColor;
		public override Color ToolStripGradientBegin => menuColor;
		public override Color ToolStripGradientEnd => menuColor;
		public override Color ToolStripGradientMiddle => menuColor;
		public override Color ToolStripPanelGradientBegin => menuColor;
		public override Color ToolStripPanelGradientEnd => menuColor;


		// are the following used? ....

		public override Color ButtonCheckedHighlight => Color.Yellow;
		public override Color ButtonCheckedHighlightBorder => Color.Blue;
		public override Color ButtonCheckedGradientBegin => Color.Yellow;
		public override Color ButtonCheckedGradientEnd => Color.Yellow;
		public override Color ButtonCheckedGradientMiddle => Color.Yellow;
		public override Color CheckPressedBackground => Color.Orange;
		public override Color ButtonPressedGradientBegin => Color.PeachPuff;
		public override Color ButtonPressedGradientEnd => Color.PeachPuff;
		public override Color ButtonPressedGradientMiddle => Color.PeachPuff;
		public override Color ButtonPressedHighlight => Color.Green;
		public override Color CheckBackground => Color.LimeGreen;
		public override Color ButtonSelectedHighlight => Color.LightPink;
		public override Color CheckSelectedBackground => Color.Gray;
	}
}
