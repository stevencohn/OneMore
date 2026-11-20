//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Images
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using System.Collections.Generic;
	using Measure = River.OneMoreAddIn.Commands.WrapTextAroundImageCommand.Measure;

	/*
	 * Test Protocol - WrapTextAroundImageCommand
	 * Text following a left or right aligned image is wrapped around the image.
	 *
	 * Test Steps:
	 *     1. Insert a picture, align it right, and size it to about 40% of the outline width.
	 *        Type several paragraphs of text after it. Select the image and invoke
	 *        Images/Wrap Text Around Image.
	 *     2. The image appears in the right column of a borderless two-column table and the
	 *        first few paragraphs appear in the left column, roughly as tall as the image.
	 *     3. Repeat with a left aligned image; the columns are mirrored.
	 *     4. Select a centered image; an error asks you to align it left or right.
	 *     5. Select an image with no text following it; an info box says there is no text.
	 *     6. Ctrl+Z restores the original page.
	 *     7. Put a blank line between the image and the first paragraph. If dropping the blank
	 *        line lets one more paragraph fit beside the image, the blank line is removed;
	 *        otherwise it stays at the top of the text column.
	 */

	[TestClass]
	public class WrapTextAroundImageCommandTests
	{
		private static Measure Text(double height) => new(height, false);
		private static Measure Blank(double height) => new(height, true);


		private static int Fit(List<Measure> list, double target)
		{
			return WrapTextAroundImageCommand.CountThatFit(list, target, out _);
		}


		[TestMethod]
		public void EstimateHeightIsLinesTimesLineHeight()
		{
			Assert.AreEqual(14.6, WrapTextAroundImageCommand.EstimateHeight(1, 14.6), 0.001);
			Assert.AreEqual(43.8, WrapTextAroundImageCommand.EstimateHeight(3, 14.6), 0.001);
		}


		[TestMethod]
		public void EstimateHeightHasAtLeastOneLine()
		{
			Assert.AreEqual(14.6, WrapTextAroundImageCommand.EstimateHeight(0, 14.6), 0.001);
		}


		[TestMethod]
		public void ToleranceAllowsEndingOneLineBelowTarget()
		{
			// 102.4 + 14.6 = 117.0 is 4.0 over a 113 target; allowed with a 14.6 tolerance
			var list = new List<Measure> { Text(43.9), Blank(14.6), Text(29.3), Blank(14.6), Text(14.6) };
			Assert.AreEqual(3,
				WrapTextAroundImageCommand.CountThatFit(list, 113, out _, 0));
			Assert.AreEqual(5,
				WrapTextAroundImageCommand.CountThatFit(list, 113, out _, 14.6));
		}


		[TestMethod]
		public void ToleranceDoesNotAllowMoreThanOneLine()
		{
			var list = new List<Measure> { Text(40), Text(40), Text(40) };
			Assert.AreEqual(2,
				WrapTextAroundImageCommand.CountThatFit(list, 70, out _, 14.6));
		}


		[TestMethod]
		public void NoParagraphs()
		{
			Assert.AreEqual(0, Fit(new List<Measure>(), 100));
		}


		[TestMethod]
		public void OnlyBlanks()
		{
			var list = new List<Measure> { Blank(14), Blank(14) };
			Assert.AreEqual(0, Fit(list, 100));
		}


		[TestMethod]
		public void TakesWholeParagraphsThatFit()
		{
			var list = new List<Measure> { Text(40), Text(40), Text(40) };
			Assert.AreEqual(2, Fit(list, 90));
		}


		[TestMethod]
		public void ExactFitIsIncluded()
		{
			var list = new List<Measure> { Text(40), Text(40), Text(40) };
			Assert.AreEqual(2, Fit(list, 80));
		}


		[TestMethod]
		public void AlwaysTakesFirstParagraphEvenIfTooTall()
		{
			var list = new List<Measure> { Text(200), Text(20) };
			Assert.AreEqual(1, Fit(list, 100));
		}


		[TestMethod]
		public void DoesNotEndOnBlank()
		{
			// text, blank, text(too tall): cut falls after the blank so the blank is dropped
			var list = new List<Measure> { Text(40), Blank(14), Text(80) };
			Assert.AreEqual(1, Fit(list, 60));
		}


		[TestMethod]
		public void KeepsInteriorBlanks()
		{
			var list = new List<Measure> { Text(30), Blank(14), Text(30), Blank(14), Text(80) };
			Assert.AreEqual(3, Fit(list, 90));
		}


		[TestMethod]
		public void LeadingBlankIsKeptWithFirstText()
		{
			var list = new List<Measure> { Blank(14), Text(200), Text(20) };
			var count = WrapTextAroundImageCommand.CountThatFit(list, 100, out var skip);
			Assert.AreEqual(0, skip);
			Assert.AreEqual(2, count);
		}


		[TestMethod]
		public void NoLeadingBlankSkipsNothing()
		{
			var list = new List<Measure> { Text(40), Text(40) };
			var count = WrapTextAroundImageCommand.CountThatFit(list, 100, out var skip);
			Assert.AreEqual(0, skip);
			Assert.AreEqual(2, count);
		}


		[TestMethod]
		public void LeadingBlankDroppedWhenItAddsParagraph()
		{
			// keep: blank+text fits (70) but second text overflows (110) => 1 text paragraph
			// drop: both text paragraphs fit (80) => 2 text paragraphs
			var list = new List<Measure> { Blank(30), Text(40), Text(40) };
			var count = WrapTextAroundImageCommand.CountThatFit(list, 80, out var skip);
			Assert.AreEqual(1, skip);
			Assert.AreEqual(2, count);
		}


		[TestMethod]
		public void LeadingBlankKeptOnTie()
		{
			// both layouts fit two text paragraphs, so the blank stays
			var list = new List<Measure> { Blank(10), Text(40), Text(40) };
			var count = WrapTextAroundImageCommand.CountThatFit(list, 90, out var skip);
			Assert.AreEqual(0, skip);
			Assert.AreEqual(3, count);
		}


		[TestMethod]
		public void MultipleLeadingBlanksDropped()
		{
			var list = new List<Measure> { Blank(14), Blank(14), Text(40), Text(40) };
			var count = WrapTextAroundImageCommand.CountThatFit(list, 80, out var skip);
			Assert.AreEqual(2, skip);
			Assert.AreEqual(2, count);
		}
	}
}
