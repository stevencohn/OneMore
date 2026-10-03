//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Identity
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Identity;
	using System;
	using System.Collections.Generic;
	using System.Linq;


	/// <summary>
	/// Scenarios here mirror what was measured against real notebooks in Phase 0: a notebook
	/// reopen regenerates every page ID, a move changes only that page's ID, and a creation
	/// date edit changes neither the ID nor the modified time.
	/// </summary>
	[TestClass]
	public class PageIdentityMatcherTests
	{
		private const string Created = "2026-01-01T00:00:00.000Z";
		private const string Modified = "2026-02-01T00:00:00.000Z";


		private static IdentityRow Row(long key, string pageID, string notebook = "nb",
			string section = "/s", string title = "t", string created = Created,
			string modified = Modified, string missing = null)
		{
			return new IdentityRow
			{
				PageKey = key, PageID = pageID, NotebookKey = notebook, SectionKey = section,
				Title = title, Created = created, Modified = modified, Level = 1,
				MissingSince = missing
			};
		}


		private static PageRef Page(string pageID, string notebook = "nb", string section = "/s",
			string title = "t", string created = Created, string modified = Modified)
		{
			return new PageRef(pageID, notebook, section, title, created, modified);
		}


		private static PageMatchResult Match(IdentityRow[] rows, PageRef[] pages)
		{
			return PageIdentityMatcher.Match(rows, pages);
		}


		[TestMethod]
		public void SamePageIDs_AreKnown()
		{
			var result = Match(
				new[] { Row(1, "a"), Row(2, "b", title: "u") },
				new[] { Page("a"), Page("b", title: "u") });

			CollectionAssert.AreEqual(new long[] { 1, 2 }, result.Pages.Select(p => p.PageKey).ToArray());
			Assert.IsTrue(result.Pages.All(p => p.Kind == ResolutionKind.Known));
			Assert.AreEqual(0, result.Orphans.Count);
		}


		[TestMethod]
		public void NotebookReopen_AllIDsChanged_KeepsEveryKeyFromHierarchyAlone()
		{
			var rows = Enumerable.Range(1, 5)
				.Select(i => Row(i, $"old{i}", title: $"title {i}", created: $"2026-01-0{i}T00:00:00.000Z"))
				.ToArray();

			var pages = Enumerable.Range(1, 5).Reverse()
				.Select(i => Page($"new{i}", title: $"title {i}", created: $"2026-01-0{i}T00:00:00.000Z"))
				.ToArray();

			var result = Match(rows, pages);

			Assert.IsTrue(result.Pages.All(p => p.Kind == ResolutionKind.SameLocation));
			CollectionAssert.AreEqual(new long[] { 5, 4, 3, 2, 1 }, result.Pages.Select(p => p.PageKey).ToArray());
			Assert.IsFalse(result.Pages.Any(p => p.NeedsRefresh));
			Assert.AreEqual(0, result.Orphans.Count);
		}


		[TestMethod]
		public void NotebookReopen_PartiallyLoaded_MatchesWhatHasAppearedAndOrphansTheRest()
		{
			var rows = new[]
			{
				Row(1, "old1", title: "one"), Row(2, "old2", title: "two"), Row(3, "old3", title: "three")
			};

			var result = Match(rows, new[] { Page("new1", title: "one") });

			Assert.AreEqual(1L, result.Pages[0].PageKey);
			CollectionAssert.AreEquivalent(new long[] { 2, 3 }, result.Orphans.Select(o => o.PageKey).ToArray());
		}


		[TestMethod]
		public void IdenticalDuplicates_SameLocationAndModified_ArePairedBecauseTheyAreInterchangeable()
		{
			var rows = new[] { Row(1, "old1"), Row(2, "old2") };

			var result = Match(rows, new[] { Page("new1"), Page("new2") });

			CollectionAssert.AreEquivalent(new long[] { 1, 2 }, result.Pages.Select(p => p.PageKey).ToArray());
			Assert.IsTrue(result.Pages.All(p => p.Kind == ResolutionKind.SameLocation));
		}


		[TestMethod]
		public void Duplicates_WithDifferentModifiedTimes_AreNotPaired()
		{
			var rows = new[] { Row(1, "old1", modified: "2026-02-01T00:00:00.000Z"), Row(2, "old2", modified: "2026-03-01T00:00:00.000Z") };
			var pages = new[] { Page("new1", modified: "2026-02-01T00:00:00.000Z"), Page("new2", modified: "2026-03-01T00:00:00.000Z") };

			var result = Match(rows, pages);

			Assert.IsTrue(result.Pages.All(p => p.Kind == ResolutionKind.New));
			Assert.AreEqual(2, result.Orphans.Count);
		}


		[TestMethod]
		public void MovedToAnotherSection_MatchesOnTitleAndCreated()
		{
			var result = Match(
				new[] { Row(1, "old", section: "/save") },
				new[] { Page("new", section: "/save2") });

			Assert.AreEqual(1L, result.Pages[0].PageKey);
			Assert.AreEqual(ResolutionKind.MovedSection, result.Pages[0].Kind);
			Assert.IsFalse(result.Pages[0].NeedsRefresh);
		}


		[TestMethod]
		public void MovedToAnotherNotebook_MatchesOnTitleAndCreated()
		{
			var result = Match(
				new[] { Row(1, "old", notebook: "local") },
				new[] { Page("new", notebook: "flux", section: "/testing") });

			Assert.AreEqual(1L, result.Pages[0].PageKey);
			Assert.AreEqual(ResolutionKind.MovedNotebook, result.Pages[0].Kind);
		}


		[TestMethod]
		public void CopyBesideItsOriginal_IsNewNotTheOriginal()
		{
			// the copy in another notebook has the same title and created time as the original,
			// but the original keeps its page ID so it is claimed first
			var result = Match(
				new[] { Row(1, "orig", notebook: "local") },
				new[] { Page("orig", notebook: "local"), Page("copy", notebook: "flux") });

			Assert.AreEqual(ResolutionKind.Known, result.Pages[0].Kind);
			Assert.AreEqual(ResolutionKind.New, result.Pages[1].Kind);
			Assert.AreEqual(0, result.Orphans.Count);
		}


		[TestMethod]
		public void MovedOriginalWithExistingCopy_RehomesToTheOriginalNotTheCopy()
		{
			var rows = new[] { Row(1, "orig", notebook: "local"), Row(2, "copy", notebook: "flux") };
			var pages = new[] { Page("copy", notebook: "flux"), Page("moved", notebook: "third") };

			var result = Match(rows, pages);

			Assert.AreEqual(2L, result.Pages[0].PageKey);
			Assert.AreEqual(1L, result.Pages[1].PageKey);
			Assert.AreEqual(ResolutionKind.MovedNotebook, result.Pages[1].Kind);
		}


		[TestMethod]
		public void EditedCreationDate_AfterReopen_MatchesOnTitleButNeedsRefresh()
		{
			var result = Match(
				new[] { Row(1, "old", title: "report", created: "2026-08-13T13:49:30.000Z") },
				new[] { Page("new", title: "report", created: "2026-01-15T09:30:00.000Z") });

			Assert.AreEqual(1L, result.Pages[0].PageKey);
			Assert.AreEqual(ResolutionKind.SameTitle, result.Pages[0].Kind);
			Assert.IsTrue(result.Pages[0].NeedsRefresh);
		}


		[TestMethod]
		public void StrongSignalBeatsWeak_CreatedTimeWinsOverTitleOnly()
		{
			// the new page matches row 1 on title and created (moved), and row 2 on section and
			// title only; the created time is the stronger evidence
			var rows = new[]
			{
				Row(1, "old1", section: "/other", title: "notes", created: "2026-01-01T00:00:00.000Z"),
				Row(2, "old2", section: "/s", title: "notes", created: "2026-05-05T00:00:00.000Z")
			};

			var result = Match(rows, new[] { Page("new", section: "/s", title: "notes", created: "2026-01-01T00:00:00.000Z") });

			Assert.AreEqual(1L, result.Pages[0].PageKey);
			Assert.AreEqual(ResolutionKind.MovedSection, result.Pages[0].Kind);
		}


		[TestMethod]
		public void DuplicateTitles_WithEditedDate_AreNotGuessed()
		{
			var rows = new[]
			{
				Row(1, "old1", title: "notes", created: "2026-01-01T00:00:00.000Z"),
				Row(2, "old2", title: "notes", created: "2026-02-02T00:00:00.000Z")
			};
			var pages = new[]
			{
				Page("new1", title: "notes", created: "2026-03-03T00:00:00.000Z"),
				Page("new2", title: "notes", created: "2026-04-04T00:00:00.000Z")
			};

			var result = Match(rows, pages);

			Assert.IsTrue(result.Pages.All(p => p.Kind == ResolutionKind.New));
			Assert.AreEqual(2, result.Orphans.Count);
		}


		[TestMethod]
		public void MissingRow_WithSamePageID_IsKnown()
		{
			var result = Match(new[] { Row(1, "a", missing: "2026-03-01T00:00:00.000Z") }, new[] { Page("a") });

			Assert.AreEqual(ResolutionKind.Known, result.Pages[0].Kind);
			Assert.AreEqual(1L, result.Pages[0].PageKey);
		}


		[TestMethod]
		public void NothingToMatch_AllPagesAreNew()
		{
			var result = Match(new IdentityRow[0], new[] { Page("a"), Page("b", title: "u") });

			Assert.IsTrue(result.Pages.All(p => p.Kind == ResolutionKind.New && p.Row is null));
		}


	}
}
