//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Clean
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using River.OneMoreAddIn;

	/*
	 * Test Protocol - ClearBackgroundCommand
	 * Clear the highlight color of all text. Ensure table cell shading has sufficient
	 * contrast with page color by ensuring they are both "light" or both "dark" colors.
	 * Examples should include a portion of a paragraph highlighted, a portion of a paragraph
	 * in a table cell highlighted, highlighting across multiple spans with different style
	 * treatments such as colors and bold or italic.
	 *
	 *   1. Invoke Clean/Clear Background
	 *   2. Confirm that all highlighting is removed, cell backgrounds are adjusted,
	 *      and font colors are preserved
	 */

	[TestClass]
	public class ClearBackgroundCommandTests : TestBase
	{
		private const string PageId = "page-1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		private static Task ExecuteCommand()
		{
			var cmd = new ClearBackgroundCommand();
			cmd.SetLogger(Logger.Current);
			return cmd.Execute();
		}


		[TestMethod]
		public async Task ClearBackground_HighlightedParagraph_RemovesHighlight()
		{
			// Arrange: single paragraph with a highlighted span in OneNote's native single-quoted
			// CDATA format. The regex in ClearTextBackground uses [^;'] to bound the color value,
			// so single-quoted style attributes are required for the regex to terminate correctly.
			var oe = new XElement(Ns + "OE",
				new XElement(Ns + "T",
					new XCData("<span style='background:#FFFF00;mso-highlight:#FFFF00'>highlighted text</span>")));

			var xml = new PageBuilder(PageId, "Clear Background Test")
				.WithElement(oe)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await ExecuteCommand();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var cdata = updated
				.Descendants(Ns + "T")
				.Select(t => t.FirstNode as XCData)
				.FirstOrDefault(cd => cd != null);

			Assert.IsNotNull(cdata, "Expected a T element with CDATA");
			Assert.IsFalse(cdata.Value.Contains("background:"),
				$"Expected 'background:' to be removed but CDATA still contains: {cdata.Value}");
			Assert.IsFalse(cdata.Value.Contains("mso-highlight:"),
				"Expected 'mso-highlight:' to be removed");
		}


		[TestMethod]
		public async Task ClearBackground_MultipleSpansWithMixedStyles_RemovesHighlightPreservesOtherStyles()
		{
			// Arrange: three runs on one page demonstrating the three spec scenarios —
			// bold+highlighted, italic+highlighted, and a colored run with no highlight.

			var oeBold = new XElement(Ns + "OE",
				new XElement(Ns + "T",
					new XCData("<span style='font-weight:bold;background:#FFFF00'>bold highlighted</span>")));

			var oeItalic = new XElement(Ns + "OE",
				new XElement(Ns + "T",
					new XCData("<span style='font-style:italic;background:#00FF00'>italic highlighted</span>")));

			// Blue (#0000FF) on a white page: brightness 0.5 → |0.5 – 1.0| = 0.5 ≥ 0.3,
			// so NOT low-contrast; the color should be left unchanged.
			var oeColored = new XElement(Ns + "OE",
				new XAttribute("style", "color:#0000FF"),
				new XElement(Ns + "T",
					new XCData("colored text no highlight")));

			var xml = new PageBuilder(PageId, "Mixed Styles Test")
				.WithElement(oeBold)
				.WithElement(oeItalic)
				.WithElement(oeColored)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await ExecuteCommand();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			// No background: in any CDATA
			var cdataValues = updated
				.Descendants(Ns + "T")
				.Select(t => t.FirstNode as XCData)
				.Where(cd => cd != null)
				.Select(cd => cd.Value)
				.ToList();

			Assert.IsTrue(cdataValues.Any(), "Expected at least one T with CDATA");
			Assert.IsFalse(cdataValues.Any(v => v.Contains("background:")),
				"Expected all 'background:' properties to be removed");

			// Bold and italic styles preserved
			Assert.IsTrue(cdataValues.Any(v => v.Contains("font-weight:bold")),
				"Expected 'font-weight:bold' to be preserved after highlight removal");
			Assert.IsTrue(cdataValues.Any(v => v.Contains("font-style:italic")),
				"Expected 'font-style:italic' to be preserved after highlight removal");

			// Good-contrast font color (#0000FF) preserved on the OE
			var coloredOeStyle = updated
				.Descendants(Ns + "OE")
				.Select(oe => (string)oe.Attribute("style"))
				.FirstOrDefault(s => s != null && s.Contains("#0000FF"));

			Assert.IsNotNull(coloredOeStyle,
				"Expected OE color '#0000FF' to be preserved (sufficient contrast with white page)");
		}


		[TestMethod]
		public async Task ClearBackground_HighlightedTextInTableCell_RemovesHighlight()
		{
			// Arrange: table with a cell containing highlighted text. ClearTextBackground
			// defaults to all T elements when there is no selection, including those inside cells.
			var table = new XElement(Ns + "Table",
				new XAttribute("bordersVisible", "false"),
				new XElement(Ns + "Columns",
					new XElement(Ns + "Column",
						new XAttribute("index", "0"),
						new XAttribute("width", "200"))),
				new XElement(Ns + "Row",
					new XElement(Ns + "Cell",
						new XElement(Ns + "OEChildren",
							new XElement(Ns + "OE",
								new XElement(Ns + "T",
									new XCData("<span style='background:#FFFF00'>cell highlighted</span>")))))));

			var oe = new XElement(Ns + "OE", table);
			var xml = new PageBuilder(PageId, "Table Cell Highlight Test")
				.WithElement(oe)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await ExecuteCommand();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var cellCdata = updated
				.Descendants(Ns + "Cell")
				.Descendants(Ns + "T")
				.Select(t => t.FirstNode as XCData)
				.FirstOrDefault(cd => cd != null);

			Assert.IsNotNull(cellCdata, "Expected a T inside a Cell with CDATA");
			Assert.IsFalse(cellCdata.Value.Contains("background:"),
				$"Expected 'background:' to be removed from cell text but found: {cellCdata.Value}");
		}


		[TestMethod]
		public async Task ClearBackground_TableCellDarkShadingOnLightPage_RemovesShadingColor()
		{
			// Arrange: light (automatic/white) page with a table cell whose shadingColor is dark.
			// When scope is None (no T selection), ClearCellBackground processes only cells that
			// carry selected="all". Dark shade on a light page → mismatch → shadingColor removed.
			var table = new XElement(Ns + "Table",
				new XAttribute("bordersVisible", "false"),
				new XElement(Ns + "Columns",
					new XElement(Ns + "Column",
						new XAttribute("index", "0"),
						new XAttribute("width", "200"))),
				new XElement(Ns + "Row",
					new XElement(Ns + "Cell",
						new XAttribute("shadingColor", "#333333"),
						new XAttribute("selected", "all"),
						new XElement(Ns + "OEChildren",
							new XElement(Ns + "OE",
								new XElement(Ns + "T",
									new XCData("cell text")))))));

			var oe = new XElement(Ns + "OE", table);
			var xml = new PageBuilder(PageId, "Dark Shading Light Page Test")
				.WithElement(oe)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await ExecuteCommand();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var cell = updated.Descendants(Ns + "Cell").FirstOrDefault();
			Assert.IsNotNull(cell, "Expected a Cell element");
			Assert.IsNull(cell.Attribute("shadingColor"),
				$"Expected 'shadingColor' to be removed (dark shade on light page) but attribute still present with value '{(string)cell.Attribute("shadingColor")}'");
		}


		[TestMethod]
		public async Task ClearBackground_TableCellLightShadingOnLightPage_PreservesShadingColor()
		{
			// Arrange: light page with a cell whose shadingColor is also light. Light-on-light is
			// a matching pair — no contrast problem — so the shading must be preserved.
			// A highlighted paragraph is also included to ensure the page is updated (so we can
			// inspect the result), without the cell shading changing.
			var highlightedOe = new XElement(Ns + "OE",
				new XElement(Ns + "T",
					new XCData("<span style='background:#FFFF00'>highlighted text</span>")));

			var table = new XElement(Ns + "Table",
				new XAttribute("bordersVisible", "false"),
				new XElement(Ns + "Columns",
					new XElement(Ns + "Column",
						new XAttribute("index", "0"),
						new XAttribute("width", "200"))),
				new XElement(Ns + "Row",
					new XElement(Ns + "Cell",
						new XAttribute("shadingColor", "#CCCCCC"),
						new XAttribute("selected", "all"),
						new XElement(Ns + "OEChildren",
							new XElement(Ns + "OE",
								new XElement(Ns + "T",
									new XCData("cell text")))))));

			var tableOe = new XElement(Ns + "OE", table);
			var xml = new PageBuilder(PageId, "Light Shading Light Page Test")
				.WithElement(highlightedOe)
				.WithElement(tableOe)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await ExecuteCommand();

			// Assert: page was updated (highlight removed) but cell shading was left alone
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var cell = updated.Descendants(Ns + "Cell").FirstOrDefault();
			Assert.IsNotNull(cell, "Expected a Cell element");
			Assert.IsNotNull(cell.Attribute("shadingColor"),
				"Expected 'shadingColor' to be preserved (light shade on light page — same contrast side)");
			Assert.AreEqual("#CCCCCC", (string)cell.Attribute("shadingColor"),
				"Expected shadingColor value to remain '#CCCCCC'");
		}


		[TestMethod]
		public async Task ClearBackground_LowContrastFontColor_CorrectsFontColor()
		{
			// Arrange: light (automatic/white) page with a paragraph whose font color is very
			// light (#FFFFE0, brightness ~0.94). After background removal the text would be nearly
			// invisible against the white page. CheckContrast detects |0.94 – 1.0| = 0.06 < 0.3
			// and resets the color to the Normal-style default (#000000).
			var oe = new XElement(Ns + "OE",
				new XAttribute("style", "color:#FFFFE0"),
				new XElement(Ns + "T",
					new XCData("light-colored text")));

			var xml = new PageBuilder(PageId, "Low Contrast Color Test")
				.WithElement(oe)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await ExecuteCommand();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var oeStyle = updated
				.Descendants(Ns + "OE")
				.Select(e => (string)e.Attribute("style"))
				.FirstOrDefault(s => s != null);

			Assert.IsNotNull(oeStyle, "Expected OE to still have a style attribute");
			Assert.IsFalse(oeStyle.Contains("#FFFFE0"),
				"Expected low-contrast color '#FFFFE0' to be replaced");
			Assert.IsTrue(oeStyle.Contains("#000000"),
				$"Expected corrected color '#000000' but style was: {oeStyle}");
		}
	}
}
