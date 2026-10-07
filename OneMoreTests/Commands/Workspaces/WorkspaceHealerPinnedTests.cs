//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Workspaces
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Commands.Workspaces;
	using River.OneMoreAddIn.Identity;
	using System.Linq;


	[TestClass]
	public class WorkspaceHealerPinnedTests
	{
		private static string Link(string id) => "link:" + id;


		private static string PageLink(string pageGuid) =>
			"onenote:https://x/Notes.one#Page&section-id={AAAAAAAA-0000-0000-0000-000000000000}&page-id=" +
			pageGuid + "&end";


		private static PinnedItem Pin(int id, string pageID, string guid = null, long? key = null,
			string objectID = null, string link = null, string path = "/Notes/Inbox/Page")
		{
			return new PinnedItem
			{
				ID = id,
				SortOrder = id,
				PageKey = key,
				Info = new OneNote.HierarchyInfo
				{
					Name = "Mine",
					Path = path,
					PageId = pageID,
					ObjectId = objectID,
					NotebookId = "{nb-old}",
					SectionId = "{sec-old}",
					Link = link ?? (guid is null ? "onenote:old" : PageLink(guid))
				}
			};
		}


		private static HealPlan Plan(IdentitySnapshot snapshot, params PinnedItem[] items)
		{
			return WorkspaceHealer.PlanPinned(items, new TargetResolver(snapshot), Link);
		}


		[TestMethod]
		public void AKeyedPage_AfterAReopen_GetsItsNewIDsAndLink_AndKeepsItsName()
		{
			var book = Snap.Book("{nb-new}", "Notes");
			Snap.Page(book, 7, "{page-new}", "Page");

			var pin = Pin(1, "{page-old}", key: 7);
			var plan = Plan(Snap.Of(0, book), pin);

			Assert.AreEqual(HealKind.Healed, plan.Items.Single().Kind);
			Assert.AreEqual("{page-new}", pin.Info.PageId);
			Assert.AreEqual("{nb-new}", pin.Info.NotebookId);
			Assert.AreEqual("link:{page-new}", pin.Info.Link);
			Assert.AreEqual("Mine", pin.Info.Name);
			Assert.AreEqual(1, pin.SortOrder);
		}


		[TestMethod]
		public void AParagraph_WhosePageGotANewID_FallsBackToThePage()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{page-new}", "Page");

			var pin = Pin(1, "{page-old}", key: 7, objectID: "{para}", link: "link:para");
			var plan = Plan(Snap.Of(0, book), pin);

			Assert.AreEqual(HealKind.Healed, plan.Items.Single().Kind);
			Assert.IsNull(pin.Info.ObjectId);
			Assert.AreEqual("link:{page-new}", pin.Info.Link);
		}


		[TestMethod]
		public void AParagraph_WhosePageKeepsItsID_KeepsItsObjectIDAndLink()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page");

			// only its keys were missing
			var pin = Pin(1, "{p}", key: 7, objectID: "{para}", link: "link:para");
			Plan(Snap.Of(0, book), pin);

			Assert.AreEqual("{para}", pin.Info.ObjectId);
			Assert.AreEqual("link:para", pin.Info.Link);
		}


		[TestMethod]
		public void APageAndAParagraphOfIt_AreNotDuplicates()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page");

			var page = Pin(1, "{p}", key: 7);
			var paragraph = Pin(2, "{p}", key: 7, objectID: "{para}", link: "link:para");
			var plan = Plan(Snap.Of(0, book), page, paragraph);

			Assert.AreEqual(0, plan.Count(HealKind.Duplicate));
		}


		[TestMethod]
		public void TwoItemsThatAreTheSamePage_SecondIsADuplicate_AndNotSaved()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page", guid: Snap.G1);

			var first = Pin(1, "{p}", key: 7);
			var second = Pin(2, "{old}", guid: Snap.G1);
			var plan = Plan(Snap.Of(0, book), first, second);

			Assert.AreEqual(HealKind.Duplicate, plan.Items.Single(i => i.ID == 2).Kind);
			Assert.IsFalse(plan.ToSave.Any(i => i.ID == 2));
		}


		[TestMethod]
		public void AMatchByNameAlone_IsAGuess_AndIsNeverSaved()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page");

			var pin = Pin(1, "{old}");
			var plan = Plan(Snap.Of(0, book), pin);

			Assert.AreEqual(HealKind.Guess, plan.Items.Single().Kind);
			Assert.AreEqual(0, plan.ToSave.Count());
			Assert.AreEqual("{old}", pin.Info.PageId);
		}


		[TestMethod]
		public void APageThatIsGone_IsBroken_AndKeptAsItIs()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Other");

			var pin = Pin(1, "{old}", path: "/Notes/Inbox/Nowhere");
			var plan = Plan(Snap.Of(0, book), pin);

			Assert.AreEqual(HealKind.Broken, plan.Items.Single().Kind);
			Assert.AreEqual(0, plan.ToSave.Count());
			Assert.AreEqual("{old}", pin.Info.PageId);
		}
	}
}
