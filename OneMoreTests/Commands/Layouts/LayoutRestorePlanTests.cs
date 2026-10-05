//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Layouts
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Commands.Layouts;
	using River.OneMoreAddIn.Commands.Workspaces;
	using River.OneMoreAddIn.Tests.Commands.Workspaces;
	using System.Linq;


	[TestClass]
	public class LayoutRestorePlanTests
	{
		private static string Link(string id) => "link:" + id;


		private static string PageLink(string pageGuid) =>
			"onenote:https://x/Notes.one#Page&section-id={AAAAAAAA-0000-0000-0000-000000000000}&page-id=" +
			pageGuid + "&end";


		private static LayoutWindow Window(string name, string stalePageID, long? key = null,
			string guid = null, string location = "/Notes/Inbox/Page", string alias = null)
		{
			return new LayoutWindow
			{
				ID = 1,
				LayoutID = 1,
				Name = name,
				Alias = alias,
				Location = location,
				Uri = guid is null ? "onenote:old" : PageLink(guid),
				NotebookID = "{nb-old}",
				SectionID = "{sec-old}",
				PageID = stalePageID,
				ZOrder = 2,
				PageKey = key
			};
		}


		#region Plan
		[TestMethod]
		public void AWindowWhoseRememberedIDIsStale_IsLookedForUnderTheCurrentID()
		{
			// the bug this fixes: after a reopen the remembered ID matches no open window, so the page
			// was opened a second time and the new window was never found
			var book = Snap.Book("{nb-new}", "Notes");
			Snap.Page(book, 7, "{page-now}", "Page", guid: Snap.G1);

			var window = Window("Page", "{page-old}", key: 7);
			var item = LayoutRestorePlan.Plan(new[] { window }, new TargetResolver(Snap.Of(0, book)), Link).Single();

			Assert.IsTrue(item.CanOpen);
			Assert.AreEqual("{page-now}", item.PageID);
			Assert.AreEqual("link:{page-now}", item.Uri);
			Assert.AreEqual(ResolveMethod.Key, item.Resolution.Method);
		}


		[TestMethod]
		public void AWindowWithoutAKey_IsFoundByTheGuidInItsLink()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{page-now}", "Renamed", "Archive", guid: Snap.G1);

			var window = Window("Page", "{page-old}", guid: Snap.G1);
			var item = LayoutRestorePlan.Plan(new[] { window }, new TargetResolver(Snap.Of(0, book)), Link).Single();

			Assert.IsTrue(item.CanOpen);
			Assert.AreEqual("{page-now}", item.PageID);
			Assert.AreEqual(ResolveMethod.Guid, item.Resolution.Method);
		}


		[TestMethod]
		public void AWindowThatCannotBeFound_IsReported_AndTheOthersStillPlanned()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{a}", "Kept", guid: Snap.G1);

			var windows = new[]
			{
				Window("Kept", "{old-a}", guid: Snap.G1),
				Window("Gone", "{old-b}", guid: Snap.G3, location: "/Notes/Inbox/Gone"),
				Window("Also kept", "{a}")
			};

			var items = LayoutRestorePlan.Plan(windows, new TargetResolver(Snap.Of(0, book)), Link);

			CollectionAssert.AreEqual(new[] { true, false, true }, items.Select(i => i.CanOpen).ToList());
			Assert.AreEqual(ResolveOutcome.Broken, items[1].Outcome);
			Assert.IsFalse(string.IsNullOrEmpty(items[1].Reason));
		}


		[TestMethod]
		public void TheOrderOfTheWindows_IsKept()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 1, "{a}", "A");
			Snap.Page(book, 2, "{b}", "B");
			Snap.Page(book, 3, "{c}", "C");

			var windows = new[]
			{
				Window("C", "{c}", location: "/Notes/Inbox/C"),
				Window("A", "{a}", location: "/Notes/Inbox/A"),
				Window("B", "{b}", location: "/Notes/Inbox/B")
			};

			var items = LayoutRestorePlan.Plan(windows, new TargetResolver(Snap.Of(0, book)), Link);

			CollectionAssert.AreEqual(new[] { "C", "A", "B" }, items.Select(i => i.Window.Name).ToList());
		}


		[TestMethod]
		public void WithoutAResolver_EachWindowIsTriedWithWhatItRemembered()
		{
			var window = Window("Page", "{remembered}");

			var item = LayoutRestorePlan.Plan(new[] { window }, null, Link).Single();

			Assert.IsTrue(item.CanOpen);
			Assert.AreEqual("{remembered}", item.PageID);
			Assert.AreEqual("onenote:old", item.Uri);
			Assert.IsNull(item.Resolution);
		}


		[TestMethod]
		public void AMatchByNameAlone_IsStillOpened_BecauseRestoringIsAnExplicitRequest()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page");

			var item = LayoutRestorePlan.Plan(
				new[] { Window("Page", "{stale}") }, new TargetResolver(Snap.Of(0, book)), Link).Single();

			Assert.IsTrue(item.CanOpen);
			Assert.AreEqual(ResolveMethod.Location, item.Resolution.Method);
			Assert.IsFalse(item.Resolution.IsConfident);
		}


		[TestMethod]
		public void WhileGuidsAreUnread_AWindowIsPending_NotBroken()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Other");

			var item = LayoutRestorePlan.Plan(
				new[] { Window("Page", "{stale}", guid: Snap.G3) },
				new TargetResolver(Snap.Of(20, book)), Link).Single();

			Assert.IsFalse(item.CanOpen);
			Assert.AreEqual(ResolveOutcome.Pending, item.Outcome);
		}


		[TestMethod]
		public void AWindowInAClosedNotebook_IsOffline()
		{
			var book = Snap.Book("{nb}", "Other");

			var item = LayoutRestorePlan.Plan(
				new[] { Window("Page", "{stale}", guid: Snap.G3, location: "/Archive/Inbox/Page") },
				new TargetResolver(Snap.Of(0, book)), Link).Single();

			Assert.AreEqual(ResolveOutcome.Offline, item.Outcome);
		}


		[TestMethod]
		public void ALinkThatCannotBeMade_FallsBackToTheStoredOne()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page");

			var window = Window("Page", "{stale}", key: 7);
			var item = LayoutRestorePlan.Plan(new[] { window }, new TargetResolver(Snap.Of(0, book)), id => null).Single();

			Assert.AreEqual("onenote:old", item.Uri);
			Assert.AreEqual("{p}", item.PageID);
		}
		#endregion Plan


		#region AssignKeys
		[TestMethod]
		public void AssignKeys_GivesEachFoundWindowItsPageKey()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{a}", "A");
			Snap.Page(book, 8, "{b}", "B");

			var a = Window("A", "{a}", location: "/Notes/Inbox/A");
			var b = Window("B", "{b}", location: "/Notes/Inbox/B");

			LayoutRestorePlan.AssignKeys(new[] { a, b }, new TargetResolver(Snap.Of(0, book)));

			Assert.AreEqual(7L, a.PageKey);
			Assert.AreEqual(8L, b.PageKey);
		}


		[TestMethod]
		public void AssignKeys_NeverKeysAWindowFoundOnlyByName()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page");

			var window = Window("Page", "{stale}");
			LayoutRestorePlan.AssignKeys(new[] { window }, new TargetResolver(Snap.Of(0, book)));

			Assert.IsNull(window.PageKey, "a guess must not be written down as the page");
		}


		[TestMethod]
		public void AssignKeys_WithoutAResolver_ChangesNothing()
		{
			var window = Window("Page", "{p}");

			LayoutRestorePlan.AssignKeys(new[] { window }, null);

			Assert.IsNull(window.PageKey);
		}
		#endregion AssignKeys


		#region Apply
		[TestMethod]
		public void Apply_CopiesWhatWasFound_AndNeverTouchesTheUsersChoices()
		{
			var window = Window("Old name", "{page-old}", alias: "Mine");
			window.LayoutID = 3;

			var found = new TargetResolution
			{
				Outcome = ResolveOutcome.Resolved,
				Method = ResolveMethod.Guid,
				PageKey = 42,
				NotebookID = "{nb-new}",
				SectionID = "{sec-new}",
				PageID = "{page-new}",
				Name = "New name",
				Location = "/Notes/Archive/New name"
			};

			Assert.IsTrue(WorkspaceResolver.Apply(window, found, Link));

			Assert.AreEqual("{page-new}", window.PageID);
			Assert.AreEqual("{sec-new}", window.SectionID);
			Assert.AreEqual("{nb-new}", window.NotebookID);
			Assert.AreEqual(42L, window.PageKey);
			Assert.AreEqual("/Notes/Archive/New name", window.Location);
			Assert.AreEqual("link:{page-new}", window.Uri);

			Assert.AreEqual("Old name", window.Name);
			Assert.AreEqual("Mine", window.Alias);
			Assert.AreEqual(3, window.LayoutID);
			Assert.AreEqual(2, window.ZOrder);
		}


		[TestMethod]
		public void Apply_AgainWithTheSameResult_ChangesNothing()
		{
			var window = Window("Page", "{old}");
			var found = new TargetResolution
			{
				Outcome = ResolveOutcome.Resolved,
				PageKey = 42,
				NotebookID = "{nb}",
				SectionID = "{sec}",
				PageID = "{new}",
				Location = "/Notes/Inbox/Page"
			};

			WorkspaceResolver.Apply(window, found, Link);

			Assert.IsFalse(WorkspaceResolver.Apply(window, found, Link));
		}
		#endregion Apply


		[TestMethod]
		public void TheQuery_CarriesTheWindowsFingerprint()
		{
			var window = Window("Page", "{p}");
			window.Fingerprint = new TargetFingerprint
			{
				Title = "T",
				Created = "2026-01-01T00:00:00.000Z",
				NotebookKey = "nk",
				SectionKey = "/Inbox"
			};

			var query = TargetQuery.From(window);

			Assert.AreEqual("T", query.Title);
			Assert.AreEqual("nk", query.NotebookKey);
			Assert.AreEqual("{p}", query.PageID);
			Assert.IsTrue(query.IsPage);
		}


		[TestMethod]
		public void NameOf_PrefersTheUsersAlias()
		{
			Assert.AreEqual("Mine", LayoutRestorePlan.NameOf(Window("Page", "{p}", alias: "Mine")));
			Assert.AreEqual("Page", LayoutRestorePlan.NameOf(Window("Page", "{p}")));
			Assert.AreEqual("Page", LayoutRestorePlan.NameOf(Window("Page", "{p}", alias: "  ")));
		}


		[TestMethod]
		public void EveryReasonAWindowCannotBeOpened_HasItsOwnMessage()
		{
			var reasons = new[]
			{
				RestoreLayoutCommand.ReasonFor(ResolveOutcome.Pending),
				RestoreLayoutCommand.ReasonFor(ResolveOutcome.Offline),
				RestoreLayoutCommand.ReasonFor(ResolveOutcome.Ambiguous),
				RestoreLayoutCommand.ReasonFor(ResolveOutcome.Broken)
			};

			Assert.IsTrue(reasons.All(r => !string.IsNullOrWhiteSpace(r)));
			Assert.AreEqual(4, reasons.Distinct().Count());
		}
	}
}
