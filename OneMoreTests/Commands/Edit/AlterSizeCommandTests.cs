//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Edit
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	[TestClass]
	public class AlterSizeCommandTests : TestBase
	{
		private const string PageId = "page-1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";

		/*
		 * Test Protocol
		 * Commands/Edit/AlterSizeCommand
		 * 
		 * Increases or decreases the font size of all text on the entire page. 
		 * Cannot be constrained to a selected block; instead use the built-in OneNote 
		 * shortcuts Ctrl+Shift+> and +<
		 *
		 * 	1. More/Edit/Increase Font Size -- or press Ctrl+Alt+OemPlus
		 * 	2. Confirm all text on the page increases font size consistencly across styles
		 * 	3. More/Edit/Decrease Font Size -- or press Ctrl+Alt+OemMinus
		 * 	4. Confirm all text on the page decreases font size consistencly across styles
		 * 
		 * Min font size is 6.0
		 * Max font size is 144.0
		 */

		[TestMethod]
		public async Task IncreaseFontSize_WithOeStyleAttribute_IncreasesSize()
		{
			// Arrange: OE whose style attribute carries font-size
			var oe = new XElement(Ns + "OE",
				new XAttribute("style", "font-family:'Segoe UI';font-size:12.0pt"),
				new XElement(Ns + "T", new XCData("Hello world")));

			var xml = new PageBuilder(PageId, "AlterSize Test")
				.WithElement(oe)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new IncreaseFontSizeCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var style = updated.Descendants(Ns + "OE")
				.Select(e => (string)e.Attribute("style"))
				.FirstOrDefault(s => s != null) ?? "";
			StringAssert.Contains(style, "font-size:13.05pt");
		}


		[TestMethod]
		public async Task DecreaseFontSize_WithOeStyleAttribute_DecreasesSize()
		{
			// Arrange
			var oe = new XElement(Ns + "OE",
				new XAttribute("style", "font-family:'Segoe UI';font-size:12.0pt"),
				new XElement(Ns + "T", new XCData("Hello world")));

			var xml = new PageBuilder(PageId, "AlterSize Test")
				.WithElement(oe)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new DecreaseFontSizeCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var style = updated.Descendants(Ns + "OE")
				.Select(e => (string)e.Attribute("style"))
				.FirstOrDefault(s => s != null) ?? "";
			StringAssert.Contains(style, "font-size:11.05pt");
		}


		[TestMethod]
		public async Task IncreaseFontSize_WithQuickStyleDef_IncreasesSize()
		{
			// Arrange: add a QuickStyleDef directly to the page root
			var pageElement = new PageBuilder(PageId, "AlterSize Test").BuildElement();
			pageElement.AddFirst(new XElement(Ns + "QuickStyleDef",
				new XAttribute("index", "1"),
				new XAttribute("name", "p"),
				new XAttribute("fontSize", "11.0")));

			SetupPage(PageId, pageElement.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act
			await new IncreaseFontSizeCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var fontSize = (string)updated
				.Elements(Ns + "QuickStyleDef")
				.First(e => (string)e.Attribute("name") == "p")
				.Attribute("fontSize");

			Assert.AreEqual("12.05", fontSize);
		}


		[TestMethod]
		public async Task DecreaseFontSize_WithQuickStyleDef_DecreasesSize()
		{
			// Arrange
			var pageElement = new PageBuilder(PageId, "AlterSize Test").BuildElement();
			pageElement.AddFirst(new XElement(Ns + "QuickStyleDef",
				new XAttribute("index", "1"),
				new XAttribute("name", "p"),
				new XAttribute("fontSize", "11.0")));

			SetupPage(PageId, pageElement.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act
			await new DecreaseFontSizeCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var fontSize = (string)updated
				.Elements(Ns + "QuickStyleDef")
				.First(e => (string)e.Attribute("name") == "p")
				.Attribute("fontSize");

			Assert.AreEqual("10.05", fontSize);
		}


		[TestMethod]
		public async Task IncreaseFontSize_AtMaximumSize_DoesNotUpdate()
		{
			// Arrange: OE at the maximum font size (144.0) — increase should clamp, no update
			var oe = new XElement(Ns + "OE",
				new XAttribute("style", "font-size:144.0pt"),
				new XElement(Ns + "T", new XCData("Max size text")));

			var xml = new PageBuilder(PageId, "AlterSize Test")
				.WithElement(oe)
				.Build();

			SetupPage(PageId, xml);
			var originalXml = Mock.GetPage(PageId);

			// Act
			await new IncreaseFontSizeCommand().Execute();

			// Assert: UpdatePageContent should not have been called
			Assert.AreEqual(originalXml, Mock.GetPage(PageId),
				"Page should not be updated when font size is already at maximum");
		}


		[TestMethod]
		public async Task DecreaseFontSize_AtMinimumSize_DoesNotUpdate()
		{
			// Arrange: OE at the minimum font size (6.0) — decrease should clamp, no update
			var oe = new XElement(Ns + "OE",
				new XAttribute("style", "font-size:6.0pt"),
				new XElement(Ns + "T", new XCData("Min size text")));

			var xml = new PageBuilder(PageId, "AlterSize Test")
				.WithElement(oe)
				.Build();

			SetupPage(PageId, xml);
			var originalXml = Mock.GetPage(PageId);

			// Act
			await new DecreaseFontSizeCommand().Execute();

			// Assert
			Assert.AreEqual(originalXml, Mock.GetPage(PageId),
				"Page should not be updated when font size is already at minimum");
		}


		[TestMethod]
		public async Task AlterFontSize_PageWithNoFontSizeContent_DoesNotUpdate()
		{
			// Arrange: plain paragraph — no style attributes, no font-size anywhere
			var xml = new PageBuilder(PageId, "AlterSize Test")
				.WithParagraph("Plain text")
				.Build();

			SetupPage(PageId, xml);
			var originalXml = Mock.GetPage(PageId);

			// Act
			await new IncreaseFontSizeCommand().Execute();

			// Assert
			Assert.AreEqual(originalXml, Mock.GetPage(PageId),
				"Page should not be updated when there is no font-size content");
		}


		[TestMethod]
		public async Task IncreaseFontSize_WithCDataSpan_IncreasesSize()
		{
			// Arrange: T element whose CDATA contains an inline span with font-size
			var oe = new XElement(Ns + "OE",
				new XElement(Ns + "T",
					new XCData("<span style=\"font-size:12.0pt\">Hello world</span>")));

			var xml = new PageBuilder(PageId, "AlterSize Test")
				.WithElement(oe)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new IncreaseFontSizeCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var cdata = updated.Descendants(Ns + "T")
				.Select(t => t.FirstNode as XCData)
				.FirstOrDefault(cd => cd?.Value.Contains("font-size:") == true);

			Assert.IsNotNull(cdata, "Expected T element with font-size in CDATA");
			StringAssert.Contains(cdata.Value, "font-size:13.05pt");
		}
	}
}
