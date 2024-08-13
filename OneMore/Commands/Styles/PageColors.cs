//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Office;
	using System.Collections.Generic;
	using System.Drawing;


	/// <summary>
	/// A custom page color swatch
	/// </summary>
	internal sealed class Swatch
	{
		public const int Size = 60;
		public const int Margin = 3;


		public readonly string Name;
		public Color Color { get; set; }
		public Color Paint { get; set; }
		public Rectangle Bounds { get; set; }
		public Swatch(string name, int color)
		{
			Name = name;
			Color = ColorHelper.FromRgb(color);
			Paint = Color.Empty;
		}
	}


	/// <summary>
	/// Custom page colors
	/// </summary>
	internal class PageColors : List<Swatch>
	{

		/// <summary>
		/// 
		/// </summary>
		public PageColors()
		{
			if (Office.IsBlackThemeEnabled())
			{
				// dark colors - the color:param is applied to the page but dynamically inverted
				// by OneNote; the rendered color follows a conversion algorithm. The paint:param
				// is used to render the swatch, showing how it will appear when applied to a page
				// in dark mode...

				// dark
				Register("Black", 0xEEEEEE, 0x131313);
				Register("Gray", 0xCFCFCF, 0x303030);
				Register("Dark Teal", 0xB6DCDB, 0x1F4140);
				Register("Dark Olive Green", 0xD0E2C4, 0x2F4321);
				Register("Dark Slate Blue", 0xB6B6D1, 0x232338);
				Register("Midnight Blue", 0xBBC2D8, 0x23293D);
				Register("Saddle Brown", 0xE5CCB3, 0x473018);
				Register("Brown", 0xDAABAB, 0x401C1C);
				Register("Dark Purple", 0xE0C2D5, 0x412135);
				// light
				Register("White", 0x010101, 0xF3F3F3);
				Register("White Smoke", 0x111111, 0xF2F2F2);
				Register("Alice Blue", 0x003E75, 0xECF5FF);
				Register("Mint Cream", 0x00753A, 0xECFFF5);
				Register("Honeydew", 0x007500, 0xECFFEC);
				Register("Ivory", 0x757500, 0xFFFFEC);
				Register("Snow", 0x225151, 0xF0F9F9);
				Register("Seashell", 0x220E00, 0xFFF0E6);
				Register("Linen", 0x1B1005, 0xFCF3EB);
			}
			else
			{
				// light colors - explicit, exact color representations from ligth to dark
				// are applied to the page and rendered as specified...

				// light
				Register("White", 0xFFFFFF);
				Register("White Smoke", 0xF2F2F2);
				Register("Alice Blue", 0xECF5FF);
				Register("Mint Cream", 0xECFFF5);
				Register("Honeydew", 0xECFFEC);
				Register("Ivory", 0xFFFFEC);
				Register("Snow", 0xF0F9F9);
				Register("Seashell", 0xFFF0E6);
				Register("Linen", 0xFCF3EB);
				// dark
				Register("Black", 0x131313);
				Register("Gray", 0x303030);
				Register("Dark Teal", 0x1F4140);
				Register("Dark Olive Green", 0x2F4321);
				Register("Dark Slate Blue", 0x232338);
				Register("Midnight Blue", 0x23293D);
				Register("Saddle Brown", 0x473018);
				Register("Brown", 0x401C1C);
				Register("Dark Purple", 0x412135);
			}
		}


		private void Register(string name, int color, int? paint = null)
		{
			var swatch = new Swatch(name, color);
			swatch.Paint = paint == null ? swatch.Color : ColorHelper.FromRgb((int)paint);

			var row = Count / 3;
			var col = Count == 0 ? 0 : Count % 3;

			swatch.Bounds = new Rectangle(
				Swatch.Margin + (col * (Swatch.Size + Swatch.Margin)),
				Swatch.Margin + (row * (Swatch.Size + Swatch.Margin)),
				Swatch.Size, Swatch.Size
				);

			Add(swatch);
		}
	}
}
