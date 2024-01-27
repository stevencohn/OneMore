//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S3011 // Reflection should not be used to increase accessibility...

namespace River.OneMoreAddIn.UI
{
	using Newtonsoft.Json;
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Drawing;
	using System.IO;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Identifies the available themes for the application
	/// </summary>
	internal enum ThemeMode
	{
		System,
		Light,
		Dark,
		User
	}


	/// <summary>
	/// Defines themes and projects onto UI elements
	/// </summary>
	internal class ThemeManager : Loggable
	{

		private const string CustomThemeFile = "OneMoreTheme.json";
		private static ThemeManager instance;
		private static bool loading = false;


		private ThemeManager()
		{
			if (!loading)
			{
				LoadColors();
			}
		}


		/// <summary>
		/// Gets a value indicating whether the theme is set to dark mode
		/// </summary>
		[JsonProperty]
		public bool DarkMode { get; private set; }


		[JsonProperty]
		private Dictionary<string, Color> Colors { get; set; }


		public static ThemeManager Instance => instance ??= new ThemeManager();


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
		public Color HotTrack => Colors[nameof(HotTrack)];
		[JsonIgnore]
		public Color Control => Colors[nameof(Control)];
		[JsonIgnore]
		public Color ControlLightLight => Colors[nameof(ControlLightLight)];
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
		#endregion Convenience Properties

		#region Native
		private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
		private const int DWMWA_MICA_EFFECT = 1029;

		[DllImport("dwmapi.dll", PreserveSig = true)]
		private static extern int DwmSetWindowAttribute(
			IntPtr hwnd, int attr, ref bool attrValue, int attrSize);

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
		public static extern int SetWindowTheme(IntPtr hWnd, String pszSubAppName, String pszSubIdList);
		#endregion Native



		/// <summary>
		/// 
		/// </summary>
		/// <param name="container"></param>
		public void InitializeTheme(ContainerControl container)
		{
			if (container == null)
			{
				return;
			}

			// apply colors...

			if (container is MoreForm form)
			{
				SetThemeRecursively(container, DarkMode);
				form.OnThemeChange();
			}
			else if (container is MoreUserControl control)
			{
				control.OnThemeChange();
			}

			Colorize(container);
		}


		private void LoadColors()
		{
			ThemeManager cache;

			loading = true;
			var path = Path.Combine(PathHelper.GetAppDataPath(), CustomThemeFile);
			if (File.Exists(path))
			{
				try
				{
					var json = File.ReadAllText(path);
					cache = JsonConvert.DeserializeObject<ThemeManager>(json, new ColorConverter());

					Colors = cache.Colors;
					DarkMode = cache.DarkMode;
					return;
				}
				catch (Exception exc)
				{
					logger.WriteLine("error loading custom theme file, using default theme", exc);
				}
			}

			var designMode =
				LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
				System.Diagnostics.Process.GetCurrentProcess().ProcessName == "devenv";

			var provider = new SettingsProvider();
			var mode = provider.Theme;

			DarkMode = !designMode &&
				(mode == ThemeMode.Dark ||
				(mode == ThemeMode.System && Office.IsBlackThemeEnabled(true)));

			// set colors...

			cache = JsonConvert.DeserializeObject<ThemeManager>(
				DarkMode ? Resx.DarkTheme : Resx.LightTheme,
				new ColorConverter());

			Colors = cache.Colors;
			loading = false;
		}


		private void SetThemeRecursively(Control control, bool dark)
		{
			void SetTheme(Control control)
			{
				bool trueValue = dark;
				// DarkMode_Explorer sets radios, checkboxes, scrollbars to dark mode Explorer 
				SetWindowTheme(control.Handle, "DarkMode_Explorer", null);

				DwmSetWindowAttribute(control.Handle, 
					DWMWA_USE_IMMERSIVE_DARK_MODE, ref trueValue, Marshal.SizeOf(typeof(bool)));

				DwmSetWindowAttribute(control.Handle,
					DWMWA_MICA_EFFECT, ref trueValue, Marshal.SizeOf(typeof(bool)));
			}

			SetTheme(control);

			foreach (Control child in control.Controls)
			{
				SetThemeRecursively(child, dark);
			}
		}


		public static bool HasCustomTheme()
		{
			var path = Path.Combine(PathHelper.GetAppDataPath(), CustomThemeFile);
			return File.Exists(path);
		}


		private void Colorize(Control control)
		{
			if (control is IThemedControl themed)
			{
				control.BackColor = GetThemedColor("Control", themed.PreferredBack);
				control.ForeColor = GetThemedColor("ControlText", themed.PreferredFore);
			}
			else
			{
				control.BackColor = GetThemedColor(control.BackColor);
				control.ForeColor = GetThemedColor(control.ForeColor);
			}

			// for each of the following, parent should have already been themed by now...

			if (control is Label label)
			{
				label.BackColor = label.Parent.BackColor;
			}
			else if (control is PictureBox pbox)
			{
				pbox.BackColor = pbox.Parent.BackColor;
			}
			else if (control is ListView view)
			{
				foreach (ListViewItem item in view.Items)
				{
					item.BackColor = ControlLightLight;
					item.ForeColor = ForeColor;
				}
			}
			else if (control is MoreLinkLabel linkLabel)
			{
				linkLabel.LinkColor = LinkColor;
				linkLabel.HoverColor = HoverColor;
				linkLabel.ActiveLinkColor = LinkColor;
				linkLabel.BackColor = linkLabel.Parent.BackColor;
			}
			else if (control is TabControl tabs)
			{
				foreach (TabPage page in tabs.TabPages)
				{
					Colorize(page);
				}
			}

			foreach (Control child in control.Controls)
			{
				Colorize(child);
			}
		}


		public Color GetThemedColor(Color color)
		{
			return color.IsSystemColor && Colors.ContainsKey(color.Name)
				? Colors[color.Name]
				: color;
		}


		public Color GetThemedColor(string key, Color preferred = default)
		{
			if (preferred == Color.Empty)
			{
				return Colors[key];
			}

			if (preferred.IsSystemColor && Colors.ContainsKey(preferred.Name))
			{
				return Colors[preferred.Name];
			}

			return preferred;
		}


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
	}
}
