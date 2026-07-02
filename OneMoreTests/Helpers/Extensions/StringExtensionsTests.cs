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
	}
}
