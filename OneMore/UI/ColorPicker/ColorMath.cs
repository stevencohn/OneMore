//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Drawing;
	using System.Globalization;


	/// <summary>
	/// Pure RGB, HSV, HSL and hex conversions used by MoreColorPicker. Hue is 0-360,
	/// saturation, value and lightness are 0-1.
	/// </summary>
	internal static class ColorMath
	{
		public static void ToHsv(Color color, out double h, out double s, out double v)
		{
			var r = color.R / 255.0;
			var g = color.G / 255.0;
			var b = color.B / 255.0;

			var max = Math.Max(r, Math.Max(g, b));
			var min = Math.Min(r, Math.Min(g, b));
			var delta = max - min;

			h = Hue(r, g, b, max, delta);
			v = max;
			s = max <= 0 ? 0 : delta / max;
		}


		public static Color FromHsv(double h, double s, double v)
		{
			h = ((h % 360) + 360) % 360;
			s = Clamp(s);
			v = Clamp(v);

			var c = v * s;
			var x = c * (1 - Math.Abs(((h / 60) % 2) - 1));
			var m = v - c;

			return FromSector(h, c, x, m);
		}


		public static void ToHsl(Color color, out double h, out double s, out double l)
		{
			var r = color.R / 255.0;
			var g = color.G / 255.0;
			var b = color.B / 255.0;

			var max = Math.Max(r, Math.Max(g, b));
			var min = Math.Min(r, Math.Min(g, b));
			var delta = max - min;

			h = Hue(r, g, b, max, delta);
			l = (max + min) / 2;
			s = delta <= 0 ? 0 : delta / (1 - Math.Abs((2 * l) - 1));
		}


		public static Color FromHsl(double h, double s, double l)
		{
			h = ((h % 360) + 360) % 360;
			s = Clamp(s);
			l = Clamp(l);

			var c = (1 - Math.Abs((2 * l) - 1)) * s;
			var x = c * (1 - Math.Abs(((h / 60) % 2) - 1));
			var m = l - (c / 2);

			return FromSector(h, c, x, m);
		}


		private static double Hue(double r, double g, double b, double max, double delta)
		{
			if (delta <= 0)
			{
				return 0;
			}

			double h;
			if (max == r)
			{
				h = 60 * (((g - b) / delta) % 6);
			}
			else if (max == g)
			{
				h = 60 * (((b - r) / delta) + 2);
			}
			else
			{
				h = 60 * (((r - g) / delta) + 4);
			}

			return h < 0 ? h + 360 : h;
		}


		private static Color FromSector(double h, double c, double x, double m)
		{
			double r, g, b;
			if (h < 60) { r = c; g = x; b = 0; }
			else if (h < 120) { r = x; g = c; b = 0; }
			else if (h < 180) { r = 0; g = c; b = x; }
			else if (h < 240) { r = 0; g = x; b = c; }
			else if (h < 300) { r = x; g = 0; b = c; }
			else { r = c; g = 0; b = x; }

			return Color.FromArgb(Byte(r + m), Byte(g + m), Byte(b + m));
		}


		private static int Byte(double value)
		{
			return (int)Math.Round(Clamp(value) * 255);
		}


		private static double Clamp(double value)
		{
			return value < 0 ? 0 : (value > 1 ? 1 : value);
		}


		/// <summary>
		/// Parses #rrggbb or rrggbb, case insensitive
		/// </summary>
		public static bool TryParseHex(string text, out Color color)
		{
			color = Color.Empty;
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}

			text = text.Trim();
			if (text[0] == '#')
			{
				text = text.Substring(1);
			}

			if (text.Length != 6 ||
				!int.TryParse(text, NumberStyles.AllowHexSpecifier,
					CultureInfo.InvariantCulture, out var rgb))
			{
				return false;
			}

			color = Color.FromArgb((rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
			return true;
		}


		public static string ToHex(Color color)
		{
			return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
		}


		/// <summary>
		/// Compares only the RGB channels
		/// </summary>
		public static bool SameRgb(Color a, Color b)
		{
			return a.R == b.R && a.G == b.G && a.B == b.B;
		}
	}
}
