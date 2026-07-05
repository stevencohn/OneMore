//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Edit
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Tests.Builders;


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

		// sample markdown with a blank line between every top-level block: a heading,
		// a two-line paragraph, a list, another two-line paragraph, a one-line paragraph,
		// a table, a paragraph, a heading, and a paragraph
		private const string BlankLineSample =
@"# head1

para1a
para1b

* list1a
* list1b

para2a
para2b

para3a parar3b

| foo | bar |
| ---- | ---- |
| a | b |

para4

# head2

para5";

		// same content as BlankLineSample but with every blank line removed, to verify
		// that a list or table isn't corrupted by CommonMark's lazy-continuation rule
		// when nothing separates it from the following content
		private const string NoBlankLineSample =
@"# head1
para1a
para1b
* list1a
* list1b
para2a
para2b
para3a parar3b
| foo | bar |
| ---- | ---- |
| a | b |
para4
# head2
para5";

		// isolates a blockquote's vulnerability to the same lazy-continuation swallow bug
		// as lists/tables: a blockquote correctly terminates a preceding list, but its own
		// inner paragraph will otherwise absorb whatever plain line follows it with no
		// blank line
		private const string BlockquoteNoBlankLineSample =
@"* list1a
* list1b
> quoted line
para2";

		// isolates a footnote definition's vulnerability to the same lazy-continuation
		// swallow bug: the definition's own text will otherwise absorb whatever plain
		// line follows it with no blank line
		private const string FootnoteNoBlankLineSample =
@"para1[^1]
* list1a
* list1b
[^1]: the footnote text
para2";


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
		public async Task ConvertMarkdown_CursorOnBlankLineBetweenParagraphs_PreservesParagraphBreak()
		{
			// Arrange: paragraph1, then a blank OE with the caret parked on it (empty
			// selected T), then paragraph2. Regression test: JoinCursorContext used to
			// unconditionally remove the caret's T, leaving a wholly-blank OE with no
			// children at all; ExtractSelectedContent then dropped that childless OE
			// entirely, erasing the blank-line separator and causing paragraph1 and
			// paragraph2 to merge into a single markdown paragraph.
			var blankOe = new XElement(Ns + "OE",
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData(string.Empty)));

			var pageElement = new PageBuilder(PageId, "Blank Line Test")
				.WithParagraph("paragraph1")
				.WithElement(blankOe)
				.WithParagraph("paragraph2")
				.BuildElement();

			SetupPage(PageId, AssignObjectIds(pageElement));

			// Act
			await WithLogger(new ConvertMarkdownCommand()).Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var cdata = updated.Descendants(Ns + "HTMLBlock").FirstOrDefault()
				?.Element(Ns + "Data")?.FirstNode as XCData;
			Assert.IsNotNull(cdata, "Expected an HTMLBlock to be inserted");

			var para1 = ExtractParagraphContaining(cdata.Value, "paragraph1");
			Assert.IsFalse(para1.Contains("paragraph2"),
				"The blank line between paragraph1 and paragraph2 must be preserved, " +
				"keeping them as separate paragraphs rather than merging into one");
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


		// The four tests below exercise OneMoreDig.ConvertMarkdownToHtml directly, using
		// BlankLineSample, across all four combinations of the gfmLineBreaks and
		// singleSpacing settings. This is Pass 1 of ConvertMarkdownCommand (markdown -> HTML);
		// the HTML produced here is what OneNote's importer expands 1:1 into OE paragraphs,
		// so it's the observable proxy for "OneNote paragraph" structure that the mock
		// (see ConvertMarkdown_InlineCode_AppliesLucidaConsole) cannot otherwise simulate.

		[TestMethod]
		public void ConvertMarkdown_AllSettingsOff_MergesLinesAndOmitsBlankLines()
		{
			var html = ConvertSample(BlankLineSample, gfmLineBreaks: false, singleSpacing: false);

			Assert.AreEqual(0, CountOccurrences(html, OneMoreDig.BlankLineMarker),
				"No blank-line markers should be inserted when singleSpacing is off");

			var para1 = ExtractParagraphContaining(html, "para1a");
			StringAssert.Contains(para1, "para1b",
				"para1a/para1b have no blank line between them in the source, " +
				"so they should merge into a single OneNote paragraph");
			Assert.IsFalse(para1.Contains("<br"),
				"Without gfmLineBreaks, the line break between them should not become <br>");
		}


		[TestMethod]
		public void ConvertMarkdown_GfmLineBreaksOnly_PreservesIndividualLinesNoBlankLines()
		{
			var html = ConvertSample(BlankLineSample, gfmLineBreaks: true, singleSpacing: false);

			Assert.AreEqual(0, CountOccurrences(html, OneMoreDig.BlankLineMarker),
				"No blank-line markers should be inserted when singleSpacing is off");

			var para1 = ExtractParagraphContaining(html, "para1a");
			StringAssert.Contains(para1, "para1b");
			StringAssert.Contains(para1, "<br",
				"With gfmLineBreaks, the line break between them should become a hard <br>, " +
				"which OneNote's HTML import turns into separate paragraphs");
		}


		[TestMethod]
		public void ConvertMarkdown_SingleSpacingOnly_PreservesBlankLinesAndMergesLines()
		{
			var html = ConvertSample(BlankLineSample, gfmLineBreaks: false, singleSpacing: true);

			Assert.AreEqual(8, CountOccurrences(html, OneMoreDig.BlankLineMarker),
				"Expected a blank-line marker at each of the 8 block boundaries in the sample");

			var para1 = ExtractParagraphContaining(html, "para1a");
			StringAssert.Contains(para1, "para1b");
			Assert.IsFalse(para1.Contains("<br"),
				"Without gfmLineBreaks, the line break between them should not become <br>");

			Assert.IsTrue(HasMarkerBefore(html, "<ul"),
				"Expected a blank-line marker immediately before the list");
			Assert.IsTrue(HasMarkerAfter(html, "</ul>"),
				"Expected a blank-line marker immediately after the list");
			Assert.IsTrue(HasMarkerBefore(html, "<table"),
				"Expected a blank-line marker immediately before the table");
			Assert.IsTrue(HasMarkerAfter(html, "</table>"),
				"Expected a blank-line marker immediately after the table");
		}


		[TestMethod]
		public void ConvertMarkdown_GfmLineBreaksAndSingleSpacing_PreservesBlankLinesAndLines()
		{
			var html = ConvertSample(BlankLineSample, gfmLineBreaks: true, singleSpacing: true);

			Assert.AreEqual(8, CountOccurrences(html, OneMoreDig.BlankLineMarker),
				"Expected a blank-line marker at each of the 8 block boundaries in the sample");

			var para1 = ExtractParagraphContaining(html, "para1a");
			StringAssert.Contains(para1, "para1b");
			StringAssert.Contains(para1, "<br",
				"With gfmLineBreaks, the line break between them should become a hard <br>, " +
				"which OneNote's HTML import turns into separate paragraphs");

			Assert.IsTrue(HasMarkerBefore(html, "<ul"),
				"Expected a blank-line marker immediately before the list");
			Assert.IsTrue(HasMarkerAfter(html, "</ul>"),
				"Expected a blank-line marker immediately after the list");
			Assert.IsTrue(HasMarkerBefore(html, "<table"),
				"Expected a blank-line marker immediately before the table");
			Assert.IsTrue(HasMarkerAfter(html, "</table>"),
				"Expected a blank-line marker immediately after the table");
		}


		// The four tests below repeat the same settings matrix against NoBlankLineSample,
		// which has no blank lines anywhere. None of them should ever see a blank-line
		// marker (nothing was blank in the source), and — the real risk with no blank
		// lines around it — the list and table must not be swallowed by CommonMark's
		// lazy-continuation rule into whatever plain-text line follows them.

		[TestMethod]
		public void ConvertMarkdown_NoBlankLines_AllSettingsOff_SeparatesListAndTable()
		{
			var html = ConvertSample(NoBlankLineSample, gfmLineBreaks: false, singleSpacing: false);

			Assert.AreEqual(0, CountOccurrences(html, OneMoreDig.BlankLineMarker),
				"No blank lines exist in the source, so no marker should ever appear");

			var listItem = ExtractListItemContaining(html, "list1b");
			Assert.IsFalse(listItem.Contains("para2a"),
				"The list must not absorb the paragraph that follows it with no blank line");

			var para2 = ExtractParagraphContaining(html, "para2a");
			StringAssert.Contains(para2, "para2b");
			StringAssert.Contains(para2, "para3a parar3b",
				"Consecutive plain paragraph lines with no blank line should still merge");
			Assert.IsFalse(para2.Contains("<br"));

			StringAssert.Contains(html, "<table",
				"The table must be recognized as a table, not swallowed as literal text");
			Assert.IsFalse(html.Contains("| foo | bar |"),
				"Raw pipe-table syntax should not leak through as literal text");
		}


		[TestMethod]
		public void ConvertMarkdown_NoBlankLines_GfmLineBreaksOnly_SeparatesListAndTable()
		{
			var html = ConvertSample(NoBlankLineSample, gfmLineBreaks: true, singleSpacing: false);

			Assert.AreEqual(0, CountOccurrences(html, OneMoreDig.BlankLineMarker),
				"No blank lines exist in the source, so no marker should ever appear");

			var listItem = ExtractListItemContaining(html, "list1b");
			Assert.IsFalse(listItem.Contains("para2a"),
				"The list must not absorb the paragraph that follows it with no blank line");

			var para2 = ExtractParagraphContaining(html, "para2a");
			StringAssert.Contains(para2, "para2b");
			StringAssert.Contains(para2, "para3a parar3b");
			StringAssert.Contains(para2, "<br",
				"With gfmLineBreaks, the lines merge into one paragraph joined by hard breaks");

			StringAssert.Contains(html, "<table",
				"The table must be recognized as a table, not swallowed as literal text");
			Assert.IsFalse(html.Contains("| foo | bar |"),
				"Raw pipe-table syntax should not leak through as literal text");
		}


		[TestMethod]
		public void ConvertMarkdown_NoBlankLines_SingleSpacingOnly_SeparatesListAndTable()
		{
			var html = ConvertSample(NoBlankLineSample, gfmLineBreaks: false, singleSpacing: true);

			Assert.AreEqual(0, CountOccurrences(html, OneMoreDig.BlankLineMarker),
				"No blank lines exist in the source, so singleSpacing must not synthesize one " +
				"just because a separator was needed internally to keep the list/table from " +
				"absorbing the following content");

			var listItem = ExtractListItemContaining(html, "list1b");
			Assert.IsFalse(listItem.Contains("para2a"),
				"The list must not absorb the paragraph that follows it with no blank line");

			var para2 = ExtractParagraphContaining(html, "para2a");
			StringAssert.Contains(para2, "para2b");
			StringAssert.Contains(para2, "para3a parar3b");
			Assert.IsFalse(para2.Contains("<br"));

			StringAssert.Contains(html, "<table",
				"The table must be recognized as a table, not swallowed as literal text");
			Assert.IsFalse(html.Contains("| foo | bar |"),
				"Raw pipe-table syntax should not leak through as literal text");
		}


		[TestMethod]
		public void ConvertMarkdown_NoBlankLines_GfmLineBreaksAndSingleSpacing_SeparatesListAndTable()
		{
			var html = ConvertSample(NoBlankLineSample, gfmLineBreaks: true, singleSpacing: true);

			Assert.AreEqual(0, CountOccurrences(html, OneMoreDig.BlankLineMarker),
				"No blank lines exist in the source, so singleSpacing must not synthesize one " +
				"just because a separator was needed internally to keep the list/table from " +
				"absorbing the following content");

			var listItem = ExtractListItemContaining(html, "list1b");
			Assert.IsFalse(listItem.Contains("para2a"),
				"The list must not absorb the paragraph that follows it with no blank line");

			var para2 = ExtractParagraphContaining(html, "para2a");
			StringAssert.Contains(para2, "para2b");
			StringAssert.Contains(para2, "para3a parar3b");
			StringAssert.Contains(para2, "<br",
				"With gfmLineBreaks, the lines merge into one paragraph joined by hard breaks");

			StringAssert.Contains(html, "<table",
				"The table must be recognized as a table, not swallowed as literal text");
			Assert.IsFalse(html.Contains("| foo | bar |"),
				"Raw pipe-table syntax should not leak through as literal text");
		}


		// A blockquote correctly terminates a preceding list on its own, but its inner
		// paragraph is just as vulnerable as a list/table to swallowing whatever plain
		// line follows it with no blank line. These two tests cover the no-swallow
		// behavior and the singleSpacing no-leak behavior; gfmLineBreaks doesn't affect
		// block-level swallowing so it isn't varied here.

		[TestMethod]
		public void ConvertMarkdown_NoBlankLines_Blockquote_DoesNotAbsorbFollowingParagraph()
		{
			var html = ConvertSample(BlockquoteNoBlankLineSample, gfmLineBreaks: false, singleSpacing: false);

			Assert.AreEqual(0, CountOccurrences(html, OneMoreDig.BlankLineMarker));

			var quote = Regex.Match(html, "<blockquote>(.*?)</blockquote>", RegexOptions.Singleline);
			Assert.IsTrue(quote.Success, "Expected a <blockquote> element");
			Assert.IsFalse(quote.Groups[1].Value.Contains("para2"),
				"The blockquote must not absorb the paragraph that follows it with no blank line");

			StringAssert.Contains(html, "para2");
		}


		[TestMethod]
		public void ConvertMarkdown_NoBlankLines_Blockquote_SingleSpacingDoesNotLeakMarker()
		{
			var html = ConvertSample(BlockquoteNoBlankLineSample, gfmLineBreaks: false, singleSpacing: true);

			Assert.AreEqual(0, CountOccurrences(html, OneMoreDig.BlankLineMarker),
				"No blank lines exist in the source, so singleSpacing must not synthesize one " +
				"just because a separator was needed internally to keep the blockquote from " +
				"absorbing the following content");

			var quote = Regex.Match(html, "<blockquote>(.*?)</blockquote>", RegexOptions.Singleline);
			Assert.IsTrue(quote.Success, "Expected a <blockquote> element");
			Assert.IsFalse(quote.Groups[1].Value.Contains("para2"),
				"The blockquote must not absorb the paragraph that follows it with no blank line");
		}


		// A footnote definition is vulnerable the same way: its own text will absorb
		// whatever plain line follows it with no blank line.

		[TestMethod]
		public void ConvertMarkdown_NoBlankLines_FootnoteDefinition_DoesNotAbsorbFollowingParagraph()
		{
			var html = ConvertSample(FootnoteNoBlankLineSample, gfmLineBreaks: false, singleSpacing: false);

			Assert.AreEqual(0, CountOccurrences(html, OneMoreDig.BlankLineMarker));

			var footnote = ExtractListItemContaining(html, "the footnote text");
			Assert.IsFalse(footnote.Contains("para2"),
				"The footnote definition must not absorb the paragraph that follows it with no blank line");

			StringAssert.Contains(html, "para2");
		}


		[TestMethod]
		public void ConvertMarkdown_NoBlankLines_FootnoteDefinition_SingleSpacingDoesNotLeakMarker()
		{
			var html = ConvertSample(FootnoteNoBlankLineSample, gfmLineBreaks: false, singleSpacing: true);

			Assert.AreEqual(0, CountOccurrences(html, OneMoreDig.BlankLineMarker),
				"No blank lines exist in the source, so singleSpacing must not synthesize one " +
				"just because a separator was needed internally to keep the footnote definition " +
				"from absorbing the following content");

			var footnote = ExtractListItemContaining(html, "the footnote text");
			Assert.IsFalse(footnote.Contains("para2"),
				"The footnote definition must not absorb the paragraph that follows it with no blank line");
		}


		// The tests below exercise MarkdownConverter.RewriteTodo directly against
		// hand-built OE shapes, since the mock cannot simulate OneNote's live
		// HTMLBlock->OE expansion (see ConvertMarkdown_InlineCode_AppliesLucidaConsole).
		// They lock in the swapped treatment: "- [ ] foo" (a real GFM task-list item,
		// i.e. an OE with a List element) becomes a Todo-tagged paragraph, while a bare
		// "[ ] foo" paragraph (no List) is left as literal text.

		private static XElement BuildListOe(string objectId, string text)
		{
			return new XElement(Ns + "OE",
				new XAttribute("objectID", objectId),
				new XElement(Ns + "List",
					new XElement(Ns + "Bullet", new XAttribute("bullet", "2"))),
				new XElement(Ns + "T", new XCData(text)));
		}


		// PageBuilder round-trips content through ToString()/XElement.Parse() (matching
		// how a live OneNote page is actually loaded), which produces new XElement
		// instances distinct from the ones passed into WithElement — so assertions
		// below look elements back up by objectID rather than by reference.

		private static XElement FindOe(Page page, string objectId)
		{
			return page.Root.Descendants(Ns + "OE")
				.Single(e => e.Attribute("objectID")?.Value == objectId);
		}


		// RewriteTodo strips objectID from converted paragraphs (so OneNote treats them
		// as brand-new content rather than an edit to a paragraph it may still
		// internally associate with list membership) — so tests that need to inspect a
		// converted element must capture its reference *before* calling RewriteTodo,
		// rather than looking it up afterward by objectID.

		[TestMethod]
		public void RewriteTodo_TaskListItem_Unchecked_ConvertsToTodoTagAndDropsBullet()
		{
			var page = new Page(XElement.Parse(
				new PageBuilder("page-todo-1", "Todo Test")
					.WithElement(BuildListOe("oe-1", "[ ] foo"))
					.Build()));

			var paragraphs = page.Root.Descendants(Ns + "OE").ToList();
			var oe = paragraphs.Single(e => e.Attribute("objectID")?.Value == "oe-1");

			new MarkdownConverter(page).RewriteTodo(paragraphs);

			Assert.IsFalse(oe.Elements(Ns + "List").Any(),
				"Expected the bullet (List) to be removed");
			Assert.IsNull(oe.Attribute("objectID"),
				"Expected objectID to be stripped so OneNote treats this as new content");

			var tag = oe.Element(Ns + "Tag");
			Assert.IsNotNull(tag, "Expected a Tag element to be added");
			Assert.AreEqual("false", tag.Attribute("completed")?.Value);

			var cdata = oe.Element(Ns + "T").FirstNode as XCData;
			Assert.AreEqual(" foo", cdata.Value);

			// no preceding paragraph existed, so an anchor must have been inserted
			// and the item nested one level under it
			var topChildren = page.Root.Element(Ns + "Outline").Element(Ns + "OEChildren");
			var anchor = topChildren.Elements(Ns + "OE").Single();
			Assert.AreNotSame(oe, anchor, "Expected a new anchor OE, distinct from the item");

			var nested = anchor.Element(Ns + "OEChildren")?.Elements(Ns + "OE").SingleOrDefault();
			Assert.AreSame(oe, nested, "Expected the item nested one level under the anchor");
		}


		[TestMethod]
		public void RewriteTodo_TaskListItem_Checked_MarksTagCompleted()
		{
			var page = new Page(XElement.Parse(
				new PageBuilder("page-todo-2", "Todo Test")
					.WithElement(BuildListOe("oe-1", "[x] foo"))
					.Build()));

			var paragraphs = page.Root.Descendants(Ns + "OE").ToList();
			var oe = paragraphs.Single(e => e.Attribute("objectID")?.Value == "oe-1");

			new MarkdownConverter(page).RewriteTodo(paragraphs);

			var tag = oe.Element(Ns + "Tag");
			Assert.IsNotNull(tag);
			Assert.AreEqual("true", tag.Attribute("completed")?.Value);
		}


		[TestMethod]
		public void RewriteTodo_ConsecutiveTaskListItems_GroupUnderOneSharedAnchor()
		{
			var page = new Page(XElement.Parse(
				new PageBuilder("page-todo-3", "Todo Test")
					.WithElement(BuildListOe("oe-1", "[ ] one"))
					.WithElement(BuildListOe("oe-2", "[x] two"))
					.WithElement(BuildListOe("oe-3", "[ ] three"))
					.Build()));

			var paragraphs = page.Root.Descendants(Ns + "OE").ToList();
			var items = new[] { "oe-1", "oe-2", "oe-3" }
				.Select(id => paragraphs.Single(e => e.Attribute("objectID")?.Value == id))
				.ToList();

			new MarkdownConverter(page).RewriteTodo(paragraphs);

			var topChildren = page.Root.Element(Ns + "Outline").Element(Ns + "OEChildren");
			var anchor = topChildren.Elements(Ns + "OE").Single();

			var nested = anchor.Element(Ns + "OEChildren")?.Elements(Ns + "OE").ToList();
			Assert.IsNotNull(nested);
			CollectionAssert.AreEqual(items, nested,
				"Expected all three items as siblings under one shared anchor, not staircased");
		}


		[TestMethod]
		public void RewriteTodo_AlreadyNestedTaskListItem_NotIndentedFurther()
		{
			// simulates a genuine markdown sub-list: the task item is already nested
			// one level inside a parent OE's OEChildren before RewriteTodo runs
			var nestedOe = BuildListOe("oe-child", "[x] nested");
			var parentOe = new XElement(Ns + "OE",
				new XAttribute("objectID", "oe-parent"),
				new XElement(Ns + "T", new XCData("parent item")),
				new XElement(Ns + "OEChildren", nestedOe));

			var page = new Page(XElement.Parse(
				new PageBuilder("page-todo-6", "Todo Test")
					.WithElement(parentOe)
					.Build()));

			var paragraphs = page.Root.Descendants(Ns + "OE").ToList();
			var oe = paragraphs.Single(e => e.Attribute("objectID")?.Value == "oe-child");
			var parent = paragraphs.Single(e => e.Attribute("objectID")?.Value == "oe-parent");

			new MarkdownConverter(page).RewriteTodo(paragraphs);

			Assert.IsFalse(oe.Elements(Ns + "List").Any());
			Assert.IsNotNull(oe.Element(Ns + "Tag"));

			// still nested exactly one level under the same original parent, not
			// wrapped in an additional, newly-created anchor
			Assert.AreEqual(1, oe.Ancestors(Ns + "OE").Count());
			Assert.AreSame(parent, oe.Ancestors(Ns + "OE").First());
			CollectionAssert.Contains(
				parent.Element(Ns + "OEChildren").Elements(Ns + "OE").ToList(), oe);
		}


		[TestMethod]
		public void RewriteTodo_BareBracketParagraph_LeftAsLiteralText()
		{
			var page = new Page(XElement.Parse(
				new PageBuilder("page-todo-4", "Todo Test")
					.WithElement(new XElement(Ns + "OE",
						new XAttribute("objectID", "oe-1"),
						new XElement(Ns + "T", new XCData("[ ] foo"))))
					.Build()));

			new MarkdownConverter(page).RewriteTodo(page.Root.Descendants(Ns + "OE").ToList());

			var oe = FindOe(page, "oe-1");

			Assert.IsNull(oe.Element(Ns + "Tag"),
				"A bare paragraph outside a list must not get a Todo tag");

			var cdata = oe.Element(Ns + "T").FirstNode as XCData;
			Assert.AreEqual("[ ] foo", cdata.Value,
				"A bare paragraph outside a list must be left as literal text");
		}


		[TestMethod]
		public void RewriteTodo_SpanWrappedListItemText_StillDetected()
		{
			var page = new Page(XElement.Parse(
				new PageBuilder("page-todo-5", "Todo Test")
					.WithElement(new XElement(Ns + "OE",
						new XAttribute("objectID", "oe-1"),
						new XElement(Ns + "List",
							new XElement(Ns + "Bullet", new XAttribute("bullet", "2"))),
						new XElement(Ns + "T",
							new XCData("<span style=\"font-family:Calibri\">[x] foo</span>"))))
					.Build()));

			var paragraphs = page.Root.Descendants(Ns + "OE").ToList();
			var oe = paragraphs.Single(e => e.Attribute("objectID")?.Value == "oe-1");

			new MarkdownConverter(page).RewriteTodo(paragraphs);

			Assert.IsFalse(oe.Elements(Ns + "List").Any());

			var tag = oe.Element(Ns + "Tag");
			Assert.IsNotNull(tag, "Expected span-wrapped list-item text to still be detected");
			Assert.AreEqual("true", tag.Attribute("completed")?.Value);

			var cdata = oe.Element(Ns + "T").FirstNode as XCData;
			StringAssert.Contains(cdata.Value, "foo");
			Assert.IsFalse(cdata.Value.Contains("[x]"));
		}


		private static string ConvertSample(string sample, bool gfmLineBreaks, bool singleSpacing)
		{
			var filepath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			return OneMoreDig.ConvertMarkdownToHtml(
				filepath, sample, gfmLineBreaks, singleSpacing,
				blankBeforeHeadings: false);
		}


		private static int CountOccurrences(string text, string value)
		{
			var count = 0;
			var index = 0;
			while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) != -1)
			{
				count++;
				index += value.Length;
			}

			return count;
		}


		private static string ExtractParagraphContaining(string html, string needle)
		{
			foreach (Match match in Regex.Matches(html, "<p[^>]*>(.*?)</p>", RegexOptions.Singleline))
			{
				if (match.Groups[1].Value.Contains(needle))
				{
					return match.Groups[1].Value;
				}
			}

			Assert.Fail($"No <p> element found containing '{needle}'");
			return null;
		}


		private static string ExtractListItemContaining(string html, string needle)
		{
			foreach (Match match in Regex.Matches(html, "<li[^>]*>(.*?)</li>", RegexOptions.Singleline))
			{
				if (match.Groups[1].Value.Contains(needle))
				{
					return match.Groups[1].Value;
				}
			}

			Assert.Fail($"No <li> element found containing '{needle}'");
			return null;
		}


		// the marker writes its own trailing newline (so OneNote's HTML importer doesn't
		// treat an immediately-following block, e.g. a list, as nested inside it), so
		// tolerate whitespace between the marker and the block it precedes
		private static bool HasMarkerBefore(string html, string tag)
		{
			return Regex.IsMatch(html,
				Regex.Escape($"<p>{OneMoreDig.BlankLineMarker}</p>") + @"\s*" + Regex.Escape(tag));
		}


		// the renderer appends its own trailing whitespace after a closing tag before the
		// next marker is written, so tolerate whitespace between them
		private static bool HasMarkerAfter(string html, string closingTag)
		{
			return Regex.IsMatch(html,
				Regex.Escape(closingTag) + @"\s*" + Regex.Escape($"<p>{OneMoreDig.BlankLineMarker}</p>"));
		}
	}
}
