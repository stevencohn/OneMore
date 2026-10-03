//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Identity
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Identity;
	using System;
	using System.Collections.Generic;
	using System.Data.SQLite;
	using System.Linq;
	using System.Threading.Tasks;


	[TestClass]
	public class LinkGuidsTests
	{
		private const string Link =
			"onenote:https://d.docs.live.net/6925d0374517d4b4/Documents/Personal/Automobiles.one" +
			"#2023%20Subaru%20Outback&section-id={726E1EF7-A728-4716-9D91-504F8A81FFF3}" +
			"&page-id={8535855b-3f24-441a-929e-2f09ad4b1354}&end";


		[TestMethod]
		public void PageGuid_IsReadFromALink_UpperCase()
		{
			Assert.AreEqual("{8535855B-3F24-441A-929E-2F09AD4B1354}", LinkGuids.PageGuid(Link));
		}


		[TestMethod]
		public void SectionGuid_IsReadFromALink()
		{
			Assert.AreEqual("{726E1EF7-A728-4716-9D91-504F8A81FFF3}", LinkGuids.SectionGuid(Link));
		}


		[TestMethod]
		public void LinkWithoutAPage_HasNoPageGuid()
		{
			var section = "onenote:https://x/Notes.one#Inbox&section-id={726E1EF7-A728-4716-9D91-504F8A81FFF3}&end";

			Assert.IsNull(LinkGuids.PageGuid(section));
			Assert.IsNotNull(LinkGuids.SectionGuid(section));
		}


		[TestMethod]
		public void NothingUsable_HasNoGuid()
		{
			Assert.IsNull(LinkGuids.PageGuid(null));
			Assert.IsNull(LinkGuids.PageGuid(string.Empty));
			Assert.IsNull(LinkGuids.PageGuid("not a link"));
			Assert.IsNull(LinkGuids.PageGuid("onenote:x#page-id={not-a-guid}&end"));
		}


		[TestMethod]
		public void AHierarchyID_IsNotALinkGuid()
		{
			// the IDs of the hierarchy look similar but are a different thing, with no mapping
			Assert.IsNull(LinkGuids.PageGuid(
				"{6FE9F8DE-538B-0217-22A8-BCF1071E8490}{1}{E19505994803043614769320119824582078987367701}"));
		}
	}


	[TestClass]
	public class PageGuidMatcherTests
	{
		private const string G1 = "{11111111-1111-1111-1111-111111111111}";
		private const string G2 = "{22222222-2222-2222-2222-222222222222}";
		private const string Modified = "2026-02-01T00:00:00.000Z";


		private static IdentityRow Row(long key, string pageID, string section, string title,
			string created, string guid)
		{
			return new IdentityRow
			{
				PageKey = key,
				PageID = pageID,
				NotebookKey = "nb",
				SectionKey = section,
				Title = title,
				Created = created,
				Modified = Modified,
				Level = 1,
				PageGuid = guid
			};
		}


		private static PageRef Page(string pageID, string section, string title, string created, string guid)
		{
			return new PageRef(pageID, "nb", section, title, created, Modified) { PageGuid = guid };
		}


		[TestMethod]
		public void RenamedMovedAndDateEdited_IsFoundByItsGuid()
		{
			var rows = new[] { Row(7, "{old}", "/one", "Old title", "2026-01-01", G1) };
			var pages = new[] { Page("{new}", "/two", "New title", "2026-03-03", G1) };

			var result = PageIdentityMatcher.Match(rows, pages);

			Assert.AreEqual(ResolutionKind.SameGuid, result.Pages[0].Kind);
			Assert.AreEqual(7, result.Pages[0].PageKey);
		}


		[TestMethod]
		public void WithoutAGuid_TheSamePageIsNew()
		{
			var rows = new[] { Row(7, "{old}", "/one", "Old title", "2026-01-01", G1) };
			var pages = new[] { Page("{new}", "/two", "New title", "2026-03-03", null) };

			Assert.AreEqual(ResolutionKind.New, PageIdentityMatcher.Match(rows, pages).Pages[0].Kind);
		}


		[TestMethod]
		public void GuidsAreComparedIgnoringCase()
		{
			var rows = new[] { Row(7, "{old}", "/one", "Old", "2026-01-01", G1.ToLowerInvariant()) };
			var pages = new[] { Page("{new}", "/two", "New", "2026-03-03", G1) };

			Assert.AreEqual(ResolutionKind.SameGuid, PageIdentityMatcher.Match(rows, pages).Pages[0].Kind);
		}


		[TestMethod]
		public void TwoPagesWithTheSameGuid_AreAmbiguous_AndDeclined()
		{
			var rows = new[] { Row(7, "{old}", "/one", "Old", "2026-01-01", G1) };
			var pages = new[]
			{
				Page("{a}", "/two", "Copy one", "2026-03-03", G1),
				Page("{b}", "/three", "Copy two", "2026-03-04", G1)
			};

			var result = PageIdentityMatcher.Match(rows, pages);

			Assert.IsTrue(result.Pages.All(r => r.Kind == ResolutionKind.New));
			Assert.AreEqual(1, result.Orphans.Count);
		}


		[TestMethod]
		public void TwoIdentitiesWithTheSameGuid_AreAmbiguous_AndDeclined()
		{
			var rows = new[]
			{
				Row(7, "{o1}", "/one", "Old one", "2026-01-01", G1),
				Row(8, "{o2}", "/one", "Old two", "2026-01-02", G1)
			};
			var pages = new[] { Page("{new}", "/two", "New", "2026-03-03", G1) };

			Assert.AreEqual(ResolutionKind.New, PageIdentityMatcher.Match(rows, pages).Pages[0].Kind);
		}


		[TestMethod]
		public void DifferentGuids_StopAWeakTitleMatch()
		{
			// same notebook, section and title, but the creation time differs: normally taken to be
			// the same page with an edited date; two different GUIDs say it is a different page
			var rows = new[] { Row(7, "{old}", "/one", "Notes", "2026-01-01", G1) };
			var pages = new[] { Page("{new}", "/one", "Notes", "2026-05-05", G2) };

			var result = PageIdentityMatcher.Match(rows, pages);

			Assert.AreEqual(ResolutionKind.New, result.Pages[0].Kind);
		}


		[TestMethod]
		public void SameGuid_ConfirmsAWeakTitleMatch()
		{
			var rows = new[] { Row(7, "{old}", "/one", "Notes", "2026-01-01", G1) };
			var pages = new[] { Page("{new}", "/one", "Notes", "2026-05-05", G1) };

			var result = PageIdentityMatcher.Match(rows, pages);

			Assert.AreEqual(7, result.Pages[0].PageKey);
		}


		[TestMethod]
		public void TheWeakTitleMatch_StillWorksWhenNeitherSideHasAGuid()
		{
			var rows = new[] { Row(7, "{old}", "/one", "Notes", "2026-01-01", null) };
			var pages = new[] { Page("{new}", "/one", "Notes", "2026-05-05", null) };

			Assert.AreEqual(ResolutionKind.SameTitle, PageIdentityMatcher.Match(rows, pages).Pages[0].Kind);
		}


		[TestMethod]
		public void StrongerMatches_ComeBeforeTheGuid()
		{
			// the same ID wins even if a GUID says otherwise
			var rows = new[] { Row(7, "{same}", "/one", "Notes", "2026-01-01", G1) };
			var pages = new[] { Page("{same}", "/one", "Notes", "2026-01-01", G2) };

			Assert.AreEqual(ResolutionKind.Known, PageIdentityMatcher.Match(rows, pages).Pages[0].Kind);
		}
	}


	[TestClass]
	public class PageGuidProviderTests
	{
		private const string G1 = "{11111111-1111-1111-1111-111111111111}";
		private const string G2 = "{22222222-2222-2222-2222-222222222222}";

		private SQLiteConnection connection;


		[TestInitialize]
		public void Setup()
		{
			connection = new SQLiteConnection("Data Source=:memory:");
			connection.Open();
		}


		[TestCleanup]
		public void Teardown()
		{
			connection?.Dispose();
		}


		private object Scalar(string sql)
		{
			using var cmd = connection.CreateCommand();
			cmd.CommandText = sql;
			return cmd.ExecuteScalar();
		}


		private void Execute(string sql)
		{
			using var cmd = connection.CreateCommand();
			cmd.CommandText = sql;
			cmd.ExecuteNonQuery();
		}


		private static PageRef Page(string pageID, string title, string section = "/s",
			string created = "2026-01-01T00:00:00.000Z", string guid = null)
		{
			return new PageRef(pageID, "nb", section, title, created, "2026-02-01T00:00:00.000Z")
			{
				PageGuid = guid
			};
		}


		private static readonly string[] Nb = { "nb" };


		[TestMethod]
		public void FreshCatalog_IsVersion1_WithTheGuidColumn()
		{
			using var provider = new PageIdentityProvider(connection);

			Assert.AreEqual(1L, Convert.ToInt64(Scalar("SELECT version FROM identity_schema")));
			Assert.AreEqual(1L, Convert.ToInt64(Scalar(
				"SELECT count(*) FROM pragma_table_info('identity_page') WHERE name = 'pageGuid'")));
		}


		[TestMethod]
		public void ADatabaseAnInterimBuildLeftAtVersion2_IsLeftAloneAndStillUsable()
		{
			// during development the catalog went to version 2 before the first release, which is
			// version 1; a database left at 2 has the same shape and is simply not touched
			using (var first = new PageIdentityProvider(connection))
			{
				Execute("UPDATE identity_schema SET version = 2");

				var second = new PageIdentityProvider(connection);
				Assert.IsNotNull(second);

				Assert.AreEqual(2L, Convert.ToInt64(Scalar("SELECT version FROM identity_schema")));

				var found = second.Reconcile(Nb, new[] { Page("{a}", "One", guid: G1) });
				Assert.AreEqual(1, found.Count);
				Assert.AreEqual(G1, second.ReadAll().Single().PageGuid);
			}
		}


		[TestMethod]
		public void Reconcile_StoresAGuid_AndAnUnknownOneDoesNotEraseIt()
		{
			using var provider = new PageIdentityProvider(connection);

			provider.Reconcile(Nb, new[] { Page("{a}", "One", guid: G1) });
			Assert.AreEqual(G1, provider.ReadAll().Single().PageGuid);

			// a later pass that did not read the GUID must not forget it
			provider.Reconcile(Nb, new[] { Page("{a}", "One", guid: null) });
			Assert.AreEqual(G1, provider.ReadAll().Single().PageGuid);
		}


		[TestMethod]
		public void WritePageGuids_ThenReadByGuid()
		{
			using var provider = new PageIdentityProvider(connection);
			var keys = provider.Reconcile(Nb, new[] { Page("{a}", "One"), Page("{b}", "Two") })
				.Select(r => r.PageKey).ToArray();

			Assert.AreEqual(2, provider.WritePageGuids(new[] { (keys[0], G1), (keys[1], G2) }));

			Assert.AreEqual(keys[0], provider.ReadByGuid(G1.ToLowerInvariant()).Single().PageKey);
			Assert.AreEqual(keys[1], provider.ReadByGuid(G2).Single().PageKey);
			Assert.AreEqual(0, provider.ReadByGuid("{33333333-3333-3333-3333-333333333333}").Count);
		}


		[TestMethod]
		public void ReadByGuid_SharedGuid_ReturnsEveryIdentity_PresentOnesFirst()
		{
			using var provider = new PageIdentityProvider(connection);
			var keys = provider.Reconcile(Nb, new[] { Page("{a}", "One"), Page("{b}", "Two") })
				.Select(r => r.PageKey).ToArray();

			provider.WritePageGuids(new[] { (keys[0], G1), (keys[1], G1) });
			Execute($"UPDATE identity_page SET missingSince = 'x' WHERE pageKey = {keys[0]}");

			var rows = provider.ReadByGuid(G1);

			Assert.AreEqual(2, rows.Count);
			Assert.AreEqual(keys[1], rows[0].PageKey);
		}


		[TestMethod]
		public void Reconcile_FindsARenamedMovedPage_ByItsStoredGuid()
		{
			using var provider = new PageIdentityProvider(connection);

			var before = provider.Reconcile(Nb, new[] { Page("{a}", "Old", "/one", guid: G1) });

			var after = provider.Reconcile(Nb, new[]
			{
				Page("{z}", "New", "/two", "2026-09-09T00:00:00.000Z", G1)
			});

			Assert.AreEqual(before[0].PageKey, after[0].PageKey);
			Assert.AreEqual(ResolutionKind.SameGuid, after[0].Kind);
			Assert.AreEqual(1, provider.ReadAll().Count);
		}


		[TestMethod]
		public void FindPagesNeedingGuid_NothingStoredHasAGuid_ReadsNone()
		{
			using var provider = new PageIdentityProvider(connection);
			provider.Reconcile(Nb, new[] { Page("{a}", "Old") });

			// a brand new page, but no stored identity has a GUID that could rescue it
			var found = provider.FindPagesNeedingGuid(Nb, new[] { Page("{new}", "Different") });

			Assert.AreEqual(0, found.Count);
		}


		[TestMethod]
		public void FindPagesNeedingGuid_ASavedGuidCouldRescue_ReadsOnlyTheUnmatchedPages()
		{
			using var provider = new PageIdentityProvider(connection);
			provider.Reconcile(Nb, new[]
			{
				Page("{a}", "Stays", guid: G1),
				Page("{b}", "Goes", "/one", guid: G2)
			});

			var found = provider.FindPagesNeedingGuid(Nb, new[]
			{
				Page("{a2}", "Stays"),                               // same location: already matched
				Page("{x}", "Renamed", "/two", "2026-09-09T00:00:00.000Z")   // matches nothing
			});

			Assert.AreEqual(1, found.Count);
			Assert.AreEqual("{x}", found[0].PageID);
		}


		[TestMethod]
		public void FindPagesNeedingGuid_NothingChanged_ReadsNone()
		{
			using var provider = new PageIdentityProvider(connection);
			provider.Reconcile(Nb, new[] { Page("{a}", "One", guid: G1) });

			Assert.AreEqual(0, provider.FindPagesNeedingGuid(Nb, new[] { Page("{a}", "One") }).Count);
		}
	}


	[TestClass]
	public class PageGuidPassTests
	{
		private const string G1 = "{11111111-1111-1111-1111-111111111111}";
		private const string G2 = "{22222222-2222-2222-2222-222222222222}";
		private const string G3 = "{33333333-3333-3333-3333-333333333333}";

		private SQLiteConnection connection;
		private PageIdentityProvider provider;


		[TestInitialize]
		public void Setup()
		{
			connection = new SQLiteConnection("Data Source=:memory:");
			connection.Open();
			provider = new PageIdentityProvider(connection);
		}


		[TestCleanup]
		public void Teardown()
		{
			provider?.Dispose();
			connection?.Dispose();
		}


		private static FakeHierarchy Hierarchy(string section, string pageID, string title, string created)
		{
			return new FakeHierarchy().Notebook("{nb}", "Notes", FakeHierarchy.Book(
				FakeHierarchy.Section("{" + section + "}", section,
					FakeHierarchy.Page(pageID, title, created))));
		}


		[TestMethod]
		public async Task GuidsAreFilledIn_AndRecorded()
		{
			var source = Hierarchy("Inbox", "{p1}", "One", "2026-01-01T00:00:00.000Z");
			source.Guids["{p1}"] = G1;

			var snapshot = await new IdentityPass(provider, source).Run();

			Assert.AreEqual(G1, snapshot.Pages.Single().PageGuid);
			Assert.AreEqual(0, snapshot.GuidsPending);
			Assert.AreEqual(G1, provider.ReadAll().Single().PageGuid);
		}


		[TestMethod]
		public async Task AGuidThatCannotBeRead_IsReportedPending_NotInvented()
		{
			var source = Hierarchy("Inbox", "{p1}", "One", "2026-01-01T00:00:00.000Z");

			var snapshot = await new IdentityPass(provider, source).Run();

			Assert.IsNull(snapshot.Pages.Single().PageGuid);
			Assert.AreEqual(1, snapshot.GuidsPending);
		}


		[TestMethod]
		public async Task OnceFilledIn_OneNoteIsNotAskedAgain_WhenNothingChanged()
		{
			var source = Hierarchy("Inbox", "{p1}", "One", "2026-01-01T00:00:00.000Z");
			source.Guids["{p1}"] = G1;

			await new IdentityPass(provider, source).Run();
			var calls = source.GuidCalls;

			var again = await new IdentityPass(provider, source).Run();

			Assert.AreEqual(calls, source.GuidCalls);
			Assert.AreEqual(G1, again.Pages.Single().PageGuid);
		}


		[TestMethod]
		public async Task ARenamedMovedAndDateEditedPage_KeepsItsKey_ByItsGuid()
		{
			var first = Hierarchy("Inbox", "{p1}", "Old title", "2026-01-01T00:00:00.000Z");
			first.Guids["{p1}"] = G1;
			var before = await new IdentityPass(provider, first).Run();

			// everything about the page changed, including its page ID, but not its GUID
			var second = Hierarchy("Archive", "{p1-after-reopen}", "New title", "2026-09-09T00:00:00.000Z");
			second.Guids["{p1-after-reopen}"] = G1;
			var after = await new IdentityPass(provider, second).Run();

			Assert.AreEqual(before.Pages.Single().PageKey, after.Pages.Single().PageKey);
			Assert.AreEqual(ResolutionKind.SameGuid, after.Pages.Single().Resolution.Kind);
			Assert.AreEqual(1, provider.ReadAll().Count);
		}


		[TestMethod]
		public async Task WithoutAGuid_TheSameChange_IsANewPage()
		{
			var first = Hierarchy("Inbox", "{p1}", "Old title", "2026-01-01T00:00:00.000Z");
			var before = await new IdentityPass(provider, first).Run();

			var second = Hierarchy("Archive", "{p1-after-reopen}", "New title", "2026-09-09T00:00:00.000Z");
			var after = await new IdentityPass(provider, second).Run();

			Assert.AreNotEqual(before.Pages.Single().PageKey, after.Pages.Single().PageKey);
		}


		[TestMethod]
		public async Task OnlyThePagesThatCouldBeMatchedByGuid_AreAskedFor()
		{
			var first = new FakeHierarchy().Notebook("{nb}", "Notes", FakeHierarchy.Book(
				FakeHierarchy.Section("{s}", "Inbox",
					FakeHierarchy.Page("{a}", "Stays"),
					FakeHierarchy.Page("{b}", "Goes"))));
			first.Guids["{a}"] = G1;
			first.Guids["{b}"] = G2;
			await new IdentityPass(provider, first).Run();

			// reopened: "Stays" has a new ID only; "Goes" was renamed and moved as well
			var second = new FakeHierarchy().Notebook("{nb}", "Notes", FakeHierarchy.Book(
				FakeHierarchy.Section("{s}", "Inbox", FakeHierarchy.Page("{a2}", "Stays")),
				FakeHierarchy.Section("{t}", "Other",
					FakeHierarchy.Page("{b2}", "Renamed", "2026-09-09T00:00:00.000Z"))));
			second.Guids["{a2}"] = G1;
			second.Guids["{b2}"] = G2;

			var after = await new IdentityPass(provider, second).Run();

			// "Goes" is found by its GUID, "Stays" by location; one identity each, none new
			Assert.AreEqual(2, provider.ReadAll().Count);
			Assert.IsTrue(after.Pages.Any(p => p.Resolution.Kind == ResolutionKind.SameGuid));
			Assert.IsTrue(after.Pages.Any(p => p.Resolution.Kind == ResolutionKind.SameLocation));
		}
	}
}
