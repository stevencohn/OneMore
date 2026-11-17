//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Reverses AddBorderCommand on each selected image by recognizing the border band and
	/// shadow from the pixels alone, then cropping back to the original content.
	/// </summary>
	internal class RemoveBorderCommand : Command
	{
		// alpha at or above this can only be the border band or the image; the add-border
		// shadow never exceeds 30% opacity (76/255)
		private const int OpaqueAlpha = 250;


		public RemoveBorderCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var guard = EnterOnce();
			if (guard is null) { return; }

			await using var one = new OneNote(out var page, out var ns, OneNote.PageDetail.All);

			var elements = page.Root.Descendants(ns + "Image")
				.Where(e => e.Attribute("selected")?.Value == "all")
				.ToList();

			if (!elements.Any())
			{
				ShowInfo(Resx.RemoveBorderCommand_noImages);
				return;
			}

			var updated = false;
			foreach (var element in elements)
			{
				if (RemoveBorder(element, ns))
				{
					updated = true;
				}
			}

			if (!updated)
			{
				ShowInfo(Resx.RemoveBorderCommand_noBorders);
				return;
			}

			await one.Update(page);
		}


		private static bool RemoveBorder(XElement element, XNamespace ns)
		{
			var data = element.Element(ns + "Data");
			if (data is null || string.IsNullOrEmpty(data.Value))
			{
				return false;
			}

			var oneImage = new OneImage(element);
			using var source = oneImage.ReadImage();

			using var result = Strip(source);
			if (result is null)
			{
				return false;
			}

			oneImage.WriteImage(result);

			// the border was added without changing the display scale, so keep it
			var size = element.Element(ns + "Size");
			var (scaleX, scaleY) = AddBorderCommand.GetScales(size, source.Width, source.Height);

			size.SetAttributeValue("width", FormattableString.Invariant(
				$"{Math.Round(result.Width * scaleX):0.0}"));

			size.SetAttributeValue("height", FormattableString.Invariant(
				$"{Math.Round(result.Height * scaleY):0.0}"));

			size.SetAttributeValue("isSetByUser", "true");

			// printouts carry XPS attributes that OneNote uses to block edits
			element.Attributes("xpsFileIndex").Remove();
			element.Attributes("originalPageNumber").Remove();
			element.Attributes("isPrintOut").Remove();

			return true;
		}


		/// <summary>
		/// Detects a border added by AddBorderCommand and returns the image with the border
		/// and shadow cropped away, or null if no such border is found.
		/// </summary>
		/// <remarks>
		/// The add step leaves an opaque single-color band on all four sides, the original
		/// pixels copied 1:1 inside it, and outside it only fully transparent pixels or
		/// black pixels of at most 30% alpha (the shadow, right and down).
		/// </remarks>
		internal static Bitmap Strip(Image image)
		{
			var bitmap = image as Bitmap ?? new Bitmap(image);

			var width = bitmap.Width;
			var height = bitmap.Height;

			var pixels = ReadPixels(bitmap, out var stride);

			// 1. outer rectangle = bounding box of opaque pixels; shadow can't reach it
			int minX = width, minY = height, maxX = -1, maxY = -1;
			for (var y = 0; y < height; y++)
			{
				var row = y * stride;
				for (var x = 0; x < width; x++)
				{
					if (pixels[row + (x * 4) + 3] >= OpaqueAlpha)
					{
						if (x < minX) { minX = x; }
						if (x > maxX) { maxX = x; }
						if (y < minY) { minY = y; }
						if (y > maxY) { maxY = y; }
					}
				}
			}

			if (maxX < 0)
			{
				return null;
			}

			var rectWidth = maxX - minX + 1;
			var rectHeight = maxY - minY + 1;

			// 2. border color is the band's top-left corner, which must be fully opaque
			var color = Pixel(pixels, stride, minX, minY);
			if (((color >> 24) & 0xFF) != 0xFF)
			{
				return null;
			}

			// 3. border width = thickest ring, inside the outer rect, that is entirely the
			// border color; always leave at least one pixel of content
			var maxRings = (Math.Min(rectWidth, rectHeight) - 1) / 2;
			var border = 0;
			while (border < maxRings
				&& IsRing(pixels, stride, color, minX, minY, maxX, maxY, border))
			{
				border++;
			}

			if (border == 0)
			{
				return null;
			}

			// 4. outside the rect there must be nothing but our own shadow
			if (!IsShadowOnly(pixels, stride, width, height, minX, minY, maxX, maxY))
			{
				return null;
			}

			// 5. crop to the original content, exact copy including alpha
			var content = new Rectangle(
				minX + border, minY + border,
				rectWidth - (2 * border), rectHeight - (2 * border));

			return bitmap.Clone(content, PixelFormat.Format32bppArgb);
		}


		private static byte[] ReadPixels(Bitmap bitmap, out int stride)
		{
			var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
			var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			try
			{
				stride = data.Stride;
				var pixels = new byte[Math.Abs(data.Stride) * bitmap.Height];
				Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
				return pixels;
			}
			finally
			{
				bitmap.UnlockBits(data);
			}
		}


		// packed ARGB of the pixel at (x,y)
		private static int Pixel(byte[] pixels, int stride, int x, int y)
		{
			return BitConverter.ToInt32(pixels, (y * stride) + (x * 4));
		}


		// true if every pixel of the ring r pixels inside the rect's edge is the color
		private static bool IsRing(
			byte[] pixels, int stride, int color, int minX, int minY, int maxX, int maxY, int r)
		{
			var x0 = minX + r;
			var x1 = maxX - r;
			var y0 = minY + r;
			var y1 = maxY - r;

			for (var x = x0; x <= x1; x++)
			{
				if (Pixel(pixels, stride, x, y0) != color
					|| Pixel(pixels, stride, x, y1) != color)
				{
					return false;
				}
			}

			for (var y = y0; y <= y1; y++)
			{
				if (Pixel(pixels, stride, x0, y) != color
					|| Pixel(pixels, stride, x1, y) != color)
				{
					return false;
				}
			}

			return true;
		}


		// true if all pixels outside the rect are transparent or black, and the shadow fades
		// away from the rect to the right and down along the middle of the image
		private static bool IsShadowOnly(
			byte[] pixels, int stride, int width, int height,
			int minX, int minY, int maxX, int maxY)
		{
			for (var y = 0; y < height; y++)
			{
				var inside = y >= minY && y <= maxY;
				var row = y * stride;

				for (var x = 0; x < width; x++)
				{
					if (inside && x >= minX && x <= maxX)
					{
						// skip over the rect
						x = maxX;
						continue;
					}

					var offset = row + (x * 4);
					if (pixels[offset + 3] != 0
						&& (pixels[offset] != 0 || pixels[offset + 1] != 0 || pixels[offset + 2] != 0))
					{
						return false;
					}
				}
			}

			var midY = (minY + maxY) / 2;
			var last = int.MaxValue;
			for (var x = maxX + 1; x < width; x++)
			{
				var alpha = pixels[(midY * stride) + (x * 4) + 3];
				if (alpha > last)
				{
					return false;
				}

				last = alpha;
			}

			var midX = (minX + maxX) / 2;
			last = int.MaxValue;
			for (var y = maxY + 1; y < height; y++)
			{
				var alpha = pixels[(y * stride) + (midX * 4) + 3];
				if (alpha > last)
				{
					return false;
				}

				last = alpha;
			}

			return true;
		}
	}
}
