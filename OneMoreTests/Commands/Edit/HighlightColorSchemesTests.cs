//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Edit
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.UI;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;


	[TestClass]
	public class HighlightColorSchemesTests
	{
		private static readonly string[] Names = { "Bright", "Faded", "Deep" };


		private static List<string> All(HighlightColorSet set)
		{
			return set.Colors.Concat(set.Extended).ToList();
		}


		[TestMethod]
		public void EverySetHasFiveBasicAndFiveExtendedValidColors()
		{
			var schemes = new HighlightColorSchemes();
			foreach (var name in Names)
			{
				var scheme = schemes.GetScheme(name);
				foreach (var set in new[] { scheme.GetSet(false), scheme.GetSet(true) })
				{
					Assert.AreEqual(5, set.Colors.Count, name);
					Assert.AreEqual(5, set.Extended.Count, name);
					foreach (var hex in All(set))
					{
						Assert.IsTrue(ColorMath.TryParseHex(hex, out _), $"{name} {hex}");
					}
				}
			}
		}


		[TestMethod]
		public void NoDuplicateColorsWithinASet()
		{
			var schemes = new HighlightColorSchemes();
			foreach (var name in Names)
			{
				var scheme = schemes.GetScheme(name);
				Assert.AreEqual(10, All(scheme.GetSet(false)).Distinct().Count(), name);
				Assert.AreEqual(10, All(scheme.GetSet(true)).Distinct().Count(), name);
			}
		}


		[TestMethod]
		public void BrightAndFadedKeepOriginalBasicColors()
		{
			var schemes = new HighlightColorSchemes();
			CollectionAssert.AreEqual(
				new[] { "#FFFF00", "#00FF00", "#00FFFF", "#FF00FF", "#0000FF" },
				schemes.GetScheme("Bright").Colors);

			CollectionAssert.AreEqual(
				new[] { "#FFFF99", "#CCFFCC", "#CCFFFF", "#FF99CC", "#99CCFF" },
				schemes.GetScheme("Faded").Colors);
		}


		[TestMethod]
		public void DarkSetsDifferFromLightSets()
		{
			var schemes = new HighlightColorSchemes();
			foreach (var name in Names)
			{
				var scheme = schemes.GetScheme(name);
				CollectionAssert.AreNotEqual(scheme.Colors, scheme.GetSet(true).Colors, name);
			}
		}


		[TestMethod]
		public void DarkColorsRoundTripThroughToLight()
		{
			var schemes = new HighlightColorSchemes();
			foreach (var name in Names)
			{
				foreach (var hex in All(schemes.GetScheme(name).GetSet(true)))
				{
					ColorMath.TryParseHex(hex, out var rendered);
					var back = ColorPalette.ToDark(ColorPalette.ToLight(rendered));
					Assert.IsTrue(Close(rendered, back, 3), $"{name} {hex}");
				}
			}
		}


		[TestMethod]
		public void GetColorHonorsDarkFlag()
		{
			var schemes = new HighlightColorSchemes();
			Assert.AreEqual("#FFFF00", schemes.GetColor("Bright", 0));
			Assert.AreEqual("#7B7B00", schemes.GetColor("Bright", 0, dark: true));
			Assert.AreEqual("#FF0000", schemes.GetColor("Bright", 5));
			Assert.IsNull(schemes.GetColor("Bright", 10));
		}


		private static bool Close(Color a, Color b, int tolerance)
		{
			return System.Math.Abs(a.R - b.R) <= tolerance
				&& System.Math.Abs(a.G - b.G) <= tolerance
				&& System.Math.Abs(a.B - b.B) <= tolerance;
		}
	}
}
