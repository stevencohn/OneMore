//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Workspaces
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands.Favorites;
	using River.OneMoreAddIn.Commands.Workspaces;
	using River.OneMoreAddIn.Identity;
	using System.Linq;


	[TestClass]
	public class WorkspaceHealerTests
	{
		private static string Link(string id) => "link:" + id;


		private static string PageLink(string pageGuid) =>
			"onenote:https://x/Notes.one#Page&section-id={AAAAAAAA-0000-0000-0000-000000000000}&page-id=" +
			pageGuid + "&end";


		private static Favorite PageFavorite(int id, string stalePageID, string guid = null,
			string location = "/Notes/Inbox/Page", long? key = null)
		{
			return new Favorite
			{
				ID = id,
				Name = "Page",
				Location = location,
				Uri = guid is null ? "onenote:old" : PageLink(guid),
				NotebookID = "{nb-old}",
				SectionID = "{sec-old}",
				PageID = stalePageID,
				PageKey = key
			};
		}


		private static HealPlan Plan(IdentitySnapshot snapshot, params Favorite[] favorites)
		{
			return WorkspaceHealer.Plan(favorites, new TargetResolver(snapshot), Link);
		}


		#region Pages
		[TestMethod]
		public void ALegacyFavorite_FoundByItsGuid_GetsItsKeysAndCurrentIDs()
		{
			var book = Snap.Book("{nb}", "Notes", "https://x/notes");
			Snap.Page(book, 7, "{page-now}", "Renamed", "Archive", Snap.G1, "{sec-now}");

			var favorite = PageFavorite(1, "{page-old}", Snap.G1);
			var plan = Plan(Snap.Of(0, book), favorite);

			Assert.AreEqual(HealKind.Healed, plan.Items.Single().Kind);
			Assert.AreEqual(1, plan.ToSave.Count());

			Assert.AreEqual(7L, favorite.PageKey);
			Assert.AreEqual("{page-now}", favorite.PageID);
			Assert.AreEqual("{sec-now}", favorite.SectionID);
			Assert.AreEqual("{nb}", favorite.NotebookID);
			Assert.AreEqual("https://x/notes", favorite.NotebookKey);
			Assert.AreEqual("Page", favorite.Name, "the favorite keeps its name when the page is renamed");
			Assert.AreEqual("/Notes/Archive/Renamed", favorite.Location);
			Assert.AreEqual("link:{page-now}", favorite.Uri);
		}


		[TestMethod]
		public void AKeyedFavorite_AfterAReopen_GetsItsNewIDs()
		{
			var book = Snap.Book("{nb-new}", "Notes");
			Snap.Page(book, 7, "{page-new}", "Page");

			var favorite = PageFavorite(1, "{page-old}", key: 7);
			var plan = Plan(Snap.Of(0, book), favorite);

			Assert.AreEqual(HealKind.Healed, plan.Items.Single().Kind);
			Assert.AreEqual("{page-new}", favorite.PageID);
			Assert.AreEqual("{nb-new}", favorite.NotebookID);
		}


		[TestMethod]
		public void AFavoriteThatIsAlreadyUpToDate_IsUnchanged_AndNotSaved()
		{
			var book = Snap.Book("{nb}", "Notes");
			var page = Snap.Page(book, 7, "{p}", "Page", guid: Snap.G1, sectionID: "{sec}");

			var favorite = new Favorite
			{
				ID = 1,
				Name = "Page",
				Location = "/Notes/Inbox/Page",
				Uri = "link:{p}",
				NotebookID = "{nb}",
				SectionID = "{sec}",
				PageID = "{p}",
				PageKey = 7,
				NotebookKey = page.Ref.NotebookKey,
				SectionKey = page.Ref.SectionKey
			};

			var plan = Plan(Snap.Of(0, book), favorite);

			Assert.AreEqual(HealKind.Unchanged, plan.Items.Single().Kind);
			Assert.AreEqual(0, plan.ToSave.Count());
			Assert.IsNull(plan.Summary());
		}


		[TestMethod]
		public void AMatchByNameAlone_IsAGuess_AndIsNeverSaved()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page");

			var favorite = PageFavorite(1, "{page-old}", location: "/Notes/Inbox/Page");
			var before = favorite.PageID;

			var plan = Plan(Snap.Of(0, book), favorite);

			Assert.AreEqual(HealKind.Guess, plan.Items.Single().Kind);
			Assert.AreEqual(0, plan.ToSave.Count());
			Assert.AreEqual(before, favorite.PageID);
			Assert.IsNull(favorite.PageKey);
		}


		[TestMethod]
		public void WhileGuidsAreUnread_AFavoriteIsPending_NotBroken()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Other");

			var plan = Plan(Snap.Of(30, book), PageFavorite(1, "{old}", Snap.G3));

			Assert.AreEqual(HealKind.Pending, plan.Items.Single().Kind);
		}


		[TestMethod]
		public void AFavoriteThatIsGone_IsReported_AndLeftAlone()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Other");

			var favorite = PageFavorite(1, "{old}", Snap.G3, "/Notes/Inbox/Gone");
			var plan = Plan(Snap.Of(0, book), favorite);

			Assert.AreEqual(HealKind.Broken, plan.Items.Single().Kind);
			Assert.AreEqual(1, plan.Items.Count, "nothing is ever removed");
			Assert.AreEqual("{old}", favorite.PageID);
		}


		[TestMethod]
		public void AFavoriteInAClosedNotebook_IsOffline()
		{
			var book = Snap.Book("{nb}", "Other");

			var plan = Plan(Snap.Of(0, book), PageFavorite(1, "{old}", Snap.G3, "/Archive/Inbox/Page"));

			Assert.AreEqual(HealKind.Offline, plan.Items.Single().Kind);
		}


		[TestMethod]
		public void CopiesThatShareAGuid_AreAmbiguous_AndNotSaved()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{a}", "Original", guid: Snap.G1);
			Snap.Page(book, 8, "{b}", "Copy", guid: Snap.G1);

			var plan = Plan(Snap.Of(0, book), PageFavorite(1, "{old}", Snap.G1));

			Assert.AreEqual(HealKind.Ambiguous, plan.Items.Single().Kind);
			Assert.AreEqual(0, plan.ToSave.Count());
		}
		#endregion Pages


		#region Duplicates
		[TestMethod]
		public void TwoFavoritesForOnePage_TheFirstWins_TheOtherIsReportedAsADuplicate()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page", guid: Snap.G1);

			var first = PageFavorite(1, "{old-a}", Snap.G1);
			var second = PageFavorite(2, "{old-b}", Snap.G1);

			var plan = Plan(Snap.Of(0, book), first, second);

			Assert.AreEqual(HealKind.Healed, plan.Items[0].Kind);
			Assert.AreEqual(HealKind.Duplicate, plan.Items[1].Kind);
			Assert.AreEqual(7L, first.PageKey);
			Assert.IsNull(second.PageKey, "the duplicate is left as it was");
			Assert.AreEqual(1, plan.ToSave.Count());
		}


		[TestMethod]
		public void AFavoriteThatAlreadyHoldsAKey_KeepsIt_AgainstOneThatResolvesToTheSamePage()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page", guid: Snap.G1);

			// the lower-numbered favorite is not yet keyed; the other already holds the key
			var unkeyed = PageFavorite(1, "{old-a}", Snap.G1);
			var holder = PageFavorite(2, "{old-b}", Snap.G1, key: 7);

			var plan = Plan(Snap.Of(0, book), unkeyed, holder);

			Assert.AreEqual(HealKind.Duplicate, plan.Items[0].Kind);
			Assert.AreEqual(HealKind.Healed, plan.Items[1].Kind);
			Assert.IsNull(unkeyed.PageKey);
		}
		#endregion Duplicates


		#region Containers
		[TestMethod]
		public void ALegacySection_StillThere_GetsItsKeys()
		{
			var book = Snap.Book("{nb}", "Notes", "https://x/notes");
			Snap.Section(book, "{sec}", "Inbox");

			var favorite = new Favorite
			{
				ID = 1,
				Name = "Inbox",
				Location = "/Notes/Inbox",
				Uri = "onenote:old",
				NotebookID = "{nb}",
				SectionID = "{sec}"
			};

			var plan = Plan(Snap.Of(0, book), favorite);

			Assert.AreEqual(HealKind.Healed, plan.Items.Single().Kind);
			Assert.AreEqual("https://x/notes", favorite.NotebookKey);
			Assert.AreEqual("/Inbox", favorite.SectionKey);
			Assert.IsNull(favorite.PageID);
			Assert.IsNull(favorite.PageKey);
		}


		[TestMethod]
		public void ALegacySection_AlreadyStale_CanOnlyBeFoundByName_SoItIsAGuess()
		{
			var book = Snap.Book("{nb-new}", "Notes");
			Snap.Section(book, "{sec-new}", "Inbox");

			var favorite = new Favorite
			{
				ID = 1,
				Name = "Inbox",
				Location = "/Notes/Inbox",
				Uri = "onenote:old",
				NotebookID = "{nb-old}",
				SectionID = "{sec-old}"
			};

			var plan = Plan(Snap.Of(0, book), favorite);

			Assert.AreEqual(HealKind.Guess, plan.Items.Single().Kind);
			Assert.IsNull(favorite.NotebookKey);
		}


		[TestMethod]
		public void TwoFavoritesForOneSection_AreADuplicate()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Section(book, "{sec}", "Inbox");

			Favorite Section(int id) => new Favorite
			{
				ID = id,
				Name = "Inbox",
				Location = "/Notes/Inbox",
				Uri = "onenote:old",
				NotebookID = "{nb}",
				SectionID = "{sec}"
			};

			var plan = Plan(Snap.Of(0, book), Section(1), Section(2));

			Assert.AreEqual(HealKind.Healed, plan.Items[0].Kind);
			Assert.AreEqual(HealKind.Duplicate, plan.Items[1].Kind);
		}


		[TestMethod]
		public void ASectionAndASectionGroupOfTheSameName_AreNotDuplicates()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Section(book, "{section}", "Work");
			Snap.Group(book, "{group}", "Work");

			var section = new Favorite
			{
				ID = 1, Name = "Work", Location = "/Notes/Work", Uri = "x",
				NotebookID = "{nb}", SectionID = "{section}"
			};

			var group = new Favorite
			{
				ID = 2, Name = "Work", Location = "/Notes/Work", Uri = "{group}",
				NotebookID = "{nb}", SectionID = "{group}", Kind = Favorite.KindSectionGroup
			};

			var plan = Plan(Snap.Of(0, book), section, group);

			Assert.AreEqual(HealKind.Healed, plan.Items[0].Kind);
			Assert.AreEqual(HealKind.Healed, plan.Items[1].Kind);
		}
		#endregion Containers


		#region Summary
		[TestMethod]
		public void TheSummary_CountsWhatHappened()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page", guid: Snap.G1);

			var plan = Plan(Snap.Of(0, book),
				PageFavorite(1, "{a}", Snap.G1),
				PageFavorite(2, "{b}", Snap.G1),
				PageFavorite(3, "{c}", Snap.G3, "/Notes/Inbox/Gone"));

			Assert.AreEqual("1 healed, 1 broken, 1 duplicate", plan.Summary());
		}
		#endregion Summary
	}
}
