//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Edit
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Linq;
	using System.Text.RegularExpressions;
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
		public async Task Highlight_SecondCall_UsesNextColor()
		{
			// Arrange: page with two paragraphs; only the first is selected
			var pageXml = new PageBuilder(PageId, "Rotation Test")
				.WithParagraph("First paragraph", selected: true)
				.WithParagraph("Second paragraph", selected: false)
				.Build();

			SetupPage(PageId, pageXml);

			// Act 1: first highlight → color index 0
			await new HighlightCommand().Execute(1);

			var afterFirst = GetUpdatedPage(PageId);
			Assert.IsNotNull(afterFirst, "First UpdatePageContent was never called");

			// Extract the first color from the CDATA of the highlighted T
			var firstColor = ExtractHighlightColor(afterFirst);
			Assert.IsFalse(string.IsNullOrEmpty(firstColor), "First call should produce a highlight color");

			// Arrange 2: swap selection — deselect all, then select the second paragraph.
			// The mock holds the optimized page from the first call; we adjust selection in-place.
			afterFirst.Descendants(Ns + "T").Attributes("selected").Remove();

			// The second paragraph T has plain CDATA (no background span in its CDATA)
			var secondT = afterFirst
				.Descendants(Ns + "T")
				.First(t => t.FirstNode is XCData cd &&
					cd.Value.IndexOf("background:", System.StringComparison.Ordinal) < 0);

			secondT.SetAttributeValue("selected", "all");

			// Store the modified page back so the next GetPage call returns it
			Mock.SetPage(PageId, afterFirst.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act 2: second highlight → color index 1 (one step forward from what was stored in Meta)
			await new HighlightCommand().Execute(1);

			var afterSecond = GetUpdatedPage(PageId);
			Assert.IsNotNull(afterSecond, "Second UpdatePageContent was never called");

			// Collect all distinct background colors on the page
			var colors = afterSecond
				.Descendants(Ns + "T")
				.Where(t => t.FirstNode is XCData cd &&
					cd.Value.IndexOf("background:", System.StringComparison.Ordinal) >= 0)
				.Select(t => ExtractHighlightColor(t.Parent))
				.Distinct()
				.ToList();

			Assert.AreEqual(2, colors.Count,
				"Expected two distinct paragraphs highlighted in different colors");

			Assert.AreNotEqual(colors[0], colors[1],
				$"Expected different colors on second highlight but both were '{colors[0]}'");
		}


		private static string ExtractHighlightColor(XElement context)
		{
			// Extract the first background color value from any CDATA in the given context.
			// Format inside CDATA: <span style="background:#RRGGBB">...</span>
			var cdata = context
				.DescendantsAndSelf()
				.Select(e => e.FirstNode as XCData)
				.FirstOrDefault(cd => cd != null &&
					cd.Value.IndexOf("background:", System.StringComparison.Ordinal) >= 0);

			if (cdata == null) return string.Empty;

			var match = Regex.Match(cdata.Value, @"background:([^;""<\s]+)");
			return match.Success ? match.Groups[1].Value : string.Empty;
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
