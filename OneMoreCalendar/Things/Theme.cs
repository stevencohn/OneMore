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


	internal enum ThemeMode
	{
		System = 0,
		Light = 1,
		Dark = 2
	}

	internal static class Theme
	{
		#region Native
		private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

		[DllImport("dwmapi.dll", PreserveSig = true)]
		private static extern int DwmSetWindowAttribute(
			IntPtr hwnd, int attr, ref bool attrValue, int attrSize);
		#endregion Native


		public static bool DarkMode { get; private set; }

		public static Color BackColor { get; private set; }
		public static Color ForeColor { get; private set; }
		public static Color Border { get; private set; }
		public static Color Highlight { get; private set; }
		public static Color Control { get; private set; }

		public static Color ButtonBack { get; private set; }
		public static Color ButtonFore { get; private set; }
		public static Color ButtonDisabled { get; private set; }
		public static Color ButtonBorder { get; private set; }
		public static Color ButtonHotBack { get; private set; }
		public static Color ButtonHotBorder { get; private set; }
		public static Color ButtonPressBorder { get; private set; }

		public static Color LinkColor { get; private set; }
		public static Color HoverColor { get; private set; }

		public static Color DetailOddBack { get; private set; }
		public static Color DetailEvenBack { get; private set; }

		public static Color MonthHeader { get; private set; }
		public static Color MonthPrimary { get; private set; }
		public static Color MonthSecondary { get; private set; }
		public static Color MonthGrid { get; private set; }
		public static Color MonthDayFore { get; private set; }
		public static Color MonthDayBack { get; private set; }
		public static Color MonthTodayFore { get; private set; }
		public static Color MonthTodayBack { get; private set; }


		public static void InitializeTheme(ContainerControl container)
		{
			var provider = new SettingsProvider();
			var theme = provider.Theme;
			DarkMode = theme == ThemeMode.Dark ||
				(theme == ThemeMode.System && Office.SystemDefaultDarkMode());

			SetColors();

			// true=dark, false=normal
			var value = DarkMode;

			DwmSetWindowAttribute(
				container.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, Marshal.SizeOf(value));

			// controls...

			if (container is ThemedForm form)
			{
				form.OnThemeChange();
			}
			else if (container is ThemedUserControl control)
			{
				control.OnThemeChange();
			}

			container.BackColor = MonthHeader;
			container.ForeColor = ForeColor;

			Colorize(container.Controls);
		}


		private static void SetColors()
		{
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
				Border = Color.DarkOrchid;
				Control = Color.MediumOrchid;

				// light purple (pink)
				Highlight = ColorTranslator.FromHtml("#FFD2A1DF");

				ButtonBack = ColorTranslator.FromHtml("#FF363636");
				ButtonFore = ColorTranslator.FromHtml("#FFE6E6E6");
				ButtonDisabled = Color.Gray;
				ButtonBorder = ColorTranslator.FromHtml("#FF555555");
				ButtonHotBack = ColorTranslator.FromHtml("#FF505050");
				ButtonHotBorder = ColorTranslator.FromHtml("#FF808080");
				ButtonPressBorder = Color.DarkOrchid;

				LinkColor = Color.MediumOrchid;
				HoverColor = Color.Orchid;

				MonthHeader = ColorTranslator.FromHtml("#FF383838");
				MonthPrimary = ColorTranslator.FromHtml("#FF1F1F1F");
				MonthSecondary = ColorTranslator.FromHtml("#FF272727");
				MonthGrid = ColorTranslator.FromHtml("#FF676767");
				MonthDayFore = Color.LightGray;
				MonthDayBack = ColorTranslator.FromHtml("#FF383838");
				MonthTodayFore = Color.LightGray;
				MonthTodayBack = ColorTranslator.FromHtml("#FF73356E"); // dark purpose

				DetailOddBack = ColorTranslator.FromHtml("#FF1F1F1F");
				DetailEvenBack = ColorTranslator.FromHtml("#FF272727");
			}
			else
			{
				BackColor = Color.White;
				ForeColor = Color.Black;
				Border = ColorTranslator.FromHtml("#FFD2A1DF");
				Control = ColorTranslator.FromHtml("#FF73356E"); // dark purple

				// light purple
				Highlight = ColorTranslator.FromHtml("#FFBC58B6");

				ButtonBack = ColorTranslator.FromHtml("#FFF7EDF7");
				ButtonFore = ColorTranslator.FromHtml("#FF73356E"); // dark purple
				ButtonDisabled = Color.Gray;
				ButtonBorder = ColorTranslator.FromHtml("#FFF0DAEE");
				ButtonHotBack = ColorTranslator.FromHtml("#FFF0DAEE");
				ButtonHotBorder = ColorTranslator.FromHtml("#FF9E5499");
				ButtonPressBorder = ColorTranslator.FromHtml("#FF9E5499");

				LinkColor = ColorTranslator.FromHtml("#FF73356E"); // dark purple
				HoverColor = Color.MediumOrchid;

				MonthHeader = SystemColors.Control;
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


		private static void Colorize(Control.ControlCollection controls)
		{
			foreach (Control control in controls)
			{
				control.BackColor = BackColor;
				control.ForeColor = ForeColor;

				if (control.Controls.Count > 0)
				{
					Colorize(control.Controls);
				}

				if (control is ListView view)
				{
					foreach (ListViewItem item in view.Items)
					{
						item.BackColor = MonthHeader;
						item.ForeColor = ForeColor;
					}
				}
				else if (control is River.OneMoreAddIn.UI.MoreLinkLabel label)
				{
					label.LinkColor= LinkColor;
					label.HoverColor= HoverColor;
				}
			}
		}
	}
}
