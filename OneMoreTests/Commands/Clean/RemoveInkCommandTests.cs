//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Clean
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	/*
	 * Test Protocol - RemoveInkCommand
	 * Removes all ink drawings and annotations from the current page
	 *
	 * 	1. More/Clean/Remove Ink
	 * 	2. Confirm the ink is removed from this page
	 */

	[TestClass]
	public class RemoveInkCommandTests : TestBase
	{
		private const string PageId = "page-1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		private static Task ExecuteCommand()
		{
			var cmd = new RemoveInkCommand();
			cmd.SetLogger(Logger.Current);
			return cmd.Execute();
		}


		[TestMethod]
		public async Task RemoveInk_InkWord_RemovesWordPreservesParagraph()
		{
			// Arrange: OE with a text run and a sibling InkWord annotation
			var oe = new XElement(Ns + "OE",
				new XElement(Ns + "T", new XCData("Hello world")),
				new XElement(Ns + "InkWord",
					new XAttribute("objectID", "ink-word-1")));

			var xml = new PageBuilder(PageId, "Ink Word Test")
				.WithElement(oe)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await ExecuteCommand();

			// Assert: InkWord is gone but the surrounding text run survives
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			Assert.IsFalse(updated.Descendants(Ns + "InkWord").Any(),
				"Expected InkWord to be removed");

			var text = updated.Descendants(Ns + "T")
				.Select(t => ((XCData)t.FirstNode).Value)
				.FirstOrDefault(v => v == "Hello world");

			Assert.IsNotNull(text, "Expected surrounding text to be preserved");
		}


		[TestMethod]
		public async Task RemoveInk_InkParagraph_RemovesParagraphAndCollapsesEmptyOE()
		{
			// Arrange: first OE is pure ink annotation (InkParagraph only, no other content),
			// second OE is a plain text paragraph. InkParagraph can only exist inside an OE,
			// so removing it leaves the first OE empty; RemoveInkCommand should then collapse
			// that now-empty OE too, while leaving the sibling paragraph untouched.
			var inkOe = new XElement(Ns + "OE",
				new XElement(Ns + "InkParagraph",
					new XAttribute("objectID", "ink-para-1")));

			var xml = new PageBuilder(PageId, "Ink Paragraph Test")
				.WithElement(inkOe)
				.WithParagraph("Second paragraph")
				.Build();

			SetupPage(PageId, xml);

			// Act
			await ExecuteCommand();

			// Assert: InkParagraph and its now-empty OE are both gone; the other paragraph remains
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			Assert.IsFalse(updated.Descendants(Ns + "InkParagraph").Any(),
				"Expected InkParagraph to be removed");

			var oes = updated.Descendants(Ns + "OEChildren").Elements(Ns + "OE").ToList();
			Assert.AreEqual(1, oes.Count,
				"Expected the empty ink OE to be removed, leaving only the text paragraph");

			var text = updated.Descendants(Ns + "T")
				.Select(t => ((XCData)t.FirstNode).Value)
				.FirstOrDefault(v => v == "Second paragraph");

			Assert.IsNotNull(text, "Expected the plain text paragraph to be preserved");
		}


		[TestMethod]
		public async Task RemoveInk_InkDrawingOnly_DoesNotUpdatePage()
		{
			// Arrange: page with only an ink drawing. Drawings are removed via a direct
			// DeletePageContent call rather than by editing the page tree, so this alone
			// should not trigger a full page update.
			var oe = new XElement(Ns + "OE",
				new XElement(Ns + "InkDrawing",
					new XAttribute("objectID", "ink-drawing-1")));

			var xml = new PageBuilder(PageId, "Ink Drawing Test")
				.WithElement(oe)
				.Build();

			SetupPage(PageId, xml);
			var originalXml = Mock.GetPage(PageId);

			// Act
			await ExecuteCommand();

			// Assert: page XML is unchanged because UpdatePageContent was never called
			var storedXml = Mock.GetPage(PageId);
			Assert.AreEqual(originalXml, storedXml,
				"Page should not be updated when the only ink content is an InkDrawing");
		}


		[TestMethod]
		public async Task RemoveInk_NoInkContent_DoesNotUpdatePage()
		{
			// Arrange: plain page with no ink content at all
			var xml = new PageBuilder(PageId, "No Ink Test")
				.WithParagraph("Just text")
				.Build();

			SetupPage(PageId, xml);
			var originalXml = Mock.GetPage(PageId);

			// Act
			await ExecuteCommand();

			// Assert: page XML is unchanged because UpdatePageContent was never called
			var storedXml = Mock.GetPage(PageId);
			Assert.AreEqual(originalXml, storedXml,
				"Page should not have been updated when there is no ink content");
		}
	}
}
