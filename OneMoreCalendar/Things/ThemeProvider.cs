//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using System;
	using System.Drawing;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;
	using ThemeManager = River.OneMoreAddIn.UI.ThemeManager;


	/// <summary>
	/// Defines themes and projects onto UI elements
	/// </summary>
	internal class ThemeProvider
	{
		private static ThemeProvider instance;


		/// <summary>
		/// Singleton constructor
		/// </summary>
		private ThemeProvider()
		{
		}


		/// <summary>
		/// Gets the ThemeProvider instance
		/// </summary>
		public static ThemeProvider Instance => instance ??= new ThemeProvider();


		// Calendar does not have its own theme; it uses the theme chosen in OneMore's
		// Settings > General, which is resolved once by ThemeManager when Calendar starts
		private static ThemeManager Manager => ThemeManager.Instance;


		/// <summary>
		/// Gets a value indicating whether the theme is set to dark mode
		/// </summary>
		public bool DarkMode => Manager.DarkMode;


		#region Convenience Properties
		public Color BackColor => Manager.GetColor(nameof(BackColor));
		public Color ForeColor => Manager.GetColor(nameof(ForeColor));
		public Color Border => Manager.GetColor(nameof(Border));
		public Color Highlight => Manager.GetColor(nameof(Highlight));
		public Color IconColor => Manager.GetColor(nameof(IconColor));
		public Color ButtonBack => Manager.GetColor(nameof(ButtonBack));
		public Color ButtonFore => Manager.GetColor(nameof(ButtonFore));
		public Color ButtonDisabled => Manager.GetColor(nameof(ButtonDisabled));
		public Color ButtonBorder => Manager.GetColor(nameof(ButtonBorder));
		public Color ButtonHotBack => Manager.GetColor(nameof(ButtonHotBack));
		public Color ButtonHotBorder => Manager.GetColor(nameof(ButtonHotBorder));
		public Color ButtonPressBorder => Manager.GetColor(nameof(ButtonPressBorder));

		/// <summary>
		/// Gets the accent used by check boxes, radio buttons and lists. OneMore's own
		/// "Control" color is a neutral surface, so this uses IconColor instead.
		/// </summary>
		public Color Control => Manager.GetColor(nameof(IconColor));

		public Color LinkColor => Manager.GetColor(nameof(LinkColor));
		public Color HoverColor => Manager.GetColor(nameof(HoverColor));

		public Color DetailOddBack => Manager.GetColor(nameof(DetailOddBack));
		public Color DetailEvenBack => Manager.GetColor(nameof(DetailEvenBack));

		public Color MonthHeader => Manager.GetColor(nameof(MonthHeader));
		public Color MonthPrimary => Manager.GetColor(nameof(MonthPrimary));
		public Color MonthSecondary => Manager.GetColor(nameof(MonthSecondary));
		public Color MonthGrid => Manager.GetColor(nameof(MonthGrid));
		public Color MonthDayFore => Manager.GetColor(nameof(MonthDayFore));
		public Color MonthDayBack => Manager.GetColor(nameof(MonthDayBack));
		public Color MonthTodayFore => Manager.GetColor(nameof(MonthTodayFore));
		public Color MonthTodayBack => Manager.GetColor(nameof(MonthTodayBack));
		#endregion Convenience Properties

		#region Native
		private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

		private static bool appModePreferred;

		[DllImport("dwmapi.dll", PreserveSig = true)]
		private static extern int DwmSetWindowAttribute(
			IntPtr hwnd, int attr, ref bool attrValue, int attrSize);

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
		private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

		// undocumented, ordinal-only exports (stable since Windows 10 1809) that opt a window
		// into dark comctl32 chrome; without these, SetWindowTheme(..., "DarkMode_Explorer", ...)
		// alone only partially dark-themes native controls
		[DllImport("uxtheme.dll", EntryPoint = "#135")]
		private static extern int SetPreferredAppMode(int preferredAppMode);

		[DllImport("uxtheme.dll", EntryPoint = "#133")]
		private static extern bool AllowDarkModeForWindow(IntPtr hWnd, bool allow);

		/// <summary>
		/// Best-effort opt-in to native dark comctl32 rendering, used to carry dark styling
		/// onto native scrollbars (ListBox, ListView, CheckedListBox). These are unsupported
		/// ordinal exports that can vary or be missing across Windows builds, so failures are
		/// swallowed and theming falls back to whatever SetWindowTheme alone achieves.
		/// </summary>
		private static void TryAllowDarkModeForWindow(IntPtr hwnd, bool allow)
		{
			try
			{
				if (!appModePreferred)
				{
					// 1 = AllowDark
					SetPreferredAppMode(1);
					appModePreferred = true;
				}

				AllowDarkModeForWindow(hwnd, allow);
			}
			catch
			{
				// ordinal not available on this Windows build; ignore
			}
		}


		/// <summary>
		/// Recursively opts native Win32 controls (and their scrollbars) into dark-mode
		/// rendering; only called when DarkMode is true since native controls default to
		/// light rendering otherwise.
		/// </summary>
		private void SetWindowTheme(Control control)
		{
			TryAllowDarkModeForWindow(control.Handle, true);

			if (control is ListView)
			{
				// "DarkMode_Explorer" is what carries the dark-themed native scrollbar
				SetWindowTheme(control.Handle, "DarkMode_Explorer", null);

				// the column header is a separate native child window (SysHeader32) that
				// needs the same explicit opt-in to pick up dark colors
				var header = Native.SendMessage(control.Handle, Native.LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
				if (header != IntPtr.Zero)
				{
					TryAllowDarkModeForWindow(header, true);
					SetWindowTheme(header, "ItemsView", null);
				}
			}
			else
			{
				// DarkMode_Explorer sets radios, checkboxes, and scrollbars to dark mode
				SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
			}

			foreach (Control child in control.Controls)
			{
				SetWindowTheme(child);
			}
		}
		#endregion Native


		/// <summary>
		/// Applies the current theme to the given form or user control; the colors come from
		/// the theme chosen in OneMore's settings and the Visual Studio designer always gets
		/// the light theme (handled by ThemeManager).
		/// </summary>
		/// <param name="container"></param>
		public void InitializeTheme(ContainerControl container)
		{

			if (container is ThemedForm form)
			{
				if (form.FormBorderStyle != FormBorderStyle.None)
				{
					var value = DarkMode; // true=dark, false=normal

					DwmSetWindowAttribute(
						container.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE,
						ref value, Marshal.SizeOf(value));
				}

				form.OnThemeChange();
			}
			else if (container is ThemedUserControl control)
			{
				control.OnThemeChange();
			}

			if (container != null)
			{
				container.BackColor = MonthHeader;
				container.ForeColor = ForeColor;

				if (DarkMode)
				{
					SetWindowTheme(container);
				}

				Colorize(container.Controls);

				if (container is ThemedForm themedForm)
				{
					themedForm.OnThemeColorized();
				}
			}
		}


		private void Colorize(Control.ControlCollection controls)
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
					label.LinkColor = LinkColor;
					label.HoverColor = HoverColor;
					label.ActiveLinkColor = LinkColor;
				}
			}
		}
	}
}
