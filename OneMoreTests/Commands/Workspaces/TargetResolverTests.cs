//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Workspaces
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands.Favorites;
	using River.OneMoreAddIn.Commands.Workspaces;
	using River.OneMoreAddIn.Identity;
	using River.OneMoreAddIn.Tests.Identity;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	internal static class Snap
	{
		public const string G1 = "{11111111-1111-1111-1111-111111111111}";
		public const string G2 = "{22222222-2222-2222-2222-222222222222}";
		public const string G3 = "{33333333-3333-3333-3333-333333333333}";


		public static IdentityNotebook Book(string id, string name, string key = null)
		{
			return new IdentityNotebook(id, name, key ?? name.ToLowerInvariant()) { Listed = true };
		}


		// adds a page to a notebook, in a section whose path is /Notebook + groups + /section
		public static IdentityPage Page(IdentityNotebook book, long key, string pageID, string title,
			string section = "Inbox", string guid = null, string sectionID = null, params string[] groups)
		{
			var sectionKey = PageIdentityKeys.SectionKey(groups, section);
			var path = book.Path + string.Concat(groups.Select(g => "/" + g)) + "/" + section;

			var reference = new PageRef(pageID, book.Key, sectionKey, title,
				"2026-01-01T00:00:00.000Z", "2026-02-01T00:00:00.000Z");

			var page = new IdentityPage(reference, book.ID, sectionID ?? ("{sec-" + section + "}"), path, false)
			{
				Resolution = new PageResolution(reference, new IdentityRow { PageKey = key }, ResolutionKind.Known),
				PageGuid = guid
			};

			book.Pages.Add(page);
			return page;
		}


		public static IdentityContainer Section(IdentityNotebook book, string id, string name, params string[] groups)
		{
			return Add(book, ContainerKind.Section, id, name, groups);
		}


		public static IdentityContainer Group(IdentityNotebook book, string id, string name, params string[] groups)
		{
			return Add(book, ContainerKind.SectionGroup, id, name, groups);
		}


		private static IdentityContainer Add(IdentityNotebook book, ContainerKind kind, string id, string name, string[] groups)
		{
			var container = new IdentityContainer(kind, id, name,
				PageIdentityKeys.SectionKey(groups, name),
				book.Path + string.Concat(groups.Select(g => "/" + g)) + "/" + name);

			book.Containers.Add(container);
			return container;
		}


		public static IdentitySnapshot Of(int pending = 0, params IdentityNotebook[] books)
		{
			return new IdentitySnapshot(books, new string[0], new long[0], TimeSpan.Zero, pending);
		}
	}


	[TestClass]
	public class TargetResolverPageTests
	{
		private static string Link(string pageGuid)
		{
			return "onenote:https://x/Notes.one#Page&section-id={AAAAAAAA-0000-0000-0000-000000000000}&page-id=" +
				pageGuid + "&end";
		}


		[TestMethod]
		public void ByKey_FoundEvenWhenEveryIDHasChanged()
		{
			var book = Snap.Book("{nb-new}", "Notes");
			Snap.Page(book, 42, "{page-new}", "Renamed page", "Archive", Snap.G1, "{sec-new}");

			var query = new TargetQuery
			{
				PageKey = 42,
				PageID = "{page-old}",
				SectionID = "{sec-old}",
				NotebookID = "{nb-old}",
				Uri = Link(Snap.G2),
				Location = "/Notes/Inbox/Old name"
			};

			var result = new TargetResolver(Snap.Of(0, book)).Resolve(query);

			Assert.AreEqual(ResolveOutcome.Resolved, result.Outcome);
			Assert.AreEqual(ResolveMethod.Key, result.Method);
			Assert.AreEqual("{page-new}", result.PageID);
			Assert.AreEqual("{sec-new}", result.SectionID);
			Assert.AreEqual("{nb-new}", result.NotebookID);
			Assert.AreEqual("Renamed page", result.Name);
			Assert.AreEqual("/Notes/Archive/Renamed page", result.Location);
			Assert.AreEqual(42L, result.PageKey);
			Assert.IsTrue(result.IsConfident);
		}


		[TestMethod]
		public void ByKey_ThePageIsInAClosedNotebook_IsOffline_NotBroken()
		{
			var open = Snap.Book("{nb}", "Other");

			var query = new TargetQuery { PageKey = 42, PageID = "{p}", Uri = Link(Snap.G1) };
			var resolver = new TargetResolver(Snap.Of(0, open),
				key => new IdentityRow { PageKey = key, MissingSince = "2026-10-04" });

			var result = resolver.Resolve(query);

			Assert.AreEqual(ResolveOutcome.Offline, result.Outcome);
			Assert.IsFalse(result.IsResolved);
		}


		[TestMethod]
		public void ByKey_ThePageIsInALockedSection_IsOffline()
		{
			var open = Snap.Book("{nb}", "Notes");

			var query = new TargetQuery { PageKey = 42, PageID = "{p}" };
			var resolver = new TargetResolver(Snap.Of(0, open),
				key => new IdentityRow { PageKey = key }); // present in the catalog, absent from the snapshot

			Assert.AreEqual(ResolveOutcome.Offline, resolver.Resolve(query).Outcome);
		}


		[TestMethod]
		public void AForgottenKey_FallsBackToTheOtherEvidence()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p-now}", "Page", guid: Snap.G1);

			// the catalog purged key 42, but the favorite's link still names the page
			var query = new TargetQuery { PageKey = 42, PageID = "{p-old}", Uri = Link(Snap.G1) };

			var result = new TargetResolver(Snap.Of(0, book), key => null).Resolve(query);

			Assert.AreEqual(ResolveMethod.Guid, result.Method);
			Assert.AreEqual(7L, result.PageKey);
		}


		[TestMethod]
		public void ByHandle_WhenTheRememberedIDStillExists()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p1}", "Page");

			var result = new TargetResolver(Snap.Of(0, book))
				.Resolve(new TargetQuery { PageID = "{p1}", Uri = "onenote:#x" });

			Assert.AreEqual(ResolveMethod.Handle, result.Method);
			Assert.AreEqual(7L, result.PageKey);
			Assert.IsTrue(result.IsConfident);
		}


		[TestMethod]
		public void ByGuid_AfterAReopenChangedEveryID()
		{
			var book = Snap.Book("{nb-new}", "Notes");
			Snap.Page(book, 7, "{p-new}", "Page", guid: Snap.G1);
			Snap.Page(book, 8, "{p-other}", "Other", guid: Snap.G2);

			var result = new TargetResolver(Snap.Of(0, book)).Resolve(
				new TargetQuery { PageID = "{p-old}", Uri = Link(Snap.G1.ToLowerInvariant()) });

			Assert.AreEqual(ResolveOutcome.Resolved, result.Outcome);
			Assert.AreEqual(ResolveMethod.Guid, result.Method);
			Assert.AreEqual("{p-new}", result.PageID);
			Assert.AreEqual(7L, result.PageKey);
			Assert.IsTrue(result.IsConfident);
		}


		[TestMethod]
		public void ByGuid_AfterARenameAndAMove_WhereTheLocationNoLongerMatches()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p-new}", "Brand new name", "Elsewhere", Snap.G1);

			var result = new TargetResolver(Snap.Of(0, book)).Resolve(new TargetQuery
			{
				PageID = "{p-old}",
				Uri = Link(Snap.G1),
				Location = "/Notes/Inbox/Old name"
			});

			Assert.AreEqual(ResolveMethod.Guid, result.Method);
			Assert.AreEqual("Brand new name", result.Name);
		}


		[TestMethod]
		public void TwoPagesWithTheSameGuid_AreAmbiguous_AndNeitherIsChosen()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{a}", "Original", guid: Snap.G1);
			Snap.Page(book, 8, "{b}", "Copy", guid: Snap.G1);

			var result = new TargetResolver(Snap.Of(0, book))
				.Resolve(new TargetQuery { PageID = "{old}", Uri = Link(Snap.G1) });

			Assert.AreEqual(ResolveOutcome.Ambiguous, result.Outcome);
			Assert.IsNull(result.PageID);
		}


		[TestMethod]
		public void AGuidNotFound_WhileSomeGuidsAreUnread_IsPending_NotBroken_AndNotGuessed()
		{
			var book = Snap.Book("{nb}", "Notes");

			// a page with the remembered name exists, but the wanted page may be one whose GUID
			// has not been read yet, so the name must not be taken for it
			Snap.Page(book, 7, "{p}", "Page", guid: null);

			var result = new TargetResolver(Snap.Of(5, book)).Resolve(new TargetQuery
			{
				PageID = "{old}",
				Uri = Link(Snap.G3),
				Location = "/Notes/Inbox/Page"
			});

			Assert.AreEqual(ResolveOutcome.Pending, result.Outcome);
		}


		[TestMethod]
		public void AGuidNotFound_WhenAllAreRead_FallsBackToTheName_AsAGuess()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page", guid: Snap.G1);

			var result = new TargetResolver(Snap.Of(0, book)).Resolve(new TargetQuery
			{
				PageID = "{old}",
				Uri = Link(Snap.G3),
				Location = "/Notes/Inbox/Page"
			});

			Assert.AreEqual(ResolveOutcome.Resolved, result.Outcome);
			Assert.AreEqual(ResolveMethod.Location, result.Method);
			Assert.IsFalse(result.IsConfident, "a match by name is good enough to navigate, not to save");
		}


		[TestMethod]
		public void ByName_WhenTheLinkHasNoGuid()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page");

			var result = new TargetResolver(Snap.Of(0, book)).Resolve(new TargetQuery
			{
				PageID = "{old}",
				Uri = "not a link",
				Location = "/notes/inbox/PAGE"
			});

			Assert.AreEqual(ResolveMethod.Location, result.Method);
		}


		[TestMethod]
		public void TwoPagesWithTheSameNameAndPath_AreAmbiguous()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{a}", "Same");
			Snap.Page(book, 8, "{b}", "Same");

			var result = new TargetResolver(Snap.Of(0, book))
				.Resolve(new TargetQuery { PageID = "{old}", Location = "/Notes/Inbox/Same" });

			Assert.AreEqual(ResolveOutcome.Ambiguous, result.Outcome);
		}


		[TestMethod]
		public void NothingFits_InAnOpenNotebook_IsBroken()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Something else");

			var result = new TargetResolver(Snap.Of(0, book)).Resolve(new TargetQuery
			{
				PageID = "{old}",
				Uri = Link(Snap.G3),
				Location = "/Notes/Inbox/Gone"
			});

			Assert.AreEqual(ResolveOutcome.Broken, result.Outcome);
			Assert.IsFalse(string.IsNullOrEmpty(result.Reason));
		}


		[TestMethod]
		public void NothingFits_BecauseTheNotebookIsClosed_IsOffline()
		{
			var book = Snap.Book("{nb}", "Notes");

			var result = new TargetResolver(Snap.Of(0, book)).Resolve(new TargetQuery
			{
				PageID = "{old}",
				Uri = Link(Snap.G3),
				Location = "/Archive/Inbox/Gone"
			});

			Assert.AreEqual(ResolveOutcome.Offline, result.Outcome);
		}


		[TestMethod]
		public void ThePageKeyBeatsAStaleLocation()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{mine}", "Mine", "Inbox");
			Snap.Page(book, 8, "{impostor}", "Old name", "Inbox");

			// the favorite was to page 7, which has since been renamed; another page now has its old name
			var result = new TargetResolver(Snap.Of(0, book)).Resolve(new TargetQuery
			{
				PageKey = 7,
				PageID = "{stale}",
				Location = "/Notes/Inbox/Old name"
			});

			Assert.AreEqual(7L, result.PageKey);
			Assert.AreEqual(ResolveMethod.Key, result.Method);
		}
	}


	[TestClass]
	public class TargetResolverContainerTests
	{
		[TestMethod]
		public void ASection_ByKeys_AfterEveryIDChanged()
		{
			var book = Snap.Book("{nb-new}", "Notes");
			Snap.Section(book, "{sec-new}", "Inbox");

			var result = new TargetResolver(Snap.Of(0, book)).Resolve(new TargetQuery
			{
				NotebookKey = "notes",
				SectionKey = "/Inbox",
				NotebookID = "{nb-old}",
				SectionID = "{sec-old}",
				Location = "/Notes/Inbox"
			});

			Assert.AreEqual(ResolveMethod.Key, result.Method);
			Assert.AreEqual("{sec-new}", result.SectionID);
			Assert.AreEqual("{nb-new}", result.NotebookID);
			Assert.AreEqual("/Notes/Inbox", result.Location);
		}


		[TestMethod]
		public void ASectionThatHoldsNoPages_IsFound()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Section(book, "{empty}", "Empty");

			var result = new TargetResolver(Snap.Of(0, book)).Resolve(
				new TargetQuery { NotebookKey = "notes", SectionKey = "/Empty", SectionID = "{old}" });

			Assert.AreEqual("{empty}", result.SectionID);
		}


		[TestMethod]
		public void ASectionGroup_ByKeys_IsNotConfusedWithASectionOfTheSameName()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Section(book, "{section}", "Work");
			Snap.Group(book, "{group}", "Work");

			var asGroup = new TargetResolver(Snap.Of(0, book)).Resolve(new TargetQuery
			{
				NotebookKey = "notes",
				SectionKey = "/Work",
				Kind = Favorite.KindSectionGroup
			});

			var asSection = new TargetResolver(Snap.Of(0, book)).Resolve(
				new TargetQuery { NotebookKey = "notes", SectionKey = "/Work" });

			Assert.AreEqual("{group}", asGroup.SectionID);
			Assert.AreEqual("{section}", asSection.SectionID);
		}


		[TestMethod]
		public void ANestedSection_ByKeys()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Group(book, "{g}", "Outer");
			Snap.Section(book, "{s}", "Deep", "Outer");

			var result = new TargetResolver(Snap.Of(0, book)).Resolve(
				new TargetQuery { NotebookKey = "notes", SectionKey = "/Outer/Deep" });

			Assert.AreEqual("{s}", result.SectionID);
			Assert.AreEqual("/Notes/Outer/Deep", result.Location);
		}


		[TestMethod]
		public void ANotebook_ByKey_StoresItsOwnIDAsTheSectionID()
		{
			var book = Snap.Book("{nb-new}", "Notes");

			var result = new TargetResolver(Snap.Of(0, book)).Resolve(new TargetQuery
			{
				NotebookKey = "notes",
				NotebookID = "{nb-old}",
				SectionID = "{nb-old}",
				Kind = Favorite.KindNotebook
			});

			Assert.AreEqual(ResolveMethod.Key, result.Method);
			Assert.AreEqual("{nb-new}", result.NotebookID);
			Assert.AreEqual("{nb-new}", result.SectionID);
			Assert.IsNull(result.SectionKey);
			Assert.AreEqual("/Notes", result.Location);
		}


		[TestMethod]
		public void ANotebookThatIsNotOpen_IsOffline()
		{
			var book = Snap.Book("{nb}", "Other");

			var result = new TargetResolver(Snap.Of(0, book)).Resolve(new TargetQuery
			{
				NotebookKey = "notes",
				SectionKey = "/Inbox"
			});

			Assert.AreEqual(ResolveOutcome.Offline, result.Outcome);
		}


		[TestMethod]
		public void ALegacySection_WithoutKeys_ByItsRememberedID()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Section(book, "{sec}", "Inbox");

			var result = new TargetResolver(Snap.Of(0, book))
				.Resolve(new TargetQuery { SectionID = "{sec}", NotebookID = "{nb}", Location = "/Notes/Inbox" });

			Assert.AreEqual(ResolveMethod.Handle, result.Method);
			Assert.AreEqual("notes", result.NotebookKey);
			Assert.AreEqual("/Inbox", result.SectionKey);
		}


		[TestMethod]
		public void ALegacySection_WithoutKeys_AfterAReopen_ByItsName_AsAGuess()
		{
			var book = Snap.Book("{nb-new}", "Notes");
			Snap.Section(book, "{sec-new}", "Inbox");

			var result = new TargetResolver(Snap.Of(0, book))
				.Resolve(new TargetQuery { SectionID = "{sec-old}", Location = "/notes/INBOX" });

			Assert.AreEqual(ResolveMethod.Location, result.Method);
			Assert.AreEqual("{sec-new}", result.SectionID);
			Assert.IsFalse(result.IsConfident);
		}


		[TestMethod]
		public void ALegacyNotebook_ByItsRememberedID_AndByName()
		{
			var book = Snap.Book("{nb}", "Notes");

			var byID = new TargetResolver(Snap.Of(0, book)).Resolve(new TargetQuery
			{
				SectionID = "{nb}",
				NotebookID = "{nb}",
				Kind = Favorite.KindNotebook,
				Location = "/Notes"
			});

			var byName = new TargetResolver(Snap.Of(0, book)).Resolve(new TargetQuery
			{
				SectionID = "{gone}",
				Kind = Favorite.KindNotebook,
				Location = "/Notes"
			});

			Assert.AreEqual(ResolveMethod.Handle, byID.Method);
			Assert.AreEqual(ResolveMethod.Location, byName.Method);
			Assert.AreEqual("{nb}", byName.NotebookID);
		}


		[TestMethod]
		public void TwoOpenNotebooksWithTheSameName_AreAmbiguousByName()
		{
			var a = Snap.Book("{a}", "Notes", "https://one/notes");
			var b = Snap.Book("{b}", "Notes", "https://two/notes");

			var result = new TargetResolver(Snap.Of(0, a, b))
				.Resolve(new TargetQuery { SectionID = "{old}", Location = "/Notes/Inbox" });

			Assert.AreEqual(ResolveOutcome.Ambiguous, result.Outcome);
		}


		[TestMethod]
		public void TheSameSectionNameInAnotherNotebook_IsNotTheOneWanted()
		{
			var a = Snap.Book("{a}", "Notes", "https://one/notes");
			var b = Snap.Book("{b}", "Notes", "https://two/notes");
			Snap.Section(a, "{sa}", "Inbox");
			Snap.Section(b, "{sb}", "Inbox");

			var result = new TargetResolver(Snap.Of(0, a, b)).Resolve(
				new TargetQuery { NotebookKey = "https://two/notes", SectionKey = "/Inbox" });

			Assert.AreEqual("{sb}", result.SectionID);
			Assert.AreEqual("{b}", result.NotebookID);
		}


		[TestMethod]
		public void ASectionThatIsGone_InAnOpenNotebook_IsBroken()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Section(book, "{other}", "Other");

			var result = new TargetResolver(Snap.Of(0, book)).Resolve(new TargetQuery
			{
				NotebookKey = "notes",
				SectionKey = "/Deleted",
				SectionID = "{old}",
				Location = "/Notes/Deleted"
			});

			Assert.AreEqual(ResolveOutcome.Broken, result.Outcome);
		}


		[TestMethod]
		public void AContainerIsNeverPending_ItDoesNotDependOnGuids()
		{
			var book = Snap.Book("{nb}", "Notes");

			var result = new TargetResolver(Snap.Of(500, book)).Resolve(new TargetQuery
			{
				NotebookKey = "notes",
				SectionKey = "/Deleted",
				Location = "/Notes/Deleted"
			});

			Assert.AreEqual(ResolveOutcome.Broken, result.Outcome);
		}
	}


	[TestClass]
	public class IdentityContainerGatherTests
	{
		private static IdentityNotebook Gather(XElement hierarchy)
		{
			var listing = new XElement(FakeHierarchy.Namespace + "Notebook",
				new XAttribute("ID", "{nb}"), new XAttribute("name", "Notes"));

			return IdentityGatherer.Gather(listing, hierarchy, new List<string>());
		}


		[TestMethod]
		public void EverySectionAndSectionGroup_IsRecorded_WithKeysAndPaths()
		{
			var book = Gather(FakeHierarchy.Book(
				FakeHierarchy.Section("{s1}", "Top"),
				FakeHierarchy.Group("Outer",
					FakeHierarchy.Group("Inner",
						FakeHierarchy.Section("{s2}", "Deep", FakeHierarchy.Page("{p}", "One"))))));

			Assert.AreEqual(4, book.Containers.Count);

			var top = book.Containers.Single(c => c.Name == "Top");
			Assert.AreEqual(ContainerKind.Section, top.Kind);
			Assert.AreEqual("{s1}", top.ID);
			Assert.AreEqual("/Top", top.SectionKey);
			Assert.AreEqual("/Notes/Top", top.Path);

			var inner = book.Containers.Single(c => c.Name == "Inner");
			Assert.AreEqual(ContainerKind.SectionGroup, inner.Kind);
			Assert.AreEqual("/Outer/Inner", inner.SectionKey);
			Assert.AreEqual("/Notes/Outer/Inner", inner.Path);

			var deep = book.Containers.Single(c => c.Name == "Deep");
			Assert.AreEqual("/Outer/Inner/Deep", deep.SectionKey);
			Assert.AreEqual("/Notes/Outer/Inner/Deep", deep.Path);
		}


		[TestMethod]
		public void ALockedSection_IsStillRecorded_ThoughItsPagesAreNot()
		{
			var locked = FakeHierarchy.Section("{l}", "Secret");
			locked.Add(new XAttribute("locked", "true"));

			var book = Gather(FakeHierarchy.Book(locked));

			Assert.AreEqual(1, book.Containers.Count);
			Assert.AreEqual(0, book.Pages.Count);
		}


		[TestMethod]
		public void RecycleBinContainers_AreNotRecorded()
		{
			var bin = FakeHierarchy.Group("OneNote_RecycleBin",
				FakeHierarchy.Section("{r}", "Deleted Pages"));
			bin.Add(new XAttribute("isRecycleBin", "true"));

			var deleted = FakeHierarchy.Section("{d}", "Deleted");
			deleted.Add(new XAttribute("isDeletedPages", "true"));

			var book = Gather(FakeHierarchy.Book(bin, deleted, FakeHierarchy.Section("{s}", "Real")));

			Assert.AreEqual(1, book.Containers.Count);
			Assert.AreEqual("Real", book.Containers[0].Name);
		}
	}
}
