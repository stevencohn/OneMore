//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Runtime.InteropServices.ComTypes;


	internal static class BitmapExtensions
	{
		public static IStream GetReadOnlyStream(this Bitmap bitmap)
		{
			ReadOnlyStream stream = null;

			try
			{
				var memory = new MemoryStream();
				bitmap.Save(memory, ImageFormat.Png);
				stream = new ReadOnlyStream(memory);
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine(exc);
			}

			return stream;
		}


		public static Image MapColor(this Image image, Color fromColor, Color toColor)
		{
			var attributes = new ImageAttributes();

			attributes.SetRemapTable(new ColorMap[]
				{
					new ColorMap
					{
						OldColor = fromColor,
						NewColor = toColor,
					}
				},
				ColorAdjustType.Bitmap);

			using (var g = Graphics.FromImage(image))
			{
				g.DrawImage(
					image,
					new Rectangle(Point.Empty, image.Size),
					0, 0, image.Width, image.Height,
					GraphicsUnit.Pixel,
					attributes);
			}

			return image;
		}


		public static string ToBase64String(this Image image)
		{
			return Convert.ToBase64String(
				(byte[])new ImageConverter().ConvertTo(image, typeof(byte[])));
		}
	}
}
