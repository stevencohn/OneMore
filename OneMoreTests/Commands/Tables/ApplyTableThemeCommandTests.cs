//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Tables
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Styles;
	using System.Drawing;
	using System.Xml.Linq;

	[TestClass]
	public class ApplyTableThemeCommandTests
	{
		// issue 2757: MakeStylizer threw NullReferenceException when a theme font role
		// specified only a color, leaving ColorFont.Font null

		private static string Stylize(TableTheme.ColorFont font)
		{
			var ns = XNamespace.Get("http://schemas.microsoft.com/office/onenote/2013/onenote");
			var element = new XElement(ns + "T", new XCData(string.Empty));

			ApplyTableThemeCommand.MakeStylizer(font).ApplyStyle(element);

			return element.Attribute("style")?.Value;
		}


		[TestMethod]
		public void MakeStylizer_NullFont_UsesDefaultsAndColor()
		{
			var style = Stylize(new TableTheme.ColorFont { Foreground = Color.Red });

			StringAssert.Contains(style, StyleBase.DefaultFontFamily);
			StringAssert.Contains(style.ToLowerInvariant(), "#ff0000");
		}


		[TestMethod]
		public void MakeStylizer_WithFont_UsesFont()
		{
			using var font = new TableTheme.ColorFont
			{
				Font = new Font("Arial", 14f, FontStyle.Bold)
			};

			var style = Stylize(font);

			StringAssert.Contains(style, "Arial");
			StringAssert.Contains(style, "14.0pt");
			StringAssert.Contains(style, "bold");
		}
	}
}
