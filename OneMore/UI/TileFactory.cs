//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Drawing.Text;
	using System.Runtime.InteropServices.ComTypes;


	internal class TileFactory
	{
		public TileFactory()
		{
		}


		public IStream MakeTile(Style themeStyle, Color pageColor)
		{
			float scale = 1.0f;

#if Unecessary // OneNote does its own scaling so we don't need this...
			using (var b = new Bitmap(1, 1)) { using (var g = Graphics.FromImage(b)) { scale = g.DpiY / 96; } }
#endif

			IStream stream = null;

			int tileWidth = (int)(70f * scale);
			int tileHeight = (int)(60f * scale);

			using (var image = new Bitmap(tileWidth, tileHeight))
			{
				using (var graphics = Graphics.FromImage(image))
				{
					graphics.Clear(pageColor);
					graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
					graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

					// TODO: scale font size?
					using (var style = new GraphicStyle(themeStyle))
					{
						// draw name...

						Size nameSize;
						using (var font = new Font("Tahoma", 6f * scale, FontStyle.Regular))
						{
							var name = FitText(style.Name, tileWidth, graphics, font, out nameSize);
							var brush = pageColor.GetBrightness() <= 0.5 ? Brushes.White : Brushes.Black;

							// centered horizontally at top of tile
							graphics.DrawString(name, font, brush, (tileWidth - nameSize.Width) / 2f, 3f);
						}

						// draw sample...

						var fore = style.ApplyColors
							? style.Foreground
							: pageColor.GetBrightness() <= 0.5 ? Color.White : Color.Black;

						using (var brush = new SolidBrush(fore))
						{
							// either centered or left justified
							var textsize = graphics.MeasureString("AaBbCc123", style.Font);
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

							graphics.SetClip(rec);

							if (style.ApplyColors &&
								!style.Background.IsEmpty &&
								!style.Background.Equals(Color.Transparent))
							{
								using (var backBrush = new SolidBrush(style.Background))
								{
									graphics.FillRectangle(backBrush, rec);
								}
							}

							graphics.DrawString("AaBbCc123", style.Font, brush, x, y);
						}
					}
				}

				stream = image.GetReadOnlyStream();
			}

			return stream;
		}


		private static string FitText(
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
}
