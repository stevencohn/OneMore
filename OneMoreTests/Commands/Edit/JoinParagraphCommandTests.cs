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
			// Arrange: three separate paragraphs (hard breaks), all selected. The first OE
			// carries a style so we can confirm it is inherited onto the joined runs.
			var oe1 = new XElement(Ns + "OE",
				new XAttribute("style", "font-weight:bold"),
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
			StringAssert.Contains(joined, "Paragraph one.");
			StringAssert.Contains(joined, "Paragraph two.");
			StringAssert.Contains(joined, "Paragraph three.");

			// style from the first (anchor) OE must be inherited onto its run
			var styledRun = oes[0].Elements(Ns + "T")
				.FirstOrDefault(t => t.GetCData().Value.Contains("Paragraph one."));
			Assert.IsNotNull(styledRun);
			StringAssert.Contains((string)styledRun.Attribute("style") ?? "", "font-weight:bold");
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
			StringAssert.Contains(joined, "Lorem ipsum");
			StringAssert.Contains(joined, "dolor sit");
			StringAssert.Contains(joined, "amet");
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
	}
}
