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
	 * Commands/Clean/RemoveCitationsCommand
	 *
	 * Removes citations (From: URL) that OneNote auto-generates when you paste screen
	 * clipping and parts of Web pages into OneNote
	 *
	 * 	1. More/Clean/Remove Pasted Citations
	 * 	2. Confirm citation is removed ("From <https...")
	 */

	[TestClass]
	public class RemoveCitationsCommandTests : TestBase
	{
		private const string PageId = "page-1";
		private const string CiteIndex = "1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		private static XElement MakeCiteStyle()
		{
			return new XElement(Ns + "QuickStyleDef",
				new XAttribute("index", CiteIndex),
				new XAttribute("name", "cite"));
		}


		private static Task ExecuteCommand()
		{
			var cmd = new RemoveCitationsCommand();
			cmd.SetLogger(Logger.Current);
			return cmd.Execute();
		}


		[TestMethod]
		public async Task RemoveCitations_FromUrlCitation_RemovesCitationParagraph()
		{
			// Arrange: a "cite"-styled paragraph whose CDATA is OneNote's auto-generated
			// "From <https://...>" citation, mixing literal HTML (the anchor) with escaped
			// angle brackets around it, as OneNote emits after pasting a web clipping.
			var citeOe = new XElement(Ns + "OE",
				new XAttribute("quickStyleIndex", CiteIndex),
				new XElement(Ns + "T",
					new XCData("From &lt;<a href=\"https://example.com\">https://example.com</a>&gt;")));

			var otherOe = new XElement(Ns + "OE",
				new XElement(Ns + "T", new XCData("Regular paragraph text")));

			var pageElement = new PageBuilder(PageId, "Citation Test")
				.WithElement(otherOe)
				.WithElement(citeOe)
				.BuildElement();

			pageElement.AddFirst(MakeCiteStyle());

			SetupPage(PageId, pageElement.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act
			await ExecuteCommand();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var texts = updated.Descendants(Ns + "T")
				.Select(t => t.GetCData().Value)
				.ToList();

			Assert.IsFalse(texts.Any(t => t.Contains("From &lt;")),
				"Expected the citation paragraph to be removed");
			Assert.IsTrue(texts.Any(t => t.Contains("Regular paragraph text")),
				"Expected the unrelated paragraph to remain");
		}


		[TestMethod]
		public async Task RemoveCitations_ScreenClippingCitation_RemovesCitationParagraph()
		{
			// Arrange: a "cite"-styled paragraph containing OneNote's "Screen clipping" text,
			// the other trigger phrase the command matches on (independent of the URL regex).
			var citeOe = new XElement(Ns + "OE",
				new XAttribute("quickStyleIndex", CiteIndex),
				new XElement(Ns + "T",
					new XCData("Screen clipping taken from https://example.com")));

			var pageElement = new PageBuilder(PageId, "Screen Clipping Test")
				.WithElement(citeOe)
				.BuildElement();

			pageElement.AddFirst(MakeCiteStyle());

			SetupPage(PageId, pageElement.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act
			await ExecuteCommand();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			Assert.IsFalse(updated.Descendants(Ns + "T")
				.Any(t => t.GetCData().Value.Contains("Screen clipping")),
				"Expected the screen clipping citation to be removed");
		}


		[TestMethod]
		public async Task RemoveCitations_TextMatchesButNotCiteStyled_IsPreserved()
		{
			// Arrange: citation-shaped text on a paragraph that does NOT carry the "cite"
			// quick style index — the command must only act on cite-styled paragraphs.
			var plainOe = new XElement(Ns + "OE",
				new XElement(Ns + "T",
					new XCData("From &lt;<a href=\"https://example.com\">https://example.com</a>&gt;")));

			var pageElement = new PageBuilder(PageId, "Non-Cite Test")
				.WithElement(plainOe)
				.BuildElement();

			pageElement.AddFirst(MakeCiteStyle());

			SetupPage(PageId, pageElement.ToString(SaveOptions.OmitDuplicateNamespaces));
			var originalXml = Mock.GetPage(PageId);

			// Act
			await ExecuteCommand();

			// Assert: no cite-styled paragraph matched, so nothing was removed and no update occurred
			Assert.AreEqual(originalXml, Mock.GetPage(PageId),
				"Page should not be updated when the matching text is not cite-styled");
		}


		[TestMethod]
		public async Task RemoveCitations_NoCiteStyleDefined_DoesNotUpdate()
		{
			// Arrange: page has no "cite" QuickStyleDef at all, e.g. a page that has never had
			// a citation pasted onto it.
			var xml = new PageBuilder(PageId, "No Cite Style Test")
				.WithParagraph("Just some regular text")
				.Build();

			SetupPage(PageId, xml);
			var originalXml = Mock.GetPage(PageId);

			// Act
			await ExecuteCommand();

			// Assert
			Assert.AreEqual(originalXml, Mock.GetPage(PageId),
				"Page should not be updated when there is no 'cite' quick style");
		}


		[TestMethod]
		public async Task RemoveCitations_CiteStyledParagraphWithoutCitationText_DoesNotUpdate()
		{
			// Arrange: a "cite"-styled paragraph exists, but its text doesn't match either
			// the "Screen clipping" phrase or the "From <url>" regex.
			var citeOe = new XElement(Ns + "OE",
				new XAttribute("quickStyleIndex", CiteIndex),
				new XElement(Ns + "T", new XCData("Just a plain caption")));

			var pageElement = new PageBuilder(PageId, "No Match Test")
				.WithElement(citeOe)
				.BuildElement();

			pageElement.AddFirst(MakeCiteStyle());

			SetupPage(PageId, pageElement.ToString(SaveOptions.OmitDuplicateNamespaces));
			var originalXml = Mock.GetPage(PageId);

			// Act
			await ExecuteCommand();

			// Assert
			Assert.AreEqual(originalXml, Mock.GetPage(PageId),
				"Page should not be updated when the cite-styled paragraph has no citation text");
		}
	}
}
