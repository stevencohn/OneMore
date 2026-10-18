//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Models
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using OnePage = River.OneMoreAddIn.Models.Page;


	[TestClass]
	public class PageHeadingsTests
	{
		[TestMethod]
		public void ExtractHeadingText_CommentWithDoubleHyphen_DiscardsComment()
		{
			var text = OnePage.ExtractHeadingText("one <!-- two -- three --> four");

			Assert.AreEqual("one   four", text);
		}


		[TestMethod]
		public void ExtractHeadingText_CommentEndingWithHyphen_DoesNotThrow()
		{
			var text = OnePage.ExtractHeadingText("one <!-- two --->");

			Assert.AreEqual("one  ", text);
		}


		[TestMethod]
		public void ExtractHeadingText_UnterminatedComment_DoesNotThrow()
		{
			var text = OnePage.ExtractHeadingText("one <!-- two -- three");

			Assert.AreEqual("one  ", text);
		}


		[TestMethod]
		public void ExtractHeadingText_NoComment_Unchanged()
		{
			Assert.AreEqual("Hello world", OnePage.ExtractHeadingText("Hello <b>world</b>"));
		}


		[TestMethod]
		public void ExtractHeadingText_ConditionalCommentMarkup_DoesNotThrow()
		{
			var text = OnePage.ExtractHeadingText("a <!--[if gte mso 9]><xml>b<![endif]--> c");

			Assert.IsTrue(text.Contains("a") && text.Contains("c"));
			Assert.IsFalse(text.Contains("b"));
		}


		[TestMethod]
		public void ExtractHeadingText_MathMLWithDoubleHyphen_YieldsNoEquationText()
		{
			// issue 2785: mathML equations are an XML comment; their text must never reach a TOC
			var text = OnePage.ExtractHeadingText(
				"Title <!--[if mathML]><math><mi>x</mi><mo>--</mo><mi>y</mi></math><![endif]-->");

			Assert.IsFalse(text.Contains("mathML"));
			Assert.IsFalse(text.Contains("endif"));
			Assert.IsTrue(text.StartsWith("Title"));
		}
	}
}
