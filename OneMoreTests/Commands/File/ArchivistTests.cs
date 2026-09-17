//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.File
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;


	[TestClass]
	public class ArchivistTests
	{
		[TestMethod]
		public void InjectHeadingAnchors_WithSingleHeading_AddsSlugId()
		{
			var html = "<H1 lang=yo style=\"FONT-SIZE: 16pt\">Breadcrumb</H1>";

			var result = Archivist.InjectHeadingAnchorsInHtml(html);

			Assert.IsTrue(result.Contains("<H1 id=\"breadcrumb\" lang=yo style=\"FONT-SIZE: 16pt\">Breadcrumb</H1>"));
		}


		[TestMethod]
		public void InjectHeadingAnchors_WithDuplicateHeadingText_SuffixesSecondOccurrence()
		{
			var html = "<H1>Boxes</H1><P>filler</P><H1>Boxes</H1>";

			var result = Archivist.InjectHeadingAnchorsInHtml(html);

			Assert.IsTrue(result.Contains("<H1 id=\"boxes\">Boxes</H1>"));
			Assert.IsTrue(result.Contains("<H1 id=\"boxes-2\">Boxes</H1>"));
		}


		[TestMethod]
		public void InjectHeadingAnchors_WithNbspOnlyHeading_AddsNoId()
		{
			var html = "<H2 style=\"COLOR: #0080ff\">&nbsp;</H2>";

			var result = Archivist.InjectHeadingAnchorsInHtml(html);

			Assert.AreEqual(html, result);
		}


		[TestMethod]
		public void InjectHeadingAnchors_WithExistingId_LeavesHeadingUntouched()
		{
			var html = "<H1 id=\"already-set\">Breadcrumb</H1>";

			var result = Archivist.InjectHeadingAnchorsInHtml(html);

			Assert.AreEqual(html, result);
		}


		[TestMethod]
		public void InjectHeadingAnchors_WithNestedMarkup_SlugsPlainText()
		{
			var html = "<H1><SPAN style=\"FONT-WEIGHT:bold\">Boxes</SPAN></H1>";

			var result = Archivist.InjectHeadingAnchorsInHtml(html);

			Assert.IsTrue(result.StartsWith("<H1 id=\"boxes\">"));
		}


		// realistic onenote: href, carrying section-id/page-id/object-id - a naive
		// "already has an id" check (bare \bid\s*=) false-matches the "-id=" in these
		// attribute names and must not; see InjectFootnoteAnchors_WithFooterBacklink_AddsOmfnId
		private const string OnenoteHref =
			"onenote:#Page&amp;section-id={A640CEA0-536E-4ED0-ACC1-428AAB96501F}&amp;" +
			"page-id={660B56BC-B6BE-4791-B556-E4BC9BA2E60C}&amp;" +
			"object-id={3F8BE067-A5EA-4025-8F40-CDFD5DC1F59F}&amp;end";


		[TestMethod]
		public void InjectFootnoteAnchors_WithRefMarker_AddsOmfnrefId()
		{
			var html =
				$"<a href=\"{OnenoteHref}\"><span style=\"vertical-align:super\">[1]</span></a>";

			var result = Archivist.InjectFootnoteAnchorsInHtml(html);

			Assert.AreEqual(
				$"<a href=\"{OnenoteHref}\" id=\"omfnref-1\">" +
				"<span style=\"vertical-align:super\">[1]</span></a>",
				result);
		}


		[TestMethod]
		public void InjectFootnoteAnchors_WithFooterBacklink_AddsOmfnId()
		{
			var html =
				$"<a href=\"{OnenoteHref}\">" +
				"<span style=\"font-family:'Calibri Light';font-size:11.0pt\">[1]</span></a>";

			var result = Archivist.InjectFootnoteAnchorsInHtml(html);

			Assert.AreEqual(
				$"<a href=\"{OnenoteHref}\" id=\"omfn-1\">" +
				"<span style=\"font-family:'Calibri Light';font-size:11.0pt\">[1]</span></a>",
				result);
		}


		[TestMethod]
		public void InjectFootnoteAnchors_WithExistingId_LeavesUntouched()
		{
			var html = "<a href=\"\" id=\"already-set\"><span style=\"vertical-align:super\">[1]</span></a>";

			var result = Archivist.InjectFootnoteAnchorsInHtml(html);

			Assert.AreEqual(html, result);
		}


		[TestMethod]
		public void InjectFootnoteAnchors_WithOrdinaryLink_AddsNoId()
		{
			var html = "<a href=\"./Other.htm\">Some other page</a>";

			var result = Archivist.InjectFootnoteAnchorsInHtml(html);

			Assert.AreEqual(html, result);
		}


		[TestMethod]
		public void ResolveFootnoteAnchor_WithRefMarkerText_ReturnsOmfnId()
		{
			var linkText = "<span style=\"vertical-align:super\">[1]</span>";

			var result = Archivist.ResolveFootnoteAnchor(linkText);

			Assert.AreEqual("omfn-1", result);
		}


		[TestMethod]
		public void ResolveFootnoteAnchor_WithBacklinkText_ReturnsOmfnrefId()
		{
			var linkText = "<span style=\"font-family:'Calibri Light';font-size:11.0pt\">[1]</span>";

			var result = Archivist.ResolveFootnoteAnchor(linkText);

			Assert.AreEqual("omfnref-1", result);
		}


		[TestMethod]
		public void ResolveFootnoteAnchor_WithOrdinaryText_ReturnsNull()
		{
			var result = Archivist.ResolveFootnoteAnchor("Groovy Page");

			Assert.IsNull(result);
		}
	}
}
