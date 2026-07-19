//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.References
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	/*
	 * Test Protocol
	 * Commands/References/EmbedCommand
	 *
	 * Embeds the body of a page, or a tag-delimited section of a page, into the current
	 * page. The embedded content is wrapped in a single-cell table with a Refresh link.
	 *
	 * These unit tests exercise the pure-XML extraction helpers directly:
	 *
	 *   FindTagIndex  — locates the first OE whose text contains a tag string
	 *   ExtractTaggedContent — returns OEChildren blocks for the OEs between two tags,
	 *     scanning a single outline's paragraphs for one or more begin/end blocks
	 *
	 * Manual integration tests (require OneNote):
	 *   1. Copy Link to Page for a source page; invoke Embed Page command.
	 *   2. Confirm embedded content appears in a bordered table with a Refresh link.
	 *   3. Edit the source page; click Refresh — confirm content updates.
	 *   4. Repeat with begin/end tags on the source page.
	 *   5. Run: OneMoreCli Embed --notebook <n> --section <s> --refresh true
	 *      Confirm each page in the section that has embeds is refreshed.
	 *   6. Repeat with multiple begin/end blocks across multiple outlines on the source
	 *      page; confirm all blocks are embedded, in outline and document order, and
	 *      that an outline with a begin tag but no matching end tag embeds through the
	 *      end of that outline.
	 */

	[TestClass]
	public class EmbedCommandTests : TestBase
	{
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		// ========================================================================================
		// FindTagIndex

		[TestMethod]
		public void FindTagIndex_MatchesCaseInsensitively()
		{
			// Arrange: tag stored in lower-case, search with upper-case
			var oes = MakeOes("Alpha", "START", "Beta");

			// Act
			var idx = EmbedCommand.FindTagIndex(oes, "start", 0);

			// Assert
			Assert.AreEqual(1, idx);
		}


		[TestMethod]
		public void FindTagIndex_TagNotPresent_ReturnsMinusOne()
		{
			var oes = MakeOes("Alpha", "Beta", "Gamma");

			var idx = EmbedCommand.FindTagIndex(oes, "START", 0);

			Assert.AreEqual(-1, idx);
		}


		[TestMethod]
		public void FindTagIndex_RespectsStartFrom()
		{
			// Two OEs contain the tag; startFrom skips the first.
			var oes = MakeOes("START", "Middle", "START again");

			var idx = EmbedCommand.FindTagIndex(oes, "start", 1);

			Assert.AreEqual(2, idx);
		}


		// ========================================================================================
		// ExtractTaggedContent

		[TestMethod]
		public void ExtractTaggedContent_BeginAndEndTag_ReturnsSlice()
		{
			// Arrange: 5 paragraphs; slice is [1..3] (tags at 0 and 4 excluded)
			var oeChildren = MakeOeChildren("intro", "START", "line1", "line2", "END", "outro");

			// Act
			var result = EmbedCommand.ExtractTaggedContent(Ns, oeChildren, "START", "END");

			// Assert
			Assert.IsNotNull(result);
			var sliced = result.Single();
			var texts = sliced.Elements(Ns + "OE")
				.Select(oe => oe.Element(Ns + "T")?.Value)
				.ToList();

			CollectionAssert.AreEqual(new[] { "line1", "line2" }, texts);
		}


		[TestMethod]
		public void ExtractTaggedContent_BeginTagOnly_ReturnsFromTagToEnd()
		{
			// No end tag — expect everything after the begin tag
			var oeChildren = MakeOeChildren("intro", "START", "line1", "line2");

			var result = EmbedCommand.ExtractTaggedContent(Ns, oeChildren, "START", null);

			Assert.IsNotNull(result);
			var sliced = result.Single();
			var texts = sliced.Elements(Ns + "OE")
				.Select(oe => oe.Element(Ns + "T")?.Value)
				.ToList();

			CollectionAssert.AreEqual(new[] { "line1", "line2" }, texts);
		}


		[TestMethod]
		public void ExtractTaggedContent_EndTagOnly_ReturnsFromStartToTag()
		{
			// No begin tag — expect everything before the end tag
			var oeChildren = MakeOeChildren("line1", "line2", "END", "outro");

			var result = EmbedCommand.ExtractTaggedContent(Ns, oeChildren, null, "END");

			Assert.IsNotNull(result);
			var sliced = result.Single();
			var texts = sliced.Elements(Ns + "OE")
				.Select(oe => oe.Element(Ns + "T")?.Value)
				.ToList();

			CollectionAssert.AreEqual(new[] { "line1", "line2" }, texts);
		}


		[TestMethod]
		public void ExtractTaggedContent_MissingBeginTag_ReturnsNull()
		{
			var oeChildren = MakeOeChildren("line1", "line2", "END");

			var result = EmbedCommand.ExtractTaggedContent(Ns, oeChildren, "START", "END");

			Assert.IsNull(result);
		}


		[TestMethod]
		public void ExtractTaggedContent_BeginFoundEndMissing_RunsToEndOfOutline()
		{
			// Begin tag found but no matching end tag in this outline — the block
			// runs from the begin tag to the end of the outline instead of failing.
			var oeChildren = MakeOeChildren("START", "line1", "line2");

			var result = EmbedCommand.ExtractTaggedContent(Ns, oeChildren, "START", "END");

			Assert.IsNotNull(result);
			var sliced = result.Single();
			var texts = sliced.Elements(Ns + "OE")
				.Select(oe => oe.Element(Ns + "T")?.Value)
				.ToList();

			CollectionAssert.AreEqual(new[] { "line1", "line2" }, texts);
		}


		[TestMethod]
		public void ExtractTaggedContent_MultipleBeginEndPairs_ReturnsAllBlocks()
		{
			var oeChildren = MakeOeChildren(
				"intro", "START", "a", "END", "mid", "START", "b", "END", "outro");

			var result = EmbedCommand.ExtractTaggedContent(Ns, oeChildren, "START", "END");

			Assert.IsNotNull(result);
			var texts = result
				.Select(block => string.Concat(block.Elements(Ns + "OE")
					.Select(oe => oe.Element(Ns + "T")?.Value)))
				.ToList();

			CollectionAssert.AreEqual(new[] { "a", "b" }, texts);
		}


		[TestMethod]
		public void ExtractTaggedContent_TrailingUnmatchedBegin_RunsToEndOfOutline()
		{
			// First block is closed by an END tag; the second begin tag has no
			// matching end tag, so it runs to the end of the outline.
			var oeChildren = MakeOeChildren("START", "a", "END", "mid", "START", "b");

			var result = EmbedCommand.ExtractTaggedContent(Ns, oeChildren, "START", "END");

			Assert.IsNotNull(result);
			var texts = result
				.Select(block => string.Concat(block.Elements(Ns + "OE")
					.Select(oe => oe.Element(Ns + "T")?.Value)))
				.ToList();

			CollectionAssert.AreEqual(new[] { "a", "b" }, texts);
		}


		[TestMethod]
		public void ExtractTaggedContent_TagsInvertedOrder_ReturnsNull()
		{
			// END appears before START — nothing to extract
			var oeChildren = MakeOeChildren("END", "line1", "START");

			var result = EmbedCommand.ExtractTaggedContent(Ns, oeChildren, "START", "END");

			Assert.IsNull(result);
		}


		// ========================================================================================
		// Helpers

		private static List<XElement> MakeOes(params string[] texts)
		{
			return texts.Select(t => new XElement(Ns + "OE",
				new XElement(Ns + "T", new XCData(t)))).ToList();
		}


		private static XElement MakeOeChildren(params string[] texts)
		{
			var oeChildren = new XElement(Ns + "OEChildren");
			foreach (var t in texts)
			{
				oeChildren.Add(new XElement(Ns + "OE",
					new XElement(Ns + "T", new XCData(t))));
			}

			return oeChildren;
		}
	}
}
