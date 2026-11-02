//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.UI
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.UI;
	using System.Drawing;


	[TestClass]
	public class ColorMathTests
	{
		[TestMethod]
		public void Hsv_RoundTripsPaletteColors()
		{
			foreach (var color in ColorPalette.Grid(false))
			{
				ColorMath.ToHsv(color, out var h, out var s, out var v);
				Assert.IsTrue(ColorMath.SameRgb(color, ColorMath.FromHsv(h, s, v)), ColorMath.ToHex(color));
			}
		}


		[TestMethod]
		public void Hsl_RoundTripsPaletteColors()
		{
			foreach (var color in ColorPalette.Standard(false))
			{
				ColorMath.ToHsl(color, out var h, out var s, out var l);
				Assert.IsTrue(ColorMath.SameRgb(color, ColorMath.FromHsl(h, s, l)), ColorMath.ToHex(color));
			}
		}


		[TestMethod]
		public void ToHsl_MatchesTechNoteValues()
		{
			// #B43512 is documented as 13,82,39
			ColorMath.ToHsl(Color.FromArgb(0xB4, 0x35, 0x12), out var h, out var s, out var l);
			Assert.AreEqual(13, (int)System.Math.Round(h));
			Assert.AreEqual(82, (int)System.Math.Round(s * 100));
			Assert.AreEqual(39, (int)System.Math.Round(l * 100));
		}


		[TestMethod]
		public void TryParseHex_AcceptsWithAndWithoutHash()
		{
			Assert.IsTrue(ColorMath.TryParseHex("#b43512", out var a));
			Assert.IsTrue(ColorMath.TryParseHex("B43512", out var b));
			Assert.AreEqual("#B43512", ColorMath.ToHex(a));
			Assert.IsTrue(ColorMath.SameRgb(a, b));
		}


		[TestMethod]
		public void TryParseHex_RejectsBadInput()
		{
			Assert.IsFalse(ColorMath.TryParseHex(null, out _));
			Assert.IsFalse(ColorMath.TryParseHex("#12345", out _));
			Assert.IsFalse(ColorMath.TryParseHex("#GGGGGG", out _));
		}


		[TestMethod]
		public void Palette_HasExpectedCounts()
		{
			Assert.AreEqual(10, ColorPalette.Theme(false).Length);
			Assert.AreEqual(10, ColorPalette.Theme(true).Length);
			Assert.AreEqual(50, ColorPalette.Grid(false).Length);
			Assert.AreEqual(50, ColorPalette.Grid(true).Length);
			Assert.AreEqual(10, ColorPalette.Standard(false).Length);
			Assert.AreEqual(10, ColorPalette.Standard(true).Length);
		}


		[TestMethod]
		public void ToDark_InvertsLightnessOutsidePalette()
		{
			// measured by Reference/Capture-DarkColors.ps1 on a dark page
			AssertNear("#9B4B4B", ColorPalette.ToDark(Color.FromArgb(0xB4, 0x64, 0x64)));
			AssertNear("#F7EEEE", ColorPalette.ToDark(Color.FromArgb(0x11, 0x08, 0x08)));
			AssertNear("#DD3C3C", ColorPalette.ToDark(Color.FromArgb(0xC3, 0x22, 0x22)));
		}


		[TestMethod]
		public void ToLight_IsInverseOfToDark()
		{
			var color = Color.FromArgb(0x33, 0x99, 0xCC);
			var back = ColorPalette.ToLight(ColorPalette.ToDark(color));
			AssertNear(ColorMath.ToHex(color), back);
		}


		private static void AssertNear(string expectedHex, Color actual)
		{
			ColorMath.TryParseHex(expectedHex, out var expected);
			Assert.IsTrue(
				System.Math.Abs(expected.R - actual.R) <= 2 &&
				System.Math.Abs(expected.G - actual.G) <= 2 &&
				System.Math.Abs(expected.B - actual.B) <= 2,
				$"expected {expectedHex} but was {ColorMath.ToHex(actual)}");
		}

		[TestMethod]
		public void ToDark_UsesPaletteTable()
		{
			// light grid row 4, col 7 is paired with dark grid row 4, col 7
			Assert.AreEqual("#F39A81",
				ColorMath.ToHex(ColorPalette.ToDark(Color.FromArgb(0xB4, 0x35, 0x12))));

			Assert.AreEqual("#201F1E", ColorMath.ToHex(ColorPalette.ToDark(Color.White)));
		}
	}
}
