//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Clean
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using River.OneMoreAddIn;

	/*
	 * Test Protocol - ClearFormattingCommand
	 * Clear all text styling but keep bullets and numbers. With only a cursor, clear the
	 * current paragraph; otherwise clear only the selected text.
	 *
	 *   1. Place the cursor in a bold, colored, large bullet item; Invoke Clean/Clear Formatting
	 *   2. Confirm the text is plain default font/size and the bullet remains
	 *   3. Select part of a paragraph with mixed styles; Invoke Clean/Clear Formatting
	 *   4. Confirm only the selected text is cleared
	 */

	[TestClass]
	public class ClearFormattingCommandTests : TestBase
	{
		private const string PageId = "page-1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		private static Task ExecuteCommand()
		{
			var cmd = new ClearFormattingCommand();
			cmd.SetLogger(Logger.Current);
			return cmd.Execute();
		}


		[TestMethod]
		public async Task ClearFormatting_CursorInListItem_ClearsParagraphKeepsList()
		{
			var oe = new XElement(Ns + "OE",
				new XAttribute("style", "font-family:Arial;font-size:20.0pt;color:#FF0000"),
				new XElement(Ns + "List", new XElement(Ns + "Bullet", new XAttribute("bullet", "2"))),
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData("")),
				new XElement(Ns + "T",
					new XCData("<span style='font-weight:bold;color:#0000FF'>styled</span> text")));

			var xml = new PageBuilder(PageId, "Clear Formatting Test").WithElement(oe).Build();
			SetupPage(PageId, xml);

			await ExecuteCommand();

			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var result = updated.Descendants(Ns + "Outline").Descendants(Ns + "OE").First();
			Assert.IsNull(result.Attribute("style"));
			Assert.IsNotNull(result.Element(Ns + "List")?.Element(Ns + "Bullet"));

			var cdata = result.Elements(Ns + "T").Last().FirstNode as XCData;
			Assert.AreEqual("styled text", cdata.Value);
		}


		[TestMethod]
		public async Task ClearFormatting_PartialSelection_ClearsOnlySelectedRun()
		{
			var oe = new XElement(Ns + "OE",
				new XElement(Ns + "T",
					new XCData("<span style='font-weight:bold'>keep</span>")),
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData("<span style='font-style:italic'>clear</span>")));

			var xml = new PageBuilder(PageId, "Clear Formatting Test").WithElement(oe).Build();
			SetupPage(PageId, xml);

			await ExecuteCommand();

			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var runs = updated.Descendants(Ns + "Outline").Descendants(Ns + "T")
				.Select(t => (XCData)t.FirstNode).ToList();
			Assert.IsTrue(runs[0].Value.Contains("font-weight:bold"));
			Assert.AreEqual("clear", runs[1].Value);
		}


		[TestMethod]
		public async Task ClearFormatting_NoSelection_DoesNotUpdate()
		{
			var oe = new XElement(Ns + "OE",
				new XElement(Ns + "T",
					new XCData("<span style='font-weight:bold'>bold</span>")));

			var xml = new PageBuilder(PageId, "Clear Formatting Test").WithElement(oe).Build();
			SetupPage(PageId, xml);

			await ExecuteCommand();

			// the mock returns the original page when never updated, so verify nothing changed
			var page = GetUpdatedPage(PageId);
			var cdata = page.Descendants(Ns + "Outline").Descendants(Ns + "T")
				.Select(t => (XCData)t.FirstNode).First();
			Assert.IsTrue(cdata.Value.Contains("font-weight:bold"));
		}
	}
}
