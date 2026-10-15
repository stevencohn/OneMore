//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Search
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;


	[TestClass]
	public class TitleQueryParserTests
	{
		[TestMethod]
		public void ExtractNotebook_CurrentNotebook_ReturnsBackslash()
		{
			var filter = TitleQueryParser.ExtractNotebook("foo \\\\", out var remaining);

			Assert.AreEqual("\\", filter);
			Assert.AreEqual("foo ", remaining);
		}


		[TestMethod]
		public void ExtractNotebook_Named_ReturnsName()
		{
			var filter = TitleQueryParser.ExtractNotebook("\\work foo", out var remaining);

			Assert.AreEqual("work", filter);
			Assert.AreEqual(" foo", remaining);
		}


		[TestMethod]
		public void ExtractNotebook_Quoted_ReturnsFullName()
		{
			var filter = TitleQueryParser.ExtractNotebook("foo \\\"my notes\"", out _);

			Assert.AreEqual("my notes", filter);
		}


		[TestMethod]
		public void ExtractNotebook_EmbeddedBackslash_IsLiteral()
		{
			var filter = TitleQueryParser.ExtractNotebook("a\\b", out var remaining);

			Assert.IsNull(filter);
			Assert.AreEqual("a\\b", remaining);
		}


		[TestMethod]
		public void Parse_TitleTextWithHashtag_SeparatesTitleAndHashtag()
		{
			var query = TitleQueryParser.Parse("OneM #noparent");

			Assert.AreEqual("OneM", query.TitleText);
			CollectionAssert.AreEqual(new[] { "#noparent" }, query.Hashtags);
			Assert.AreEqual(0, query.ExcludeHashtags.Count);
		}


		[TestMethod]
		public void Parse_AndBeforeHashtag_IsIgnoredNotMergedIntoTitleText()
		{
			var query = TitleQueryParser.Parse("OneM AND #noparent");

			Assert.AreEqual("OneM", query.TitleText);
			CollectionAssert.AreEqual(new[] { "#noparent" }, query.Hashtags);
		}


		[TestMethod]
		public void Parse_OrBeforeHashtag_IsIgnoredNotMergedIntoTitleText()
		{
			var query = TitleQueryParser.Parse("OneM OR #noparent");

			Assert.AreEqual("OneM", query.TitleText);
			CollectionAssert.AreEqual(new[] { "#noparent" }, query.Hashtags);
		}


		[TestMethod]
		public void Parse_NotBeforeHashtag_IsIgnoredNotTreatedAsExclusion()
		{
			// "NOT" is discarded like AND/OR, not treated as negation - only the "-#tag"
			// prefix excludes a hashtag
			var query = TitleQueryParser.Parse("OneM NOT #noparent");

			Assert.AreEqual("OneM", query.TitleText);
			CollectionAssert.AreEqual(new[] { "#noparent" }, query.Hashtags);
			Assert.AreEqual(0, query.ExcludeHashtags.Count);
		}


		[TestMethod]
		public void Parse_NegatedHashtag_GoesToExcludeHashtags()
		{
			var query = TitleQueryParser.Parse("OneM -#scratch");

			Assert.AreEqual("OneM", query.TitleText);
			Assert.AreEqual(0, query.Hashtags.Count);
			CollectionAssert.AreEqual(new[] { "#scratch" }, query.ExcludeHashtags);
		}


		[TestMethod]
		public void Parse_NegatedHashtagAlone_LeavesTitleTextEmpty()
		{
			var query = TitleQueryParser.Parse("-#scratch");

			Assert.AreEqual(string.Empty, query.TitleText);
			CollectionAssert.AreEqual(new[] { "#scratch" }, query.ExcludeHashtags);
		}


		[TestMethod]
		public void Parse_MixOfIncludeAndExcludeHashtags_SeparatesEachList()
		{
			var query = TitleQueryParser.Parse("#a #b -#c");

			CollectionAssert.AreEqual(new[] { "#a", "#b" }, query.Hashtags);
			CollectionAssert.AreEqual(new[] { "#c" }, query.ExcludeHashtags);
		}


		[TestMethod]
		public void Parse_HyphenGluedToWord_IsNotTreatedAsNegation()
		{
			// the hyphen isn't at a word boundary, so "well-#tag" isn't recognized as a
			// hashtag token at all and is left as literal title text
			var query = TitleQueryParser.Parse("well-#tag");

			Assert.AreEqual("well-#tag", query.TitleText);
			Assert.AreEqual(0, query.Hashtags.Count);
			Assert.AreEqual(0, query.ExcludeHashtags.Count);
		}
	}
}
