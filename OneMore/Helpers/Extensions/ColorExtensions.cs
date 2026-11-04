//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Helpers.Office;
	using System;
	using System.Drawing;
	using System.Windows.Forms;


	internal static class ColorExtensions
	{

		/// <summary>
		/// OneMore Extension >> Inverts the color to estimate how the color would be presented
		/// by OneNote in if shown in dark mode.
		/// </summary>
		/// <param name="color">The raw color to invert</param>
		/// <returns>An inverted color</returns>
		public static Color Invert(this Color color)
		{
			if (color.Equals(Color.Transparent) || color.Equals(Color.Empty))
			{
				return color;
			}

			return color.IsLight()
				? ControlPaint.Dark(color, 0.9f)
				: ControlPaint.Light(color, 1.9f);
		}


		/// <summary>
		/// OneMore Extension >> Determines if a color is generally "dark" which would imply low
		/// constrast resulting in readability issues against a dark background
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public static bool IsDark(this Color color)
		{
			var limit = Office.IsBlackThemeEnabled() ? 0.3 : 0.5;
			return color.GetBrightness() < limit;
		}


		/// <summary>
		/// OneMore Extension >> Determines if a color is generally "light" which would imply low
		/// constrast resulting in readability issues against a light background
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public static bool IsLight(this Color color)
		{
			var limit = Office.IsBlackThemeEnabled() ? 0.6 : 0.5;
			return color.GetBrightness() > limit;
		}


		/// <summary>
		/// OneMore Extension >> Determines if the color describes any shade of gray in where
		/// all of R, G, and B values are equivalent which includes both white and black.
		/// Equivalence means within +2/-2
		/// </summary>
		/// <param name="color">The Color to test</param>
		/// <returns>True if the color is a shade of gray; false otherwise</returns>
		public static bool IsGray(this Color color)
		{
			return (Math.Abs(color.R - color.G) < 3) && (Math.Abs(color.R - color.B) < 3);
		}


		/// <summary>
		/// OneMore Extension >> 
		/// </summary>
		/// <param name="color"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		public static bool LowContrast(this Color color, Color other)
		{
			if (color.Equals(other))
			{
				return true;
			}

			//if (Office.IsBlackThemeEnabled())
			//{
			//	color = color.Invert();
			//	other = other.Invert();
			//	return Math.Abs(color.GetBrightness() - other.GetBrightness()) < 0.2;
			//}

			return Math.Abs(color.GetBrightness() - other.GetBrightness()) < 0.3;
		}


		/// <summary>
		/// OneMore Extension >> Compare this color with a given color and determines if they are
		/// close enough to be called the same color
		/// </summary>
		/// <param name="color">The Color value</param>
		/// <param name="candidate">A candidate color to test</param>
		/// <returns>True if the candiate color is a close match to this color</returns>
		public static bool Matches(this Color color, Color candidate)
		{
			return
				Math.Abs(color.A - candidate.A) < 3 &&
				Math.Abs(color.R - candidate.R) < 3 &&
				Math.Abs(color.G - candidate.G) < 3 &&
				Math.Abs(color.B - candidate.B) < 3;
		}


		/// <summary>
		/// OneMore Extension >> Determines whether two colors look alike, using the perceptual
		/// distance (CIE76 delta-E) rather than per-channel RGB differences. Alpha is ignored.
		/// </summary>
		/// <param name="color">The Color value</param>
		/// <param name="other">A color to compare</param>
		/// <param name="threshold">
		/// The largest delta-E still considered the same; about 2.3 is barely noticeable and
		/// about 10 is the same color family
		/// </param>
		public static bool IsSimilar(this Color color, Color other, double threshold = 10)
		{
			return color.DeltaE(other) <= threshold;
		}


		/// <summary>
		/// OneMore Extension >> Gets the perceptual distance between two colors as the
		/// Euclidean distance in CIE L*a*b* space: 0 is identical, black to white is about 100.
		/// </summary>
		public static double DeltaE(this Color color, Color other)
		{
			var (l1, a1, b1) = ToLab(color);
			var (l2, a2, b2) = ToLab(other);

			return Math.Sqrt(
				((l1 - l2) * (l1 - l2)) + ((a1 - a2) * (a1 - a2)) + ((b1 - b2) * (b1 - b2)));
		}


		// sRGB -> linear RGB -> XYZ (D65) -> Lab
		private static (double L, double A, double B) ToLab(Color color)
		{
			static double Linear(byte channel)
			{
				var c = channel / 255.0;
				return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
			}

			static double Pivot(double t)
			{
				return t > 0.008856 ? Math.Pow(t, 1.0 / 3.0) : (7.787 * t) + (16.0 / 116.0);
			}

			var r = Linear(color.R);
			var g = Linear(color.G);
			var b = Linear(color.B);

			var x = ((r * 0.4124564) + (g * 0.3575761) + (b * 0.1804375)) / 0.95047;
			var y = (r * 0.2126729) + (g * 0.7151522) + (b * 0.0721750);
			var z = ((r * 0.0193339) + (g * 0.1191920) + (b * 0.9503041)) / 1.08883;

			var fx = Pivot(x);
			var fy = Pivot(y);
			var fz = Pivot(z);

			return ((116.0 * fy) - 16.0, 500.0 * (fx - fy), 200.0 * (fy - fz));
		}


		/// <summary>
		/// OneMore Extension >> Formats an HTML RGB seven character (without AA) to appease
		/// OneNote and with all uppercase letters to avoid case-sensitive comparison problems
		/// </summary>
		/// <param name="color">The Color value</param>
		/// <returns>A string specifying #RRGGBB</returns>
		public static string ToRGBHtml(this Color color)
		{
			return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
		}


		/// <summary>
		/// OneMore Extension >> Gets either the known name of the color or its RGBHtml string
		/// value if it is not a known color.
		/// </summary>
		/// <param name="color">The Color value</param>
		/// <returns>A string specifying either the name or #RRGGBB</returns>
		public static string ToNamedString(this Color color)
		{
			if (color.IsEmpty)
			{
				return "auto";
			}

			if (color.IsKnownColor || color.IsNamedColor || color.IsSystemColor)
			{
				return color.Name;
			}

			return color.ToRGBHtml();
		}
	}
}
