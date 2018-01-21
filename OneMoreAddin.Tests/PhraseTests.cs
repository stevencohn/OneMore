//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreAddin.Tests
{
	using System.Linq;
	using System.Xml;
	using System.Xml.Linq;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn;


	[TestClass]
	public class PhraseTests
	{
		private string xml =
@"<one:T xmlns:one=""http://schemas.microsoft.com/office/onenote/2013/onenote"">
  <![CDATA[One <span style='font-weight:bold'>two </span><span style='font-style:italic'>three</span> four]]>
</one:T>";


		[TestMethod]
		public void FirstLast ()
		{
			var range = XElement.Parse(xml);

			var cdata = range.DescendantNodes()
				.Where(e => e.NodeType == XmlNodeType.CDATA)
				.First() as XCData;

			var phrase = new Phrase(cdata);

			Assert.IsNotNull(phrase);
			Assert.IsFalse(phrase.IsEmpty);
			Assert.IsTrue(phrase.ContainsMultipleWords);
			Assert.IsFalse(phrase.StartsWithSpace);
			Assert.IsFalse(phrase.EndsWithSpace);

			var word = phrase.ExtractFirstWord();
			Assert.IsNotNull(word);
			Assert.AreEqual("One ", word);

			word = phrase.ExtractFirstWord();
			Assert.IsNotNull(word);
			Assert.AreEqual("two ", word);

			word = phrase.ExtractLastWord();
			Assert.IsNotNull(word);
			Assert.AreEqual(" four", word);

			word = phrase.ExtractLastWord();
			Assert.IsNotNull(word);
			Assert.AreEqual("three", word);

			word = phrase.ExtractLastWord();
			Assert.IsNotNull(word);
			Assert.AreEqual(string.Empty, word);
		}


		[TestMethod]
		public void ClearFormatting ()
		{
			var range = XElement.Parse(xml);

			var cdata = range.DescendantNodes()
				.Where(e => e.NodeType == XmlNodeType.CDATA)
				.First() as XCData;

			var phrase = new Phrase(cdata);

			phrase.ClearFormatting();
			Assert.AreEqual("One two three four", phrase.CData.Value);
		}
	}
}
