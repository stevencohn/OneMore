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

	/*
	 * Test Protocol
	 * Commands/Edit/JoinParagraphCommand
	 * Join multiple lines of text into a single running paragraph, removing line breaks.
	 *
	 *  1. Select multiple paragraphs
	 *  2. Edit/Join Paragraph
	 *  3. Confirm the paragraphs have been joined consistently and styles are preserved
	 *  4. Move caret to one of the Soft Break lines, without selecting a range (zero-width range)
	 *  5. Edit/Join Paragraph
	 *  6. Confirm that the lines are joined into a single paragraph and styles are preserved
	 *  7. Move caret to the one of the list items and select at least two items
	 *  8. Edit/Join Paragraph
	 *  9. Confirm that the items are joined to the first item selected
	 *
	 * Hard Breaks
	 * Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt
	 * ut labore et dolore magna aliqua.
	 *
	 * Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea
	 * commodo consequat.
	 *
	 * Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat
	 * nulla pariatur.
	 *
	 * Soft Breaks
	 * Lorem ipsum
	 * dolor sit
	 * amet
	 *
	 * Lists
	 *     - One
	 *     - Two
	 *     - Three
	 *     - Four
	 */

	// Note: when JoinParagraphCommand finds no selection at all, it calls ShowInfo(), which pops
	// a real modal MoreMessageBox. That path is intentionally not exercised here since it would
	// hang an automated test run.

	[TestClass]
	public class JoinParagraphCommandTests : TestBase
	{
		private const string PageId = "page-1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		private static XElement BuildListItem(string text)
		{
			return new XElement(Ns + "OE",
				new XElement(Ns + "List",
					new XElement(Ns + "Bullet", new XAttribute("bullet", "2"))),
				new XElement(Ns + "T", new XCData(text)));
		}


		[TestMethod]
		public async Task JoinParagraph_MultipleSelectedParagraphs_JoinsIntoSingleParagraphPreservingStyle()
		{
			// Arrange: three separate paragraphs (hard breaks), all selected, none of
			// which carry a paragraph-level style of their own.
			var oe1 = new XElement(Ns + "OE",
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData("Paragraph one.")));

			var oe2 = new XElement(Ns + "OE",
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData("Paragraph two.")));

			var oe3 = new XElement(Ns + "OE",
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData("Paragraph three.")));

			var xml = new PageBuilder(PageId, "Join Test")
				.WithElement(oe1)
				.WithElement(oe2)
				.WithElement(oe3)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new JoinParagraphCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var oes = updated.Element(Ns + "Outline")
				.Descendants(Ns + "OE")
				.Where(e => e.Elements(Ns + "T").Any())
				.ToList();

			Assert.AreEqual(1, oes.Count,
				"Expected the three paragraphs to collapse into a single OE");

			var joined = string.Concat(oes[0].Elements(Ns + "T").Select(t => t.GetCData().Value));
			Assert.AreEqual("Paragraph one. Paragraph two. Paragraph three.", joined);

			Assert.IsFalse(joined.Contains("<span"),
				"None of the source paragraphs had a distinct style, so nothing needed wrapping");
		}


		[TestMethod]
		public async Task JoinParagraph_DifferingParagraphStyles_WrapsMergedInTextInASpan()
		{
			// Arrange: two paragraphs share the same quickStyle (e.g. both are SQL
			// code lines) but carry different "style" attributes -- reproduces a
			// real page where a syntax-highlighted continuation line lacked the
			// first line's "color" declaration. The join must not let the second
			// line silently inherit the first line's style.
			var oe1 = new XElement(Ns + "OE",
				new XAttribute("quickStyleIndex", "1"),
				new XAttribute("style", "font-family:'Lucida Console';font-size:10.0pt;color:#2E75B5"),
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData("SELECT")));

			var oe2 = new XElement(Ns + "OE",
				new XAttribute("quickStyleIndex", "1"),
				new XAttribute("style", "font-family:'Lucida Console';font-size:10.0pt"),
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData("GETDATE() AS [Today],")));

			var xml = new PageBuilder(PageId, "Differing Style Join Test")
				.WithElement(oe1)
				.WithElement(oe2)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new JoinParagraphCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var oes = updated.Element(Ns + "Outline")
				.Descendants(Ns + "OE")
				.Where(e => e.Elements(Ns + "T").Any())
				.ToList();

			Assert.AreEqual(1, oes.Count, "Expected the two lines to join despite differing style");

			var joined = string.Concat(oes[0].Elements(Ns + "T").Select(t => t.GetCData().Value));
			Assert.AreEqual(
				"SELECT <span style=\"font-family:'Lucida Console';font-size:10.0pt\">" +
				"GETDATE() AS [Today],</span>",
				joined,
				"Expected the second line's own style to be preserved via a span, " +
				"rather than blending into the first line's style");
		}


		[TestMethod]
		public async Task JoinParagraph_CursorAtEndOfLine_RealWorldSqlSample_JoinsCorrectly()
		{
			// Arrange: reproduces a real page exactly -- cursor (bare, zero-width)
			// parked at the very end of "SELECT", nothing after it in that OE, and
			// the next paragraph's indentation is nbsp entities wrapped in their
			// own <span> for coloring (syntax highlighting). This exercises three
			// things together: leading indentation hidden inside a span, a
			// boundary that sits on the far side of the caret (nothing between the
			// caret and the next paragraph in its own OE), and differing styles.
			var oe1 = new XElement(Ns + "OE",
				new XAttribute("alignment", "left"),
				new XAttribute("quickStyleIndex", "1"),
				new XAttribute("style",
					"font-family:'Lucida Console';font-size:10.0pt;color:#2E75B5"),
				new XElement(Ns + "T", new XCData("SELECT")),
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData(string.Empty)));

			var oe2 = new XElement(Ns + "OE",
				new XAttribute("alignment", "left"),
				new XAttribute("quickStyleIndex", "1"),
				new XAttribute("style", "font-family:'Lucida Console';font-size:10.0pt"),
				new XElement(Ns + "T", new XCData(
					"<span style='color:black'>&nbsp;&nbsp;&nbsp;&nbsp;</span>" +
					"<span style='color:#CC00FF'>GETDATE</span>" +
					"<span style='color:black'>() </span>" +
					"<span style='color:#2E75B5'>AS</span>" +
					"<span style='color:black'> [Today],</span>")));

			var xml = new PageBuilder(PageId, "Real World Sql Test")
				.WithElement(oe1)
				.WithElement(oe2)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new JoinParagraphCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var oes = updated.Element(Ns + "Outline")
				.Descendants(Ns + "OE")
				.Where(e => e.Elements(Ns + "T").Any())
				.ToList();

			Assert.AreEqual(1, oes.Count, "Expected the two lines to join into a single OE");

			var joined = string.Concat(oes[0].Elements(Ns + "T").Select(t => t.GetCData().Value));
			Assert.AreEqual(
				"SELECT <span style=\"font-family:'Lucida Console';font-size:10.0pt\">" +
				"<span style='color:#CC00FF'>GETDATE</span>" +
				"<span style='color:black'>() </span>" +
				"<span style='color:#2E75B5'>AS</span>" +
				"<span style='color:black'> [Today],</span></span>",
				joined,
				"Expected: a single separating space after SELECT (even though the " +
				"caret sits between it and the paragraph boundary), the span-wrapped " +
				"nbsp indentation fully stripped, and the second line's own style " +
				"preserved via an outer span");
		}


		[TestMethod]
		public async Task JoinParagraph_CaretAtSoftBreak_JoinsLinesIntoSingleParagraph()
		{
			// Arrange: one paragraph already containing soft breaks (<br>), with the caret
			// (a zero-width empty selected T) parked between the first and second lines --
			// simulating "move caret to one of the Soft Break lines" with nothing selected.
			var oe = new XElement(Ns + "OE",
				new XElement(Ns + "T", new XCData("Lorem ipsum<br>\n")),
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData(string.Empty)),
				new XElement(Ns + "T", new XCData("dolor sit<br>\namet")));

			var xml = new PageBuilder(PageId, "Soft Break Test")
				.WithElement(oe)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new JoinParagraphCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var oes = updated.Element(Ns + "Outline")
				.Descendants(Ns + "OE")
				.Where(e => e.Elements(Ns + "T").Any())
				.ToList();

			Assert.AreEqual(1, oes.Count,
				"Expected the soft-break lines to remain/collapse into a single paragraph");

			var joined = string.Concat(oes[0].Elements(Ns + "T").Select(t => t.GetCData().Value));
			Assert.IsFalse(joined.Contains("<br>"),
				"Soft breaks must be removed once the paragraph is joined");
			Assert.AreEqual("Lorem ipsum dolor sit amet", joined,
				"Expected soft breaks to collapse to exactly one space each, no more, no less");
		}


		[TestMethod]
		public async Task JoinParagraph_CursorSplitMidWord_PreservesAdjacentSpace()
		{
			// Arrange: no soft breaks at all -- just a plain one-line paragraph where
			// OneNote split the run around a bare cursor placed right after the space
			// in "Lorem ipsum". The space touching the caret must survive the join,
			// not just the words on either side of it.
			var oe = new XElement(Ns + "OE",
				new XElement(Ns + "T", new XCData("Lorem ipsum ")),
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData(string.Empty)),
				new XElement(Ns + "T", new XCData("dolor sit amet")));

			var xml = new PageBuilder(PageId, "Cursor Split Test")
				.WithElement(oe)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new JoinParagraphCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var oes = updated.Element(Ns + "Outline")
				.Descendants(Ns + "OE")
				.Where(e => e.Elements(Ns + "T").Any())
				.ToList();

			Assert.AreEqual(1, oes.Count);

			var joined = string.Concat(oes[0].Elements(Ns + "T").Select(t => t.GetCData().Value));
			Assert.AreEqual("Lorem ipsum dolor sit amet", joined,
				"Expected the space adjacent to the removed caret to be preserved");
		}


		[TestMethod]
		public async Task JoinParagraph_HardBreakIndentation_CollapsesToSingleSpace()
		{
			// Arrange: a SQL-style sample as three separate hard-break paragraphs,
			// where the continuation lines carry leading indentation. Joining must
			// not pile that indentation up into a run of several spaces.
			var middle = BuildCursorParagraph("    FROM ", "foo");

			var xml = new PageBuilder(PageId, "SQL Indent Test")
				.WithParagraph("SELECT *")
				.WithElement(middle)
				.WithParagraph("    WHERE x = 1")
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new JoinParagraphCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var oes = updated.Element(Ns + "Outline")
				.Descendants(Ns + "OE")
				.Where(e => e.Elements(Ns + "T").Any())
				.ToList();

			Assert.AreEqual(1, oes.Count);

			var joined = string.Concat(oes[0].Elements(Ns + "T").Select(t => t.GetCData().Value));
			Assert.AreEqual("SELECT * FROM foo WHERE x = 1", joined,
				"Expected each continuation line's indentation to collapse to a single space");
		}


		[TestMethod]
		public async Task JoinParagraph_HardBreakIndentation_EncodedAsNumericNbsp_CollapsesToSingleSpace()
		{
			// Arrange: OneNote sometimes encodes preserved/pasted indentation as
			// "&#160;" (numeric non-breaking-space entity) rather than plain
			// ASCII spaces, not just for a single space but for a whole
			// indentation run -- confirm those collapse too.
			var oe1 = new XElement(Ns + "OE",
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData("SELECT")));

			var oe2 = new XElement(Ns + "OE",
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData("&#160;&#160;&#160;&#160;GETDATE() AS [Today],")));

			var xml = new PageBuilder(PageId, "Numeric Nbsp Indent Test")
				.WithElement(oe1)
				.WithElement(oe2)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new JoinParagraphCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var oes = updated.Element(Ns + "Outline")
				.Descendants(Ns + "OE")
				.Where(e => e.Elements(Ns + "T").Any())
				.ToList();

			Assert.AreEqual(1, oes.Count);

			var joined = string.Concat(oes[0].Elements(Ns + "T").Select(t => t.GetCData().Value));
			Assert.AreEqual("SELECT GETDATE() AS [Today],", joined,
				"Expected the &#160;-encoded indentation to collapse to a single space, " +
				"not survive untouched alongside the boundary separator");
		}


		[TestMethod]
		public async Task JoinParagraph_HardBreakIndentation_EncodedAsNamedNbsp_CollapsesToSingleSpace()
		{
			// Arrange: real-world repro -- pasted indentation encoded with the
			// named "&nbsp;" entity (not the numeric "&#160;" form), e.g. a
			// SQL sample pasted as:
			//   SELECT
			//       GETDATE() AS [Today],
			// which must not survive as literal "&nbsp;" runs once joined.
			var oe1 = new XElement(Ns + "OE",
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData("SELECT")));

			var oe2 = new XElement(Ns + "OE",
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData("&nbsp;&nbsp;&nbsp;&nbsp;GETDATE() AS [Today],")));

			var xml = new PageBuilder(PageId, "Named Nbsp Indent Test")
				.WithElement(oe1)
				.WithElement(oe2)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new JoinParagraphCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var oes = updated.Element(Ns + "Outline")
				.Descendants(Ns + "OE")
				.Where(e => e.Elements(Ns + "T").Any())
				.ToList();

			Assert.AreEqual(1, oes.Count);

			var joined = string.Concat(oes[0].Elements(Ns + "T").Select(t => t.GetCData().Value));
			Assert.AreEqual("SELECT GETDATE() AS [Today],", joined,
				"Expected the &nbsp;-encoded indentation to collapse to a single space, " +
				"not survive untouched alongside the boundary separator");
		}


		[TestMethod]
		public async Task JoinParagraph_SoftBreakIndentation_CollapsesToSingleSpace()
		{
			// Arrange: the same SQL sample, but as soft breaks within one selected
			// run (the more common shape for a pasted multi-line snippet).
			var oe = new XElement(Ns + "OE",
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData("SELECT *<br>\n    FROM foo<br>\n    WHERE x = 1")));

			var xml = new PageBuilder(PageId, "SQL Soft Break Test")
				.WithElement(oe)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new JoinParagraphCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var oes = updated.Element(Ns + "Outline")
				.Descendants(Ns + "OE")
				.Where(e => e.Elements(Ns + "T").Any())
				.ToList();

			Assert.AreEqual(1, oes.Count);

			var joined = string.Concat(oes[0].Elements(Ns + "T").Select(t => t.GetCData().Value));
			Assert.AreEqual("SELECT * FROM foo WHERE x = 1", joined,
				"Expected each soft-break continuation's indentation to collapse to a single space");
		}


		[TestMethod]
		public async Task JoinParagraph_CursorPosition_IsPreservedAcrossJoinedBlock()
		{
			// Arrange: same shape as the block-join test, but here we confirm the
			// *cursor itself* -- not just the text -- lands back where the user
			// left it, giving continuity of context after the join.
			var middle = BuildCursorParagraph("Paragraph ", "two.");

			var xml = new PageBuilder(PageId, "Cursor Position Test")
				.WithParagraph("Paragraph one.")
				.WithElement(middle)
				.WithParagraph("Paragraph three.")
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new JoinParagraphCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var runs = updated.Element(Ns + "Outline")
				.Descendants(Ns + "OE")
				.Elements(Ns + "T")
				.ToList();

			var selected = runs.Where(t => t.Attribute("selected")?.Value == "all").ToList();
			Assert.AreEqual(1, selected.Count, "Expected exactly one T to remain marked as the cursor");

			var caretIndex = runs.IndexOf(selected[0]);

			Assert.AreEqual(string.Empty, runs[caretIndex].GetCData().Value,
				"The restored cursor position must still be a zero-width caret");

			var before = string.Concat(runs.Take(caretIndex).Select(t => t.GetCData().Value));
			var after = string.Concat(runs.Skip(caretIndex + 1).Select(t => t.GetCData().Value));

			Assert.AreEqual("Paragraph one. Paragraph ", before,
				"Expected the cursor to stay right after 'Paragraph ' in the joined text");
			Assert.AreEqual("two. Paragraph three.", after,
				"Expected the cursor to stay right before 'two.' in the joined text");
		}


		[TestMethod]
		public async Task JoinParagraph_MultipleSelectedListItems_JoinsIntoFirstSelectedItem()
		{
			// Arrange: four list items; select the middle two ("Two" and "Three"), leaving
			// "One" and "Four" untouched.
			var one = BuildListItem("One");
			var two = BuildListItem("Two");
			var three = BuildListItem("Three");
			var four = BuildListItem("Four");

			two.Element(Ns + "T").SetAttributeValue("selected", "all");
			three.Element(Ns + "T").SetAttributeValue("selected", "all");

			var xml = new PageBuilder(PageId, "List Join Test")
				.WithElement(one)
				.WithElement(two)
				.WithElement(three)
				.WithElement(four)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new JoinParagraphCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var items = updated.Element(Ns + "Outline")
				.Descendants(Ns + "OE")
				.Where(e => e.Elements(Ns + "T").Any())
				.ToList();

			// "Three" is folded into "Two" (the first selected item), leaving three list items
			Assert.AreEqual(3, items.Count,
				"Expected One, Two(+Three), and Four to remain as three list items");

			// NOTE: JoinParagraphCommand.Cleanup() removes every empty Bullet/List/OE across
			// the *entire* page.Root, not just the ones orphaned by this join (Bullet elements
			// are always leaf nodes, so "!e.HasElements" matches all of them unconditionally).
			// The practical effect: every list item on the page -- including ones never
			// selected, like "One" and "Four" here -- loses its List/Bullet markup and
			// becomes a plain paragraph. This looks like an existing bug in the command
			// rather than intended behavior, but this test documents current behavior as
			// observed rather than the (arguably more correct) scoped-cleanup behavior.
			Assert.IsFalse(items.Any(e => e.Descendants(Ns + "Bullet").Any()),
				"Documents current behavior: JoinParagraphCommand.Cleanup() strips Bullet/List " +
				"markup page-wide, even from list items outside the join's own scope");

			var mergedItem = items.SingleOrDefault(e =>
				e.Elements(Ns + "T").Any(t => t.GetCData().Value.Contains("Two")));
			Assert.IsNotNull(mergedItem, "Expected to find the item that absorbed 'Three'");

			var mergedText = string.Concat(mergedItem.Elements(Ns + "T").Select(t => t.GetCData().Value));
			StringAssert.Contains(mergedText, "Two");
			StringAssert.Contains(mergedText, "Three");

			Assert.IsTrue(items.Any(e =>
				e.Elements(Ns + "T").Count() == 1 &&
				e.Elements(Ns + "T").First().GetCData().Value == "One"),
				"Expected 'One' to remain untouched as its own list item");

			Assert.IsTrue(items.Any(e =>
				e.Elements(Ns + "T").Count() == 1 &&
				e.Elements(Ns + "T").First().GetCData().Value == "Four"),
				"Expected 'Four' to remain untouched as its own list item");
		}


		private static XElement BuildCursorParagraph(string before, string after)
		{
			// simulates OneNote splitting a single-run paragraph into a zero-width
			// caret T flanked by the text before/after the cursor, with nothing
			// actually selected -- i.e. a bare cursor placed within plain text
			return new XElement(Ns + "OE",
				new XElement(Ns + "T", new XCData(before)),
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData(string.Empty)),
				new XElement(Ns + "T", new XCData(after)));
		}


		[TestMethod]
		public async Task JoinParagraph_CursorInPlainParagraph_JoinsSurroundingSameStyleBlock()
		{
			// Arrange: cursor (bare, no drag-selection) parked inside the middle of
			// three plain, same-style paragraphs (hard breaks) -- the whole block
			// should be joined, not just the paragraph the cursor sits in.
			var middle = BuildCursorParagraph("Paragraph ", "two.");

			var xml = new PageBuilder(PageId, "Block Join Test")
				.WithParagraph("Paragraph one.")
				.WithElement(middle)
				.WithParagraph("Paragraph three.")
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new JoinParagraphCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var oes = updated.Element(Ns + "Outline")
				.Descendants(Ns + "OE")
				.Where(e => e.Elements(Ns + "T").Any())
				.ToList();

			Assert.AreEqual(1, oes.Count,
				"Expected all three paragraphs of the block to collapse into a single OE");

			var joined = string.Concat(oes[0].Elements(Ns + "T").Select(t => t.GetCData().Value));
			Assert.AreEqual("Paragraph one. Paragraph two. Paragraph three.", joined,
				"Expected exactly one space between the joined paragraphs");
		}


		[TestMethod]
		public async Task JoinParagraph_CursorBlockStopsAtBlankParagraph()
		{
			// Arrange: an empty paragraph sits between "Before text." and the
			// cursor's paragraph; the block must not reach across it, but should
			// still extend forward into the matching paragraph that follows.
			var blank = new XElement(Ns + "OE", new XElement(Ns + "T", new XCData(string.Empty)));
			var cursor = BuildCursorParagraph("Cursor ", "text.");

			var xml = new PageBuilder(PageId, "Blank Boundary Test")
				.WithParagraph("Before text.")
				.WithElement(blank)
				.WithElement(cursor)
				.WithParagraph("After text.")
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new JoinParagraphCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var oes = updated.Element(Ns + "Outline")
				.Descendants(Ns + "OE")
				.Where(e => e.Elements(Ns + "T").Any())
				.ToList();

			Assert.AreEqual(3, oes.Count,
				"Expected 'Before text.', the blank paragraph, and the merged " +
				"cursor+'After text.' paragraph to remain as three OEs");

			Assert.IsTrue(oes.Any(e =>
				e.Elements(Ns + "T").Count() == 1 &&
				e.Elements(Ns + "T").First().GetCData().Value == "Before text."),
				"Expected 'Before text.' to remain untouched across the blank boundary");

			var merged = oes.SingleOrDefault(e =>
				e.Elements(Ns + "T").Any(t => t.GetCData().Value.Contains("Cursor")));
			Assert.IsNotNull(merged, "Expected to find the paragraph that absorbed the cursor text");

			var mergedText = string.Concat(merged.Elements(Ns + "T").Select(t => t.GetCData().Value));
			Assert.AreEqual("Cursor text. After text.", mergedText,
				"Expected exactly one space between the joined paragraphs");
		}


		[TestMethod]
		public async Task JoinParagraph_CursorBlockStopsAtStyleChange()
		{
			// Arrange: a heading precedes the cursor's paragraph; the differing
			// quickStyleIndex must stop the block from reaching into the heading,
			// while still joining forward into the matching plain paragraph.
			var heading = new XElement(Ns + "OE",
				new XAttribute("quickStyleIndex", "1"),
				new XElement(Ns + "T", new XCData("Heading")));

			var cursor = BuildCursorParagraph("Cursor ", "text.");

			var xml = new PageBuilder(PageId, "Style Boundary Test")
				.WithElement(heading)
				.WithElement(cursor)
				.WithParagraph("Trailing text.")
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new JoinParagraphCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var oes = updated.Element(Ns + "Outline")
				.Descendants(Ns + "OE")
				.Where(e => e.Elements(Ns + "T").Any())
				.ToList();

			Assert.AreEqual(2, oes.Count,
				"Expected the heading to remain separate from the merged cursor+trailing paragraph");

			Assert.IsTrue(oes.Any(e =>
				e.Elements(Ns + "T").Count() == 1 &&
				e.Elements(Ns + "T").First().GetCData().Value == "Heading"),
				"Expected the heading to remain untouched across the style boundary");

			var merged = oes.SingleOrDefault(e =>
				e.Elements(Ns + "T").Any(t => t.GetCData().Value.Contains("Cursor")));
			Assert.IsNotNull(merged, "Expected to find the paragraph that absorbed the cursor text");

			var mergedText = string.Concat(merged.Elements(Ns + "T").Select(t => t.GetCData().Value));
			Assert.AreEqual("Cursor text. Trailing text.", mergedText,
				"Expected exactly one space between the joined paragraphs");
		}
	}
}
