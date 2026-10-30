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


		// Reproduces GitHub #1425: mathML equations are stored as a one:T whose CDATA is
		// entirely an XML comment. XElement.Value skips comment content, so a wildcard regex
		// (e.g. ".*") that legitimately matches everywhere else also produces a zero-length
		// match against the comment's empty rawtext, which used to flow into Replace() and
		// NullReferenceException inside AtomicFactory/ElementAtom. The mathML run must be
		// skipped entirely and left byte-for-byte untouched.
		[TestMethod]
		public void SearchAndReplace_MathMLEquation_IsNotModified()
		{
			const string mathml =
				"<!--[if mathML]><math xmlns=\"http://www.w3.org/1998/Math/MathML\" " +
				"display=\"block\"><mi>A</mi><mo>=</mo><mi>&#120587;</mi><msup><mi>r</mi>" +
				"<mn>2</mn></msup></math><![endif]-->";

			var mathT = new XElement(Ns + "T", new XCData(mathml));
			var textT = new XElement(Ns + "T", new XCData("hello world"));

			var oe = new XElement(Ns + "OE",
				new XAttribute("objectID", "{OE1}"),
				new XAttribute("lastModifiedTime", "2026-01-01T00:00:00.000Z"),
				mathT,
				textT);

			var page = new PageBuilder().WithElement(oe).BuildElement();
			var paragraph = page.Elements(Ns + "Outline").Descendants(Ns + "OE").First();

			// wildcard regex matches (and would otherwise zero-length-match the empty
			// rawtext of the mathML comment)
			var editor = new SearchAndReplaceEditor(@".*", "X", true, false);

			editor.SearchAndReplace(paragraph);

			var runs = paragraph.Elements(Ns + "T").ToList();
			Assert.AreEqual(mathml, runs[0].GetCData().Value,
				"mathML CDATA must remain byte-for-byte unchanged");
			Assert.AreNotEqual("hello world", runs[1].GetCData().Value,
				"plain text run should still have been replaced");
		}


		private static string Highlight(string cdata, string pattern, out int count)
		{
			var t = new XElement(Ns + "T", new XCData(cdata));

			var oe = new XElement(Ns + "OE",
				new XAttribute("objectID", "{OE1}"),
				new XAttribute("lastModifiedTime", "2026-01-01T00:00:00.000Z"),
				t);

			var page = new PageBuilder().WithElement(oe).BuildElement();
			var paragraph = page.Elements(Ns + "Outline").Descendants(Ns + "OE").First();

			var editor = new SearchAndReplaceEditor(pattern, false, false, "#FFFF00");
			count = editor.SearchAndReplace(paragraph);

			return paragraph.Element(Ns + "T").GetCData().Value;
		}


		[TestMethod]
		public void Highlight_PlainText_WrapsMatchOnly()
		{
			var result = Highlight("hello world", "world", out var count);

			Assert.AreEqual(1, count);
			Assert.AreEqual("hello <span style=\"background:#FFFF00\">world</span>", result);
		}


		[TestMethod]
		public void Highlight_StyledSpan_KeepsExistingStyles()
		{
			var result = Highlight("<span style='font-weight:bold'>world</span>", "world", out _);

			StringAssert.Contains(result.ToLowerInvariant(), "bold");
			StringAssert.Contains(result.ToUpperInvariant(), "FFFF00");
		}


		[TestMethod]
		public void Highlight_ExistingBackground_IsReplaced()
		{
			var result = Highlight(
				"<span style='background:#00FF00;font-style:italic'>world</span>", "world", out _);

			Assert.IsFalse(result.ToUpperInvariant().Contains("00FF00"));
			StringAssert.Contains(result.ToUpperInvariant(), "FFFF00");
			StringAssert.Contains(result.ToLowerInvariant(), "italic");
		}


		[TestMethod]
		public void Highlight_PartialSpan_LeavesRestUnhighlighted()
		{
			var result = Highlight(
				"<span style='font-weight:bold'>hello world</span>", "world", out _);

			StringAssert.Contains(result.ToLowerInvariant(), "bold");
			// consecutive sibling spans, not a span nested in a span, so OneNote cannot fold
			// the neighboring text into the highlight
			StringAssert.Contains(result, "hello </span>");
			StringAssert.Contains(result, ">world</span>");
			Assert.IsFalse(result.Contains("</span></span>"));
			Assert.AreEqual(1, System.Text.RegularExpressions.Regex.Matches(
				result.ToUpperInvariant(), "FFFF00").Count);
		}


		[TestMethod]
		public void Highlight_TrailingSpace_IsNotLeftAsAStyledRunOfItsOwn()
		{
			var result = Highlight(
				"<span style='font-weight:bold'>Searching </span><span style='font-style:italic'>line</span>",
				"Searching", out _);

			// OneNote folds a whitespace-only span into the highlighted run before it
			Assert.IsFalse(result.Contains("> </span>"), result);
			StringAssert.Contains(result, "> line</span>");
			Assert.AreEqual(1, System.Text.RegularExpressions.Regex.Matches(
				result.ToUpperInvariant(), "FFFF00").Count);
		}


		[TestMethod]
		public void Highlight_MatchSpanningSpans_HighlightsBothParts()
		{
			var result = Highlight("hel<span style='font-weight:bold'>lo wo</span>rld", "llo wo", out var count);

			Assert.AreEqual(1, count);
			Assert.AreEqual(2, System.Text.RegularExpressions.Regex.Matches(
				result.ToUpperInvariant(), "FFFF00").Count);
		}
	}
}
