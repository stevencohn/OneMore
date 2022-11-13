//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using River.OneMoreAddIn.Helpers.Office;
	using System;
	using System.Drawing;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;

	internal static class Theme
	{
		#region Native
		private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

		[DllImport("dwmapi.dll", PreserveSig = true)]
		private static extern int DwmSetWindowAttribute(
			IntPtr hwnd, int attr, ref bool attrValue, int attrSize);
		#endregion Native


		public static readonly bool DarkMode;

		public static readonly Color BackColor;
		public static readonly Color ForeColor;
		public static readonly Color BorderColor;
		public static readonly Color HighlightForeColor;
		public static readonly Color ControlColor;

		public static readonly Color ButtonBack;
		public static readonly Color ButtonFore;
		public static readonly Color ButtonDisabled;
		public static readonly Color ButtonBorder;
		public static readonly Color ButtonHotBack;
		public static readonly Color ButtonHotBorder;
		public static readonly Color ButtonPressBorder;

		public static readonly Color HeadBackColor;
		public static readonly Color TodayHeadColor;

		public static readonly Color DetailOddBack;
		public static readonly Color DetailEvenBack;

		public static readonly Color MonthPrimary;
		public static readonly Color MonthSecondary;
		public static readonly Color MonthGrid;
		public static readonly Color MonthDayFore;
		public static readonly Color MonthDayBack;
		public static readonly Color MonthTodayFore;
		public static readonly Color MonthTodayBack;


		static Theme()
		{
			DarkMode = Office.SystemDefaultDarkMode();
			if (DarkMode)
			{
				// primary-dark = 202020 / 1F1F1F
				// medium-dark  = 2B2B2B / 272727
				// light-dark   = 363636 / 383838
				// light purple = D2A1DF / D6A6D3
				// dark purple  = 73356E (DarkOrchid)
				// ---------------------------------------------------------

				BackColor = ColorTranslator.FromHtml("#FF383838");
				ForeColor = ColorTranslator.FromHtml("#FFE6E6E6");
				BorderColor = Color.DarkOrchid;
				ControlColor = ColorTranslator.FromHtml("#FF73356E");// dark purple

				// light purple (pink)
				HighlightForeColor = ColorTranslator.FromHtml("#FFD2A1DF");

				HeadBackColor = ColorTranslator.FromHtml("#FFF4E8F3");
				TodayHeadColor = ColorTranslator.FromHtml("#FFD6A6D3");

				ButtonBack = ColorTranslator.FromHtml("#FF363636");
				ButtonFore = ColorTranslator.FromHtml("#FFE6E6E6");
				ButtonDisabled = Color.Gray;
				ButtonBorder = ColorTranslator.FromHtml("#FF555555");
				ButtonHotBack = ColorTranslator.FromHtml("#FF505050");
				ButtonHotBorder = ColorTranslator.FromHtml("#FF808080");
				ButtonPressBorder = Color.DarkOrchid;

				MonthPrimary = ColorTranslator.FromHtml("#FF1F1F1F");
				MonthSecondary = ColorTranslator.FromHtml("#FF272727");
				MonthGrid = ColorTranslator.FromHtml("#FF676767");
				MonthDayFore = Color.LightGray;
				MonthDayBack = ColorTranslator.FromHtml("#FF383838");
				MonthTodayFore = Color.LightGray;
				MonthTodayBack = ColorTranslator.FromHtml("#FF73356E");

				DetailOddBack = ColorTranslator.FromHtml("#FF1F1F1F");
				DetailEvenBack = ColorTranslator.FromHtml("#FF272727");
			}
			else
			{
				BackColor = Color.White;
				ForeColor = Color.Black;
				BorderColor = ColorTranslator.FromHtml("#FFD2A1DF");
				ControlColor = ColorTranslator.FromHtml("#FF73356E"); // dark purple

				// light purple
				HighlightForeColor = ColorTranslator.FromHtml("#FFBC58B6");

				ButtonBack = ColorTranslator.FromHtml("#FFF7EDF7");
				ButtonFore = Color.FromArgb(115, 53, 110);
				ButtonDisabled = Color.Gray;
				ButtonBorder = ColorTranslator.FromHtml("#FFF0DAEE");
				ButtonHotBack = ColorTranslator.FromHtml("#FFF0DAEE");
				ButtonHotBorder = ColorTranslator.FromHtml("#FF9E5499");
				ButtonPressBorder = ColorTranslator.FromHtml("#FF9E5499");

				HeadBackColor = ColorTranslator.FromHtml("#FFF4E8F3");
				TodayHeadColor = ColorTranslator.FromHtml("#FFD6A6D3");

				MonthPrimary = Color.White;
				MonthSecondary = Color.WhiteSmoke;
				MonthGrid = Color.DarkGray;
				MonthDayFore = Color.Gray;
				MonthDayBack = ColorTranslator.FromHtml("#FFF4E8F3");
				MonthTodayFore = Color.Black;
				MonthTodayBack = ColorTranslator.FromHtml("#FFD6A6D3");

				DetailOddBack = ColorTranslator.FromHtml("#FFFDFAFE");
				DetailEvenBack = Color.White;
			}
		}


		public static void InitializeTheme(ContainerControl container)
		{
			if (DarkMode)
			{
				// true=dark, false=normal
				var value = true;

				DwmSetWindowAttribute(
					container.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, Marshal.SizeOf(value));

				// controls...

				container.BackColor = Theme.BackColor;
				container.ForeColor = Theme.ForeColor;

				Colorize(container.Controls);
			}
		}


		private static void Colorize(Control.ControlCollection controls)
		{
			foreach (Control control in controls)
			{
				control.BackColor = Theme.BackColor;
				control.ForeColor = Theme.ForeColor;

				if (control.Controls.Count > 0)
				{
					Colorize(control.Controls);
				}

				if (control is ListView view)
				{
					foreach (ListViewItem item in view.Items)
					{
						item.BackColor = Theme.BackColor;
						item.ForeColor = Theme.ForeColor;
					}
				}
			}
		}
	}
}
