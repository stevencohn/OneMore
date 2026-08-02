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
	}
}
