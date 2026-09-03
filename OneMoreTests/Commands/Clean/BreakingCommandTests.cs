//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Clean
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Linq;
	using System.Xml.Linq;

	/*
	 * Test Protocol
	 * Commands/Clean/BreakingCommand
	 *
	 * This command expands or collapses whitespace between sentences.
	 *
	 * 	1. Move caret to paragraph
	 * 	2. More/Clean/Change Sentence Spacing
	 * 	3. Select "Two…" and click OK
	 * 	4. Confirm spacing between sentences include two spaces
	 * 	5. More/Clean/Change Sentence Spacing
	 * 	6. Select "One…" and click OK
	 * 	7. Confirm spacing between sentences include one space
	 * Confirm headings are not affected
	 */

	[TestClass]
	public class BreakingCommandTests : TestBase
	{
		private const string PageId = "page-1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		private static string GetText(XElement page, string marker)
		{
			return page
				.Descendants(Ns + "T")
				.Select(t => t.FirstNode as XCData)
				.FirstOrDefault(cd => cd != null && cd.Value.Contains(marker))
				?.Value;
		}


		[TestMethod]
		public void ChangeSpacing_SelectTwo_ExpandsSingleSpaceToDouble()
		{
			// Arrange: a paragraph whose sentences are separated by a single space
			var pageElement = new PageBuilder(PageId, "Two Space Test")
				.WithParagraph("First sentence. Second sentence.")
				.BuildElement();

			var page = new Page(pageElement);

			// Act: "Two…" selected in the dialog -> singleSpace: false
			var updated = new BreakingCommand().Run(page, singleSpace: false);

			// Assert
			Assert.IsTrue(updated, "Expected the page to be modified");
			var text = GetText(page.Root, "First sentence");
			Assert.AreEqual("First sentence.  Second sentence.", text,
				"Expected two spaces between sentences");
		}


		[TestMethod]
		public void ChangeSpacing_SelectOne_CollapsesDoubleSpaceToSingle()
		{
			// Arrange: a paragraph whose sentences are separated by two spaces
			var pageElement = new PageBuilder(PageId, "One Space Test")
				.WithParagraph("First sentence.  Second sentence.")
				.BuildElement();

			var page = new Page(pageElement);

			// Act: "One…" selected in the dialog -> singleSpace: true
			var updated = new BreakingCommand().Run(page, singleSpace: true);

			// Assert
			Assert.IsTrue(updated, "Expected the page to be modified");
			var text = GetText(page.Root, "First sentence");
			Assert.AreEqual("First sentence. Second sentence.", text,
				"Expected a single space between sentences");
		}


		[TestMethod]
		public void ChangeSpacing_HeadingParagraph_IsNotAffected()
		{
			// Arrange: a heading-styled paragraph with plain title text (no sentence break),
			// alongside a body paragraph that does have a matching sentence break. Only the
			// body paragraph should change.
			var headingOe = new XElement(Ns + "OE",
				new XAttribute("quickStyleIndex", "1"),
				new XElement(Ns + "T", new XCData("Chapter One")));

			var pageElement = new PageBuilder(PageId, "Heading Test")
				.WithElement(headingOe)
				.WithParagraph("First sentence. Second sentence.")
				.BuildElement();

			pageElement.AddFirst(new XElement(Ns + "QuickStyleDef",
				new XAttribute("index", "1"),
				new XAttribute("name", "h1")));

			var page = new Page(pageElement);

			// Act
			var updated = new BreakingCommand().Run(page, singleSpace: false);

			// Assert
			Assert.IsTrue(updated, "Expected the body paragraph to be modified");
			Assert.AreEqual("Chapter One", GetText(page.Root, "Chapter One"),
				"Expected the heading text to remain untouched");
			Assert.AreEqual("First sentence.  Second sentence.", GetText(page.Root, "First sentence"),
				"Expected the body paragraph's sentence spacing to be expanded");
		}


		[TestMethod]
		public void ChangeSpacing_NoSentenceBreaks_ReturnsFalse()
		{
			// Arrange: page text has no period at all, so neither pattern can match
			var pageElement = new PageBuilder(PageId, "No Match Test")
				.WithParagraph("Just some plain text with no sentence break")
				.BuildElement();

			var page = new Page(pageElement);
			var originalXml = page.Root.ToString();

			// Act
			var updated = new BreakingCommand().Run(page, singleSpace: false);

			// Assert
			Assert.IsFalse(updated, "Expected no modification when there is no matching content");
			Assert.AreEqual(originalXml, page.Root.ToString(), "Expected the page to be unchanged");
		}
	}
}
