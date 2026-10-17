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
		public void ExtractHeadingText_CommentWithDoubleHyphen_KeepsCommentText()
		{
			var text = OnePage.ExtractHeadingText("one <!-- two -- three --> four");

			Assert.AreEqual("one  two -- three  four", text);
		}


		[TestMethod]
		public void ExtractHeadingText_CommentEndingWithHyphen_DoesNotThrow()
		{
			var text = OnePage.ExtractHeadingText("one <!-- two --->");

			Assert.AreEqual("one  two -", text);
		}


		[TestMethod]
		public void ExtractHeadingText_UnterminatedComment_DoesNotThrow()
		{
			var text = OnePage.ExtractHeadingText("one <!-- two -- three");

			Assert.AreEqual("one  two -- three", text);
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
		}
	}
}
