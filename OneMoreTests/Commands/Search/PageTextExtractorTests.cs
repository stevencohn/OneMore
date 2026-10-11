//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Search
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Xml.Linq;


	[TestClass]
	public class PageTextExtractorTests
	{
		private const string Ns = "http://schemas.microsoft.com/office/onenote/2013/onenote";


		private static Page MakePage(string outlineContent)
		{
			var xml =
				$"<one:Page xmlns:one=\"{Ns}\" ID=\"{{p}}\" name=\"t\">" +
				$"<one:Outline objectID=\"{{outline}}\"><one:OEChildren>{outlineContent}</one:OEChildren></one:Outline>" +
				"</one:Page>";

			return new Page(XElement.Parse(xml));
		}


		private static string OE(string objectID, string text, string extra = "")
		{
			return $"<one:OE objectID=\"{objectID}\"><one:T><![CDATA[{text}]]></one:T>{extra}</one:OE>";
		}


		[TestMethod]
		public void Extract_ReturnsParagraphsWithTextInOrder()
		{
			var page = MakePage(OE("{a}", "first") + OE("{b}", "second") + OE("{c}", "third"));

			var paragraphs = PageTextExtractor.Extract(page);

			Assert.AreEqual(3, paragraphs.Count);
			CollectionAssert.AreEqual(
				new[] { "{a}", "{b}", "{c}" }, paragraphs.Select(p => p.ObjectID).ToArray());
			CollectionAssert.AreEqual(
				new[] { "first", "second", "third" }, paragraphs.Select(p => p.Text).ToArray());
			CollectionAssert.AreEqual(new[] { 0, 1, 2 }, paragraphs.Select(p => p.Ordinal).ToArray());
		}


		[TestMethod]
		public void Extract_StripsSpanAndAnchorMarkup()
		{
			var page = MakePage(OE("{a}", "plain <span style='color:red'>red</span> and <a href='x'>link</a> end"));

			var paragraphs = PageTextExtractor.Extract(page);

			Assert.AreEqual("plain red and link end", paragraphs.Single().Text);
		}


		[TestMethod]
		public void Extract_SkipsParagraphsWithoutText_AndNumbersTheRest()
		{
			var page = MakePage(OE("{a}", "one") + OE("{b}", "   ") + OE("{c}", "three"));

			var paragraphs = PageTextExtractor.Extract(page);

			Assert.AreEqual(2, paragraphs.Count);
			Assert.AreEqual(1, paragraphs[1].Ordinal);
			Assert.AreEqual("{c}", paragraphs[1].ObjectID);
		}


		[TestMethod]
		public void Extract_TocParagraphs_ExcludedUnlessRequested()
		{
			var toc = OE("{toc}", "Table of Contents", $"<one:Meta name=\"omToc\" content=\"x\" />");
			var page = MakePage(OE("{a}", "real") + toc);

			Assert.AreEqual(1, PageTextExtractor.Extract(page).Count);
			Assert.AreEqual(2, PageTextExtractor.Extract(page, includeToc: true).Count);
		}


		[TestMethod]
		public void Extract_InsertedFileParagraph_UsesTheFileNameAndPath()
		{
			var file =
				"<one:OE objectID=\"{f}\"><one:InsertedFile pathSource=\"C:\\docs\\a.pdf\" preferredName=\"a.pdf\" /></one:OE>";

			var page = MakePage(OE("{a}", "text") + file);

			var paragraphs = PageTextExtractor.Extract(page);

			Assert.AreEqual(2, paragraphs.Count);
			StringAssert.Contains(paragraphs[1].Text, "C:\\docs\\a.pdf");
			StringAssert.Contains(paragraphs[1].Text, "a.pdf");
		}


		private static string Image(string objectID, string ocrText, string attributes = "")
		{
			var id = objectID is null ? string.Empty : $" objectID=\"{objectID}\"";
			return $"<one:Image{id}{attributes}>" +
				$"<one:OCRData lang=\"en-US\"><one:OCRText><![CDATA[{ocrText}]]></one:OCRText></one:OCRData>" +
				"</one:Image>";
		}


		[TestMethod]
		public void Extract_Image_OmittedUnlessRequested()
		{
			var page = MakePage(
				OE("{a}", "text") +
				$"<one:OE objectID=\"{{img}}\">{Image("{pic}", "from the picture")}</one:OE>");

			Assert.AreEqual(1, PageTextExtractor.Extract(page).Count);

			var all = PageTextExtractor.Extract(page, includeOcr: true);
			Assert.AreEqual(2, all.Count);
			Assert.AreEqual(ParagraphKind.Image, all[1].Kind);
			Assert.AreEqual("{pic}", all[1].ObjectID);
			Assert.AreEqual("from the picture", all[1].Text);
		}


		[TestMethod]
		public void Extract_Image_IsOrderedAmongTheParagraphsAroundIt()
		{
			var page = MakePage(
				OE("{a}", "first") +
				$"<one:OE objectID=\"{{img}}\">{Image("{pic}", "middle")}</one:OE>" +
				OE("{c}", "last"));

			var items = PageTextExtractor.Extract(page, includeOcr: true);

			CollectionAssert.AreEqual(
				new[] { "first", "middle", "last" }, items.Select(p => p.Text).ToArray());
			CollectionAssert.AreEqual(new[] { 0, 1, 2 }, items.Select(p => p.Ordinal).ToArray());
		}


		[TestMethod]
		public void Extract_Image_WithoutObjectID_UsesTheParagraphHoldingIt()
		{
			var page = MakePage($"<one:OE objectID=\"{{holder}}\">{Image(null, "no id")}</one:OE>");

			var item = PageTextExtractor.Extract(page, includeOcr: true).Single();

			Assert.AreEqual("{holder}", item.ObjectID);
		}


		[TestMethod]
		public void Extract_Image_CollapsesLinesAndKeepsMarkupAsText()
		{
			var page = MakePage(
				$"<one:OE objectID=\"{{img}}\">{Image("{pic}", "line one\r\nline <span>two</span>\n\n  three")}</one:OE>");

			var item = PageTextExtractor.Extract(page, includeOcr: true).Single();

			Assert.AreEqual("line one line <span>two</span> three", item.Text);
		}


		[TestMethod]
		public void Extract_Image_WithoutOrWithEmptyText_IsSkipped()
		{
			var page = MakePage(
				"<one:OE objectID=\"{plain}\"><one:Image objectID=\"{p1}\" /></one:OE>" +
				$"<one:OE objectID=\"{{blank}}\">{Image("{p2}", "  \r\n ")}</one:OE>");

			Assert.AreEqual(0, PageTextExtractor.Extract(page, includeOcr: true).Count);
		}


		[TestMethod]
		public void Extract_NestedParagraphs_AreAllFound()
		{
			var nested =
				$"<one:OE objectID=\"{{parent}}\"><one:T><![CDATA[parent]]></one:T>" +
				$"<one:OEChildren>{OE("{child}", "child")}</one:OEChildren></one:OE>";

			var paragraphs = PageTextExtractor.Extract(MakePage(nested));

			CollectionAssert.AreEquivalent(
				new[] { "parent", "child" }, paragraphs.Select(p => p.Text).ToArray());
		}
	}
}
