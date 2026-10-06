//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Workspaces
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Commands.Favorites;
	using River.OneMoreAddIn.Commands.Workspaces;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	[TestClass]
	public class WorkspaceResolverTests
	{
		private static string Link(string id) => "onenote:link-to-" + id;


		private static TargetResolution PageFound()
		{
			return new TargetResolution
			{
				Outcome = ResolveOutcome.Resolved,
				Method = ResolveMethod.Guid,
				PageKey = 42,
				NotebookKey = "https://x/notes",
				SectionKey = "/Archive",
				NotebookID = "{nb-new}",
				SectionID = "{sec-new}",
				PageID = "{page-new}",
				Name = "New name",
				Location = "/Notes/Archive/New name"
			};
		}


		private static Favorite OldPageFavorite()
		{
			return new Favorite
			{
				ID = 7,
				FolderID = 3,
				Name = "Old name",
				Alias = "Mine",
				SortOrder = 9,
				Location = "/Notes/Inbox/Old name",
				Uri = "onenote:old",
				NotebookID = "{nb-old}",
				SectionID = "{sec-old}",
				PageID = "{page-old}"
			};
		}


		#region NavigationTarget
		[TestMethod]
		public void APage_IsOpenedByAFreshLink()
		{
			var target = WorkspaceResolver.NavigationTarget(PageFound(), null, Link);

			Assert.AreEqual("onenote:link-to-{page-new}", target);
		}


		[TestMethod]
		public void ASection_IsOpenedByAFreshLink()
		{
			var section = new TargetResolution { Outcome = ResolveOutcome.Resolved, SectionID = "{s}", NotebookID = "{nb}" };

			Assert.AreEqual("onenote:link-to-{s}", WorkspaceResolver.NavigationTarget(section, null, Link));
		}


		[TestMethod]
		public void ASectionGroupOrANotebook_IsOpenedByItsID_NotByALink()
		{
			var container = new TargetResolution { Outcome = ResolveOutcome.Resolved, SectionID = "{g}", NotebookID = "{nb}" };
			var called = false;

			string Spy(string id) { called = true; return "link"; }

			Assert.AreEqual("{g}", WorkspaceResolver.NavigationTarget(container, Favorite.KindSectionGroup, Spy));
			Assert.AreEqual("{g}", WorkspaceResolver.NavigationTarget(container, Favorite.KindNotebook, Spy));
			Assert.IsFalse(called, "links to notebooks and section groups are unreliable");
		}
		#endregion NavigationTarget


		#region Apply
		[TestMethod]
		public void Apply_CopiesWhatWasFound_AndNeverTouchesTheUsersChoices()
		{
			var favorite = OldPageFavorite();

			var changed = WorkspaceResolver.Apply(favorite, PageFound(), Link);

			Assert.IsTrue(changed);
			Assert.AreEqual("{page-new}", favorite.PageID);
			Assert.AreEqual("{sec-new}", favorite.SectionID);
			Assert.AreEqual("{nb-new}", favorite.NotebookID);
			Assert.AreEqual(42L, favorite.PageKey);
			Assert.AreEqual("https://x/notes", favorite.NotebookKey);
			Assert.AreEqual("/Archive", favorite.SectionKey);
			Assert.AreEqual("/Notes/Archive/New name", favorite.Location);
			Assert.AreEqual("onenote:link-to-{page-new}", favorite.Uri);

			// the page was renamed, but the favorite keeps the name it was given
			Assert.AreEqual("Old name", favorite.Name);
			Assert.AreEqual("Mine", favorite.Alias);
			Assert.AreEqual(3, favorite.FolderID);
			Assert.AreEqual(9, favorite.SortOrder);
			Assert.AreEqual(7, favorite.ID);
		}


		[TestMethod]
		public void Apply_AgainWithTheSameResult_ChangesNothing()
		{
			var favorite = OldPageFavorite();
			WorkspaceResolver.Apply(favorite, PageFound(), Link);

			Assert.IsFalse(WorkspaceResolver.Apply(favorite, PageFound(), Link));
		}


		[TestMethod]
		public void Apply_NothingChanged_DoesNotMakeALink()
		{
			var favorite = OldPageFavorite();
			WorkspaceResolver.Apply(favorite, PageFound(), Link);

			var calls = 0;
			WorkspaceResolver.Apply(favorite, PageFound(), id => { calls++; return "x"; });

			Assert.AreEqual(0, calls);
		}


		[TestMethod]
		public void Apply_NothingChangedButTheLinkGuidDiffers_MakesALink()
		{
			var favorite = OldPageFavorite();
			WorkspaceResolver.Apply(favorite, PageFound(), Link);
			favorite.Uri = "onenote:#p&section-id={11111111-1111-1111-1111-111111111111}" +
				"&page-id={22222222-2222-2222-2222-222222222222}&end";

			var found = PageFound();
			found.PageGuid = "{33333333-3333-3333-3333-333333333333}";

			var calls = 0;
			WorkspaceResolver.Apply(favorite, found, id => { calls++; return "x"; });

			Assert.AreEqual(1, calls);
		}


		[TestMethod]
		public void Apply_ALinkThatCannotBeMade_LeavesTheStoredOneInPlace()
		{
			var favorite = OldPageFavorite();

			WorkspaceResolver.Apply(favorite, PageFound(), id => null);

			Assert.AreEqual("onenote:old", favorite.Uri);
			Assert.AreEqual("{page-new}", favorite.PageID, "the rest is still saved");
		}


		[TestMethod]
		public void Apply_ANotebook_StoresItsIDAsTheSectionID_AndNoLink()
		{
			var favorite = new Favorite
			{
				Kind = Favorite.KindNotebook,
				Name = "Notes",
				Location = "/Notes",
				Uri = "{nb-old}",
				NotebookID = "{nb-old}",
				SectionID = "{nb-old}"
			};

			var found = new TargetResolution
			{
				Outcome = ResolveOutcome.Resolved,
				Method = ResolveMethod.Key,
				NotebookKey = "notes",
				SectionKey = null,
				NotebookID = "{nb-new}",
				SectionID = "{nb-new}",
				Name = "Notes",
				Location = "/Notes"
			};

			var called = false;
			Assert.IsTrue(WorkspaceResolver.Apply(favorite, found, id => { called = true; return "x"; }));

			Assert.AreEqual("{nb-new}", favorite.NotebookID);
			Assert.AreEqual("{nb-new}", favorite.SectionID);
			Assert.AreEqual("{nb-new}", favorite.Uri);
			Assert.AreEqual("notes", favorite.NotebookKey);
			Assert.IsNull(favorite.SectionKey);
			Assert.IsNull(favorite.PageID);
			Assert.IsNull(favorite.PageKey);
			Assert.IsFalse(called);
		}


		[TestMethod]
		public void Apply_ASection_NeverGainsAPageID()
		{
			var favorite = new Favorite
			{
				Name = "Inbox",
				Location = "/Notes/Inbox",
				Uri = "onenote:old",
				NotebookID = "{nb}",
				SectionID = "{sec-old}"
			};

			var found = new TargetResolution
			{
				Outcome = ResolveOutcome.Resolved,
				NotebookKey = "notes",
				SectionKey = "/Inbox",
				NotebookID = "{nb}",
				SectionID = "{sec-new}",
				Name = "Inbox",
				Location = "/Notes/Inbox"
			};

			WorkspaceResolver.Apply(favorite, found, Link);

			Assert.IsNull(favorite.PageID);
			Assert.AreEqual("{sec-new}", favorite.SectionID);
			Assert.AreEqual("onenote:link-to-{sec-new}", favorite.Uri);
		}
		#endregion Apply


		#region Landing
		private const string PageGuid = "{8535855B-3F24-441A-929E-2F09AD4B1354}";
		private const string OtherGuid = "{11111111-1111-1111-1111-111111111111}";
		private const string SectionGuid = "{726E1EF7-A728-4716-9D91-504F8A81FFF3}";

		private static string PageLink(string guid) =>
			$"onenote:https://x/Notes.one#Page&section-id={SectionGuid}&page-id={guid}&end";

		private static string SectionLink(string guid) =>
			$"onenote:https://x/Notes.one#Inbox&section-id={guid}&end";


		[TestMethod]
		public void APage_WhenOneNoteIsOnThatPage_HasLanded()
		{
			var favorite = new Favorite { PageID = "{p}", Uri = PageLink(PageGuid) };

			Assert.IsTrue(WorkspaceResolver.LandedOn(favorite, PageLink(PageGuid.ToLowerInvariant()), null));
		}


		[TestMethod]
		public void APage_WhenOneNoteIsOnAnotherPage_HasNotLanded()
		{
			// what happens when a link names a page that no longer exists: OneNote opens something
			// else, such as the section, and still says it succeeded
			var favorite = new Favorite { PageID = "{p}", Uri = PageLink(PageGuid) };

			Assert.IsFalse(WorkspaceResolver.LandedOn(favorite, PageLink(OtherGuid), SectionLink(SectionGuid)));
		}


		[TestMethod]
		public void ASection_IsComparedBySectionGuid_NotByPage()
		{
			var favorite = new Favorite { Uri = SectionLink(SectionGuid) };

			Assert.IsTrue(WorkspaceResolver.LandedOn(favorite, PageLink(OtherGuid), SectionLink(SectionGuid)));
			Assert.IsFalse(WorkspaceResolver.LandedOn(favorite, PageLink(OtherGuid), SectionLink(OtherGuid)));
		}


		[TestMethod]
		public void ANotebookOrASectionGroup_IsNeverChecked_ItIsOpenedByID()
		{
			var notebook = new Favorite { Kind = Favorite.KindNotebook, Uri = "{nb}" };
			var group = new Favorite { Kind = Favorite.KindSectionGroup, Uri = "{g}" };

			Assert.IsTrue(WorkspaceResolver.LandedOn(notebook, PageLink(OtherGuid), SectionLink(OtherGuid)));
			Assert.IsTrue(WorkspaceResolver.LandedOn(group, PageLink(OtherGuid), SectionLink(OtherGuid)));
		}


		[TestMethod]
		public void WhenThereIsNoWayToTell_ADoubtNeverTurnsAGoodClickIntoAnError()
		{
			var page = new Favorite { PageID = "{p}", Uri = PageLink(PageGuid) };
			var noGuid = new Favorite { PageID = "{p}", Uri = "not a link" };

			// OneNote could not make a link to where it is now
			Assert.IsTrue(WorkspaceResolver.LandedOn(page, null, null));

			// the favorite's own link names no page
			Assert.IsTrue(WorkspaceResolver.LandedOn(noGuid, PageLink(OtherGuid), null));
		}
		#endregion Landing

		#region The command and the menu
		[TestMethod]
		public void ParseFavoriteID_ReadsADatabaseID_AndNothingElse()
		{
			Assert.AreEqual(7, FavoritesCommand.ParseFavoriteID("7"));
			Assert.AreEqual(12345, FavoritesCommand.ParseFavoriteID("12345"));

			Assert.IsNull(FavoritesCommand.ParseFavoriteID(null));
			Assert.IsNull(FavoritesCommand.ParseFavoriteID(""));
			Assert.IsNull(FavoritesCommand.ParseFavoriteID("-3"));
			Assert.IsNull(FavoritesCommand.ParseFavoriteID("onenote:https://x/Notes.one#Page&end"));
			Assert.IsNull(FavoritesCommand.ParseFavoriteID("{AAAA-1}{1}{B0}"));
			Assert.IsNull(FavoritesCommand.ParseFavoriteID("99999999999999"));
		}


		[TestMethod]
		public void EveryFailureHasItsOwnMessage()
		{
			var messages = new[]
			{
				FavoritesCommand.MessageFor(ResolveOutcome.Pending),
				FavoritesCommand.MessageFor(ResolveOutcome.Offline),
				FavoritesCommand.MessageFor(ResolveOutcome.Ambiguous),
				FavoritesCommand.MessageFor(ResolveOutcome.Broken)
			};

			Assert.IsTrue(messages.All(m => !string.IsNullOrWhiteSpace(m)));
			Assert.AreEqual(4, messages.Distinct().Count());
		}


		[TestMethod]
		public void TheMenuButtonCarriesTheFavoritesID_NotALink()
		{
			var collection = new FavoritesCollection();
			collection.Items.Add(new Favorite
			{
				ID = 7,
				Name = "Page",
				Location = "/Notes/Inbox/Page",
				Uri = "onenote:https://x/Notes.one#Page&end",
				PageID = "{p}"
			});

			collection.Items.Add(new Favorite
			{
				ID = 8,
				Name = "Notebook",
				Location = "/Notes",
				Uri = "{nb}",
				SectionID = "{nb}",
				Kind = Favorite.KindNotebook
			});

			var menu = FavoritesMenu.BuildMenu(collection, false);
			XNamespace ns = "http://schemas.microsoft.com/office/2009/07/customui";

			string TagOf(string id) => menu.Descendants(ns + "button")
				.Single(b => (string)b.Attribute("id") == id).Attribute("tag").Value;

			Assert.AreEqual("7", TagOf("omFavorite7"));
			Assert.AreEqual("8", TagOf("omFavorite8"));
		}
		#endregion The command and the menu
	}
}
