//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Clean
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	/*
	 * Test Protocol
	 * Trims trailing whitespace from selected text or all text on the page
	 *
	 *     1. More/Clean/Trim Whitespace
	 *     2. Confirm whitespace is trimmed from Trailing paragraphs
	 *     3. More/Clean/Trim Leading Whitespace
	 *     Confirm leading whitespace is trimmed from Leading paragraphs
	 */

	[TestClass]
	public class TrimCommandTests : TestBase
	{
		private const string PageId = "page-1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		private static Task ExecuteCommand(bool leading)
		{
			var cmd = new TrimCommand();
			cmd.SetLogger(Logger.Current);
			return cmd.Execute(leading);
		}


		private static string GetText(XElement page, string marker)
		{
			return page
				.Descendants(Ns + "T")
				.Select(t => t.FirstNode as XCData)
				.FirstOrDefault(cd => cd != null && cd.Value.Contains(marker))
				?.Value;
		}


		[TestMethod]
		public async Task TrimTrailingWhitespace_TrimsTrailingKeepsLeading()
		{
			// Arrange: one paragraph with trailing whitespace to trim, one with leading
			// whitespace that a trailing-only trim must leave untouched
			var xml = new PageBuilder(PageId, "Trim Trailing Test")
				.WithParagraph("Lorem ipsum dolor sit amet   ")
				.WithParagraph("   Leading whitespace text")
				.Build();

			SetupPage(PageId, xml);

			// Act
			await ExecuteCommand(leading: false);

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var trailingText = GetText(updated, "Lorem ipsum");
			Assert.IsNotNull(trailingText, "Expected the trailing-whitespace paragraph to remain");
			Assert.AreEqual("Lorem ipsum dolor sit amet", trailingText,
				"Expected trailing whitespace to be trimmed");

			var leadingText = GetText(updated, "Leading whitespace text");
			Assert.IsNotNull(leadingText, "Expected the leading-whitespace paragraph to remain");
			Assert.AreEqual("   Leading whitespace text", leadingText,
				"Expected leading whitespace to be left untouched by a trailing-only trim");
		}


		[TestMethod]
		public async Task TrimLeadingWhitespace_TrimsLeadingKeepsTrailing()
		{
			// Arrange: one paragraph with leading whitespace to trim, one with trailing
			// whitespace that a leading-only trim must leave untouched
			var xml = new PageBuilder(PageId, "Trim Leading Test")
				.WithParagraph("   Lorem ipsum dolor sit amet")
				.WithParagraph("Trailing whitespace text   ")
				.Build();

			SetupPage(PageId, xml);

			// Act
			await ExecuteCommand(leading: true);

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var leadingText = GetText(updated, "Lorem ipsum");
			Assert.IsNotNull(leadingText, "Expected the leading-whitespace paragraph to remain");
			Assert.AreEqual("Lorem ipsum dolor sit amet", leadingText,
				"Expected leading whitespace to be trimmed");

			var trailingText = GetText(updated, "Trailing whitespace text");
			Assert.IsNotNull(trailingText, "Expected the trailing-whitespace paragraph to remain");
			Assert.AreEqual("Trailing whitespace text   ", trailingText,
				"Expected trailing whitespace to be left untouched by a leading-only trim");
		}


		[TestMethod]
		public async Task TrimTrailingWhitespace_OnlyLastRunInParagraphIsTrimmed()
		{
			// Arrange: a single OE with three T runs, mirroring a split selection range -
			// only the last T in the OE is eligible for trimming, and an empty CDATA run
			// (the cursor placeholder) in the middle must be skipped rather than throw.
			var oe = new XElement(Ns + "OE",
				new XElement(Ns + "T", new XCData("Lorem ipsum d   ")),
				new XElement(Ns + "T", new XCData("")),
				new XElement(Ns + "T", new XCData("olor sit amet   ")));

			var xml = new PageBuilder(PageId, "Split Run Test")
				.WithElement(oe)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await ExecuteCommand(leading: false);

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var runs = updated
				.Descendants(Ns + "T")
				.Select(t => (t.FirstNode as XCData)?.Value)
				.ToList();

			Assert.AreEqual("Lorem ipsum d   ", runs[0],
				"Expected the first run in the paragraph to be left untouched");
			Assert.AreEqual("olor sit amet", runs[2],
				"Expected only the last run in the paragraph to have trailing whitespace trimmed");
		}


		[TestMethod]
		public async Task TrimWhitespace_NoWhitespacePresent_DoesNotCallUpdate()
		{
			// Arrange: page with no leading or trailing whitespace anywhere
			var xml = new PageBuilder(PageId, "No Whitespace Test")
				.WithParagraph("Lorem ipsum dolor sit amet")
				.WithParagraph("Another clean paragraph")
				.Build();

			SetupPage(PageId, xml);
			var originalXml = Mock.GetPage(PageId);

			// Act
			await ExecuteCommand(leading: false);

			// Assert: page XML is unchanged because UpdatePageContent was never called
			var storedXml = Mock.GetPage(PageId);
			Assert.AreEqual(originalXml, storedXml,
				"Page should not have been updated when there is no whitespace to trim");
		}
	}
}
