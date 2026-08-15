//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	[TestClass]
	public class OneNoteTests : TestBase
	{
		private const string Xmlns =
			"xmlns:one=\"http://schemas.microsoft.com/office/onenote/2013/onenote\"";

		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


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


		[TestMethod]
		public async Task Update_LowContrastQuickStyleColor_InjectsContrastingColor()
		{
			const string PageId = "page-1";

			// a paragraph with local style (font-family/size only, no color) referencing a
			// QuickStyleDef whose fontColor is invisible against the page's white background
			var oe = new XElement(Ns + "OE",
				new XAttribute("quickStyleIndex", "1"),
				new XAttribute("style", "font-family:'Segoe UI';font-size:11.0pt;"),
				new XElement(Ns + "T", new XCData("bullet text")));

			var page = new PageBuilder(PageId, "Color Test", pageColor: "#FFFFFF")
				.WithElement(oe)
				.BuildElement();

			page.AddFirst(new XElement(Ns + "QuickStyleDef",
				new XAttribute("index", "1"),
				new XAttribute("name", "p"),
				new XAttribute("font", "Segoe UI"),
				new XAttribute("fontSize", "11.0"),
				new XAttribute("fontColor", "#FFFFFF")));

			SetupPage(PageId, page.ToString(SaveOptions.OmitDuplicateNamespaces));

			await using var one = new OneNote();
			var model = await one.GetPage(PageId);

			// force:true so OptimizeForSave doesn't prune the (unmodified since load) Outline
			// before the stabilizer ever sees it
			var ok = await one.Update(model, force: true);
			Assert.IsTrue(ok, "Update should succeed");

			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var style = (string)updated.Descendants(Ns + "OE")
				.FirstOrDefault(e => (string)e.Attribute("quickStyleIndex") == "1")
				?.Attribute("style");

			Assert.IsNotNull(style, "style attribute should still be present");
			StringAssert.Contains(style, "color:#000000");
		}
	}
}
