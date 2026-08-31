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
	 * Test Protocol - InsertBoxCommand
	 * Regression test for GitHub issue #2558: NullReferenceException thrown by
	 * InsertBoxCommand.Execute when the text cursor resolves to SelectionScope.TextCursor
	 * (e.g. the caret is in the page Title) but the body Outline itself has no OE content
	 * for PageEditor.ExtractSelectedContent to anchor to. In that state, ExtractSelectedContent
	 * sets PageEditor.Anchor to null, and the command must fall back to inserting into the
	 * page's content container rather than dereferencing the null Anchor.
	 *
	 *   1. Create a page whose body Outline is empty (no paragraphs) but is marked as having
	 *      focus, with the caret actually parked in the page Title.
	 *   2. Run the CLI InsertTextBox command.
	 *   3. Confirm the command completes without throwing and inserts the box table into the
	 *      page rather than silently failing or crashing.
	 */

	[TestClass]
	public class InsertBoxCommandTests : TestBase
	{
		private const string PageId = "page-1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		/// <summary>
		/// Builds a page where the body Outline carries selected="partial" (so
		/// ConfirmBodyContext passes) but contains no OE paragraphs at all, while the actual
		/// empty-cursor selection (selected="all" on an empty T) lives in the Title. This
		/// reproduces the state where SelectionRange resolves SelectionScope.TextCursor but
		/// PageEditor.ExtractSelectedContent finds no content to anchor to.
		/// </summary>
		private static string BuildPageWithNoBodyContent()
		{
			var page = new XElement(Ns + "Page",
				new XAttribute("ID", PageId),
				new XAttribute("pageColor", "automatic"),
				new XAttribute("lang", "en-US"),
				new XAttribute(XNamespace.Xmlns + "one", Ns.NamespaceName),
				new XElement(Ns + "PageSettings",
					new XAttribute("color", "automatic")),
				new XElement(Ns + "Title",
					new XElement(Ns + "OE",
						new XElement(Ns + "T",
							new XAttribute("selected", "all"),
							new XCData(string.Empty)))),
				new XElement(Ns + "Outline",
					new XAttribute("selected", "partial"),
					new XElement(Ns + "Position",
						new XAttribute("x", "36.0"), new XAttribute("y", "100.0")),
					new XElement(Ns + "Size",
						new XAttribute("width", "300.0"), new XAttribute("height", "14.0"))));

			return page.ToString(SaveOptions.OmitDuplicateNamespaces);
		}


		[TestMethod]
		public async Task InsertTextBox_WithNoBodyContentToAnchorTo_DoesNotThrow()
		{
			// Arrange
			SetupPage(PageId, BuildPageWithNoBodyContent());

			// Act
			var command = new InsertTextBoxCommand();
			command.RunFromCli();
			await command.Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var table = updated.Descendants(Ns + "Table").FirstOrDefault();
			Assert.IsNotNull(table, "Box table should have been inserted into the page");
		}
	}
}
