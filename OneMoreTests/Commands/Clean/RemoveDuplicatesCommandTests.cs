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
	 * Basic-depth matching fix:
	 *   1. Create 3+ unrelated non-empty pages in a test section, plus one true text-duplicate
	 *      pair (same visible text, different titles or attributes).
	 *   2. Invoke Clean/Remove Duplicate Pages with Basic depth.
	 *   3. Confirm only the true duplicate pair is grouped together; unrelated pages are not
	 *      grouped under the first page scanned.
	 *
	 * Deep-mode performance guard:
	 *   1. Add an image-heavy page (several MB of embedded pictures/ink) plus a near-duplicate
	 *      of it to a test section.
	 *   2. Invoke Clean/Remove Duplicate Pages with Deep depth.
	 *   3. Confirm the scan completes promptly and the pair's Distance column renders "-"
	 *      (too large to compare) rather than hanging.
	 *
	 * Near-duplicate detection:
	 *   1. Create two pages with matching structure but a few words changed.
	 *   2. Invoke Clean/Remove Duplicate Pages, Simple depth, with "Also detect similar
	 *      (non-identical) pages" checked.
	 *   3. Confirm the pages group under "Pages similar to X" with a similarity percentage
	 *      shown, and that true duplicates elsewhere show "Duplicates of X" with no percentage.
	 *
	 * Keep Newest bulk action:
	 *   1. Create 3 copies of a page in a test section, editing/saving each at a different time
	 *      so lastModifiedTime differs.
	 *   2. Run the scan, then click "Keep Newest" on that group in the results Navigator.
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


		[TestMethod]
		public void NormalizedSimilarity_ZeroDistance_ReturnsOne()
		{
			Assert.AreEqual(1.0, RemoveDuplicatesCommand.NormalizedSimilarity(0, 100, 100));
		}


		[TestMethod]
		public void NormalizedSimilarity_FullDistance_ReturnsZero()
		{
			Assert.AreEqual(0.0, RemoveDuplicatesCommand.NormalizedSimilarity(100, 100, 100));
		}


		[TestMethod]
		public void NormalizedSimilarity_PartialDistance_ReturnsExpectedRatio()
		{
			Assert.AreEqual(0.9, RemoveDuplicatesCommand.NormalizedSimilarity(10, 100, 100), 0.0001);
		}


		[TestMethod]
		public void NormalizedSimilarity_BothEmpty_ReturnsOne()
		{
			Assert.AreEqual(1.0, RemoveDuplicatesCommand.NormalizedSimilarity(0, 0, 0));
		}
	}
}
