//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Page
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Linq;
	using System.Xml.Linq;

	/*
	 * Test Protocol - ResetCursorCommand (command palette only)
	 * Moves the text cursor to the start of the title on each page in a scope
	 *
	 * 	1. Place the cursor in the body of a few pages
	 * 	2. Open the command palette and run "Reset Cursor to Title"; choose a scope
	 * 	3. Confirm each page opens with the cursor at the start of its title
	 */

	[TestClass]
	public class ResetCursorTests
	{
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		private static XElement TitleOE(Page page) =>
			page.Root.Element(Ns + "Title").Element(Ns + "OE");


		[TestMethod]
		public void SetCursorAtTitle_BodySelection_MovesToEmptyRunFirstInTitle()
		{
			var body = new XElement(Ns + "OE",
				new XElement(Ns + "T", new XAttribute("selected", "all"),
					new XCData("body text")));

			var root = new PageBuilder("page-1", "My Title").WithElement(body).BuildElement();
			var page = new Page(root);

			Assert.IsTrue(new PageEditor(page).SetCursorAtTitle());

			var selected = page.Root.Descendants()
				.Where(e => e.Attribute("selected") is not null).ToList();

			Assert.AreEqual(1, selected.Count, "Expected exactly one selection");
			Assert.AreSame(TitleOE(page).FirstNode, selected[0], "Cursor must be first run");
			Assert.AreEqual(string.Empty, selected[0].Value);
			Assert.AreEqual("My Title", page.Title);
		}


		[TestMethod]
		public void SetCursorAtTitle_CalledTwice_DoesNotDuplicateCursor()
		{
			var page = new Page(new PageBuilder("page-1", "My Title").BuildElement());
			var editor = new PageEditor(page);

			editor.SetCursorAtTitle();
			editor.SetCursorAtTitle();

			Assert.AreEqual(1, TitleOE(page).Elements(Ns + "T")
				.Count(t => t.Value.Length == 0));
		}


		[TestMethod]
		public void IsSelectionOnlyInTitle_CursorInTitle_True_CursorInBody_False()
		{
			var body = new XElement(Ns + "OE",
				new XElement(Ns + "T", new XCData("body text")));
			var page = new Page(new PageBuilder("page-1", "My Title").WithElement(body).BuildElement());
			var editor = new PageEditor(page);

			Assert.IsFalse(editor.IsSelectionOnlyInTitle(), "no selection at all");

			editor.SetCursorAtTitle();
			Assert.IsTrue(editor.IsSelectionOnlyInTitle(), "cursor in title");

			// OneNote also marks the enclosing Title and OE as partially selected
			var title = page.Root.Element(Ns + "Title");
			title.SetAttributeValue("selected", "partial");
			title.Element(Ns + "OE").SetAttributeValue("selected", "partial");
			Assert.IsTrue(editor.IsSelectionOnlyInTitle(), "title and OE marked partial");

			page.Root.Descendants(Ns + "T")
				.First(t => t.Value == "body text")
				.SetAttributeValue("selected", "partial");
			Assert.IsFalse(editor.IsSelectionOnlyInTitle(), "selection also in body");
		}


		[TestMethod]
		public void SetCursorAtTitle_NoTitle_ReturnsFalse()
		{
			var root = new PageBuilder("page-1", "x").BuildElement();
			root.Element(Ns + "Title")?.Remove();
			var page = new Page(root);

			Assert.IsFalse(new PageEditor(page).SetCursorAtTitle());
		}
	}
}
