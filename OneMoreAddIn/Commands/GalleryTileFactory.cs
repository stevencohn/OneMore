//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Runtime.InteropServices.ComTypes;


	internal class GalleryTileFactory : Command
	{
		private readonly StyleProvider provider;


		public GalleryTileFactory() : base()
		{
			provider = new StyleProvider();
		}


		public IStream MakeTile(string controlId, int itemIndex)
		{
			const int tileWidth = 70;
			const int tileHeight = 60;
			const int dpi = 96;

			logger.WriteLine($"MakeTile({controlId}, {itemIndex})");

			IStream stream = null;

			using (var image = new Bitmap(tileWidth, tileHeight))
			{
				using (var graphics = Graphics.FromImage(image))
				{
					graphics.Clear(Color.White);
					graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
					graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;

					using (var style = new GraphicStyle(provider.GetStyle(itemIndex)))
					{
						var fore = style.ApplyColors ? style.Foreground : Color.Black;
						using (var brush = new SolidBrush(fore))
						{
							var textsize = graphics.MeasureString("AaBbCc123", style.Font);
							var x = textsize.Width >= tileWidth ? 0 : (tileWidth - textsize.Width) / 2;

							if (style.ApplyColors &&
								!style.Background.IsEmpty &&
								!style.Background.Equals(Color.Transparent))
							{
								using (var bb = new SolidBrush(style.Background))
								{
									graphics.FillRectangle(bb, x, 5, textsize.Width, textsize.Height);
								}
							}

							graphics.DrawString("AaBbCc123", style.Font, brush, x, 5);
						}

						var scaledSize = 8f * (dpi / graphics.DpiY);

						using (var font = new Font("Tahoma", scaledSize, FontStyle.Regular))
						{
							var name = TrimText(graphics, style.Name, font, tileWidth, out var measuredWidth);
							graphics.DrawString(name, font, Brushes.Black, (tileWidth - measuredWidth) / 2, 40);
						}

						graphics.Save();
					}
				}

				stream = image.GetReadOnlyStream();
			}

			return stream;
		}


		private string TrimText(Graphics graphics, string text, Font font, int width, out float measuredWidth)
		{
			if ((measuredWidth = graphics.MeasureString(text, font).Width) > width)
			{
				text = text.Substring(0, text.Length - 1);
				while ((measuredWidth = graphics.MeasureString(text + "...", font).Width) > width)
				{
					text = text.Substring(0, text.Length - 1);
					if (text.Length <= 3) break;
				}
				text += "...";
			}

			return text;
		}
	}
}
