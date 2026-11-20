//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Images
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using System.Drawing;
	using System.Drawing.Imaging;

	/*
	 * Test Protocol - RemoveBorderCommand
	 * Add Border then Remove Border should restore the original image and its size.
	 *
	 * Test Steps:
	 *     1. Insert a picture. Select it. Invoke Images/Add Border with shadow on.
	 *     2. Select the image again and invoke Images/Remove Border.
	 *     3. The picture returns to its original pixels and size.
	 *     4. Repeat without shadow, with different widths, and after applying Add Border twice
	 *        (two Remove Border invocations peel the borders in reverse order).
	 *     5. Select an image without a border and invoke Remove Border; an info box says
	 *        no borders were found and the page is unchanged.
	 */

	[TestClass]
	public class RemoveBorderCommandTests
	{
		private const int BlurPx = 8;
		private const int DistancePx = 5;


		[TestMethod]
		public void RoundTripNoShadow()
		{
			using var source = MakeImage(40, 30);
			using var bordered = AddBorderCommand.Render(
				source, Color.Red, 3, false, BlurPx, DistancePx);

			using var restored = RemoveBorderCommand.Strip(bordered);

			AssertSame(source, restored);
		}


		[TestMethod]
		public void RoundTripShadow()
		{
			using var source = MakeImage(60, 40);
			using var bordered = AddBorderCommand.Render(
				source, Color.Blue, 2, true, BlurPx, DistancePx);

			Assert.IsTrue(bordered.Width > source.Width + 4);

			using var restored = RemoveBorderCommand.Strip(bordered);

			AssertSame(source, restored);
		}


		[TestMethod]
		public void RoundTripShadowWiderBorders()
		{
			foreach (var border in new[] { 1, 8 })
			{
				foreach (var (blur, distance) in new[] { (12, 12), (4, 10), (30, 3) })
				{
					using var source = MakeImage(50, 35);
					using var bordered = AddBorderCommand.Render(
						source, Color.DarkGreen, border, true, blur, distance);

					using var restored = RemoveBorderCommand.Strip(bordered);

					AssertSame(source, restored);
				}
			}
		}


		[TestMethod]
		public void SemiTransparentSourceIsRestoredToSize()
		{
			using var source = MakeImage(30, 30, 128);
			using var bordered = AddBorderCommand.Render(
				source, Color.Black, 2, true, BlurPx, DistancePx);

			using var restored = RemoveBorderCommand.Strip(bordered);

			// Add Border draws the image over the opaque band, so translucent source pixels
			// are blended with the border color; the geometry is still recovered exactly
			Assert.IsNotNull(restored);
			Assert.AreEqual(source.Width, restored.Width);
			Assert.AreEqual(source.Height, restored.Height);
		}


		[TestMethod]
		public void TwoBordersPeelInReverseOrder()
		{
			using var source = MakeImage(40, 30);
			using var first = AddBorderCommand.Render(
				source, Color.Red, 2, false, BlurPx, DistancePx);
			using var second = AddBorderCommand.Render(
				first, Color.Blue, 4, true, BlurPx, DistancePx);

			using var peeled = RemoveBorderCommand.Strip(second);

			// the first border's marker row is left behind when the second is added, so
			// the peeled image is the first border minus that row
			Assert.IsNotNull(peeled);
			Assert.AreEqual(first.Width, peeled.Width);
			Assert.AreEqual(first.Height - 1, peeled.Height);
			for (var y = 0; y < peeled.Height; y++)
			{
				for (var x = 0; x < peeled.Width; x++)
				{
					Assert.AreEqual(
						first.GetPixel(x, y).ToArgb(), peeled.GetPixel(x, y).ToArgb(),
						$"pixel mismatch at {x},{y}");
				}
			}

			// no marker remains, so this peel falls back to pixel analysis
			using var original = RemoveBorderCommand.Strip(peeled);
			AssertSame(source, original);
		}


		[TestMethod]
		public void ZeroBorderShadowOnlyRoundTrip()
		{
			using var source = MakeImage(50, 35);
			using var shadowed = AddBorderCommand.Render(
				source, Color.Black, 0, true, BlurPx, DistancePx);

			using var restored = RemoveBorderCommand.Strip(shadowed);

			AssertSame(source, restored);
		}


		[TestMethod]
		public void MarkerRecordsGeometry()
		{
			using var source = MakeImage(40, 30);
			using var bordered = AddBorderCommand.Render(
				source, Color.Red, 3, true, BlurPx, DistancePx);

			Assert.IsTrue(BorderMarker.TryRead(bordered, out var info));
			Assert.IsTrue(info.Shadow);
			Assert.AreEqual(3, info.Border);
			Assert.AreEqual(source.Width, info.Content.Width);
			Assert.AreEqual(source.Height, info.Content.Height);

			using var plain = AddBorderCommand.Render(
				source, Color.Red, 3, false, BlurPx, DistancePx);

			Assert.IsTrue(BorderMarker.TryRead(plain, out info));
			Assert.IsFalse(info.Shadow);
		}


		[TestMethod]
		public void PlainImageHasNoBorder()
		{
			using var source = MakeImage(40, 30);
			Assert.IsNull(RemoveBorderCommand.Strip(source));
		}


		[TestMethod]
		public void NonBlackMarginIsNotOurs()
		{
			using var source = MakeImage(40, 30);
			using var bordered = AddBorderCommand.Render(
				source, Color.Red, 3, true, BlurPx, DistancePx);

			// a colored translucent pixel outside the band cannot be an Add Border shadow
			bordered.SetPixel(bordered.Width - 1, bordered.Height - 1, Color.FromArgb(40, 200, 10, 10));

			Assert.IsNull(RemoveBorderCommand.Strip(bordered));
		}


		// a deterministic image whose edge pixels vary so no border ring can match by chance
		private static Bitmap MakeImage(int width, int height, int alpha = 255)
		{
			var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					bitmap.SetPixel(x, y, Color.FromArgb(
						alpha, (x * 7) % 256, (y * 11) % 256, ((x * 3) + (y * 5)) % 256));
				}
			}

			return bitmap;
		}


		private static void AssertSame(Bitmap expected, Bitmap actual)
		{
			Assert.IsNotNull(actual);
			Assert.AreEqual(expected.Width, actual.Width);
			Assert.AreEqual(expected.Height, actual.Height);

			for (var y = 0; y < expected.Height; y++)
			{
				for (var x = 0; x < expected.Width; x++)
				{
					Assert.AreEqual(
						expected.GetPixel(x, y).ToArgb(), actual.GetPixel(x, y).ToArgb(),
						$"pixel mismatch at {x},{y}");
				}
			}
		}
	}
}
