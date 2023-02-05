//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Drawing;


	internal static class ColorHelper
	{
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
