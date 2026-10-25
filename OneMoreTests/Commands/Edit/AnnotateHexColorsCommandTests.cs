//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Edit
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using System.Drawing;
	using System.Xml.Linq;

	/*
	 * Test Protocol
	 * Commands/Edit/AnnotateHexColorsCommand
	 *
	 * 	1. On a page with the text below, place the cursor anywhere and run Annotate Hex Colors
	 * 	2. Confirm every value is prefaced by a colored box in Segoe UI 10pt, with a space
	 * 	3. Run again and confirm no duplicate boxes are added
	 * 	4. Select only the second line and run on a fresh copy; confirm only it is annotated
	 *
	 * red #FF0000 and blue #0000ff
	 * white #FFFFFF and black #000000
	 */

	[TestClass]
	public class AnnotateHexColorsCommandTests
	{
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		private static XElement Run(string content)
		{
			return new XElement(Ns + "T", new XCData(content));
		}


		private static string Annotate(string content, Color background, out bool updated)
		{
			var run = Run(content);
			updated = new AnnotateHexColorsCommand().Annotate(new[] { run }, background);
			return ((XCData)run.FirstNode).Value;
		}


		[TestMethod]
		public void PlainValue_GetsSwatchSpan()
		{
			var result = Annotate("color #336699 here", Color.White, out var updated);

			Assert.IsTrue(updated);
			Assert.AreEqual(
				"color <span style=\"font-family:'Segoe UI';font-size:10.0pt;color:#336699\">" +
				"\u25A0 </span>#336699 here",
				result);
		}


		[TestMethod]
		public void StyledSpan_PreservesStyleOnSplitPieces()
		{
			var result = Annotate(
				"<span style=\"font-family:Consolas\">a #336699 b</span>",
				Color.White, out var updated);

			Assert.IsTrue(updated);
			StringAssert.Contains(result, "<span style=\"font-family:Consolas\">a </span>");
			StringAssert.Contains(result, "<span style=\"font-family:Consolas\">#336699 b</span>");
		}


		[TestMethod]
		public void MultipleValues_AllAnnotated()
		{
			var result = Annotate("#FF0000 and #0000ff", Color.White, out _);

			StringAssert.Contains(result, "color:#FF0000\">\u25A0 </span>#FF0000");
			StringAssert.Contains(result, "color:#0000ff\">\u25A0 </span>#0000ff");
		}


		[TestMethod]
		public void AlreadyAnnotated_IsSkipped_AndSecondPassIsNoop()
		{
			var first = Annotate("x #336699", Color.White, out _);

			var run = Run(first);
			var updated = new AnnotateHexColorsCommand().Annotate(new[] { run }, Color.White);

			Assert.IsFalse(updated);
			Assert.AreEqual(first, ((XCData)run.FirstNode).Value);
		}


		[TestMethod]
		public void TypedBoxPrefix_IsSkipped()
		{
			Annotate("\u25A0 #336699", Color.White, out var updated);
			Assert.IsFalse(updated);
		}


		[TestMethod]
		public void WhiteOnWhite_UsesOutlineInBlack()
		{
			var result = Annotate("#FFFFFF", Color.White, out _);
			StringAssert.Contains(result, "color:#000000\">\u25A1 </span>");
		}


		[TestMethod]
		public void BlackOnBlack_UsesOutlineInWhite()
		{
			var result = Annotate("#000000", Color.Black, out _);
			StringAssert.Contains(result, "color:#FFFFFF\">\u25A1 </span>");
		}


		[TestMethod]
		public void BlackOnWhite_UsesSolid()
		{
			var result = Annotate("#000000", Color.White, out _);
			StringAssert.Contains(result, "color:#000000\">\u25A0 </span>");
		}


		[TestMethod]
		public void NonMatches_AreIgnored()
		{
			Annotate("#123 #12345 #1234567 #12345g a#336699", Color.White, out var updated);
			Assert.IsFalse(updated);
		}
	}
}
