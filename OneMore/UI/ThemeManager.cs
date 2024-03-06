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
	using System.Linq;
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


		[JsonIgnore]
		public Color ButtonBack => Colors[nameof(ButtonBack)];
		[JsonIgnore]
		public Color ButtonBorder => Colors[nameof(ButtonBorder)];
		[JsonIgnore]
		public Color ButtonHotBack => Colors[nameof(ButtonHotBack)];
		[JsonIgnore]
		public Color ButtonHotBorder => Colors[nameof(ButtonHotBorder)];
		[JsonIgnore]
		public Color ButtonPressBorder => Colors[nameof(ButtonPressBorder)];

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
		/// For individual controls like MoreAutocompleteList...
		/// </summary>
		/// <param name="control"></param>
		public void InitializeTheme(Control control)
		{
			if (DarkMode)
			{
				SetWindowTheme(control);
			}
		}


		/// <summary>
		/// For Forms and UserControls...
		/// </summary>
		/// <param name="container"></param>
		public void InitializeTheme(ContainerControl container)
		{
			if (container == null)
			{
				return;
			}

			// apply colors...

			if (DarkMode && (container is MoreForm || container is MoreUserControl))
			{
				SetWindowTheme(container);
				if (container is MoreForm form)
				{
					form.OnThemeChange();
				}
				else
				{
					((MoreUserControl)container).OnThemeChange();
				}
			}

			Colorize(container);
		}


		public void LoadColors(int modeIndex = -1)
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

			var mode = modeIndex >= 0
				? (ThemeMode)modeIndex
				: new SettingsProvider().Theme;

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


		private void SetWindowTheme(Control control)
		{
			if (control is MenuStrip)
			{
				return;
			}

			//logger.WriteLine($"SetWindowTheme {control.Name} {control.GetType()}");

			bool trueValue = DarkMode;

			// DarkMode_Explorer sets radios, checkboxes, scrollbars to dark mode Explorer 
			SetWindowTheme(control.Handle, "DarkMode_Explorer", null);

			DwmSetWindowAttribute(control.Handle,
				DWMWA_USE_IMMERSIVE_DARK_MODE, ref trueValue, Marshal.SizeOf(typeof(bool)));

			DwmSetWindowAttribute(control.Handle,
				DWMWA_MICA_EFFECT, ref trueValue, Marshal.SizeOf(typeof(bool)));

			// now its children...

			foreach (Control child in control.Controls)
			{
				if (child is StatusStrip || child is not ToolStrip)
				{
					//logger.WriteLine($"SetWindowTheme >> {child.Name} {child.GetType()}");
					SetWindowTheme(child);
				}
			}
		}


		public static bool HasCustomTheme()
		{
			var path = Path.Combine(PathHelper.GetAppDataPath(), CustomThemeFile);
			return File.Exists(path);
		}


		private void Colorize(Control control)
		{
			if (control is ListView ||
				control is MenuStrip ||
				(control is ToolStrip && control is not StatusStrip))
			{
				//logger.WriteLine($"skipping {control.Name} {control.GetType()}");
				return;
			}

			if (control is IThemedControl themed)
			{
				themed.ApplyTheme(this);
			}
			else
			{
				control.BackColor = GetColor(control.BackColor);
				control.ForeColor = control.Enabled
					? GetColor(control.ForeColor)
					: GetColor("GrayText");
			}

			// for each of the following, parent should have already been themed by now...

			if (control is ComboBox combo && DarkMode)
			{
				combo.FlatStyle = FlatStyle.Popup;
			}
			else if (control is Label label && control is not MoreLabel)
			{
				label.BackColor = label.Parent.BackColor;
			}
			else if (control is PictureBox pbox && pbox.BackColor.Equals(Color.Transparent))
			{
				pbox.BackColor = pbox.Parent.BackColor;
			}
			else if (control is ListView view)
			{
				foreach (ListViewItem item in view.Items)
				{
					item.BackColor = Colors["ControlLightLight"];
					item.ForeColor = Colors["ControlText"];
				}
			}
			else if (control is StatusStrip strip)
			{
				foreach (ToolStripItem item in strip.Items)
				{
					item.ForeColor = strip.ForeColor;
					item.BackColor = strip.BackColor;
				}
			}
			else if (control is DateTimePicker picker)
			{
				picker.CalendarForeColor = Colors["WindowText"];
				picker.CalendarMonthBackground = Colors["Window"];
				picker.CalendarTitleBackColor = Colors["Control"];
				picker.CalendarTitleForeColor = Colors["ControlText"];
			}

			// temp filter for ListView until that one is refactored
			foreach (var child in control.Controls.OfType<Control>()
				.Where(c =>
					c is not ListView &&
					(c is StatusStrip || c is not ToolStrip)))
			{
				//logger.WriteLine($"Colorize {child.Name} {child.GetType()}");
				Colorize(child);
			}
		}


		public Image GetGrayImage(Image image)
		{
			var editor = new Commands.ImageEditor { Style = Commands.ImageEditor.Stylization.GrayScale };
			return editor.Apply(image);
		}


		public Color GetColor(Color color)
		{
			return color.IsSystemColor && Colors.ContainsKey(color.Name)
				? Colors[color.Name]
				: color;
		}


		public Color GetColor(string key, string themed = null)
		{
			// preferred could be a SystemColor like "ControlText" or NamedColor like "Blue"
			if (!string.IsNullOrWhiteSpace(themed) && Colors.ContainsKey(themed))
			{
				return Colors[themed];
			}

			if (!string.IsNullOrWhiteSpace(key) && Colors.ContainsKey(key))
			{
				return Colors[key];
			}

			return Color.Magenta;
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
