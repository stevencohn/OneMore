//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Search
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Linq;
	using System.Xml.Linq;


	[TestClass]
	public class SearchAndReplaceEditorTests
	{
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		// Reproduces GitHub #1683 / #2528: a hashtag styled by a prior scan (or wrapped in a
		// link, as originally reported) leaves a T run whose top-level node is a SPAN/anchor
		// containing more than one child node. ElementAtom.Length reports the length of the
		// element's full concatenated text, but Replace() used to delegate straight to the
		// first child node's own (shorter) text, causing TextAtom.Replace to Substring() out
		// of range and crash the hashtag service.
		[TestMethod]
		public void SearchAndReplace_HashtagsInsideAnchorWithMultipleSpans_DoesNotThrow()
		{
			var replacement = new XElement("span",
				new XAttribute("style", "color:red"),
				"$1");

			var editor = new SearchAndReplaceEditor(@"(#\w+)", replacement, true, false);

			var t = new XElement(Ns + "T", new XCData(
				"<a href=\"c:\">aaa " +
				"<span style='background:#FFFF99'>#abb</span> " +
				"<span style='background:#FFFF99'>#acc</span></a>"));

			var oe = new XElement(Ns + "OE",
				new XAttribute("objectID", "{OE1}"),
				new XAttribute("lastModifiedTime", "2026-01-01T00:00:00.000Z"),
				t);

			var page = new PageBuilder().WithElement(oe).BuildElement();
			var paragraph = page.Elements(Ns + "Outline").Descendants(Ns + "OE").First();

			var count = editor.SearchAndReplace(paragraph);

			Assert.AreEqual(2, count);

			var text = paragraph.Element(Ns + "T").GetCData().Value;
			Assert.AreEqual(2, System.Text.RegularExpressions.Regex.Matches(
				text, "color:red").Count);
		}
	}
}
