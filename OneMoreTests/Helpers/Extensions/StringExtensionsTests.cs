//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Helpers.Extensions
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using System.Linq;


	[TestClass]
	public class StringExtensionsTests
	{
		[TestMethod]
		public void StripInvalidXmlChars_RemovesControlCharacters()
		{
			// 0x08 (backspace) and 0x0B (vertical tab) are illegal in XML 1.0
			var input = $"before{(char)0x08}middle{(char)0x0B}after";
			var result = input.StripInvalidXmlChars();

			Assert.AreEqual("beforemiddleafter", result);
		}


		[TestMethod]
		public void StripInvalidXmlChars_PreservesLegalWhitespace()
		{
			var input = "line1\tline2\r\nline3";
			var result = input.StripInvalidXmlChars();

			Assert.AreEqual(input, result);
		}


		[TestMethod]
		public void ToXmlWrapper_WithRawControlCharacter_DoesNotThrowAndStripsChar()
		{
			var input = $"some text with a backspace{(char)0x08} char";

			var element = input.ToXmlWrapper();

			Assert.IsFalse(element.Value.Contains((char)0x08));
			Assert.AreEqual("some text with a backspace char", element.Value);
		}


		[TestMethod]
		public void ToSlug_WithPlainText_LowercasesAndHyphenates()
		{
			Assert.AreEqual("breadcrumb", "Breadcrumb".ToSlug());
			Assert.AreEqual("table-of-contents-page", "Table of Contents (Page)".ToSlug());
		}


		[TestMethod]
		public void ToSlug_WithNbspOnlyText_ReturnsEmpty()
		{
			Assert.AreEqual(string.Empty, "&nbsp;".ToSlug());
			Assert.AreEqual(string.Empty, "   ".ToSlug());
			Assert.AreEqual(string.Empty, string.Empty.ToSlug());
			Assert.AreEqual(string.Empty, ((string)null).ToSlug());
		}


		[TestMethod]
		public void ToSlug_WithHtmlEntities_DecodesBeforeSlugging()
		{
			Assert.AreEqual("r-d", "R&amp;D".ToSlug());
		}


		[TestMethod]
		public void DistanceFrom_BothEmpty_ReturnsZero()
		{
			Assert.AreEqual(0, string.Empty.DistanceFrom(string.Empty));
		}


		[TestMethod]
		public void DistanceFrom_OneEmpty_ReturnsLengthOfOther()
		{
			Assert.AreEqual(5, string.Empty.DistanceFrom("hello"));
			Assert.AreEqual(5, "hello".DistanceFrom(string.Empty));
		}


		[TestMethod]
		public void DistanceFrom_EqualStrings_ReturnsZero()
		{
			Assert.AreEqual(0, "hello".DistanceFrom("hello"));
		}


		[TestMethod]
		public void DistanceFrom_SingleSubstitution_ReturnsOne()
		{
			Assert.AreEqual(1, "hello".DistanceFrom("hallo"));
		}


		[TestMethod]
		public void DistanceFrom_SingleInsertOrDelete_ReturnsOne()
		{
			Assert.AreEqual(1, "hello".DistanceFrom("helloo"));
			Assert.AreEqual(1, "hello".DistanceFrom("hell"));
		}
	}
}
