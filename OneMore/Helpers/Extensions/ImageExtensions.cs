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
		/// Gets the average brightness of the entire image where 0 is black and
		/// 100 is white.
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
		/// Render a new image from the given image with the specified brightness and
		/// contrast adjustments.
		/// </summary>
		/// <param name="image">The image to edit</param>
		/// <param name="brightness">Change in brightness where 0.0 is no change</param>
		/// <param name="contrast">Change in contrast 0.0 to 1.0</param>
		/// <returns></returns>
		/// <seealso cref="https://docs.rainmeter.net/tips/colormatrix-guide/"/>
		public static Image SetBrightnessContrast(this Image image, float brightness, float contrast)
		{
			ColorMatrix bmat = null;
			if (brightness != 0)
			{
				bmat = new ColorMatrix
				{
					Matrix00 = 1f,
					Matrix11 = 1f,
					Matrix22 = 1f,
					Matrix33 = 1f,
					Matrix40 = brightness,
					Matrix41 = brightness,
					Matrix42 = brightness,
					Matrix44 = 1f
				};
			}

			ColorMatrix cmat = null;
			if (contrast != 0)
			{
				contrast += 1f;
				var t = (1.0f - contrast) / 2.0f;
				cmat = new ColorMatrix
				{
					Matrix00 = contrast,    // scale red
					Matrix11 = contrast,    // scale green
					Matrix22 = contrast,    // scale blue
					Matrix33 = 1f,          // no change in alpha
					Matrix40 = t,           // transformation
					Matrix41 = t,           // transformation
					Matrix42 = t,           // transformation
					Matrix44 = 1f
				};
			}

			var matrix = bmat != null && cmat != null
				? bmat.Multiply(cmat)
				: bmat ?? cmat;

			if (matrix == null)
			{
				return image;
			}

			return Apply(image, matrix);
		}


		/// <summary>
		/// Renders a new image as a copy of the given image with a desired opacity
		/// </summary>
		/// <param name="image">The original image to copy</param>
		/// <param name="opacity">The desired opacity value as a percentage (0.0 .. 1.0)</param>
		/// <returns></returns>
		public static Image SetOpacity(this Image image, float opacity)
		{
			var canvas = new Bitmap(image.Width, image.Height);
			using (var graphics = Graphics.FromImage(canvas))
			{
				var matrix = new ColorMatrix
				{
					// row 3, col 3 (alpha,alpha) represents alpha component
					Matrix33 = opacity
				};

				using (var atts = new ImageAttributes())
				{
					atts.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

					graphics.DrawImage(image,
						new Rectangle(0, 0, canvas.Width, canvas.Height),
						0, 0, image.Width, image.Height,
						GraphicsUnit.Pixel, atts);
				}
			}

			return canvas;
		}


		/// <summary>
		/// Renders a new image by adjusting the quality level of the given image.
		/// </summary>
		/// <param name="image"></param>
		/// <param name="quality">The quality level, 1..100</param>
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
		/// Renders a new image as a copy of the given image with a desired saturation
		/// </summary>
		/// <param name="image">The original image to copy</param>
		/// <param name="saturation">The desired saturation value (-1.0 .. 1.0)</param>
		/// <returns></returns>
		public static Image SetSaturation(this Image image, float saturation)
		{
			var s = 1f - (saturation * -1f); // shift -(flip -1..1 to 1..-1)

			var sr = (1.0f - s) * 0.3086f;
			var sg = (1.0f - s) * 0.6094f;
			var sb = (1.0f - s) * 0.0820f;

			return Apply(image, new ColorMatrix
			{
				Matrix00 = sr + s,
				Matrix01 = sr,
				Matrix02 = sr,
				Matrix10 = sg,
				Matrix11 = sg + s,
				Matrix12 = sg,
				Matrix20 = sb,
				Matrix21 = sb,
				Matrix22 = sb + s,
				Matrix33 = 1.0f,
				Matrix44 = 1.0f
			});
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


		/// <summary>
		/// Render a new image by converting the given image to gray scale.
		/// </summary>
		/// <param name="image">Colorful image</param>
		/// <returns></returns>
		public static Image ToGrayscale(this Image image)
		{
			return Apply(image, new ColorMatrix
			{
				Matrix00 = 0.30f,
				Matrix01 = 0.30f,
				Matrix02 = 0.30f,
				Matrix10 = 0.59f,
				Matrix11 = 0.59f,
				Matrix12 = 0.59f,
				Matrix20 = 0.11f,
				Matrix21 = 0.11f,
				Matrix22 = 0.11f,
				Matrix33 = 1.00f,
				Matrix44 = 1.00f
			});
		}


		/// <summary>
		/// Render a new image by converting the given image to Polaroid style.
		/// </summary>
		/// <param name="image">Colorful image</param>
		/// <returns></returns>
		public static Image ToPolaroid(this Image image)
		{
			return Apply(image, new ColorMatrix
			{
				Matrix00 = 1.438f,
				Matrix01 = -0.062f,
				Matrix02 = -0.062f,
				Matrix10 = -0.122f,
				Matrix11 = 1.378f,
				Matrix12 = -0.122f,
				Matrix20 = -0.016f,
				Matrix21 = -0.016f,
				Matrix22 = 1.483f,
				Matrix40 = -0.03f,
				Matrix41 = 0.05f,
				Matrix42 = -0.02f,
				Matrix44 = 1.0f
			});
		}


		/// <summary>
		/// Render a new image by converting the given image to sepia.
		/// </summary>
		/// <param name="image">Colorful image</param>
		/// <returns></returns>
		public static Image ToSepia(this Image image)
		{
			return Apply(image, new ColorMatrix
			{
				Matrix00 = 0.393f,
				Matrix01 = 0.394f,
				Matrix02 = 0.272f,
				Matrix10 = 0.769f,
				Matrix11 = 0.686f,
				Matrix12 = 0.534f,
				Matrix20 = 0.189f,
				Matrix21 = 0.168f,
				Matrix22 = 0.131f
			});
		}


		private static Image Apply(Image image, ColorMatrix matrix)
		{
			var canvas = new Bitmap(image.Width, image.Height);

			using (var g = Graphics.FromImage(canvas))
			{
				using (var attributes = new ImageAttributes())
				{
					attributes.ClearColorMatrix();
					attributes.SetColorMatrix(matrix);
					//attributes.SetGamma(1.0f, ColorAdjustType.Bitmap); // 1.0 = no change

					g.DrawImage(image,
						new Rectangle(0, 0, image.Width, image.Height),
						0, 0, image.Width, image.Height,
						GraphicsUnit.Pixel, attributes);
				}
			}

			return canvas;
		}
	}
}
