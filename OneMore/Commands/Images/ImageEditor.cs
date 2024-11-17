//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Linq;


	internal class ImageEditor
	{

		public enum Stylization
		{
			GrayScale,
			Sepia,
			Polaroid,
			Invert,
			None
		}


		public enum SizeConstraint
		{
			// image may grow or shink
			All,
			// only grow image if smaller than target; do not shrink images larger than targer
			OnlyEnlarge,
			// only shrink image if larger than target; do not grow images smaller than targer
			OnlyShrink
		}


		private bool dirty;


		public ImageEditor()
		{
			Brightness = float.MinValue;
			Contrast = float.MinValue;
			Opacity = float.MinValue;
			Quality = long.MinValue;
			Saturation = float.MinValue;
			Style = Stylization.None;
			Size = Size.Empty;
			SizePercent = float.MinValue;
			Constraint = SizeConstraint.All;
			PreserveQualityOnResize = true;

			dirty = false;
		}


		public bool AutoSize { get; set; }

		public float Brightness { private get; set; }

		public float Contrast { private get; set; }

		public float Opacity { private get; set; }

		public bool PreserveQualityOnResize { private get; set; }

		public bool PreserveStorageSize { private get; set; }

		public long Quality { private get; set; }

		public float Saturation { private get; set; }

		public Size Size { get; set; }

		public float SizePercent { get; set; }

		public SizeConstraint Constraint { get; set; }

		public Stylization Style { private get; set; }


		public bool IsReady =>
			!Brightness.EstEquals(float.MinValue) ||
			!Contrast.EstEquals(float.MinValue) ||
			!Opacity.EstEquals(float.MinValue) ||
			Quality != long.MinValue ||
			!Saturation.EstEquals(float.MinValue) ||
			!SizePercent.EstEquals(float.MinValue) ||
			Size != Size.Empty ||
			Style != Stylization.None;


		public Image Apply(OneImage wrapper)
		{
			using var image = wrapper.ReadImage();
			var edit = Apply(image);
			if (dirty)
			{
				wrapper.WriteImage(edit);
			}

			if (AutoSize)
			{
				wrapper.SetAutoSize();
			}
			else if (SizePercent > float.MinValue)
			{
				wrapper.SetSize(
					(int)(SizePercent * wrapper.Width),
					(int)(SizePercent * wrapper.Height),
					true);
			}
			else if (Size != Size.Empty)
			{
				wrapper.SetSize(Size.Width, Size.Height, true);
			}

			return edit;
		}


		/// <summary>
		/// Generate a new image, applying all properties that were explicitly set
		/// </summary>
		/// <param name="edit"></param>
		/// <returns></returns>
		public Image Apply(Image edit)
		{
			var trash = new Stack<Image>();
			dirty = false;

			// quality first to potentially minimize size of image
			if (Quality > long.MinValue)
			{
				trash.Push(edit);
				edit = ChangeQuality(edit, Quality);
			}

			// TODO: is there an optimal way to combine transformations, in other words.
			// can we use matrix.Multiple(other) so avoid creating interim images

			if (Brightness > float.MinValue)
			{
				trash.Push(edit);
				edit = Transform(edit, MakeBrightnessMatrix(Brightness));
			}

			if (Contrast > float.MinValue)
			{
				trash.Push(edit);
				edit = Transform(edit, MakeContrastMatrix(Contrast));
			}

			if (Saturation > float.MinValue)
			{
				trash.Push(edit);
				edit = Transform(edit, MakeSaturationMatrix(Saturation));
			}

			if (Style != Stylization.None)
			{
				var m = Style switch
				{
					Stylization.GrayScale => MakeGrayscaleMatrix(),
					Stylization.Sepia => MakeSepiaMatrix(),
					Stylization.Polaroid => MakePolaroidMatrix(),
					_ => MakeInvertedMatrix(),
				};

				trash.Push(edit);
				edit = Transform(edit, m);
			}

			// opacity must be set last, do not combined matrix
			if (Opacity > float.MinValue)
			{
				trash.Push(edit);
				edit = Transform(edit, MakeOpacityMatrix(Opacity));
			}

			// resize last, to attempt to preserve quality
			if (!PreserveStorageSize &&
				(SizePercent > float.MinValue || Size != Size.Empty))
			{
				trash.Push(edit);
				edit = Resize(edit);
			}

			dirty = trash.Count > 0;

			while (trash.Count > 0)
			{
				var popped = trash.Pop();
				// keep original; let consumer dispose
				if (trash.Count > 0)
				{
					popped.Dispose();
				}
			}

			return edit;
		}


		private Image ChangeQuality(Image original, long quality)
		{
			// quality: the quality level, 1..100
			// it is possible that this will result in a larger storage model than the original

			var parameters = new EncoderParameters(1);
			parameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);

			var codec = ImageCodecInfo.GetImageEncoders().First(e => e.MimeType == "image/jpeg");

			using var stream = new MemoryStream();
			original.Save(stream, codec, parameters);
			var edit = new Bitmap(Image.FromStream(stream));
			return edit;
		}


		#region Matrices
		private ColorMatrix MakeBrightnessMatrix(float brightness)
		{
			// brightness: change in brightness where 0.0 is no change

			return new ColorMatrix
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


		private ColorMatrix MakeContrastMatrix(float contrast)
		{
			// contrast: change in contrast 0.0 to 1.0

			contrast += 1f;
			var t = (1.0f - contrast) / 2.0f;
			return new ColorMatrix
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


		private ColorMatrix MakeGrayscaleMatrix()
		{
			return new ColorMatrix
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
			};
		}


		private ColorMatrix MakeInvertedMatrix()
		{
			return new ColorMatrix
			{
				Matrix00 = -1,
				Matrix11 = -1,
				Matrix22 = -1,
				Matrix33 = 1,
				Matrix40 = 1,
				Matrix41 = 1,
				Matrix42 = 1,
				Matrix44 = 1
			};
		}


		private ColorMatrix MakeOpacityMatrix(float opacity)
		{
			// opacity: the desired opacity value as a percentage (0.0 .. 1.0)

			return new ColorMatrix
			{
				// row 3, col 3 (alpha,alpha) represents alpha component
				Matrix33 = opacity
			};
		}


		private ColorMatrix MakePolaroidMatrix()
		{
			return new ColorMatrix
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
			};
		}


		private ColorMatrix MakeSaturationMatrix(float saturation)
		{
			// staturation: the desired saturation value(-1.0..1.0)

			var s = 1f - (saturation * -1f); // shift -(flip -1..1 to 1..-1)

			var sr = (1.0f - s) * 0.3086f;
			var sg = (1.0f - s) * 0.6094f;
			var sb = (1.0f - s) * 0.0820f;

			return new ColorMatrix
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
			};
		}


		private ColorMatrix MakeSepiaMatrix()
		{
			return new ColorMatrix
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
			};
		}
		#endregion Matrices


		private Image Transform(Image original, ColorMatrix matrix)
		{
			var edit = new Bitmap(original.Width, original.Height);

			using var g = Graphics.FromImage(edit);
			using var attributes = new ImageAttributes();
			attributes.ClearColorMatrix();
			attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
			//attributes.SetGamma(1.0f, ColorAdjustType.Bitmap); // 1.0 = no change

			g.DrawImage(original,
				new Rectangle(0, 0, original.Width, original.Height),
				0, 0, original.Width, original.Height,
				GraphicsUnit.Pixel, attributes);

			return edit;
		}


		private Image Resize(Image original)
		{
			// Resize the physical storage model of the given image, creating a new Image instance
			// with the specified width and height, optionally decreasing the quality level

			int width;
			int height;

			// when resizing many, height could be zero so much adjust per image
			if (SizePercent > float.MinValue)
			{
				width = (int)(original.Width * SizePercent);
				height = (int)(original.Height * SizePercent);
			}
			else
			{
				width = Size.Width;
				height = Size.Height == 0
					? (int)(original.Height * (1.0 * Size.Width / original.Width))
					: Size.Height;
			}

			var edit = new Bitmap(width, height);
			edit.SetResolution(original.HorizontalResolution, original.VerticalResolution);

			using var g = Graphics.FromImage(edit);
			g.CompositingMode = CompositingMode.SourceCopy;

			if (PreserveQualityOnResize)
			{
				g.CompositingQuality = CompositingQuality.HighQuality;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.SmoothingMode = SmoothingMode.HighQuality;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			}

			using var attributes = new ImageAttributes();
			attributes.SetWrapMode(WrapMode.TileFlipXY);

			g.DrawImage(original,
				new Rectangle(0, 0, width, height),
				0, 0, original.Width, original.Height,
				GraphicsUnit.Pixel,
				attributes);

			return edit;
		}
	}
}
