//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Snippets
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	/*
	 * Test Protocol - InsertTocCommand
	 * Generate and refresh a table of contents on the current page. An initial page should
	 * have at least two levels of headings and some content. There should also be a table
	 * with Heading styled content in its cells to prove that those headings are not included
	 * in the Table of Content.
	 *
	 *   1. Run the Table of Contents command using the CLI InsertToc command for the current page
	 *   2. Confirm that a new table of contents is inserted at the top of the page and includes
	 *      all top level headings.
	 *   3. Change the text of one or more of the headings and run the CLI InsertToc command for
	 *      the current page.
	 *   4. Confirm that the table of contents was properly updated with the new text
	 */

	[TestClass]
	public class InsertTocCommandTests : TestBase
	{
		private const string PageId = "page-1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		/// <summary>
		/// Builds a well-formed page with h1/h2 headings, a body paragraph, and optional
		/// table containing a heading-styled cell. The Title OE carries an objectID attribute
		/// because GetHeadings() clears all results when it is absent.
		/// </summary>
		private string BuildPageXml(bool withTableHeadings = false)
		{
			var page = new XElement(Ns + "Page",
				new XAttribute("ID", PageId),
				new XAttribute("pageColor", "automatic"),
				new XAttribute("lang", "en-US"),
				new XAttribute(XNamespace.Xmlns + "one", Ns.NamespaceName),
				// QuickStyleDef elements are excluded from hash tracking and are always
				// preserved after OptimizeForSave, so the second Execute() call still
				// sees them and can recognise heading quick-style indices.
				new XElement(Ns + "QuickStyleDef",
					new XAttribute("index", "0"), new XAttribute("name", "p"),
					new XAttribute("font", "Calibri"), new XAttribute("fontSize", "11.0"),
					new XAttribute("fontColor", "automatic")),
				new XElement(Ns + "QuickStyleDef",
					new XAttribute("index", "1"), new XAttribute("name", "h1"),
					new XAttribute("font", "Calibri"), new XAttribute("fontSize", "20.0"),
					new XAttribute("fontColor", "automatic")),
				new XElement(Ns + "QuickStyleDef",
					new XAttribute("index", "2"), new XAttribute("name", "h2"),
					new XAttribute("font", "Calibri"), new XAttribute("fontSize", "14.0"),
					new XAttribute("fontColor", "automatic")),
				new XElement(Ns + "PageSettings",
					new XAttribute("color", "automatic")),
				new XElement(Ns + "Title",
					new XElement(Ns + "OE",
						new XAttribute("objectID", "title-oe"),
						new XElement(Ns + "T", new XCData("TOC Test")))));

			var oeChildren = new XElement(Ns + "OEChildren",
				new XElement(Ns + "OE",
					new XAttribute("quickStyleIndex", "1"),
					new XAttribute("objectID", "oe-h1"),
					new XElement(Ns + "T", new XCData("First Heading"))),
				new XElement(Ns + "OE",
					new XAttribute("objectID", "oe-p1"),
					new XElement(Ns + "T", new XCData("Some body content"))),
				new XElement(Ns + "OE",
					new XAttribute("quickStyleIndex", "2"),
					new XAttribute("objectID", "oe-h2"),
					new XElement(Ns + "T", new XCData("Sub Heading"))));

			if (withTableHeadings)
			{
				// Table containing a Heading 1 cell — spec says these should NOT appear in TOC
				oeChildren.Add(new XElement(Ns + "OE",
					new XAttribute("objectID", "oe-table"),
					new XElement(Ns + "Table",
						new XAttribute("bordersVisible", "true"),
						new XElement(Ns + "Row",
							new XElement(Ns + "Cell",
								new XElement(Ns + "OEChildren",
									new XElement(Ns + "OE",
										new XAttribute("quickStyleIndex", "1"),
										new XAttribute("objectID", "oe-table-h1"),
										new XElement(Ns + "T",
											new XCData("Table Heading")))))))));
			}

			page.Add(new XElement(Ns + "Outline", oeChildren));
			return page.ToString(SaveOptions.OmitDuplicateNamespaces);
		}


		/// <summary>
		/// Builds a page that already has an omToc Meta marker (simulating the state after a
		/// previous TOC build). The heading text is parameterised so that the test can verify
		/// the TOC is rebuilt from the current headings rather than appended.
		/// </summary>
		private string BuildPageXmlWithExistingToc(string heading1Text = "Changed Heading")
		{
			var page = new XElement(Ns + "Page",
				new XAttribute("ID", PageId),
				new XAttribute("pageColor", "automatic"),
				new XAttribute("lang", "en-US"),
				new XAttribute(XNamespace.Xmlns + "one", Ns.NamespaceName),
				new XElement(Ns + "QuickStyleDef",
					new XAttribute("index", "0"), new XAttribute("name", "p"),
					new XAttribute("font", "Calibri"), new XAttribute("fontSize", "11.0"),
					new XAttribute("fontColor", "automatic")),
				new XElement(Ns + "QuickStyleDef",
					new XAttribute("index", "1"), new XAttribute("name", "h1"),
					new XAttribute("font", "Calibri"), new XAttribute("fontSize", "20.0"),
					new XAttribute("fontColor", "automatic")),
				new XElement(Ns + "QuickStyleDef",
					new XAttribute("index", "2"), new XAttribute("name", "h2"),
					new XAttribute("font", "Calibri"), new XAttribute("fontSize", "14.0"),
					new XAttribute("fontColor", "automatic")),
				new XElement(Ns + "PageSettings",
					new XAttribute("color", "automatic")),
				new XElement(Ns + "Title",
					new XElement(Ns + "OE",
						new XAttribute("objectID", "title-oe"),
						new XElement(Ns + "T", new XCData("TOC Test")))));

			// Outline: TOC placeholder OE (just the Meta marker) followed by page headings.
			// LocateBestContainerOE finds the Meta, clears the container, and rebuilds in place.
			page.Add(new XElement(Ns + "Outline",
				new XElement(Ns + "OEChildren",
					new XElement(Ns + "OE",
						new XElement(Ns + "Meta",
							new XAttribute("name", "omToc"),
							new XAttribute("content", "page"))),
					new XElement(Ns + "OE",
						new XAttribute("quickStyleIndex", "1"),
						new XAttribute("objectID", "oe-h1"),
						new XElement(Ns + "T", new XCData(heading1Text))),
					new XElement(Ns + "OE",
						new XAttribute("quickStyleIndex", "2"),
						new XAttribute("objectID", "oe-h2"),
						new XElement(Ns + "T", new XCData("Sub Heading"))))));

			return page.ToString(SaveOptions.OmitDuplicateNamespaces);
		}


		[TestMethod]
		public async Task InsertToc_WithPageHeadings_BuildsTocAtTopOfOutline()
		{
			// Arrange
			SetupPage(PageId, BuildPageXml());

			// Act
			var command = new InsertTocCommand();
			command.RunFromCli();
			await command.Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			// The TOC OE is inserted as the first element in the outline
			var firstOE = updated
				.Elements(Ns + "Outline").First()
				.Element(Ns + "OEChildren")
				.Elements(Ns + "OE").First();

			var hasTocMeta = firstOE
				.Elements(Ns + "Meta")
				.Any(m => m.Attribute("name")?.Value == "omToc");
			Assert.IsTrue(hasTocMeta, "First OE in outline should be the TOC (omToc Meta expected)");

			// Both heading levels must appear in the TOC body
			var tocText = firstOE.Value;
			StringAssert.Contains(tocText, "First Heading",
				"TOC should include the h1 heading text");
			StringAssert.Contains(tocText, "Sub Heading",
				"TOC should include the h2 heading text");
		}


		[TestMethod]
		public async Task InsertToc_TableCellHeadings_NotIncludedInToc()
		{
			// Arrange: page with page-level headings and a table whose cell has a heading style
			SetupPage(PageId, BuildPageXml(withTableHeadings: true));

			// Act
			var command = new InsertTocCommand();
			command.RunFromCli();
			await command.Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var tocOE = updated
				.Elements(Ns + "Outline").First()
				.Element(Ns + "OEChildren")
				.Elements(Ns + "OE").First();

			Assert.IsTrue(
				tocOE.Elements(Ns + "Meta").Any(m => m.Attribute("name")?.Value == "omToc"),
				"First OE should be the TOC container");

			// The heading entries live in row[1] of the TOC table (row[0] is the title row)
			var headingsRow = tocOE
				.Descendants(Ns + "Table").First()
				.Elements(Ns + "Row")
				.ElementAt(1);

			StringAssert.Contains(headingsRow.Value, "First Heading",
				"TOC headings row should contain the page-level h1");

			Assert.IsFalse(headingsRow.Value.Contains("Table Heading"),
				"Heading inside a table cell should not appear in the TOC");
		}


		[TestMethod]
		public async Task InsertToc_WithExistingTocMarker_RebuildsInPlace()
		{
			// Arrange: page already has an omToc Meta marker with "Changed Heading" as the h1.
			// On execute the command must find the existing marker, clear the container,
			// and rebuild the TOC from the current headings — without inserting a second TOC.
			SetupPage(PageId, BuildPageXmlWithExistingToc("Changed Heading"));

			// Act
			var command = new InsertTocCommand();
			command.RunFromCli();
			await command.Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			// Exactly one TOC should exist on the page (no duplication)
			var tocMetas = updated
				.Descendants(Ns + "Meta")
				.Where(m => m.Attribute("name")?.Value == "omToc")
				.ToList();
			Assert.AreEqual(1, tocMetas.Count, "There should be exactly one TOC on the page");

			// The rebuilt TOC must reflect the current heading text
			var tocText = tocMetas[0].Parent.Value;
			StringAssert.Contains(tocText, "Changed Heading",
				"Rebuilt TOC should contain the current heading text");
		}
	}
}
