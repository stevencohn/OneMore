//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.References
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	/*
	 * Test Protocol
	 * Commands/References/AddFootnoteCommand and RemoveFootnoteCommand
	 *
	 * Add Footnote inserts a new footnote from the current cursor to a reference at the
	 * bottom of the page. Remove Footnote deletes the current footnote (with the cursor
	 * either placed on the reference label in the content or on a footnote at the bottom
	 * of the page) and remaining footnotes are resequenced on the page.
	 *
	 * Add Footnotes
	 *   1. Move the caret to a location in the content; the content needs at least two
	 *      paragraphs and run References/Add Footnote
	 *   2. Confirm a reference label is added at the caret position and a new footnote is
	 *      appended to the page
	 *   3. Provide some text for the footnote. Add two more footnotes. Reorder the content
	 *      so the footnotes are out of sequence (top-down, numerically), Click the Refresh
	 *      link in the footnotes area.
	 *   4. Confirm that footnotes are reordered in relation to their top-down arrangement
	 *      on the page
	 *
	 * Remove Footnotes
	 *   1. Move the caret immediately after the second footnote in the content. Run
	 *      References/Delete Footnote
	 *   2. Confirm that the reference label and its related footnote are deleted and that
	 *      the remaining footnotes are renumbered in sequence from the top of the page to
	 *      the bottom.
	 *   3. Move the caret onto the text of the first footnote at the bottom of the page
	 *      and run References/Delete Footnote
	 *   4. Confirm that the footnote is deleted, remaining footnotes are renumbered.
	 *   5. Delete the remaining footnotes using the Remove Footnote command
	 *   6. Confirm that the footnotes section is deleted from the bottom of the page
	 */

	[TestClass]
	public class FootnoteCommandTests : TestBase
	{
		private const string PageId = "page-1";

		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		// The reference link written by WriteFootnoteRef into the body CDATA
		private static string RefLink(string label) =>
			$"<a href=\"\"><span style=\"vertical-align:super\">[{label}]</span></a>";


		// The footnote text written by WriteFootnoteText into the footer CDATA (LTR, light mode)
		private static string NoteText(string label, string text) =>
			$"<a href=\"\"><span style=\"font-family:'Calibri Light';font-size:11.0pt\">[{label}]</span></a>" +
			$"<span style=\"font-family:'Calibri Light';font-size:11.0pt\"> {text}</span>";


		// =====================================================================================
		// AddFootnoteCommand tests
		// =====================================================================================

		[TestMethod]
		public async Task AddFootnote_WithSelectedCursor_CreatesFootnoteStructure()
		{
			// Arrange: two-paragraph page with cursor at the end of the second paragraph.
			// Outline needs selected="partial" so ConfirmBodyContext passes.
			// Cursor OE needs objectID so WriteFootnoteText can resolve its textId.
			// Note: because the mock does not generate objectIDs for newly added OEs, the
			// second Update (adding the body reference link) is skipped. The first Update
			// (creating the footer and footnote OE) is still verifiable.

			var pageEl = new PageBuilder(PageId, "Footnote Test")
				.WithElement(new XElement(Ns + "OE",
					new XAttribute("objectID", "oe-p1"),
					new XElement(Ns + "T", new XCData("First paragraph"))))
				.WithElement(new XElement(Ns + "OE",
					new XAttribute("objectID", "oe-cursor"),
					new XElement(Ns + "T",
						new XAttribute("selected", "all"),
						new XCData(string.Empty))))
				.BuildElement();

			pageEl.Element(Ns + "Outline")?.SetAttributeValue("selected", "partial");
			SetupPage(PageId, pageEl.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act
			await new AddFootnoteCommand().Execute();

			// Assert: first Update (inside WriteFootnoteText) stored the page
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var divider = updated.Descendants(Ns + "Meta")
				.FirstOrDefault(e =>
					(string)e.Attribute("name") == "omfootnotes" &&
					(string)e.Attribute("content") == "divider");
			Assert.IsNotNull(divider, "Expected a divider OE with omfootnotes/divider meta");

			var footnote = updated.Descendants(Ns + "Meta")
				.FirstOrDefault(e =>
					(string)e.Attribute("name") == "omfootnote" &&
					(string)e.Attribute("content") == "1");
			Assert.IsNotNull(footnote, "Expected a footnote OE with omfootnote/1 meta");
		}


		// =====================================================================================
		// RemoveFootnoteCommand tests
		// =====================================================================================

		/// <summary>
		/// Builds a page with one footnote. When cursorOnRef is true the cursor is on the
		/// body reference link; when false it is on the footnote text at the bottom.
		/// </summary>
		private XElement BuildSingleFootnotePage(bool cursorOnRef)
		{
			var refCdata = RefLink("1");
			var noteCdata = NoteText("1", "First footnote text");

			return new XElement(Ns + "Page",
				new XAttribute("ID", PageId),
				new XAttribute("pageColor", "automatic"),
				new XAttribute("lang", "en-US"),
				new XAttribute(XNamespace.Xmlns + "one", Ns.NamespaceName),
				new XElement(Ns + "PageSettings", new XAttribute("color", "automatic")),
				new XElement(Ns + "Title",
					new XElement(Ns + "OE",
						new XElement(Ns + "T", new XCData("Footnote Test")))),
				new XElement(Ns + "Outline",
					new XAttribute("selected", "partial"),
					new XElement(Ns + "OEChildren",
						// Body paragraph with reference [1]
						new XElement(Ns + "OE",
							new XAttribute("objectID", "oe-body"),
							new XElement(Ns + "T",
								cursorOnRef ? new XAttribute("selected", "all") : null,
								new XCData(refCdata))),
						// Spacer OEs
						new XElement(Ns + "OE",
							new XElement(Ns + "Meta",
								new XAttribute("name", "omfootnotes"),
								new XAttribute("content", "empty")),
							new XElement(Ns + "T", new XCData(string.Empty))),
						new XElement(Ns + "OE",
							new XElement(Ns + "Meta",
								new XAttribute("name", "omfootnotes"),
								new XAttribute("content", "empty")),
							new XElement(Ns + "T", new XCData(string.Empty))),
						// Divider
						new XElement(Ns + "OE",
							new XElement(Ns + "Meta",
								new XAttribute("name", "omfootnotes"),
								new XAttribute("content", "divider")),
							new XElement(Ns + "T", new XCData("- - -"))),
						// Footnote [1]
						new XElement(Ns + "OE",
							new XAttribute("objectID", "oe-note-1"),
							new XElement(Ns + "Meta",
								new XAttribute("name", "omfootnote"),
								new XAttribute("content", "1")),
							new XElement(Ns + "T",
								cursorOnRef ? null : new XAttribute("selected", "all"),
								new XCData(noteCdata))))));
		}


		[TestMethod]
		public async Task RemoveFootnote_CursorOnReference_RemovesFootnoteAndDivider()
		{
			// Arrange: one footnote with cursor on the body reference link
			var page = BuildSingleFootnotePage(cursorOnRef: true);
			SetupPage(PageId, page.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act
			await new RemoveFootnoteCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var divider = updated.Descendants(Ns + "Meta")
				.FirstOrDefault(e => (string)e.Attribute("name") == "omfootnotes" &&
									 (string)e.Attribute("content") == "divider");
			Assert.IsNull(divider, "Divider OE should have been removed with the last footnote");

			var footnote = updated.Descendants(Ns + "Meta")
				.FirstOrDefault(e => (string)e.Attribute("name") == "omfootnote");
			Assert.IsNull(footnote, "Footnote OE should have been removed");

			var anyRef = updated.Descendants(Ns + "T")
				.SelectMany(t => t.DescendantNodes().OfType<XCData>())
				.Any(c => c.Value.Contains("vertical-align:super"));
			Assert.IsFalse(anyRef, "Reference link should be removed from body");
		}


		[TestMethod]
		public async Task RemoveFootnote_CursorOnFootnoteText_RemovesFootnoteAndDivider()
		{
			// Arrange: one footnote with cursor on the footnote text at the bottom of the page
			var page = BuildSingleFootnotePage(cursorOnRef: false);
			SetupPage(PageId, page.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act
			await new RemoveFootnoteCommand().Execute();

			// Assert: same expectations as cursor-on-reference
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var divider = updated.Descendants(Ns + "Meta")
				.FirstOrDefault(e => (string)e.Attribute("name") == "omfootnotes" &&
									 (string)e.Attribute("content") == "divider");
			Assert.IsNull(divider, "Divider OE should have been removed with the last footnote");

			var footnote = updated.Descendants(Ns + "Meta")
				.FirstOrDefault(e => (string)e.Attribute("name") == "omfootnote");
			Assert.IsNull(footnote, "Footnote OE should have been removed");

			var anyRef = updated.Descendants(Ns + "T")
				.SelectMany(t => t.DescendantNodes().OfType<XCData>())
				.Any(c => c.Value.Contains("vertical-align:super"));
			Assert.IsFalse(anyRef, "Reference link should be removed from body");
		}


		[TestMethod]
		public async Task RemoveFootnote_WithTwoFootnotes_ResequencesRemaining()
		{
			// Arrange: page with two footnotes; cursor on the first body reference [1].
			// After removing [1], reference [2] and footnote [2] are renumbered to [1].

			var page = new XElement(Ns + "Page",
				new XAttribute("ID", PageId),
				new XAttribute("pageColor", "automatic"),
				new XAttribute("lang", "en-US"),
				new XAttribute(XNamespace.Xmlns + "one", Ns.NamespaceName),
				new XElement(Ns + "PageSettings", new XAttribute("color", "automatic")),
				new XElement(Ns + "Title",
					new XElement(Ns + "OE",
						new XElement(Ns + "T", new XCData("Footnote Test")))),
				new XElement(Ns + "Outline",
					new XAttribute("selected", "partial"),
					new XElement(Ns + "OEChildren",
						// Body paragraph 1 — cursor on reference [1]
						new XElement(Ns + "OE",
							new XAttribute("objectID", "oe-body-1"),
							new XElement(Ns + "T",
								new XAttribute("selected", "all"),
								new XCData(RefLink("1")))),
						// Body paragraph 2 — reference [2]
						new XElement(Ns + "OE",
							new XAttribute("objectID", "oe-body-2"),
							new XElement(Ns + "T",
								new XCData(RefLink("2")))),
						// Spacer
						new XElement(Ns + "OE",
							new XElement(Ns + "Meta",
								new XAttribute("name", "omfootnotes"),
								new XAttribute("content", "empty")),
							new XElement(Ns + "T", new XCData(string.Empty))),
						// Divider
						new XElement(Ns + "OE",
							new XElement(Ns + "Meta",
								new XAttribute("name", "omfootnotes"),
								new XAttribute("content", "divider")),
							new XElement(Ns + "T", new XCData("- - -"))),
						// Footnote [1]
						new XElement(Ns + "OE",
							new XAttribute("objectID", "oe-note-1"),
							new XElement(Ns + "Meta",
								new XAttribute("name", "omfootnote"),
								new XAttribute("content", "1")),
							new XElement(Ns + "T", new XCData(NoteText("1", "First footnote")))),
						// Footnote [2]
						new XElement(Ns + "OE",
							new XAttribute("objectID", "oe-note-2"),
							new XElement(Ns + "Meta",
								new XAttribute("name", "omfootnote"),
								new XAttribute("content", "2")),
							new XElement(Ns + "T", new XCData(NoteText("2", "Second footnote")))))));

			SetupPage(PageId, page.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act
			await new RemoveFootnoteCommand().Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			// Divider remains because one footnote still exists
			var divider = updated.Descendants(Ns + "Meta")
				.FirstOrDefault(e => (string)e.Attribute("name") == "omfootnotes" &&
									 (string)e.Attribute("content") == "divider");
			Assert.IsNotNull(divider, "Divider should remain when a footnote still exists");

			// Exactly one footnote remains, renumbered to 1
			var footnotes = updated.Descendants(Ns + "Meta")
				.Where(e => (string)e.Attribute("name") == "omfootnote")
				.ToList();
			Assert.AreEqual(1, footnotes.Count, "Expected exactly one remaining footnote");
			Assert.AreEqual("1", (string)footnotes[0].Attribute("content"),
				"Remaining footnote should be renumbered to 1");

			// Body paragraph 2 reference renumbered from [2] to [1]
			var body2Cdata = updated
				.Descendants(Ns + "OE")
				.FirstOrDefault(e => (string)e.Attribute("objectID") == "oe-body-2")?
				.Descendants(Ns + "T")
				.SelectMany(t => t.DescendantNodes().OfType<XCData>())
				.FirstOrDefault();
			Assert.IsNotNull(body2Cdata, "Body paragraph 2 should still exist");
			StringAssert.Contains(body2Cdata.Value, "[1]",
				"Body paragraph 2 reference should be renumbered from [2] to [1]");
			Assert.IsFalse(body2Cdata.Value.Contains("[2]"),
				"Body paragraph 2 should no longer reference [2]");
		}


		[TestMethod]
		public async Task RemoveFootnote_CursorOnRegularText_DoesNotUpdate()
		{
			// Arrange: page with plain selected text (not a hyperlink). SelectionRange reports
			// Run scope and RemoveFootnote aborts without calling UpdatePageContent.

			var pageEl = new PageBuilder(PageId, "No Footnote Test")
				.WithElement(new XElement(Ns + "OE",
					new XElement(Ns + "T",
						new XAttribute("selected", "all"),
						new XCData("Regular text — not a hyperlink"))))
				.BuildElement();
			pageEl.Element(Ns + "Outline")?.SetAttributeValue("selected", "partial");

			var originalXml = pageEl.ToString(SaveOptions.OmitDuplicateNamespaces);
			SetupPage(PageId, originalXml);

			// Act
			await new RemoveFootnoteCommand().Execute();

			// Assert: page unchanged, UpdatePageContent never called
			var storedXml = Mock.GetPage(PageId);
			Assert.AreEqual(originalXml, storedXml,
				"Page should not be updated when cursor is not on a footnote or reference");
		}
	}
}
