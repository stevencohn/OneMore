//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Page
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;


	[TestClass]
	public class WordCountCommandTests
	{
		[TestMethod]
		public void CountWords_PlainText_CountsSpaceSeparatedWords()
		{
			Assert.AreEqual(2, WordCountCommand.CountWords("Hello world"));
		}


		[TestMethod]
		public void CountWords_Contraction_CountsAsSingleWord()
		{
			// "He's" must count as one word, not two ("He", "s")
			Assert.AreEqual(4, WordCountCommand.CountWords("He's a good friend"));
		}


		[TestMethod]
		public void CountWords_CurlyApostropheContraction_CountsAsSingleWord()
		{
			// OneNote/Word autocorrect straight quotes to curly apostrophes (U+2019)
			Assert.AreEqual(2, WordCountCommand.CountWords("It’s working"));
		}


		[TestMethod]
		public void CountWords_HyphenatedCompound_CountsAsSingleWord()
		{
			// "one-two-three" must count as one word, not three
			Assert.AreEqual(4, WordCountCommand.CountWords("one-two-three is a test"));
		}


		[TestMethod]
		public void CountWords_PunctuatedSentence_IgnoresSurroundingPunctuation()
		{
			Assert.AreEqual(2, WordCountCommand.CountWords("Hello, world!"));
		}


		[TestMethod]
		public void CountWords_TrailingHyphen_DoesNotAbsorbNeighboringWord()
		{
			// a dangling connector (nothing on one side) must not merge unrelated words
			Assert.AreEqual(2, WordCountCommand.CountWords("well- known"));
		}


		[TestMethod]
		public void CountWords_EmptyOrWhitespace_ReturnsZero()
		{
			Assert.AreEqual(0, WordCountCommand.CountWords(string.Empty));
			Assert.AreEqual(0, WordCountCommand.CountWords(null));
		}


		[TestMethod]
		public void CountWords_MixedCjkAndLatin_CountsBothSegments()
		{
			// CJK text is segmented via dictionary tokenization; combined with a Latin word,
			// the total must be greater than the Latin-only count and must not throw
			var cjkOnly = WordCountCommand.CountWords("你好世界");
			var mixed = WordCountCommand.CountWords("你好世界 hello");

			Assert.AreEqual(cjkOnly + 1, mixed);
		}
	}
}
