//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Styles
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Styles;
	using System.Xml.Linq;

	[TestClass]
	public class ThemeTests
	{
		// issue 2755: GetStyle(int) threw ArgumentOutOfRangeException instead of returning
		// null, so ApplyStyleCommand never reached its "style doesn't exist" check

		private static Theme MakeTheme(int count)
		{
			var root = new XElement("Theme", new XAttribute("name", "Test"));
			for (var i = 0; i < count; i++)
			{
				root.Add(new XElement("Style",
					new XAttribute("name", $"Style{i}"),
					new XAttribute("index", i),
					new XAttribute("font", "Calibri"),
					new XAttribute("fontSize", "11")));
			}

			return new Theme(root, "test");
		}


		[TestMethod]
		public void GetStyle_InRange_ReturnsStyle()
		{
			var theme = MakeTheme(3);

			Assert.AreEqual("Style0", theme.GetStyle(0).Name);
			Assert.AreEqual("Style2", theme.GetStyle(2).Name);
		}


		[TestMethod]
		public void GetStyle_PastEnd_ReturnsNull()
		{
			Assert.IsNull(MakeTheme(3).GetStyle(3));
		}


		[TestMethod]
		public void GetStyle_Negative_ReturnsNull()
		{
			Assert.IsNull(MakeTheme(3).GetStyle(-1));
		}


		[TestMethod]
		public void GetStyle_EmptyTheme_ReturnsNull()
		{
			Assert.IsNull(MakeTheme(0).GetStyle(0));
		}
	}
}
