//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Drawing.Text;
	using System.Runtime.InteropServices.ComTypes;


	internal static class TileFactory
	{

		/// <summary>
		/// Create a new image of a font style sample for use in the Custom Styles ribbon gallery
		/// </summary>
		/// <param name="themeStyle">The Style of the font sample to render</param>
		/// <param name="background">The background color of the given page</param>
		/// <returns>A new Image instance</returns>
		public static IStream MakeStyleTile(Style themeStyle, Color background)
		{
			var scale = 1.0f;

			var tileWidth = (int)(70f * scale);
			var tileHeight = (int)(60f * scale);

			using var image = new Bitmap(tileWidth, tileHeight);
			using var g = Graphics.FromImage(image);

#if Unecessary // Used in LinqPad but OneNote does its own scaling so we don't need this...
			using (var b = new Bitmap(1, 1)) { scale = g.DpiY / 144; }
#endif

			g.Clear(background);
			//g.InterpolationMode = InterpolationMode.NearestNeighbor;
			//g.TextRenderingHint = TextRenderingHint.AntiAlias;

			using var style = new GraphicStyle(themeStyle, true);

			// draw name...

			using var font = new Font("Tahoma", 6f * scale, FontStyle.Regular);
			var name = FitText(style.Name, tileWidth, g, font, out var nameSize);
			var backbrush = background.GetBrightness() <= 0.5 ? Brushes.White : Brushes.Black;

			// centered horizontally at top of tile
			g.DrawString(name, font, backbrush, (tileWidth - nameSize.Width) / 2f, 3f);

			// draw sample...

			var fore = style.ApplyColors
				? style.Foreground
				: background.GetBrightness() <= 0.5 ? Color.White : Color.Black;

			using var brush = new SolidBrush(fore);
			// either centered or left justified
			var textsize = g.MeasureString("AaBbCc123", style.Font);
			var x = textsize.Width >= tileWidth ? 0 : (tileWidth - textsize.Width) / 2;

			// throw away font decent so all baselies are aligned
			var decentDesignUnits = style.Font.FontFamily.GetCellDescent(style.Font.Style);
			var emHeight = style.Font.FontFamily.GetEmHeight(style.Font.Style);
			var decent = style.Font.Size * decentDesignUnits / emHeight * scale;
			var y = tileHeight - textsize.Height + decent;

			// adjustment for stupid decents
			if (style.Font.Size < 12) y -= 6;
			else if (style.Font.Size < 20) y -= 3;
			else y -= 3;

			// clipping rectangle for background and text overflow
			var rec = new Rectangle(
				(int)x, (int)Math.Max(y, nameSize.Height),
				tileWidth, (int)textsize.Height);

			g.SetClip(rec);

			if (style.ApplyColors &&
				!style.Background.IsEmpty &&
				!style.Background.Equals(Color.Transparent))
			{
				using var backBrush = new SolidBrush(style.Background);
				g.FillRectangle(backBrush, rec);
			}

			g.DrawString("AaBbCc123", style.Font, brush, x, y);

			return image.GetReadOnlyStream();


			// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

			static string FitText(
				string text, int width, Graphics graphics, Font font, out Size size)
			{
				var sizef = graphics.MeasureString(text, font);
				if ((sizef.Width) > width)
				{
					text = text.Substring(0, text.Length - 1);
					sizef = graphics.MeasureString(text + "...", font);

					while (sizef.Width > width)
					{
						text = text.Substring(0, text.Length - 1);
						if (text.Length <= 3) break;

						sizef = graphics.MeasureString(text + "...", font);
					}

					text += "...";
				}

				size = sizef.ToSize();
				return text;
			}
		}


		//========================================================================================

		/// <summary>
		/// 
		/// </summary>
		/// <param name="theme"></param>
		/// <param name="background"></param>
		/// <returns></returns>
		public static IStream MakeTableTile(TableTheme theme, Color background)
		{
			var image = new Bitmap(70, 60);

			// inset table so there's a margin
			var bounds = new Rectangle(7, 7, 55, 45);

			var painter = new TableThemePainter(image, bounds, background);
			painter.Paint(theme);

			return image.GetReadOnlyStream();
		}
	}
}
