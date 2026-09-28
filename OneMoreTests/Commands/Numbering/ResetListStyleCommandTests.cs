//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Numbering
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Tests.Builders;


	/*
	 * Test Protocol
	 * Commands/Numbering/ResetListStyle
	 * Remove stray styling from list numbers and bullets and apply the Normal font and size
	 *
	 *  1. Import a markdown file with numbered list items that start with `inline code`,
	 *     or otherwise give the numbers of a list a different font, size, color and bold
	 *  2. Place the cursor in one of the list items, then Numbering/Reset List Styling
	 *  3. Confirm every number in the list, including nested items, now uses the font and
	 *     size of the Normal style and is not bold or colored
	 *  4. Revert. Select two of the list items, then Numbering/Reset List Styling
	 *  5. Confirm only the numbers of the selected items were reset
	 */

	[TestClass]
	public class ResetListStyleCommandTests : TestBase
	{
		private const string PageId = "page-1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		// builds the marker as OneNote's HTML importer leaves it for an item that
		// starts with an inline code span
		private static XElement StyledNumber(int number)
		{
			return new XElement(Ns + "Number",
				new XAttribute("numberSequence", "0"),
				new XAttribute("numberFormat", "##."),
				new XAttribute("fontColor", "#000000"),
				new XAttribute("fontSize", "9.0"),
				new XAttribute("font", "Consolas"),
				new XAttribute("bold", "true"),
				new XAttribute("italic", "true"),
				new XAttribute("text", $"{number}."));
		}


		private static XElement StyledBullet()
		{
			return new XElement(Ns + "Bullet",
				new XAttribute("bullet", "2"),
				new XAttribute("fontColor", "#000000"),
				new XAttribute("fontSize", "9.0"));
		}


		// an empty text with selected=true is the text cursor; non-empty is a selected run
		private static XElement Item(
			string id, XElement marker, string text, bool selected = false, XElement child = null)
		{
			var run = new XElement(Ns + "T", new XCData(text));
			if (selected)
			{
				run.SetAttributeValue("selected", "all");
			}

			var oe = new XElement(Ns + "OE",
				new XAttribute("objectID", id),
				new XElement(Ns + "List", marker),
				run);

			if (child is not null)
			{
				oe.Add(new XElement(Ns + "OEChildren", child));
			}

			return oe;
		}


		private static string BuildPage(params XElement[] items)
		{
			var page = new PageBuilder(PageId, "Reset List Style Test").BuildElement();

			page.AddFirst(new XElement(Ns + "QuickStyleDef",
				new XAttribute("index", "0"),
				new XAttribute("name", "p"),
				new XAttribute("font", "Calibri"),
				new XAttribute("fontSize", "11.5"),
				new XAttribute("spaceBefore", "0.0"),
				new XAttribute("spaceAfter", "0.0")));

			page.Add(new XElement(Ns + "Outline",
				new XElement(Ns + "OEChildren", items)));

			return page.ToString(SaveOptions.OmitDuplicateNamespaces);
		}


		private XElement Marker(XElement updated, string id)
		{
			return updated.Descendants(Ns + "OE")
				.First(e => (string)e.Attribute("objectID") == id)
				.Element(Ns + "List")
				.Elements()
				.First();
		}


		private static void AssertReset(XElement marker)
		{
			Assert.AreEqual("Calibri", (string)marker.Attribute("font"));
			Assert.AreEqual("11.5", (string)marker.Attribute("fontSize"));
			Assert.IsNull(marker.Attribute("fontColor"), "fontColor should be removed");
			Assert.IsNull(marker.Attribute("bold"), "bold should be removed");
			Assert.IsNull(marker.Attribute("italic"), "italic should be removed");
		}


		private static void AssertUntouched(XElement marker)
		{
			Assert.AreEqual("Consolas", (string)marker.Attribute("font"));
			Assert.AreEqual("9.0", (string)marker.Attribute("fontSize"));
			Assert.AreEqual("true", (string)marker.Attribute("bold"));
		}


		[TestMethod]
		public async Task ResetListStyle_Cursor_ResetsWholeListIncludingNested()
		{
			var nested = Item("oe-2a", StyledNumber(1), "nested");

			SetupPage(PageId, BuildPage(
				Item("oe-1", StyledNumber(1), string.Empty, selected: true),
				Item("oe-2", StyledNumber(2), "two", child: nested),
				Item("oe-3", StyledNumber(3), "three")));

			await new ResetListStyleCommand().Execute();

			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			AssertReset(Marker(updated, "oe-1"));
			AssertReset(Marker(updated, "oe-2"));
			AssertReset(Marker(updated, "oe-2a"));
			AssertReset(Marker(updated, "oe-3"));
		}


		[TestMethod]
		public async Task ResetListStyle_Cursor_LeavesOtherListsUntouched()
		{
			var page = XElement.Parse(BuildPage(
				Item("oe-1", StyledNumber(1), string.Empty, selected: true)));

			// a second, unrelated list in another outline
			page.Add(new XElement(Ns + "Outline",
				new XElement(Ns + "OEChildren",
					Item("oe-other", StyledNumber(1), "other"))));

			SetupPage(PageId, page.ToString(SaveOptions.OmitDuplicateNamespaces));

			await new ResetListStyleCommand().Execute();

			var updated = GetUpdatedPage(PageId);
			AssertReset(Marker(updated, "oe-1"));

			// Update drops outlines it did not modify, so the other list is either
			// absent from the saved page or present and unchanged, never reset
			var other = updated.Descendants(Ns + "OE")
				.FirstOrDefault(e => (string)e.Attribute("objectID") == "oe-other");

			if (other is not null)
			{
				AssertUntouched(Marker(updated, "oe-other"));
			}
		}


		[TestMethod]
		public async Task ResetListStyle_Range_ResetsOnlySelectedItems()
		{
			SetupPage(PageId, BuildPage(
				Item("oe-1", StyledNumber(1), "one"),
				Item("oe-2", StyledNumber(2), "two", selected: true),
				Item("oe-3", StyledNumber(3), "three", selected: true),
				Item("oe-4", StyledNumber(4), "four")));

			await new ResetListStyleCommand().Execute();

			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			AssertUntouched(Marker(updated, "oe-1"));
			AssertReset(Marker(updated, "oe-2"));
			AssertReset(Marker(updated, "oe-3"));
			AssertUntouched(Marker(updated, "oe-4"));
		}


		[TestMethod]
		public async Task ResetListStyle_PreservesStructuralAttributes()
		{
			SetupPage(PageId, BuildPage(
				Item("oe-1", StyledNumber(7), string.Empty, selected: true)));

			await new ResetListStyleCommand().Execute();

			var marker = Marker(GetUpdatedPage(PageId), "oe-1");
			Assert.AreEqual("##.", (string)marker.Attribute("numberFormat"));
			Assert.AreEqual("0", (string)marker.Attribute("numberSequence"));
			Assert.AreEqual("7.", (string)marker.Attribute("text"));
		}


		[TestMethod]
		public async Task ResetListStyle_Bullet_GetsSizeButNoFont()
		{
			SetupPage(PageId, BuildPage(
				Item("oe-1", StyledBullet(), string.Empty, selected: true)));

			await new ResetListStyleCommand().Execute();

			var marker = Marker(GetUpdatedPage(PageId), "oe-1");
			Assert.AreEqual("11.5", (string)marker.Attribute("fontSize"));
			Assert.IsNull(marker.Attribute("font"), "a bullet has no font attribute");
			Assert.IsNull(marker.Attribute("fontColor"), "fontColor should be removed");
			Assert.AreEqual("2", (string)marker.Attribute("bullet"));
		}
	}
}
