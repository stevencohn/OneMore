//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Clean
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;

	/*
	 * Test Protocol - RemoveDuplicatesCommand
	 *
	 * Exact-match AND-based grouping:
	 *   1. Create a true text-and-XML duplicate pair (identical export) plus a pair that shares
	 *      visible text but differs in an embedded image.
	 *   2. Invoke Clean/Remove Duplicate Pages with the Media metric checked and "Also detect
	 *      similar" checked.
	 *   3. Confirm the true duplicate shows the fixed "100% · identical" chip, while the
	 *      text-identical/image-different pair instead shows a clickable, scored chip whose
	 *      popup reports a low Media rubric score.
	 *
	 * Near-duplicate detection:
	 *   1. Create two pages with matching structure but a few words changed.
	 *   2. Invoke Clean/Remove Duplicate Pages with "Also detect similar (non-identical) pages"
	 *      checked and the default metrics selected.
	 *   3. Confirm the pages group under "Pages similar to X" with a clickable similarity chip,
	 *      and that true duplicates elsewhere show "Duplicates of X" with the fixed chip.
	 *
	 * Keep Newest bulk action:
	 *   1. Create 3 copies of a page in a test section, editing/saving each at a different time
	 *      so lastModifiedTime differs.
	 *   2. Run the scan, then click "Keep Newest" on that group in the results dialog.
	 *   3. Confirm only the two older copies are queued for deletion (with the usual confirm
	 *      prompt) and the newest page survives.
	 */

	[TestClass]
	public class RemoveDuplicatesCommandTests
	{
		[TestMethod]
		public void PassesLengthPrefilter_EqualLengths_ReturnsTrue()
		{
			Assert.IsTrue(RemoveDuplicatesCommand.PassesLengthPrefilter(100, 100, 0.85));
		}


		[TestMethod]
		public void PassesLengthPrefilter_WithinThreshold_ReturnsTrue()
		{
			// 10% shorter is within a 15% allowed dissimilarity (1.0 - 0.85)
			Assert.IsTrue(RemoveDuplicatesCommand.PassesLengthPrefilter(100, 90, 0.85));
		}


		[TestMethod]
		public void PassesLengthPrefilter_BeyondThreshold_ReturnsFalse()
		{
			// 50% shorter is far beyond a 15% allowed dissimilarity
			Assert.IsFalse(RemoveDuplicatesCommand.PassesLengthPrefilter(100, 50, 0.85));
		}


		[TestMethod]
		public void PassesLengthPrefilter_EitherEmpty_ReturnsFalse()
		{
			Assert.IsFalse(RemoveDuplicatesCommand.PassesLengthPrefilter(0, 100, 0.85));
			Assert.IsFalse(RemoveDuplicatesCommand.PassesLengthPrefilter(100, 0, 0.85));
			Assert.IsFalse(RemoveDuplicatesCommand.PassesLengthPrefilter(0, 0, 0.85));
		}
	}
}
