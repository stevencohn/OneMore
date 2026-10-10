//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Clean
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	/*
	 * Test Protocol
	 * Adjust blank lines between paragraphs and remove empty headers, custom and standard
	 *
	 *     1. More/Clean/Adjust Blank Lines
	 *     2. Choose "Keep at most one blank line" and click OK
	 *     3. Confirm one empty lines remains between all paragraphs
	 *     4. Confirm that the starred "Lorem" paragraph can collapse the following "Ut" paragraph
	 *     5. Rerun command and choose "Remove all blank lines"
	 *     Confirm no empty lines remain between all paragraphs
	 *     6. Rerun and choose "Ensure exactly one blank line"
	 *     Confirm one blank line separates plain paragraphs but not list items or table rows
	 *     7. With "Put one blank line after each heading" checked, confirm a blank line
	 *     follows each heading regardless of the option chosen above; when unchecked, confirm
	 *     no blank line follows any heading
	 */

	[TestClass]
	public class RemoveEmptyCommandTests : TestBase
	{
		private const string PageId = "page-1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		// RemoveEmptyCommand's dialog-driven path (RunInteractive) cannot be exercised
		// headlessly since it shows RemoveEmptyDialog. The command also implements
		// ICliPageCommand, whose Execute branch accepts a CliParameterSet carrying "pageId",
		// "all", "mode" and "headingBlank" and drives the same Run logic without any UI, so
		// these tests exercise it that way.
		private static Task ExecuteCommand(string pageId, bool all,
			string mode = null, bool headingBlank = false)
		{
			var cliParams = new CliParameterSet();
			cliParams.Set("pageId", pageId);
			cliParams.Set("all", all);

			if (mode is not null)
			{
				cliParams.Set("mode", mode);
			}

			cliParams.Set("headingBlank", headingBlank);

			var cmd = new RemoveEmptyCommand();
			cmd.SetLogger(Logger.Current);
			return cmd.Execute(cliParams);
		}


		// Adds a standard "hN" QuickStyleDef to the page root so an OE referencing it via
		// quickStyleIndex is recognized as a known Heading style by Page.GetQuickStyles().
		private static void AddHeadingQuickStyle(XElement page, int index)
		{
			page.AddFirst(new XElement(Ns + "QuickStyleDef",
				new XAttribute("index", index.ToString()),
				new XAttribute("name", "h1"),
				new XAttribute("font", "Calibri"),
				new XAttribute("fontSize", "16.0"),
				new XAttribute("fontColor", "automatic")));
		}


		// the text of each OE in document order, with empty paragraphs as ""
		private static string[] Lines(XElement page)
		{
			return page.Descendants(Ns + "OE")
				.Where(oe => oe.Elements(Ns + "T").Any())
				.Select(oe => string.Concat(oe.Elements(Ns + "T").Select(e => e.Value)).Trim())
				.ToArray();
		}


		[TestMethod]
		public async Task RemoveEmpty_ConsecutiveEmptyLines_CollapseToOne()
		{
			// Arrange: three consecutive empty paragraphs between two text paragraphs
			var xml = new PageBuilder(PageId, "Collapse Test")
				.WithParagraph("Lorem ipsum dolor sit amet")
				.WithParagraph("")
				.WithParagraph("")
				.WithParagraph("")
				.WithParagraph("Ut enim ad minim veniam")
				.Build();

			SetupPage(PageId, xml);

			// Act: keep at most one empty line between paragraphs
			await ExecuteCommand(PageId, all: false);

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var paragraphs = updated.Descendants(Ns + "OE").ToList();
			Assert.AreEqual(3, paragraphs.Count,
				"Expected Lorem, one collapsed empty line, and Ut to remain");

			StringAssert.Contains(paragraphs[0].TextValue(), "Lorem ipsum");
			Assert.AreEqual(string.Empty, paragraphs[1].TextValue().Trim(),
				"Expected exactly one empty paragraph to remain between Lorem and Ut");
			StringAssert.Contains(paragraphs[2].TextValue(), "Ut enim");
		}


		[TestMethod]
		public async Task RemoveEmpty_EmptyHeading_RemovedWhenRemovingAll()
		{
			// Arrange: Lorem paragraph followed directly by a single empty heading-styled
			// paragraph (quickStyleIndex references a known "hN" QuickStyleDef), then Ut.
			var page = new PageBuilder(PageId, "Heading Removal Test")
				.WithParagraph("Lorem ipsum dolor sit amet")
				.WithParagraph("", quickStyleIndex: 1)
				.WithParagraph("Ut enim ad minim veniam")
				.BuildElement();

			AddHeadingQuickStyle(page, 1);

			SetupPage(PageId, page.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act: remove all empty lines
			await ExecuteCommand(PageId, all: true);

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var paragraphs = updated.Descendants(Ns + "OE")
				.Where(oe => oe.Elements(Ns + "T").Any())
				.ToList();

			Assert.AreEqual(2, paragraphs.Count,
				"Expected the empty heading paragraph to be removed, leaving only Lorem and Ut");

			StringAssert.Contains(paragraphs[0].TextValue(), "Lorem ipsum");
			StringAssert.Contains(paragraphs[1].TextValue(), "Ut enim");
		}


		[TestMethod]
		public async Task RemoveEmpty_EmptyHeading_BecomesPlainBlankWhenKeepingOne()
		{
			// Arrange: same page as above, but the lone empty heading separates the paragraphs
			var page = new PageBuilder(PageId, "Heading Conversion Test")
				.WithParagraph("Lorem ipsum dolor sit amet")
				.WithParagraph("", quickStyleIndex: 1)
				.WithParagraph("Ut enim ad minim veniam")
				.BuildElement();

			AddHeadingQuickStyle(page, 1);

			SetupPage(PageId, page.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act: keep at most one empty line
			await ExecuteCommand(PageId, all: false);

			// Assert: the separator remains but is no longer styled as a heading
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var paragraphs = updated.Descendants(Ns + "OE")
				.Where(oe => oe.Elements(Ns + "T").Any())
				.ToList();

			Assert.AreEqual(3, paragraphs.Count,
				"Expected Lorem, a plain blank line, and Ut");

			Assert.AreEqual(string.Empty, paragraphs[1].TextValue().Trim());
			Assert.IsNull(paragraphs[1].Attribute("quickStyleIndex"),
				"Expected the blank line to have its heading style cleared");
		}


		[TestMethod]
		public async Task RemoveEmpty_AllFlagTrue_RemovesAllEmptyLines()
		{
			// Arrange: same consecutive-empty-lines setup as the collapse test
			var xml = new PageBuilder(PageId, "Remove All Test")
				.WithParagraph("Lorem ipsum dolor sit amet")
				.WithParagraph("")
				.WithParagraph("")
				.WithParagraph("")
				.WithParagraph("Ut enim ad minim veniam")
				.Build();

			SetupPage(PageId, xml);

			// Act: remove every empty line
			await ExecuteCommand(PageId, all: true);

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var paragraphs = updated.Descendants(Ns + "OE").ToList();
			Assert.AreEqual(2, paragraphs.Count,
				"Expected no empty lines to remain between Lorem and Ut");

			StringAssert.Contains(paragraphs[0].TextValue(), "Lorem ipsum");
			StringAssert.Contains(paragraphs[1].TextValue(), "Ut enim");
		}


		[TestMethod]
		public async Task RemoveEmpty_RerunWithAllTrue_RemovesRemainingEmptyLine()
		{
			// Arrange: three consecutive empty paragraphs between two text paragraphs
			var xml = new PageBuilder(PageId, "Rerun Test")
				.WithParagraph("Lorem ipsum dolor sit amet")
				.WithParagraph("")
				.WithParagraph("")
				.WithParagraph("")
				.WithParagraph("Ut enim ad minim veniam")
				.Build();

			SetupPage(PageId, xml);

			// Act 1: keep one, collapses the run down to a single empty line
			await ExecuteCommand(PageId, all: false);

			var afterFirst = GetUpdatedPage(PageId);
			Assert.IsNotNull(afterFirst, "First UpdatePageContent was never called");

			var emptyCount = afterFirst.Descendants(Ns + "OE")
				.Count(oe => oe.TextValue().Trim().Length == 0);
			Assert.AreEqual(1, emptyCount, "Expected exactly one empty line after the first pass");

			// persist the result of the first pass so the second pass starts from it
			Mock.SetPage(PageId, afterFirst.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act 2: rerun removing all, removes the last remaining empty line
			await ExecuteCommand(PageId, all: true);

			var afterSecond = GetUpdatedPage(PageId);
			Assert.IsNotNull(afterSecond, "Second UpdatePageContent was never called");

			var remainingEmpty = afterSecond.Descendants(Ns + "OE")
				.Count(oe => oe.TextValue().Trim().Length == 0);
			Assert.AreEqual(0, remainingEmpty,
				"Expected no empty lines to remain after rerunning with all=true");
		}


		[TestMethod]
		public async Task RemoveEmpty_NoEmptyLines_DoesNotCallUpdate()
		{
			// Arrange: page with no empty paragraphs anywhere
			var xml = new PageBuilder(PageId, "No Empty Lines Test")
				.WithParagraph("Lorem ipsum dolor sit amet")
				.WithParagraph("Ut enim ad minim veniam")
				.Build();

			SetupPage(PageId, xml);
			var originalXml = Mock.GetPage(PageId);

			// Act
			await ExecuteCommand(PageId, all: false);

			// Assert: page XML is unchanged because UpdatePageContent was never called
			var storedXml = Mock.GetPage(PageId);
			Assert.AreEqual(originalXml, storedXml,
				"Page should not have been updated when there are no empty lines to remove");
		}


		[TestMethod]
		public async Task RemoveEmpty_ExactlyOne_InsertsBlankBetweenPlainParagraphs()
		{
			// Arrange: three adjacent paragraphs with no blank lines
			var xml = new PageBuilder(PageId, "Exactly One Insert Test")
				.WithParagraph("Lorem ipsum")
				.WithParagraph("Ut enim")
				.WithParagraph("Duis aute")
				.Build();

			SetupPage(PageId, xml);

			// Act
			await ExecuteCommand(PageId, all: false, mode: "exactly-one");

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			CollectionAssert.AreEqual(
				new[] { "Lorem ipsum", "", "Ut enim", "", "Duis aute" },
				Lines(updated));
		}


		[TestMethod]
		public async Task RemoveEmpty_ExactlyOne_CollapsesAndKeepsExistingBlank()
		{
			// Arrange: two blanks between the first pair, none between the second pair
			var xml = new PageBuilder(PageId, "Exactly One Mixed Test")
				.WithParagraph("Lorem ipsum")
				.WithParagraph("")
				.WithParagraph("")
				.WithParagraph("Ut enim")
				.WithParagraph("Duis aute")
				.Build();

			SetupPage(PageId, xml);

			// Act
			await ExecuteCommand(PageId, all: false, mode: "exactly-one");

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			CollectionAssert.AreEqual(
				new[] { "Lorem ipsum", "", "Ut enim", "", "Duis aute" },
				Lines(updated));
		}


		[TestMethod]
		public async Task RemoveEmpty_ExactlyOne_DoesNotSeparateListItems()
		{
			// Arrange: two adjacent bulleted list items between plain paragraphs
			XElement Bullet(string text) =>
				new XElement(Ns + "OE",
					new XElement(Ns + "List",
						new XElement(Ns + "Bullet", new XAttribute("bullet", "2"))),
					new XElement(Ns + "T", new XCData(text)));

			var xml = new PageBuilder(PageId, "Exactly One List Test")
				.WithElement(Bullet("one"))
				.WithElement(Bullet("two"))
				.Build();

			SetupPage(PageId, xml);
			var originalXml = Mock.GetPage(PageId);

			// Act
			await ExecuteCommand(PageId, all: false, mode: "exactly-one");

			// Assert: nothing to change, so the page is never updated
			Assert.AreEqual(originalXml, Mock.GetPage(PageId),
				"Blank lines must not be inserted between list items");
		}


		[TestMethod]
		public async Task RemoveEmpty_HeadingBlank_AddsBlankAfterHeadingWhenRemovingAll()
		{
			// Arrange: heading directly followed by a paragraph, then a second paragraph
			var page = new PageBuilder(PageId, "Heading Blank Test")
				.WithParagraph("Section", quickStyleIndex: 1)
				.WithParagraph("Lorem ipsum")
				.WithParagraph("")
				.WithParagraph("Ut enim")
				.BuildElement();

			AddHeadingQuickStyle(page, 1);

			SetupPage(PageId, page.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act: remove all blank lines but always keep one after a heading
			await ExecuteCommand(PageId, all: true, headingBlank: true);

			// Assert: blank after heading, none between the paragraphs
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			CollectionAssert.AreEqual(
				new[] { "Section", "", "Lorem ipsum", "Ut enim" },
				Lines(updated));
		}


		[TestMethod]
		public async Task RemoveEmpty_HeadingBlank_CollapsesMultipleBlanksAfterHeading()
		{
			// Arrange: heading followed by three blank lines
			var page = new PageBuilder(PageId, "Heading Collapse Test")
				.WithParagraph("Section", quickStyleIndex: 1)
				.WithParagraph("")
				.WithParagraph("")
				.WithParagraph("")
				.WithParagraph("Lorem ipsum")
				.BuildElement();

			AddHeadingQuickStyle(page, 1);

			SetupPage(PageId, page.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act
			await ExecuteCommand(PageId, all: false, headingBlank: true);

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			CollectionAssert.AreEqual(
				new[] { "Section", "", "Lorem ipsum" },
				Lines(updated));
		}

		[TestMethod]
		[DataRow("keep")]
		[DataRow("exactly-one")]
		public async Task RemoveEmpty_HeadingBlankOff_RemovesBlankAfterHeading(string mode)
		{
			// Arrange: heading, a blank line, then paragraphs
			var page = new PageBuilder(PageId, "Heading No Blank Test")
				.WithParagraph("Section", quickStyleIndex: 1)
				.WithParagraph("")
				.WithParagraph("Lorem ipsum")
				.WithParagraph("Ut enim")
				.BuildElement();

			AddHeadingQuickStyle(page, 1);

			SetupPage(PageId, page.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act: heading option off, so no blank line follows the heading
			await ExecuteCommand(PageId, all: false, mode: mode, headingBlank: false);

			// Assert: only the heading's blank line is removed; paragraphs keep their spacing
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var expected = mode == "keep"
				? new[] { "Section", "Lorem ipsum", "Ut enim" }
				: new[] { "Section", "Lorem ipsum", "", "Ut enim" };

			CollectionAssert.AreEqual(expected, Lines(updated));
		}

		[TestMethod]
		[DataRow("keep")]
		[DataRow("exactly-one")]
		public async Task RemoveEmpty_ParagraphWithTrailingCursorRun_IsNotRemoved(string mode)
		{
			// Arrange: with only a text cursor, OneNote emits the caret as an empty selected T
			// run trailing the paragraph's text run; a blank line follows that paragraph
			var cursorParagraph = new XElement(Ns + "OE",
				new XElement(Ns + "T", new XCData("Lorem ipsum")),
				new XElement(Ns + "T", new XAttribute("selected", "all"), new XCData(string.Empty)));

			var xml = new PageBuilder(PageId, "Cursor Paragraph Test")
				.WithElement(cursorParagraph)
				.WithParagraph("")
				.WithParagraph("Ut enim")
				.Build();

			SetupPage(PageId, xml);

			// Act
			await ExecuteCommand(PageId, all: false, mode: mode);

			// Assert: the paragraph containing the cursor survives
			var updated = GetUpdatedPage(PageId);
			if (updated is null)
			{
				// nothing needed changing, which is also acceptable
				updated = XElement.Parse(Mock.GetPage(PageId));
			}

			CollectionAssert.AreEqual(
				new[] { "Lorem ipsum", "", "Ut enim" },
				Lines(updated));
		}

		[TestMethod]
		public async Task RemoveEmpty_ExactlyOne_NestedTrailingBlankIsNotDoubled()
		{
			// Arrange: the second paragraph is an empty run above an indented child, and the
			// last child is already a blank line, so one blank line already follows "5a"
			XElement Para(string text, params object[] content) =>
				new XElement(Ns + "OE",
					new XAttribute("quickStyleIndex", "2"),
					new XElement(Ns + "T", new XCData(text)),
					content);

			var indented = Para("",
				new XElement(Ns + "OEChildren",
					Para("5a Ut enim ad minim veniam"),
					Para("")));

			var xml = new PageBuilder(PageId, "Nested Blank Test")
				.WithElement(Para("5 Lorem ipsum dolor sit amet"))
				.WithElement(indented)
				.WithElement(Para("6 Excepteur sint occaecat cupidatat non proident"))
				.Build();

			SetupPage(PageId, xml);

			// Act
			await ExecuteCommand(PageId, all: false, mode: "exactly-one");

			// Assert: exactly one blank line above and one below the indented paragraph
			var updated = GetUpdatedPage(PageId) ?? XElement.Parse(Mock.GetPage(PageId));

			CollectionAssert.AreEqual(
				new[]
				{
					"5 Lorem ipsum dolor sit amet",
					"",
					"5a Ut enim ad minim veniam",
					"",
					"6 Excepteur sint occaecat cupidatat non proident"
				},
				Lines(updated));
		}

		[TestMethod]
		public async Task RemoveEmpty_RemoveAll_NestedLeadingBlankIsRemoved()
		{
			// Arrange: the second paragraph is an empty run above an indented child, which
			// is followed by a blank line; none of these blank lines should remain
			XElement Para(string text, params object[] content) =>
				new XElement(Ns + "OE",
					new XAttribute("quickStyleIndex", "2"),
					new XElement(Ns + "T", new XCData(text)),
					content);

			var indented = Para("",
				new XElement(Ns + "OEChildren",
					Para("5a Ut enim ad minim veniam"),
					Para("")));

			var xml = new PageBuilder(PageId, "Nested Remove All Test")
				.WithElement(Para("5 Lorem ipsum dolor sit amet"))
				.WithElement(indented)
				.WithElement(Para("6 Excepteur sint occaecat cupidatat non proident"))
				.Build();

			SetupPage(PageId, xml);

			// Act
			await ExecuteCommand(PageId, all: true);

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			CollectionAssert.AreEqual(
				new[]
				{
					"5 Lorem ipsum dolor sit amet",
					"5a Ut enim ad minim veniam",
					"6 Excepteur sint occaecat cupidatat non proident"
				},
				Lines(updated));

			var five = updated.Descendants(Ns + "OE").First();
			Assert.IsTrue(five.Descendants(Ns + "OE")
				.Any(oe => oe.TextValue().Contains("5a Ut enim")),
				"Expected the indented paragraph to remain nested under '5'");
		}
	}
}
