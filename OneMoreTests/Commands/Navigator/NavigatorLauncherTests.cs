//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Navigator
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Commands;
	using System.Collections.Generic;
	using System.Linq;
	using HistoryRecord = River.OneMoreAddIn.OneNote.HierarchyInfo;


	[TestClass]
	public class NavigatorLauncherTests
	{
		private const string PageA = "{11111111-1111-1111-1111-111111111111}";
		private const string PageB = "{22222222-2222-2222-2222-222222222222}";

		private static string LinkTo(string guid)
		{
			return "onenote:https://x/Notes.one#Title&section-id={33333333-3333-3333-3333-333333333333}" +
				$"&page-id={guid}&end";
		}


		[TestMethod]
		public void LandedOn_SamePage_IsTrue()
		{
			Assert.IsTrue(NavigatorLauncher.LandedOn(LinkTo(PageA), LinkTo(PageA.ToLowerInvariant())));
		}


		[TestMethod]
		public void LandedOn_OtherPage_IsFalse()
		{
			Assert.IsFalse(NavigatorLauncher.LandedOn(LinkTo(PageA), LinkTo(PageB)));
		}


		[TestMethod]
		public void LandedOn_NoWayToTell_IsTrue()
		{
			Assert.IsTrue(NavigatorLauncher.LandedOn(null, LinkTo(PageA)));
			Assert.IsTrue(NavigatorLauncher.LandedOn(LinkTo(PageA), null));
			Assert.IsTrue(NavigatorLauncher.LandedOn("onenote:#Section", LinkTo(PageA)));
		}


		[TestMethod]
		public void QueryFor_CarriesWhatIsRemembered()
		{
			var record = new HistoryRecord
			{
				PageId = "p", SectionId = "s", NotebookId = "n", Link = LinkTo(PageA), Path = "/N/S/P"
			};

			var query = NavigatorLauncher.QueryFor(record);

			Assert.AreEqual("p", query.PageID);
			Assert.AreEqual("s", query.SectionID);
			Assert.AreEqual("n", query.NotebookID);
			Assert.AreEqual(record.Link, query.Uri);
			Assert.AreEqual("/N/S/P", query.Location);
			Assert.IsNull(query.PageKey);
		}


		[TestMethod]
		public void BuildMenu_SkipsRecordsWithoutLinkOrName()
		{
			var history = new List<HistoryRecord>
			{
				new() { Name = "good", Path = "/a/good", Link = LinkTo(PageA) },
				new() { Name = "nolink", Path = "/a/nolink", Link = null },
				new() { Name = null, Path = "/a/noname", Link = LinkTo(PageB) },
				new() { Name = "nopath", Path = null, Link = LinkTo(PageB) }
			};

			var root = HistoryMenu.BuildMenu(history, 20);

			var buttons = root.Elements().Where(e =>
				((string)e.Attribute("id") ?? string.Empty).StartsWith("omHistory") &&
				e.Name.LocalName == "button" &&
				!((string)e.Attribute("id")).StartsWith("omHistorySeparator")).ToList();

			Assert.AreEqual(2, buttons.Count);
		}
	}
}
