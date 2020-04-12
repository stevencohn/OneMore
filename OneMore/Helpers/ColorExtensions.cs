//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Drawing;


	internal static class ColorExtensions
	{

		/// <summary>
		/// Formats an HTML RGB seven character (without AA) to appease OneNote
		/// and with all uppercase letters to avoid case-sensitive comparison problems
		/// </summary>
		/// <param name="color">The Color value</param>
		/// <returns>A string specifying #RRGGBB</returns>
		public static string ToRGBHtml(this Color color)
		{
			return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
		}
	}
}
