//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
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
