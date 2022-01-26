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
	using System.Linq;


	internal static class ImageExtensions
	{

		/// <summary>
		/// Replaces fromColor with toColor in the given image. This can be used to "colorize"
		/// an image mask such as Notebooks and Sections in the SearchCommand dialog
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
		/// Resize the physical storage model of the given image, creating a new Image
		/// instance with the specified width and height, optionally decreasing the quality level
		/// </summary>
		/// <param name="image">The image to resize</param>
		/// <param name="width">The new width in pixels</param>
		/// <param name="height">The new height in pixels</param>
		/// <param name="quality">The quality level; only applies if it is less than 100</param>
		/// <returns></returns>
		public static Image Resize(this Image image, int width, int height)
		{
			var result = new Bitmap(width, height);
			result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using (var g = Graphics.FromImage(result))
			{
				g.CompositingMode = CompositingMode.SourceCopy;
				g.CompositingQuality = CompositingQuality.HighQuality;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.SmoothingMode = SmoothingMode.HighQuality;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;

				using (var attributes = new ImageAttributes())
				{
					attributes.SetWrapMode(WrapMode.TileFlipXY);

					g.DrawImage(image,
						new Rectangle(0, 0, width, height),
						0, 0, image.Width, image.Height,
						GraphicsUnit.Pixel,
						attributes);
				}
			}

			return result;
		}


		/// <summary>
		/// Renders a new image as a copy of the given image with a desired opacity
		/// </summary>
		/// <param name="image">The original image to copy</param>
		/// <param name="opacity">The desired opacity value as a percentage (0.0 .. 1.0)</param>
		/// <returns></returns>
		public static Image SetOpacity(this Image image, float opacity)
		{
			var copy = new Bitmap(image.Width, image.Height);
			using (var graphics = Graphics.FromImage(copy))
			{
				var matrix = new ColorMatrix
				{
					// row 3, col 3 represents alpha component
					Matrix33 = opacity
				};

				using (var atts = new ImageAttributes())
				{
					atts.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

					graphics.DrawImage(image,
						new Rectangle(0, 0, copy.Width, copy.Height),
						0, 0, image.Width, image.Height,
						GraphicsUnit.Pixel, atts);
				}
			}

			return copy;
		}


		/// <summary>
		/// Sets the quality level of the given image.
		/// </summary>
		/// <param name="image"></param>
		/// <param name="quality"></param>
		/// <returns></returns>
		/// <remarks>
		/// It is possible that this will result in a larger storage model than the original
		/// </remarks>
		public static Image SetQuality(this Image image, long quality)
		{
			try
			{
				var parameters = new EncoderParameters(1);
				parameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);

				var codec = ImageCodecInfo.GetImageEncoders().First(e => e.MimeType == "image/jpeg");

				using (var stream = new MemoryStream())
				{
					image.Save(stream, codec, parameters);
					image = new Bitmap(Image.FromStream(stream));
					return image;
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine(exc);
			}

			return null;
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
