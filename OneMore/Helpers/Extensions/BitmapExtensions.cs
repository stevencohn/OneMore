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

		/// <summary>
		/// Gets a readonly stream from the given bitmap.
		/// </summary>
		/// <param name="bitmap"></param>
		/// <returns></returns>
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


		/// <summary>
		/// Replaces fromColor with toColor in the given image. This can be used to "colorize"
		/// an image mask such as Notebooks and Section in the SearchCommand dialog
		/// </summary>
		/// <param name="image"></param>
		/// <param name="fromColor"></param>
		/// <param name="toColor"></param>
		/// <returns></returns>
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


		/// <summary>
		/// Serializes the given image as a base64 string.
		/// </summary>
		/// <param name="image"></param>
		/// <returns></returns>
		public static string ToBase64String(this Image image)
		{
			return Convert.ToBase64String(
				(byte[])new ImageConverter().ConvertTo(image, typeof(byte[])));
		}
	}
}
