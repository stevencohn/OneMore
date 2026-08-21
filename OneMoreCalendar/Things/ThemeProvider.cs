//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields

namespace OneMoreCalendar
{
	using Newtonsoft.Json;
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Helpers.Office;
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Drawing;
	using System.IO;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;


	/// <summary>
	/// Defines themes and projects onto UI elements
	/// </summary>
	internal class ThemeProvider
	{
		private const string CustomThemeFile = "OneMoreCalendarTheme.json";


		#region ColorConverter
		private sealed class ColorConverter : JsonConverter<Color>
		{
			public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
			{
				writer.WriteValue(value.IsKnownColor
					? value.Name
					: $"#{value.R:X2}{value.G:X2}{value.B:X2}");
			}

			public override Color ReadJson(
				JsonReader reader, Type objectType, Color existingValue,
				bool hasExistingValue, JsonSerializer serializer)
			{
				return ColorTranslator.FromHtml((string)reader.Value);
			}
		}
		#endregion ColorConverter


		private static ThemeProvider instance;


		/// <summary>
		/// Singleton constructor
		/// </summary>
		private ThemeProvider()
		{
			Colors = new Dictionary<string, Color>();
		}


		/// <summary>
		/// Gets the ThemeManager instance
		/// </summary>
		public static ThemeProvider Instance => instance ??= new ThemeProvider();


		/// <summary>
		/// Gets a value indicating whether the theme is set to dark mode
		/// </summary>
		[JsonProperty]
		public bool DarkMode { get; private set; }


		[JsonProperty]
		private Dictionary<string, Color> Colors { get; set; }


		#region Convenience Properties
		[JsonIgnore]
		public Color BackColor => Colors[nameof(BackColor)];
		[JsonIgnore]
		public Color ForeColor => Colors[nameof(ForeColor)];
		[JsonIgnore]
		public Color Border => Colors[nameof(Border)];
		[JsonIgnore]
		public Color Highlight => Colors[nameof(Highlight)];
		[JsonIgnore]
		public Color Control => Colors[nameof(Control)];
		[JsonIgnore]
		public Color IconColor => Colors[nameof(IconColor)];
		[JsonIgnore]
		public Color ButtonBack => Colors[nameof(ButtonBack)];
		[JsonIgnore]
		public Color ButtonFore => Colors[nameof(ButtonFore)];
		[JsonIgnore]
		public Color ButtonDisabled => Colors[nameof(ButtonDisabled)];
		[JsonIgnore]
		public Color ButtonBorder => Colors[nameof(ButtonBorder)];
		[JsonIgnore]
		public Color ButtonHotBack => Colors[nameof(ButtonHotBack)];
		[JsonIgnore]
		public Color ButtonHotBorder => Colors[nameof(ButtonHotBorder)];
		[JsonIgnore]
		public Color ButtonPressBorder => Colors[nameof(ButtonPressBorder)];

		[JsonIgnore]
		public Color LinkColor => Colors[nameof(LinkColor)];
		[JsonIgnore]
		public Color HoverColor => Colors[nameof(HoverColor)];

		[JsonIgnore]
		public Color DetailOddBack => Colors[nameof(DetailOddBack)];
		[JsonIgnore]
		public Color DetailEvenBack => Colors[nameof(DetailEvenBack)];

		[JsonIgnore]
		public Color MonthHeader => Colors[nameof(MonthHeader)];
		[JsonIgnore]
		public Color MonthPrimary => Colors[nameof(MonthPrimary)];
		[JsonIgnore]
		public Color MonthSecondary => Colors[nameof(MonthSecondary)];
		[JsonIgnore]
		public Color MonthGrid => Colors[nameof(MonthGrid)];
		[JsonIgnore]
		public Color MonthDayFore => Colors[nameof(MonthDayFore)];
		[JsonIgnore]
		public Color MonthDayBack => Colors[nameof(MonthDayBack)];
		[JsonIgnore]
		public Color MonthTodayFore => Colors[nameof(MonthTodayFore)];
		[JsonIgnore]
		public Color MonthTodayBack => Colors[nameof(MonthTodayBack)];
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
		/// 
		/// </summary>
		/// <param name="container"></param>
		public void InitializeTheme(ContainerControl container)
		{
			var provider = new SettingsProvider();
			var mode = provider.Theme;

			var designMode = false;
			if (container is Component component)
			{
				// pull DesignMode protected property from container
				// so we can support the Visual Studio Forms Designer
				designMode = (bool)container.GetType()
					.BaseType
					.GetProperty("DesignMode", BindingFlags.Instance | BindingFlags.NonPublic)?
					.GetValue(component);
			}

			if ((mode != ThemeMode.User) || !LoadColors())
			{
				DarkMode = !designMode &&
					(mode == ThemeMode.Dark ||
					(mode == ThemeMode.System && Office.SystemDefaultDarkMode()));

				// set colors...

				var cache = JsonConvert.DeserializeObject<ThemeProvider>(
					DarkMode ? Properties.Resources.DarkTheme : Properties.Resources.LightTheme,
					new ColorConverter());

				Colors.Clear();
				Colors = cache.Colors;
			}

			// apply colors...

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
			}
		}


		public static bool HasCustomTheme()
		{
			var path = Path.Combine(PathHelper.GetAppDataPath(), CustomThemeFile);
			return File.Exists(path);
		}


		private bool LoadColors()
		{
			var path = Path.Combine(PathHelper.GetAppDataPath(), CustomThemeFile);
			if (File.Exists(path))
			{
				try
				{
					var json = File.ReadAllText(path);
					var cache = JsonConvert.DeserializeObject<ThemeProvider>(json, new ColorConverter());

					Colors.Clear();
					Colors = cache.Colors;
					DarkMode = cache.DarkMode;
					return true;
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine("error loading custom theme file", exc);
				}
			}

			return false;
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
