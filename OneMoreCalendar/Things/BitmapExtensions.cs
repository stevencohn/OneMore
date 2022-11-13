//************************************************************************************************
// Copyright © 2022 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System.Drawing;
	using System.Drawing.Imaging;


	internal static class BitmapExtensions
	{

		public static Bitmap ConvertToGrayscale(this Bitmap original)
		{
			// blank bitmap the same size as original
			var grayscale = new Bitmap(original.Width, original.Height);

			using (var g = Graphics.FromImage(grayscale))
			{
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
			}

			return grayscale;
		}
	}
}
