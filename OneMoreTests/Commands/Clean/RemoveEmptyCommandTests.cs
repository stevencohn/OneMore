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
	 * Collapse multiple consecutive empty lines into a single empty line. Also removes empty
	 * headers, custom and standard
	 *
	 *     1. More/Clean/Remove Empty Lines
	 *     2. Click No
	 *     3. Confirm one empty lines remains between all paragraphs
	 *     4. Confirm that the starred "Lorem" paragraph can collapse the following "Ut" paragraph
	 *     5. Rerun command and click Yes
	 *     Confirm no empty lines remain between all paragraphs
	 */

	[TestClass]
	public class RemoveEmptyCommandTests : TestBase
	{
		private const string PageId = "page-1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		// RemoveEmptyCommand's dialog-driven path (RunInteractive) cannot be exercised
		// headlessly since it reads Yes/No/Cancel from MoreMessageBox.ShowQuestion. The
		// command also implements ICliPageCommand, whose Execute branch accepts a
		// CliParameterSet carrying "pageId" and "all" and drives the same Run logic
		// without any UI, so these tests exercise it that way.
		private static Task ExecuteCommand(string pageId, bool all)
		{
			var cliParams = new CliParameterSet();
			cliParams.Set("pageId", pageId);
			cliParams.Set("all", all);

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

			// Act: click No -> all=false, keep one empty line between paragraphs
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
		public async Task RemoveEmpty_EmptyHeadingParagraph_RemovedRegardlessOfAllFlag()
		{
			// Arrange: Lorem paragraph followed directly by a single empty heading-styled
			// paragraph (quickStyleIndex references a known "hN" QuickStyleDef), then Ut.
			// This single empty line is not part of a consecutive run, so the normal
			// "keep one" rule would leave it in place; the heading-removal rule must
			// remove it anyway.
			var page = new PageBuilder(PageId, "Heading Removal Test")
				.WithParagraph("Lorem ipsum dolor sit amet")
				.WithParagraph("", quickStyleIndex: 1)
				.WithParagraph("Ut enim ad minim veniam")
				.BuildElement();

			AddHeadingQuickStyle(page, 1);

			SetupPage(PageId, page.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act: click No -> all=false
			await ExecuteCommand(PageId, all: false);

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

			// Act: click Yes -> all=true, remove every empty line
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

			// Act 1: click No -> all=false, collapses the run down to a single empty line
			await ExecuteCommand(PageId, all: false);

			var afterFirst = GetUpdatedPage(PageId);
			Assert.IsNotNull(afterFirst, "First UpdatePageContent was never called");

			var emptyCount = afterFirst.Descendants(Ns + "OE")
				.Count(oe => oe.TextValue().Trim().Length == 0);
			Assert.AreEqual(1, emptyCount, "Expected exactly one empty line after the first pass");

			// persist the result of the first pass so the second pass starts from it
			Mock.SetPage(PageId, afterFirst.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act 2: rerun and click Yes -> all=true, removes the last remaining empty line
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
	}
}
