//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Drawing.Imaging;
	using System.IO;


	internal enum ImageSignature
	{
		Unknown,
		BMP,
		GIF,
		JPG,
		PNG,
		TIFF
	}


	internal static class ImageExtensions
	{

		/// <summary>
		/// OneMore Extension >> Gets the average brightness of the entire image where 0 is
		/// black and 100 is white.
		/// </summary>
		/// <param name="image">The image to scan</param>
		/// <returns>A number between 0 and 100.</returns>
		public static int GetBrightness(this Image image)
		{
			if (image is Bitmap bitmap)
			{
				try
				{
					// the average brightness of the entire image (0=black, 100=white)
					float brightnessValue = 0;

					for (int i = 0; i < bitmap.Size.Width; i++)
					{
						for (int j = 0; j < bitmap.Size.Height; j++)
						{
							var color = bitmap.GetPixel(i, j);
							brightnessValue += color.GetBrightness();
						}
					}

					return (int)(brightnessValue / (bitmap.Size.Width * bitmap.Size.Height) * 100);
				}
				catch
				{
					return 100;
				}
			}

			return 100;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="img"></param>
		/// <returns></returns>
		public static ImageSignature GetSignature(this Image img)
		{
			if (img.RawFormat.Equals(ImageFormat.Jpeg))
				return ImageSignature.JPG;
			if (img.RawFormat.Equals(ImageFormat.Bmp))
				return ImageSignature.BMP;
			if (img.RawFormat.Equals(ImageFormat.Png))
				return ImageSignature.PNG;
			if (img.RawFormat.Equals(ImageFormat.Gif))
				return ImageSignature.GIF;
			if (img.RawFormat.Equals(ImageFormat.Tiff))
				return ImageSignature.TIFF;

			return ImageSignature.Unknown;
		}


		/// <summary>
		/// OneMore Extension >> Replaces fromColor with toColor in the given image. This can be
		/// used to "colorize" an image mask such as Notebooks and Sections in the SearchCommand
		/// dialog
		/// </summary>
		/// <param name="image">The image to use as a mask</param>
		/// <param name="fromColor">The color to replace</param>
		/// <param name="toColor">The color to apply</param>
		/// <returns></returns>
		public static Image MapColor(this Image image, Color fromColor, Color toColor)
		{
			var attributes = new ImageAttributes();

			attributes.SetRemapTable(new ColorMap[]
				{
					new() {
						OldColor = fromColor,
						NewColor = toColor,
					}
				},
				ColorAdjustType.Bitmap);

			using var g = Graphics.FromImage(image);
			g.DrawImage(
				image,
				new Rectangle(Point.Empty, image.Size),
				0, 0, image.Width, image.Height,
				GraphicsUnit.Pixel,
				attributes);

			return image;
		}


		/// <summary>
		/// OneMore Extension >> Resize the physical storage model of the given image, creating
		/// a new Image instance with the specified width and height, optionally decreasing the
		/// quality level
		/// </summary>
		/// <param name="image">The image to resize</param>
		/// <param name="width">The new width in pixels</param>
		/// <param name="height">The new height in pixels</param>
		/// <param name="quality">The quality level; only applies if it is less than 100</param>
		/// <returns></returns>
		public static Image Resize(this Image image, int width, int height, bool highQuality = true)
		{
			var result = new Bitmap(width, height);
			result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using var g = Graphics.FromImage(result);
			g.CompositingMode = CompositingMode.SourceCopy;

			if (highQuality)
			{
				g.CompositingQuality = CompositingQuality.HighQuality;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.SmoothingMode = SmoothingMode.HighQuality;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			}

			using var attributes = new ImageAttributes();
			attributes.SetWrapMode(WrapMode.TileFlipXY);

			g.DrawImage(image,
				new Rectangle(0, 0, width, height),
				0, 0, image.Width, image.Height,
				GraphicsUnit.Pixel,
				attributes);

			return result;
		}


		/// <summary>
		/// OneMore Extension >> Convert the given (png) image to jpeg format and serialize
		/// as a base64 string.
		/// </summary>
		/// <param name="image"></param>
		/// <returns></returns>
		public static string ToBase64Jpeg(this Image image)
		{
			using var jstream = new MemoryStream();
			image.Save(jstream, ImageFormat.Jpeg);
			return Image.FromStream(jstream).ToBase64String();
		}


		/// <summary>
		/// OneMore Extension >> Serializes the given image as a base64 string.
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
