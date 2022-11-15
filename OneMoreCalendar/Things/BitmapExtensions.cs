//************************************************************************************************
// Copyright © 2022 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System.Drawing;
	using System.Drawing.Imaging;


	internal static class BitmapExtensions
	{

		/// <summary>
		/// Converts all colored pixels to a grayscale based on the pixel's brightness
		/// and alpha value.
		/// </summary>
		/// <param name="original">The image to modify</param>
		/// <returns>The original image with modified pixels</returns>
		public static Bitmap ConvertToGrayscale(this Bitmap original)
		{
			// blank bitmap the same size as original
			var grayscale = new Bitmap(original.Width, original.Height);

			var g = Graphics.FromImage(grayscale);

			// grayscale matrix
			var colorMatrix = new ColorMatrix(new float[][]
			{
					new float[] { 0.3f,  0.3f,  0.3f,  0, 0},
					new float[] { 0.59f, 0.59f, 0.59f, 0, 0},
					new float[] { 0.11f, 0.11f, 0.11f, 0, 0},
					new float[] { 0,     0,     0,     1, 0},
					new float[] { 0,     0,     0,     0, 1}
			});

			using var attributes = new ImageAttributes();
			attributes.SetColorMatrix(colorMatrix);

			// redraw original image onto new image using grayscale color matrix
			g.DrawImage(original,
				new Rectangle(0, 0, original.Width, original.Height),
				0, 0, original.Width, original.Height,
				GraphicsUnit.Pixel,
				attributes);

			return grayscale;
		}


		/// <summary>
		/// Converts all colored pixels in the given image to the specified color, preserving the
		/// original alpha value of each pixel. This indiscriminately replaces all colors in the
		/// image with the new color, typically used to colorize template shapes.
		/// </summary>
		/// <param name="original">The image to modify</param>
		/// <param name="newColor">The new desired color</param>
		/// <returns>The original image with modified pixels</returns>
		public static Bitmap MapColor(this Bitmap original, Color newColor)
		{
			for (int x = 0; x < original.Width; x++)
			{
				for (int y = 0; y < original.Height; y++)
				{
					original.SetPixel(x, y,
						Color.FromArgb(
							original.GetPixel(x, y).A,
							newColor.R, newColor.G, newColor.B));
				}
			}

			return original;
		}
	}
}
