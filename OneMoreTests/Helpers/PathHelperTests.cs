//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Helpers
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using System.IO;


	[TestClass]
	public class PathHelperTests
	{
		[TestMethod]
		public void BuildSectionFolderPath_NoSectionGroups_ReturnsSectionFolderOnly()
		{
			var path = PathHelper.BuildSectionFolderPath(
				@"C:\exports", new string[0], "SectionX");

			Assert.AreEqual(Path.Combine(@"C:\exports", "(SectionX)"), path);
		}


		[TestMethod]
		public void BuildSectionFolderPath_OneSectionGroup_NestsGroupAndSection()
		{
			var path = PathHelper.BuildSectionFolderPath(
				@"C:\exports", new[] { "GroupA" }, "SectionX");

			var expected = Path.Combine(
				Path.Combine(@"C:\exports", "[GroupA]"), "(SectionX)");

			Assert.AreEqual(expected, path);
		}


		[TestMethod]
		public void BuildSectionFolderPath_NestedSectionGroups_PreservesOrder()
		{
			var path = PathHelper.BuildSectionFolderPath(
				@"C:\exports", new[] { "GroupA", "GroupB" }, "SectionX");

			var expected = Path.Combine(
				Path.Combine(Path.Combine(@"C:\exports", "[GroupA]"), "[GroupB]"),
				"(SectionX)");

			Assert.AreEqual(expected, path);
		}


		[TestMethod]
		public void BuildSectionFolderPath_NamesWithInvalidChars_AreSanitized()
		{
			var path = PathHelper.BuildSectionFolderPath(
				@"C:\exports", new[] { "Group:A" }, "Section/X");

			var expected = Path.Combine(
				Path.Combine(@"C:\exports", "[Group_A]"), "(Section_X)");

			Assert.AreEqual(expected, path);
		}
	}
}
