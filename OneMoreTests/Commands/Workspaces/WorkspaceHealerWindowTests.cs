//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Workspaces
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands.Favorites;
	using River.OneMoreAddIn.Commands.Layouts;
	using River.OneMoreAddIn.Commands.Workspaces;
	using River.OneMoreAddIn.Identity;
	using System.Linq;


	[TestClass]
	public class WorkspaceHealerWindowTests
	{
		private static string Link(string id) => "link:" + id;


		private static string PageLink(string pageGuid) =>
			"onenote:https://x/Notes.one#Page&section-id={AAAAAAAA-0000-0000-0000-000000000000}&page-id=" +
			pageGuid + "&end";


		private static LayoutWindow Window(int id, int layoutID, string stalePageID, string guid = null,
			string location = "/Notes/Inbox/Page", long? key = null)
		{
			return new LayoutWindow
			{
				ID = id,
				LayoutID = layoutID,
				Name = "Page",
				Alias = "Mine",
				Location = location,
				Uri = guid is null ? "onenote:old" : PageLink(guid),
				NotebookID = "{nb-old}",
				SectionID = "{sec-old}",
				PageID = stalePageID,
				ZOrder = 3,
				PageKey = key
			};
		}


		private static HealPlan Plan(IdentitySnapshot snapshot, params LayoutWindow[] windows)
		{
			return WorkspaceHealer.PlanWindows(windows, new TargetResolver(snapshot), Link);
		}


		[TestMethod]
		public void ALegacyWindow_FoundByItsGuid_GetsItsKeyAndCurrentIDs_AndKeepsTheUsersChoices()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{page-now}", "Renamed", "Archive", Snap.G1, "{sec-now}");

			var window = Window(1, 1, "{page-old}", Snap.G1);
			var plan = Plan(Snap.Of(0, book), window);

			Assert.AreEqual(HealKind.Healed, plan.Items.Single().Kind);
			Assert.AreEqual(1, plan.ToSave.Count());

			Assert.AreEqual(7L, window.PageKey);
			Assert.AreEqual("{page-now}", window.PageID);
			Assert.AreEqual("{sec-now}", window.SectionID);
			Assert.AreEqual("{nb}", window.NotebookID);
			Assert.AreEqual("/Notes/Archive/Renamed", window.Location);
			Assert.AreEqual("link:{page-now}", window.Uri);

			Assert.AreEqual("Page", window.Name, "the window keeps its name when the page is renamed");
			Assert.AreEqual("Mine", window.Alias);
			Assert.AreEqual(3, window.ZOrder);
		}


		[TestMethod]
		public void AKeyedWindow_AfterAReopen_GetsItsNewIDs()
		{
			var book = Snap.Book("{nb-new}", "Notes");
			Snap.Page(book, 7, "{page-new}", "Page");

			var window = Window(1, 1, "{page-old}", key: 7);
			var plan = Plan(Snap.Of(0, book), window);

			Assert.AreEqual(HealKind.Healed, plan.Items.Single().Kind);
			Assert.AreEqual("{page-new}", window.PageID);
		}


		[TestMethod]
		public void AWindowAlreadyUpToDate_IsUnchanged_AndNotSaved()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page", guid: Snap.G1, sectionID: "{sec}");

			var window = new LayoutWindow
			{
				ID = 1, LayoutID = 1, Name = "Page", Location = "/Notes/Inbox/Page", Uri = "link:{p}",
				NotebookID = "{nb}", SectionID = "{sec}", PageID = "{p}", PageKey = 7
			};

			var plan = Plan(Snap.Of(0, book), window);

			Assert.AreEqual(HealKind.Unchanged, plan.Items.Single().Kind);
			Assert.AreEqual(0, plan.ToSave.Count());
		}


		[TestMethod]
		public void AMatchByNameAlone_IsAGuess_AndIsNeverSaved()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page");

			var window = Window(1, 1, "{old}");
			var plan = Plan(Snap.Of(0, book), window);

			Assert.AreEqual(HealKind.Guess, plan.Items.Single().Kind);
			Assert.AreEqual(0, plan.ToSave.Count());
			Assert.IsNull(window.PageKey);
			Assert.AreEqual("{old}", window.PageID);
		}


		[TestMethod]
		public void AWindowThatIsGone_IsReported_AndLeftAlone()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Other");

			var plan = Plan(Snap.Of(0, book), Window(1, 1, "{old}", Snap.G3, "/Notes/Inbox/Gone"));

			Assert.AreEqual(HealKind.Broken, plan.Items.Single().Kind);
			Assert.AreEqual(1, plan.Items.Count, "nothing is ever removed");
		}


		[TestMethod]
		public void WhileGuidsAreUnread_AWindowIsPending()
		{
			var book = Snap.Book("{nb}", "Notes");

			var plan = Plan(Snap.Of(30, book), Window(1, 1, "{old}", Snap.G3, "/Notes/Inbox/Gone"));

			Assert.AreEqual(HealKind.Pending, plan.Items.Single().Kind);
		}


		[TestMethod]
		public void AWindowInAClosedNotebook_IsOffline()
		{
			var plan = Plan(Snap.Of(0, Snap.Book("{nb}", "Other")),
				Window(1, 1, "{old}", Snap.G3, "/Archive/Inbox/Page"));

			Assert.AreEqual(HealKind.Offline, plan.Items.Single().Kind);
		}


		[TestMethod]
		public void ThePageInTwoDifferentLayouts_IsNotADuplicate()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page", guid: Snap.G1);

			var inWork = Window(1, 10, "{old-a}", Snap.G1);
			var inHome = Window(2, 20, "{old-b}", Snap.G1);

			var plan = Plan(Snap.Of(0, book), inWork, inHome);

			Assert.AreEqual(HealKind.Healed, plan.Items[0].Kind);
			Assert.AreEqual(HealKind.Healed, plan.Items[1].Kind);
			Assert.AreEqual(7L, inWork.PageKey);
			Assert.AreEqual(7L, inHome.PageKey);
		}


		[TestMethod]
		public void ThePageTwiceInOneLayout_TheFirstWins_TheOtherIsReportedAsADuplicate()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page", guid: Snap.G1);

			var first = Window(1, 10, "{old-a}", Snap.G1);
			var second = Window(2, 10, "{old-b}", Snap.G1);

			var plan = Plan(Snap.Of(0, book), first, second);

			Assert.AreEqual(HealKind.Healed, plan.Items[0].Kind);
			Assert.AreEqual(HealKind.Duplicate, plan.Items[1].Kind);
			Assert.IsNull(second.PageKey);
			Assert.AreEqual(1, plan.ToSave.Count());
		}


		[TestMethod]
		public void AWindowThatAlreadyHoldsAKey_KeepsIt_AgainstOneThatResolvesToTheSamePage()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page", guid: Snap.G1);

			var unkeyed = Window(1, 10, "{old-a}", Snap.G1);
			var holder = Window(2, 10, "{old-b}", Snap.G1, key: 7);

			var plan = Plan(Snap.Of(0, book), unkeyed, holder);

			Assert.AreEqual(HealKind.Duplicate, plan.Items[0].Kind);
			Assert.AreEqual(HealKind.Healed, plan.Items[1].Kind);
		}


		[TestMethod]
		public void AHealItem_DescribesAWindowAndAFavoriteForTheLog()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page", guid: Snap.G1);

			var windowItem = Plan(Snap.Of(0, book), Window(5, 1, "{old}", Snap.G1)).Items.Single();
			Assert.AreEqual("layout window", windowItem.What);
			Assert.AreEqual(5, windowItem.ID);
			Assert.IsNotNull(windowItem.Label);

			var favorite = new Favorite
			{
				ID = 9, Name = "Page", Location = "/Notes/Inbox/Page", Uri = PageLink(Snap.G1),
				NotebookID = "{nb}", SectionID = "{s}", PageID = "{old}"
			};

			var favoriteItem = WorkspaceHealer.Plan(new[] { favorite }, new TargetResolver(Snap.Of(0, book)), Link).Items.Single();
			Assert.AreEqual("favorite", favoriteItem.What);
			Assert.AreEqual(9, favoriteItem.ID);
		}


		[TestMethod]
		public void TheSummary_CountsWhatHappenedToWindows()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page", guid: Snap.G1);

			var plan = Plan(Snap.Of(0, book),
				Window(1, 10, "{a}", Snap.G1),
				Window(2, 10, "{b}", Snap.G1),
				Window(3, 10, "{c}", Snap.G3, "/Notes/Inbox/Gone"));

			Assert.AreEqual("1 healed, 1 broken, 1 duplicate", plan.Summary());
		}
	}
}
