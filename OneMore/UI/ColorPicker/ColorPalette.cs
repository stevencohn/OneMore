//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;


	/// <summary>
	/// The Office-style swatch palettes in light mode and the colors OneNote renders for each
	/// when the page background is dark. OneNote offers no formula for the dark-mode mapping, see
	/// docs/developers/TechNote - Colors.htm, so it is a literal table; each dark entry
	/// is positionally paired with the light entry at the same index.
	/// </summary>
	internal static class ColorPalette
	{
		public const int Columns = 10;
		public const int GridRows = 5;

		private static readonly string[] lightTheme =
		{
			"FFFFFF", "000000", "E7E6E6", "44546A", "5B9BD5",
			"ED7D31", "E84C22", "FFC000", "8064A2", "70AD47"
		};

		private static readonly string[] darkTheme =
		{
			"201F1E", "EDEBE9", "1C1C1C", "BBC7D3", "327DC3",
			"C15710", "EA5F39", "664E00", "AB99C2", "517F34"
		};

		// row-major, 5 rows of 10
		private static readonly string[] lightGrid =
		{
			"F2F2F2", "7F7F7F", "D0CECE", "D6DCE4", "DEEBF6", "FBE5D5", "FADBD2", "FFF2CC", "E5E0EC", "E2EFD9",
			"D8D8D8", "595959", "AEABAB", "ADB9CA", "BDD7EE", "F7CBAC", "F5B7A6", "FEE599", "CCC1D9", "C5E0B3",
			"BFBFBF", "3F3F3F", "757070", "8496B0", "9CC3E5", "F4B183", "F1937A", "FFD965", "B2A1C7", "A8D08D",
			"A5A5A5", "262626", "3A3838", "323F4F", "2E75B5", "C55A11", "B43512", "BF9000", "5F497A", "538135",
			"7F7F7F", "0C0C0C", "171616", "222A35", "1E4E79", "833C0B", "78230C", "7F6000", "3F3151", "375623"
		};

		private static readonly string[] darkGrid =
		{
			"0D0D0D", "8F8F8F", "544F4F", "3C465D", "1A4665", "69310C", "7F2A10", "4D3B00", "4E3C62", "27441D",
			"4D4D4D", "B3B0AD", "6C6565", "545783", "1D5072", "7B3909", "9A3013", "564201", "614F7D", "33521E",
			"414140", "E8E8E8", "7C7676", "677D9E", "275F90", "90440E", "BA3712", "5C4500", "7C5D98", "405E26",
			"5A5A5A", "D9D9D9", "F2F2F2", "E6EBEF", "69A4D8", "EC7627", "F39A81", "997300", "CFC3DA", "6CAA46",
			"8F8F8F", "F3F3F3", "FFFFFF", "F7F9FB", "B2D0EB", "F9C59F", "FCDED9", "DBA400", "F6F2F8", "B6D9A1"
		};

		private static readonly string[] lightStandard =
		{
			"C00000", "FF0000", "FFC000", "FFFF00", "92D050",
			"00B050", "00B0F0", "0070C0", "002060", "7030A0"
		};

		private static readonly string[] darkStandard =
		{
			"FF6363", "FF0505", "893A06", "8A6700", "466D1D",
			"00853C", "018687", "3DAEFF", "76CAFF", "A76ED4"
		};

		private static readonly Dictionary<int, Color> darkMap = BuildDarkMap();


		public static Color[] Theme(bool dark) => Convert(dark ? darkTheme : lightTheme);

		public static Color[] Grid(bool dark) => Convert(dark ? darkGrid : lightGrid);

		public static Color[] Standard(bool dark) => Convert(dark ? darkStandard : lightStandard);


		/// <summary>
		/// Gets the color OneNote would render on a dark page for the given light-mode color.
		/// Palette colors use the literal table; any other color, such as a recent or custom
		/// color, such as a recent or custom color, is computed by inverting its lightness.
		/// </summary>
		public static Color ToDark(Color color)
		{
			if (color.IsEmpty || color.A == 0)
			{
				return color;
			}

			if (darkMap.TryGetValue(color.ToArgb() & 0xFFFFFF, out var dark))
			{
				return dark;
			}

			return InvertLightness(color);
		}


		/// <summary>
		/// Gets the logical color that OneNote renders as the given color on a dark page; the
		/// inverse of ToDark. Lightness inversion is its own inverse.
		/// </summary>
		public static Color ToLight(Color rendered)
		{
			if (rendered.IsEmpty || rendered.A == 0)
			{
				return rendered;
			}

			return InvertLightness(rendered);
		}


		/// <summary>
		/// For colors outside the palette tables, OneNote keeps hue and HSL saturation and
		/// inverts HSL lightness (L becomes 1 - L). This was measured by rendering a grid of
		/// 730 colors on a dark page, see Reference/Capture-DarkColors.ps1. Only grays near
		/// the palette grays deviate, and those are covered by the palette tables.
		/// </summary>
		public static Color InvertLightness(Color color)
		{
			ColorMath.ToHsl(color, out var h, out var s, out var l);
			return ColorMath.FromHsl(h, s, 1 - l);
		}

		private static Dictionary<int, Color> BuildDarkMap()
		{
			var map = new Dictionary<int, Color>();
			Pair(map, lightTheme, darkTheme);
			Pair(map, lightGrid, darkGrid);
			Pair(map, lightStandard, darkStandard);
			return map;
		}


		private static void Pair(Dictionary<int, Color> map, string[] light, string[] dark)
		{
			for (var i = 0; i < light.Length; i++)
			{
				var key = Parse(light[i]).ToArgb() & 0xFFFFFF;
				if (!map.ContainsKey(key))
				{
					map.Add(key, Parse(dark[i]));
				}
			}
		}


		private static Color[] Convert(string[] hex)
		{
			var colors = new Color[hex.Length];
			for (var i = 0; i < hex.Length; i++)
			{
				colors[i] = Parse(hex[i]);
			}

			return colors;
		}


		private static Color Parse(string hex)
		{
			ColorMath.TryParseHex(hex, out var color);
			return color;
		}
	}
}
