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
	using System.Xml;
	using System.Xml.Linq;


	[TestClass]
	public class HighlightCommandTests : TestBase
	{
		private const string PageId = "page-1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		[TestMethod]
		public async Task Highlight_SelectedText_WrapsInSpanWithBackground()
		{
			// Arrange: page with one selected text run
			var xml = new PageBuilder(PageId, "Highlight Test")
				.WithParagraph("Hello world", selected: true)
				.Build();

			SetupPage(PageId, xml);

			// Act: highlight with increment=1 (apply next color)
			await new HighlightCommand().Execute(1);

			// Assert: page was updated and the CDATA content contains a background color.
			// HighlightCommand stores the result inside the CDATA of the T element, not as a
			// child XElement — the CDATA format is: <span style="background:#RRGGBB">text</span>
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var hasBg = updated
				.Descendants(Ns + "T")
				.Where(t => t.FirstNode?.NodeType == XmlNodeType.CDATA)
				.Any(t => ((XCData)t.FirstNode).Value.IndexOf(
					"background:", System.StringComparison.Ordinal) >= 0);

			Assert.IsTrue(hasBg, "Expected CDATA to contain a background color after highlight");
		}


		[TestMethod]
		public async Task Highlight_SelectedTextWithExistingHighlight_RemovesHighlight()
		{
			// Arrange: page with selected T whose CDATA already contains a highlighted span.
			// This is the real OneNote format for highlighted text: content lives inside the CDATA.
			var oe = new XElement(Ns + "OE",
				new XElement(Ns + "T",
					new XAttribute("selected", "all"),
					new XCData("<span style=\"background:#FFFF00\">Hello world</span>")));

			var pageXml = new PageBuilder(PageId, "Highlight Remove Test")
				.WithElement(oe)
				.Build();

			SetupPage(PageId, pageXml);

			// Act: increment=-1 replaces background with 'transparent' (remove highlight)
			await new HighlightCommand().Execute(-1);

			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			// The CDATA should now carry 'transparent' instead of the original color
			var cdataValues = updated
				.Descendants(Ns + "T")
				.Where(t => t.FirstNode?.NodeType == XmlNodeType.CDATA)
				.Select(t => ((XCData)t.FirstNode).Value)
				.ToList();

			Assert.IsTrue(cdataValues.Any(), "Expected at least one T with CDATA");
			Assert.IsTrue(
				cdataValues.All(v =>
					v.IndexOf("transparent", System.StringComparison.OrdinalIgnoreCase) >= 0
					|| v.IndexOf("background:", System.StringComparison.Ordinal) < 0),
				"Expected all background references to use 'transparent'");
		}


		[TestMethod]
		public async Task Highlight_NoSelection_DoesNotCallUpdate()
		{
			// Arrange: page with NO selected text — EditSelected should return false,
			// so UpdatePageContent should never be called.
			var xml = new PageBuilder(PageId, "No Selection Test")
				.WithParagraph("Unselected text", selected: false)
				.Build();

			SetupPage(PageId, xml);
			var originalXml = Mock.GetPage(PageId);

			// Act
			await new HighlightCommand().Execute(1);

			// Assert: page XML should be unchanged (UpdatePageContent not called)
			var storedXml = Mock.GetPage(PageId);
			Assert.AreEqual(originalXml, storedXml,
				"Page should not have been updated when nothing is selected");
		}
	}
}
