//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using System.Threading.Tasks;

	[TestClass]
	public class OneNoteTests : TestBase
	{
		private const string Xmlns =
			"xmlns:one=\"http://schemas.microsoft.com/office/onenote/2013/onenote\"";


		[TestMethod]
		public async Task GetSectionInfo_NestedSectionGroups_ReturnsGroupsOutermostFirst()
		{
			Mock.SetHierarchyXml("section-1",
				$"<one:Section {Xmlns} ID=\"section-1\" name=\"SectionX\" />");
			Mock.SetHierarchyXml("sg-2",
				$"<one:SectionGroup {Xmlns} ID=\"sg-2\" name=\"GroupB\" />");
			Mock.SetHierarchyXml("sg-1",
				$"<one:SectionGroup {Xmlns} ID=\"sg-1\" name=\"GroupA\" />");
			Mock.SetHierarchyXml("nb-1",
				$"<one:Notebook {Xmlns} ID=\"nb-1\" name=\"Notebook\" />");

			Mock.SetHierarchyParent("section-1", "sg-2");
			Mock.SetHierarchyParent("sg-2", "sg-1");
			Mock.SetHierarchyParent("sg-1", "nb-1");

			await using var one = new OneNote();
			var info = await one.GetSectionInfo("section-1");

			CollectionAssert.AreEqual(new[] { "GroupA", "GroupB" }, info.SectionGroups);
		}


		[TestMethod]
		public async Task GetSectionInfo_SectionDirectlyUnderNotebook_ReturnsEmptySectionGroups()
		{
			Mock.SetHierarchyXml("section-1",
				$"<one:Section {Xmlns} ID=\"section-1\" name=\"SectionX\" />");
			Mock.SetHierarchyXml("nb-1",
				$"<one:Notebook {Xmlns} ID=\"nb-1\" name=\"Notebook\" />");

			Mock.SetHierarchyParent("section-1", "nb-1");

			await using var one = new OneNote();
			var info = await one.GetSectionInfo("section-1");

			Assert.AreEqual(0, info.SectionGroups.Count);
		}
	}
}
