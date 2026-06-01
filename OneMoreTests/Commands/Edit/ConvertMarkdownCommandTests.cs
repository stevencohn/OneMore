//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Edit
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/*
	 * Test Protocol
	 * Commands/Edit/ConvertMarkDown
	 * Convert the selected markdown text, or the entire page, to OneNote native styled content
	 *
	 *  1. Edit/Convert Markdown
	 *  2. Confirm the full page contents below have all been converted to standard styles
	 *  3. Edit/Convert Markdown
	 *  4. Confirm the full page contents are exactly the same as in step #2
	 *  5. Revert. Select a block of markdown and then Edit/Convert Markdown
	 *  6. Confirm just the selected block has been converted
	 */

	[TestClass]
	public class ConvertMarkdownCommandTests : TestBase
	{
		private const string PageId = "page-1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		// ConvertMarkdownCommand reads objectID from every OE when diffing pre- and
		// post-conversion content.  Assign sequential IDs to any OE that lacks one so
		// the command does not throw NullReferenceException accessing Attribute("objectID").Value.
		private static string AssignObjectIds(XElement pageElement)
		{
			var counter = 0;
			foreach (var oe in pageElement.Descendants(Ns + "OE"))
			{
				if (oe.Attribute("objectID") is null)
				{
					oe.SetAttributeValue("objectID", $"oe-auto-{++counter}");
				}
			}

			return pageElement.ToString(SaveOptions.OmitDuplicateNamespaces);
		}


		// Command.logger is normally injected by CommandFactory; wire it up here so the
		// logger.Debug calls inside ConvertMarkdownCommand do not throw NullReferenceException.
		private static River.OneMoreAddIn.Command WithLogger(River.OneMoreAddIn.Command cmd)
		{
			return cmd.SetLogger(River.OneMoreAddIn.Logger.Current);
		}


		[TestMethod]
		public async Task ConvertMarkdown_FullPage_InsertsHtmlBlock()
		{
			// Arrange: page with two markdown paragraphs plus an empty cursor OE.
			// The empty selected T causes SelectionRange to report TextCursor scope,
			// which sets allContent=true so the entire page body is converted.
			var pageElement = new PageBuilder(PageId, "Markdown Test")
				.WithParagraph("# Hello World")
				.WithParagraph("This is **bold** text.")
				.BuildElement();

			// Prepend a cursor OE (empty selected T) to simulate the insertion point
			var cursorOe = new XElement(Ns + "OE",
				new XAttribute("objectID", "oe-cursor"),
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData(string.Empty)));

			pageElement
				.Element(Ns + "Outline")
				?.Element(Ns + "OEChildren")
				?.AddFirst(cursorOe);

			SetupPage(PageId, AssignObjectIds(pageElement));

			// Act
			await WithLogger(new ConvertMarkdownCommand()).Execute();

			// Assert: UpdatePageContent was called and an HTMLBlock was inserted.
			// (In the mock the HTMLBlock is not expanded to OE elements the way a live
			// OneNote would, so the first-pass HTMLBlock is the observable artifact.)
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var htmlBlock = updated.Descendants(Ns + "HTMLBlock").FirstOrDefault();
			Assert.IsNotNull(htmlBlock, "Expected an HTMLBlock to be inserted by the converter");

			var cdata = htmlBlock.Element(Ns + "Data")?.FirstNode as XCData;
			Assert.IsNotNull(cdata, "Expected HTMLBlock/Data to contain CDATA");
			StringAssert.Contains(cdata.Value, "<html>", "Expected HTML wrapping inside CDATA");
		}


		[TestMethod]
		public async Task ConvertMarkdown_SelectedBlock_InsertsHtmlBlockForSelectionOnly()
		{
			// Arrange: one selected markdown paragraph (scope=Run → allContent=false) and
			// one unselected paragraph. Only the selected outline should be converted.
			var selectedOe = new XElement(Ns + "OE",
				new XAttribute("objectID", "oe-selected"),
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData("## Selected Section")));

			var unselectedOe = new XElement(Ns + "OE",
				new XAttribute("objectID", "oe-unselected"),
				new XElement(Ns + "T",
					new XCData("Unselected paragraph")));

			var pageElement = new PageBuilder(PageId, "Partial Conversion Test")
				.WithElement(selectedOe)
				.WithElement(unselectedOe)
				.BuildElement();

			SetupPage(PageId, AssignObjectIds(pageElement));

			// Act
			await WithLogger(new ConvertMarkdownCommand()).Execute();

			// Assert: an HTMLBlock was produced from the selected markdown only.
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var htmlBlock = updated.Descendants(Ns + "HTMLBlock").FirstOrDefault();
			Assert.IsNotNull(htmlBlock, "Expected an HTMLBlock for the selected block");

			var cdata = htmlBlock.Element(Ns + "Data")?.FirstNode as XCData;
			Assert.IsNotNull(cdata, "Expected CDATA inside HTMLBlock/Data");
			StringAssert.Contains(cdata.Value, "Selected",
				"Expected converted HTML to include text from the selected heading");
			Assert.IsFalse(cdata.Value.Contains("Unselected"),
				"Unselected paragraph should not appear inside the converted HTMLBlock");
		}


		[TestMethod]
		public async Task ConvertMarkdown_InlineCode_AppliesLucidaConsole()
		{
			// ConvertMarkdownCommand runs in two passes:
			//   Pass 1 – Markdig converts backtick spans to <code>; stored as HTMLBlock.
			//   Pass 2 – OneNote expands HTMLBlock to OE elements (Consolas spans);
			//            RewriteInlineCode replaces Consolas with Lucida Console 9pt.
			//
			// The mock cannot simulate OneNote's HTMLBlock→OE expansion, so Pass 2 is
			// exercised by calling RewriteInlineCode directly on the shape OneNote produces.

			// --- Pass 1: convert the markdown paragraph via Execute() ---

			var pageElement = new PageBuilder(PageId, "Inline Code Test")
				.WithParagraph("See the `inline code` example.")
				.BuildElement();

			var cursorOe = new XElement(Ns + "OE",
				new XAttribute("objectID", "oe-cursor"),
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData(string.Empty)));

			pageElement
				.Element(Ns + "Outline")
				?.Element(Ns + "OEChildren")
				?.AddFirst(cursorOe);

			SetupPage(PageId, AssignObjectIds(pageElement));

			await WithLogger(new ConvertMarkdownCommand()).Execute();

			// The first-pass HTMLBlock must contain a Consolas-styled span; the OneMore
			// Markdig extensions render backtick code directly to font-family:Consolas
			// (rather than a bare <code> element), which OneNote then expands to a
			// Consolas-styled T run when it processes the HTMLBlock.
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var firstPassCdata =
				updated.Descendants(Ns + "HTMLBlock").FirstOrDefault()
					?.Element(Ns + "Data")?.FirstNode as XCData;

			Assert.IsNotNull(firstPassCdata, "Expected an HTMLBlock after first-pass conversion");
			StringAssert.Contains(firstPassCdata.Value, "font-family:Consolas",
				"Expected the OneMore Markdig pipeline to render the backtick span as Consolas");

			// --- Pass 2: RewriteInlineCode on the OE OneNote produces from that HTML ---

			// OneNote expands <code>inline code</code> into a T run whose CDATA contains
			// a Consolas-styled span.  Build that OE directly and run RewriteInlineCode.
			var expandedOe = new XElement(Ns + "OE",
				new XAttribute("objectID", "oe-expanded"),
				new XElement(Ns + "T",
					new XCData(
						"See the <span style=\"font-family:Consolas\">inline code</span> example.")));

			var expandedPage = new Page(XElement.Parse(
				new PageBuilder("page-expanded", "Inline Code Expanded")
					.WithElement(expandedOe)
					.Build()));

			var paragraphs = expandedPage.Root
				.Elements(Ns + "Outline")
				.Descendants(Ns + "OE")
				.Where(e => e.Elements(Ns + "T").Any())
				.ToList();

			new MarkdownConverter(expandedPage).RewriteInlineCode(paragraphs);

			// Consolas must be replaced with Lucida Console 9pt; surrounding text preserved.
			// Scope to body Outline elements to avoid matching the page Title T.
			var secondPassCdata = expandedPage.Root
				.Elements(Ns + "Outline")
				.Descendants(Ns + "T")
				.Select(t => t.FirstNode as XCData)
				.First(cd => cd != null);

			StringAssert.Contains(secondPassCdata.Value, "Lucida Console",
				"Expected Consolas to be replaced with 'Lucida Console'");
			StringAssert.Contains(secondPassCdata.Value, "9.0pt",
				"Expected font-size to be set to 9.0pt");
			Assert.IsFalse(secondPassCdata.Value.Contains("Consolas"),
				"Expected Consolas to be fully removed from the span style");
			StringAssert.Contains(secondPassCdata.Value, "inline code",
				"Expected the code text to be preserved");
		}


		[TestMethod]
		public async Task ConvertMarkdown_NoSelection_InsertsNoHtmlBlock()
		{
			// Arrange: plain page with no selected T elements.  SelectionScope = None
			// → allContent=false and selectedRuns is empty → no outlines to process.
			// The command still calls one.Update(page) unconditionally (to sync page
			// state), but no HTMLBlock should be inserted.
			var xml = new PageBuilder(PageId, "No Selection Test")
				.WithParagraph("Normal text without any selection")
				.Build();

			SetupPage(PageId, xml);

			// Act
			await WithLogger(new ConvertMarkdownCommand()).Execute();

			// Assert: page was updated (sync call) but contains no HTMLBlock
			var updated = GetUpdatedPage(PageId);
			var htmlBlock = updated?.Descendants(Ns + "HTMLBlock").FirstOrDefault();
			Assert.IsNull(htmlBlock,
				"No HTMLBlock should be inserted when there is no selection");
		}
	}
}
