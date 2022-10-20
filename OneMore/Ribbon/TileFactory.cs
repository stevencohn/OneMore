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


	internal static class TileFactory
	{

		/// <summary>
		/// Create a new image of a font style sample for use in the Custom Styles ribbon gallery
		/// </summary>
		/// <param name="themeStyle">The Style of the font sample to render</param>
		/// <param name="pageColor">The background color of the given page</param>
		/// <returns>A new Image instance</returns>
		public static IStream MakeStyleTile(Style themeStyle, Color pageColor)
		{
			var scale = 1.0f;

#if Unecessary // Used in LinqPad but OneNote does its own scaling so we don't need this...
			using (var b = new Bitmap(1, 1)) { using (var g = Graphics.FromImage(b)) { scale = g.DpiY / 96; } }
#endif
			var tileWidth = (int)(70f * scale);
			var tileHeight = (int)(60f * scale);

			using var image = new Bitmap(tileWidth, tileHeight);
			using var g = Graphics.FromImage(image);
			g.Clear(pageColor);
			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			g.TextRenderingHint = TextRenderingHint.AntiAlias;

			// TODO: scale font size?
			using var style = new GraphicStyle(themeStyle);

			// draw name...

			using var font = new Font("Tahoma", 6f * scale, FontStyle.Regular);
			var name = FitText(style.Name, tileWidth, g, font, out var nameSize);
			var backbrush = pageColor.GetBrightness() <= 0.5 ? Brushes.White : Brushes.Black;

			// centered horizontally at top of tile
			g.DrawString(name, font, backbrush, (tileWidth - nameSize.Width) / 2f, 3f);

			// draw sample...

			var fore = style.ApplyColors
				? style.Foreground
				: pageColor.GetBrightness() <= 0.5 ? Color.White : Color.Black;

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
		/// <param name="pageColor"></param>
		/// <returns></returns>
		public static IStream MakeTableTile(TableTheme theme, Color pageColor)
		{
			var bow0 = new string[] { "#B2A1C7", "#9CC3E5", "#A8D08D", "#FFD965", "#F4B183", "#F1937A" };
			var bow1 = new string[] { "#E5E0EC", "#DEEBF6", "#E2EFD9", "#FFF2CC", "#FBE5D5", "#FADBD2" };

			var scale = 1.0f;

#if Unecessary // Used in LinqPad but OneNote does its own scaling so we don't need this...
			using (var b = new Bitmap(1, 1)) { using (var g = Graphics.FromImage(b)) { scale = g.DpiY / 96; } }
#endif
			var tileWidth = (int)(70f * scale);
			var tileHeight = (int)(60f * scale);

			// inset table so there's a 10pt margin
			var bounds = new Rectangle(7, 7, 55, 45);
			//var bounds = new Rectangle(10, 10, 50, 40);
			//private Point TopLeft = new Point(7, 7);
			//private Point BottomRight = new Point(62, 52);

			// preset 5x5 grid
			var rowHeight = bounds.Height / 5;
			var columnWidth = bounds.Width / 5;

			var image = new Bitmap(tileWidth, tileHeight);

			using var g = Graphics.FromImage(image);
			g.Clear(pageColor);
			g.InterpolationMode = InterpolationMode.NearestNeighbor;

			DrawGrid();
			FillTable();

			if (theme.FirstColumn == TableTheme.Rainbow)
			{
				for (int r = 0; r < 5; r++)
				{
					using var b = new SolidBrush(ColorTranslator.FromHtml(bow0[r % bow0.Length]));
					FillCell(0, r, b);
				}
			}
			else if (theme.FirstColumn != Color.Empty)
			{
				using var b = new SolidBrush(theme.FirstColumn);
				for (int r = 0; r < 5; r++)
				{
					FillCell(0, r, b);
				}
			}

			if (theme.LastColumn != Color.Empty)
			{
				using var b = new SolidBrush(theme.LastColumn);
				for (int r = 0; r < 5; r++)
				{
					FillCell(4, r, b);
				}
			}

			if (theme.HeaderRow == TableTheme.Rainbow)
			{
				for (int c = 0; c < 5; c++)
				{
					using var b = new SolidBrush(ColorTranslator.FromHtml(bow0[c % 6]));
					FillCell(c, 0, b);
				}
			}
			else if (theme.HeaderRow != Color.Empty)
			{
				using var b = new SolidBrush(theme.HeaderRow);
				for (int c = 0; c < 5; c++)
				{
					FillCell(c, 0, b);
				}
			}

			if (theme.TotalRow != Color.Empty)
			{
				using var b = new SolidBrush(theme.TotalRow);
				for (int c = 0; c < 5; c++)
				{
					FillCell(c, 4, b);
				}
			}

			if (theme.HeaderFirstCell != Color.Empty)
			{
				using var b = new SolidBrush(theme.HeaderFirstCell);
				FillCell(0, 0, b);
			}

			if (theme.HeaderLastCell != Color.Empty)
			{
				using var b = new SolidBrush(theme.HeaderLastCell);
				FillCell(4, 0, b);
			}

			if (theme.TotalFirstCell != Color.Empty)
			{
				using var b = new SolidBrush(theme.TotalFirstCell);
				FillCell(0, 4, b);
			}

			if (theme.TotalLastCell != Color.Empty)
			{
				using var b = new SolidBrush(theme.TotalLastCell);
				FillCell(4, 4, b);
			}

			return image.GetReadOnlyStream();


			// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

			void DrawGrid()
			{
				var pen = pageColor.GetBrightness() <= 0.5 ? Pens.WhiteSmoke : Pens.Gray;
				g.DrawRectangle(pen, bounds);

				for (int x = bounds.X + columnWidth; x < bounds.Right - 5; x += columnWidth)
				{
					g.DrawLine(pen, x, bounds.Top, x, bounds.Bottom);
				}

				for (int y = bounds.Y + rowHeight; y < bounds.Bottom - 5; y += rowHeight)
				{
					g.DrawLine(pen, bounds.Left, y, bounds.Right, y);
				}
			}

			void FillTable()
			{
				if (theme.WholeTable == TableTheme.Rainbow)
				{
					if (theme.FirstColumn == TableTheme.Rainbow)
					{
						for (var r = 0; r < 5; r++)
						{
							for (var c = 0; c < 5; c++)
							{
								using var brush = new SolidBrush(ColorTranslator.FromHtml(bow1[r % bow1.Length]));
								FillCell(c, r, brush);
							}
						}

						return;
					}

					if (theme.HeaderRow == TableTheme.Rainbow)
					{
						for (var c = 0; c < 5; c++)
						{
							for (var r = 0; r < 5; r++)
							{
								using var brush = new SolidBrush(ColorTranslator.FromHtml(bow1[c % bow1.Length]));
								FillCell(c, r, brush);
							}
						}

						return;
					}
				}

				Color c0 = Color.Empty; // even
				Color c1 = Color.Empty; // odd
				bool rows = true;

				if (theme.FirstRowStripe != Color.Empty && theme.SecondRowStripe != Color.Empty)
				{
					c0 = theme.FirstRowStripe;
					c1 = theme.SecondRowStripe;
				}
				else if (theme.FirstColumnStripe != Color.Empty && theme.SecondColumnStripe != Color.Empty)
				{
					c0 = theme.FirstColumnStripe;
					c1 = theme.SecondColumnStripe;
					rows = false;
				}
				else if (theme.WholeTable != Color.Empty)
				{
					c0 = c1 = theme.WholeTable;
				}

				if (c0 != Color.Empty)
				{
					using var b0 = new SolidBrush(c0);
					using var b1 = new SolidBrush(c1);
					for (var c = 0; c < 5; c++)
					{
						for (var r = 0; r < 5; r++)
						{
							FillCell(c, r, (rows ? r : c) % 2 == 0 ? b0 : b1);
						}
					}
				}
			}

			void FillCell(int c, int r, Brush brush)
			{
				var x = bounds.Left + (c * columnWidth) + 1;
				var y = bounds.Top + (r * rowHeight) + 1;

				g.FillRectangle(brush, x, y, columnWidth - 1, rowHeight - 1);

				x += 2;
				y += (rowHeight - 1) / 2;
				g.DrawLine(Pens.Black, x, y, x + (columnWidth - 6), y);
			}
		}
	}
}
