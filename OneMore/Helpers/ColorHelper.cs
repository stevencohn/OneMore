//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Drawing;


	internal static class ColorHelper
	{

		/// <summary>
		/// Converts a CSS color specification, a color name, or the constant "automatic"
		/// to a Color instance.
		/// </summary>
		/// <param name="html">The color specification</param>
		/// <returns>
		/// A new Color value, Color.Transparent, or Color.Empty if the html specification
		/// cannot be converted to a proper value
		/// </returns>
		public static Color FromHtml(string html)
		{
			if (html.Equals(StyleBase.Automatic))
			{
				return Color.Transparent;
			}

			if (html.StartsWith("#"))
			{
				return ColorTranslator.FromHtml(html);
			}

			if (html.Equals("none", StringComparison.InvariantCultureIgnoreCase))
			{
				return Color.Empty;
			}

			try
			{
				return Color.FromName(html);
			}
			catch
			{
				return Color.Empty;
			}
		}


		/// <summary>
		/// Converts a simple 0x123456 int color value to a Color
		/// </summary>
		/// <param name="rgb">Color represented as an int</param>
		/// <returns>A Color instance representing the value</returns>
		/// <remarks>
		/// This is required because Color.FromArgb(0x123456) will result in an alpha value of
		/// zero instead of 0xFF
		/// </remarks>
		public static Color FromRgb(int rgb)
		{
			var r = (rgb & 0xFF0000) >> 16;
			var g = (rgb & 0x00FF00) >> 8;
			var b = rgb & 0x0000FF;
			return Color.FromArgb(0xFF, r, g, b);
		}
	}
}
