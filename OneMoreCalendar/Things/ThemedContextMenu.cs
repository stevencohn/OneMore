//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// A ContextMenuStrip that renders with the OneMoreCalendar theme colors, regardless of
	/// the OneNote/Windows theme. Call ApplyTheme before each Show to pick up theme changes.
	/// </summary>
	internal class ThemedContextMenuStrip : ContextMenuStrip
	{
		private static ThemeProvider Theme => ThemeProvider.Instance;


		public ThemedContextMenuStrip()
		{
			ApplyTheme();
		}


		/// <summary>
		/// Re-reads the current theme colors
		/// </summary>
		public void ApplyTheme()
		{
			BackColor = Theme.BackColor;
			ForeColor = Theme.ForeColor;
			Renderer = new ThemedMenuRenderer(new ThemedMenuColors());
		}
	}


	/// <summary>
	/// Maps ProfessionalColorTable menu colors onto the OneMoreCalendar theme
	/// </summary>
	internal class ThemedMenuColors : ProfessionalColorTable
	{
		private static ThemeProvider Theme => ThemeProvider.Instance;

		public ThemedMenuColors()
		{
			UseSystemColors = false;
		}

		public override Color ToolStripDropDownBackground => Theme.BackColor;
		public override Color ImageMarginGradientBegin => Theme.BackColor;
		public override Color ImageMarginGradientMiddle => Theme.BackColor;
		public override Color ImageMarginGradientEnd => Theme.BackColor;
		public override Color MenuBorder => Theme.ButtonBorder;
		public override Color MenuItemBorder => Theme.ButtonHotBorder;
		public override Color MenuItemSelected => Theme.ButtonHotBack;
		public override Color MenuItemSelectedGradientBegin => Theme.ButtonHotBack;
		public override Color MenuItemSelectedGradientEnd => Theme.ButtonHotBack;
		public override Color SeparatorDark => Theme.MonthGrid;
		public override Color SeparatorLight => Theme.BackColor;
	}


	internal class ThemedMenuRenderer : ToolStripProfessionalRenderer
	{
		private static ThemeProvider Theme => ThemeProvider.Instance;

		public ThemedMenuRenderer(ProfessionalColorTable colorTable)
			: base(colorTable)
		{
			RoundedEdges = false;
		}

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			e.TextColor = e.Item.Enabled ? Theme.ForeColor : Theme.ButtonDisabled;
			base.OnRenderItemText(e);
		}

		protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
		{
			e.ArrowColor = Theme.ForeColor;
			base.OnRenderArrow(e);
		}
	}
}
