//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Styles
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Styles;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	/*
	 * Test Protocol for Commands/Styles/ApplyStylesCommand
	 * Applies all styles from the currently loaded custom styles theme to the page.
	 *
	 *     1. Custom Styles/Apply Styles to Page
	 *     2. Confirm that all custom styles are applied appropriately to all content on the page
	 *     3. Apply Styles to Page again.
	 *     4. Confirm that no changes are made. The apply styles action should be a no-op since
	 *        it was already applied.
	 *     5. Revert. Select a block of content, such as from Heading1 down to the "normal
	 *        paragraph" and Apply Styles to Page.
	 *     6. Confirm that custom styles are applied only the selected content; all other content
	 *        remains in the original style.
	 */

	[TestClass]
	public class ApplyStylesCommandTests : TestBase
	{
		private const string PageId = "page-1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";

		// Minimal deterministic test theme: h1 → TestHeadFont/24.0pt, p → TestBodyFont/12.0pt.
		// color="" so ApplyToPage skips UpdatePageColor (which calls page.Title), keeping the
		// test focused on QuickStyleDef font/size changes and not the page-color path.
		// Kept separate from any user-installed theme so tests are machine-independent.
		private static readonly Theme TestTheme = new Theme(
			XElement.Parse(
				"<Theme key=\"Test\" name=\"Test\" color=\"\"" +
				" setColor=\"False\" dark=\"False\">" +
				"  <Style index=\"0\" name=\"Heading 1\" font=\"TestHeadFont\"" +
				"   fontColor=\"#AA0000\" fontSize=\"24.0\" spaceBefore=\"12.0\"" +
				"   spaceAfter=\"2.0\" applyColors=\"true\" styleType=\"Heading\" />" +
				"  <Style index=\"1\" name=\"Normal\" font=\"TestBodyFont\"" +
				"   fontColor=\"Black\" fontSize=\"12.0\" spaceBefore=\"4.0\"" +
				"   spaceAfter=\"4.0\" applyColors=\"true\" styleType=\"Paragraph\" />" +
				"</Theme>"),
			"Test");


		// Builds a page element with p and h1 QuickStyleDefs pre-populated with "OldFont".
		// The QSDs must carry all four attributes that ApplyStyles writes via .Attribute().Value.
		private static XElement BuildPageWithQuickStyleDefs()
		{
			var page = new PageBuilder(PageId, "Apply Styles Test").BuildElement();
			page.AddFirst(new XElement(Ns + "QuickStyleDef",
				new XAttribute("index", "1"),
				new XAttribute("name", "h1"),
				new XAttribute("font", "OldFont"),
				new XAttribute("fontSize", "9.0"),
				new XAttribute("spaceBefore", "0.0"),
				new XAttribute("spaceAfter", "0.0")));
			page.AddFirst(new XElement(Ns + "QuickStyleDef",
				new XAttribute("index", "0"),
				new XAttribute("name", "p"),
				new XAttribute("font", "OldFont"),
				new XAttribute("fontSize", "9.0"),
				new XAttribute("spaceBefore", "0.0"),
				new XAttribute("spaceAfter", "0.0")));
			return page;
		}


		[TestMethod]
		public async Task ApplyStyles_WithTextCursor_AppliesToQuickStyleDefs()
		{
			// Arrange: page with p and h1 QuickStyleDefs and a text cursor (empty selected T).
			// An empty-CDATA selected T yields SelectionScope.TextCursor, which triggers the
			// full-page apply path (ApplyToPage → ApplyThemeStyles → ApplyStyles).
			var page = BuildPageWithQuickStyleDefs();
			page.Add(new XElement(Ns + "Outline",
				new XElement(Ns + "OEChildren",
					new XElement(Ns + "OE",
						new XElement(Ns + "T",
							new XAttribute("selected", "all"),
							new XCData(""))))));

			SetupPage(PageId, page.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act
			await new ApplyStylesCommand(TestTheme).Execute();

			// Assert: QuickStyleDefs updated with theme fonts and sizes
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var h1Font = (string)updated
				.Elements(Ns + "QuickStyleDef")
				.FirstOrDefault(q => (string)q.Attribute("name") == "h1")
				?.Attribute("font");
			Assert.AreEqual("TestHeadFont", h1Font, "h1 font should match theme");

			var h1Size = (string)updated
				.Elements(Ns + "QuickStyleDef")
				.FirstOrDefault(q => (string)q.Attribute("name") == "h1")
				?.Attribute("fontSize");
			Assert.AreEqual("24.0", h1Size, "h1 fontSize should match theme");

			var pFont = (string)updated
				.Elements(Ns + "QuickStyleDef")
				.FirstOrDefault(q => (string)q.Attribute("name") == "p")
				?.Attribute("font");
			Assert.AreEqual("TestBodyFont", pFont, "p font should match theme");
		}


		[TestMethod]
		public async Task ApplyStyles_SecondApply_ProducesSameResult()
		{
			// Arrange: same page as the full-page test
			var page = BuildPageWithQuickStyleDefs();
			page.Add(new XElement(Ns + "Outline",
				new XElement(Ns + "OEChildren",
					new XElement(Ns + "OE",
						new XElement(Ns + "T",
							new XAttribute("selected", "all"),
							new XCData(""))))));

			SetupPage(PageId, page.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act 1: first apply
			await new ApplyStylesCommand(TestTheme).Execute();
			var afterFirst = GetUpdatedPage(PageId);
			Assert.IsNotNull(afterFirst, "First apply: UpdatePageContent was never called");

			// Act 2: apply the same theme again to the already-styled page
			await new ApplyStylesCommand(TestTheme).Execute();
			var afterSecond = GetUpdatedPage(PageId);
			Assert.IsNotNull(afterSecond, "Second apply: UpdatePageContent was never called");

			// Assert: both passes produce identical QuickStyleDef attribute values
			string FontOf(XElement root, string name) =>
				(string)root.Elements(Ns + "QuickStyleDef")
					.FirstOrDefault(q => (string)q.Attribute("name") == name)
					?.Attribute("font");

			Assert.AreEqual(FontOf(afterFirst, "h1"), FontOf(afterSecond, "h1"),
				"h1 font must be identical after second apply");
			Assert.AreEqual(FontOf(afterFirst, "p"), FontOf(afterSecond, "p"),
				"p font must be identical after second apply");
		}


		[TestMethod]
		public async Task ApplyStyles_WithSelection_AppliesToSelectedParagraphsOnly()
		{
			// Arrange: page with two paragraphs — only the h1 OE is selected.
			// A non-empty selected T yields SelectionScope.Run, which triggers
			// ApplyToSelectedParagraphs; unselected paragraphs must not be touched.
			var page = BuildPageWithQuickStyleDefs();

			var h1OE = new XElement(Ns + "OE",
				new XAttribute("quickStyleIndex", "1"),
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData("Heading text")));

			var pOE = new XElement(Ns + "OE",
				new XAttribute("quickStyleIndex", "0"),
				new XElement(Ns + "T", new XCData("Normal text")));

			page.Add(new XElement(Ns + "Outline",
				new XElement(Ns + "OEChildren", h1OE, pOE)));

			SetupPage(PageId, page.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act
			await new ApplyStylesCommand(TestTheme).Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			// Selected h1 OE should have an inline style attribute from the theme
			var selectedStyle = (string)updated.Descendants(Ns + "OE")
				.FirstOrDefault(oe => (string)oe.Attribute("quickStyleIndex") == "1")
				?.Attribute("style");
			Assert.IsNotNull(selectedStyle,
				"Selected h1 paragraph should have an inline style attribute");
			StringAssert.Contains(selectedStyle, "TestHeadFont",
				"Inline style should reference the theme font");

			// Unselected p OE should remain unchanged (no inline style added)
			var unselectedStyle = (string)updated.Descendants(Ns + "OE")
				.FirstOrDefault(oe => (string)oe.Attribute("quickStyleIndex") == "0")
				?.Attribute("style");
			Assert.IsNull(unselectedStyle,
				"Unselected normal paragraph should not have an inline style applied");
		}
	}
}
