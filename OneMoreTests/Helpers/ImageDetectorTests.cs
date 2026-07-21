//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Helpers
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Svg;
	using System.IO;
	using System.Text;


	[TestClass]
	public class ImageDetectorTests
	{
		private static MemoryStream StreamOf(byte[] bytes)
		{
			var stream = new MemoryStream();
			stream.Write(bytes, 0, bytes.Length);
			return stream;
		}


		[TestMethod]
		public void GetSignature_PngHeader_ReturnsPNG()
		{
			var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0 };
			var signature = new ImageDetector().GetSignature(StreamOf(bytes));

			Assert.AreEqual(ImageSignature.PNG, signature);
		}


		[TestMethod]
		public void GetSignature_SvgWithXmlDeclaration_ReturnsSVG()
		{
			var svg =
				"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
				"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"10\" height=\"10\">" +
				"<rect width=\"10\" height=\"10\" fill=\"red\"/></svg>";

			var signature = new ImageDetector().GetSignature(
				StreamOf(Encoding.UTF8.GetBytes(svg)));

			Assert.AreEqual(ImageSignature.SVG, signature);
		}


		[TestMethod]
		public void GetSignature_SvgWithoutXmlDeclaration_ReturnsSVG()
		{
			var svg =
				"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"10\" height=\"10\">" +
				"<rect width=\"10\" height=\"10\" fill=\"blue\"/></svg>";

			var signature = new ImageDetector().GetSignature(
				StreamOf(Encoding.UTF8.GetBytes(svg)));

			Assert.AreEqual(ImageSignature.SVG, signature);
		}


		[TestMethod]
		public void GetSignature_PlainText_ReturnsUnknown()
		{
			var signature = new ImageDetector().GetSignature(
				StreamOf(Encoding.UTF8.GetBytes("just some plain text, not an image")));

			Assert.AreEqual(ImageSignature.Unknown, signature);
		}


		[TestMethod]
		public void Rasterize_SimpleSvg_ProducesNonEmptyBitmap()
		{
			var svg =
				"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"12\" height=\"8\">" +
				"<rect width=\"12\" height=\"8\" fill=\"green\"/></svg>";

			using var stream = StreamOf(Encoding.UTF8.GetBytes(svg));
			stream.Position = 0;

			using var bitmap = SvgDocument.Open<SvgDocument>(stream).Draw();

			Assert.IsTrue(bitmap.Width > 0);
			Assert.IsTrue(bitmap.Height > 0);
		}
	}
}
