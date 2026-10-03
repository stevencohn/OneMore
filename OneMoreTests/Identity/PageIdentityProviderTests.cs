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
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;


	[TestClass]
	public class PageIdentityProviderTests
	{
		private const string Modified = "2026-02-01T00:00:00.000Z";

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


		private static PageRef Page(string pageID, string title, string notebook = "nb",
			string section = "/s", string created = null, string modified = Modified)
		{
			return new PageRef(pageID, notebook, section, title,
				created ?? "2026-01-01T00:00:00.000Z", modified);
		}


		private static readonly string[] Nb = { "nb" };


		private long[] Keys(IEnumerable<PageResolution> resolutions)
		{
			return resolutions.Select(r => r.PageKey).ToArray();
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


		[TestMethod]
		public void Reconcile_NewPages_GetDistinctKeysAndAreStored()
		{
			var result = provider.Reconcile(Nb, new[] { Page("a", "one"), Page("b", "two") });

			Assert.IsTrue(result.All(r => r.Kind == ResolutionKind.New));
			Assert.IsTrue(result.All(r => r.PageKey > 0));
			Assert.AreEqual(2, result.Select(r => r.PageKey).Distinct().Count());
			Assert.AreEqual(2, provider.ReadAll().Count);
		}


		[TestMethod]
		public void Reconcile_SecondScan_ReturnsTheSameKeys()
		{
			var pages = new[] { Page("a", "one"), Page("b", "two") };
			var first = Keys(provider.Reconcile(Nb, pages));

			var second = provider.Reconcile(Nb, pages);

			CollectionAssert.AreEqual(first, Keys(second));
			Assert.IsTrue(second.All(r => r.Kind == ResolutionKind.Known));
			Assert.AreEqual(2, provider.ReadAll().Count);
		}


		[TestMethod]
		public void Reconcile_NotebookReopen_KeepsKeysAndUpdatesPageIDs()
		{
			var before = new[] { Page("a1", "one"), Page("b1", "two"), Page("c1", "three") };
			var keys = Keys(provider.Reconcile(Nb, before));

			var after = new[] { Page("a2", "one"), Page("b2", "two"), Page("c2", "three") };
			var result = provider.Reconcile(Nb, after);

			CollectionAssert.AreEqual(keys, Keys(result));
			Assert.IsTrue(result.All(r => r.Kind == ResolutionKind.SameLocation));
			CollectionAssert.AreEquivalent(new[] { "a2", "b2", "c2" }, provider.ReadAll().Select(r => r.PageID).ToArray());
			Assert.IsFalse(provider.ReadAll().Any(r => r.IsMissing));
		}


		[TestMethod]
		public void Reconcile_ReopenFillsInGradually_NothingIsLostAndNothingIsDeleted()
		{
			var first = new[] { Page("a1", "one"), Page("b1", "two"), Page("c1", "three"), Page("d1", "four") };
			var keys = Keys(provider.Reconcile(Nb, first));

			// a scan during the reopen sees only half of the notebook
			var partial = provider.Reconcile(Nb, new[] { Page("a2", "one"), Page("b2", "two") });
			CollectionAssert.AreEqual(keys.Take(2).ToArray(), Keys(partial));

			var all = provider.ReadAll();
			Assert.AreEqual(4, all.Count, "pages not yet seen are kept");
			Assert.AreEqual(2, all.Count(r => r.IsMissing));

			// the rest appear, with new IDs, on a later scan
			var rest = provider.Reconcile(Nb, new[]
			{
				Page("a2", "one"), Page("b2", "two"), Page("c2", "three"), Page("d2", "four")
			});

			CollectionAssert.AreEqual(keys, Keys(rest));
			Assert.IsFalse(provider.ReadAll().Any(r => r.IsMissing));
		}


		[TestMethod]
		public void Reconcile_PageMovedToAnotherSection_KeepsItsKey()
		{
			var key = provider.Reconcile(Nb, new[] { Page("a", "one", section: "/save") })[0].PageKey;

			var result = provider.Reconcile(Nb, new[] { Page("a-moved", "one", section: "/save2") });

			Assert.AreEqual(key, result[0].PageKey);
			Assert.AreEqual(ResolutionKind.MovedSection, result[0].Kind);
			Assert.AreEqual("/save2", provider.Read(key).SectionKey);
		}


		[TestMethod]
		public void Reconcile_PageMovedBetweenNotebooksInOneScan_KeepsItsKey()
		{
			var scope = new[] { "local", "flux" };
			var key = provider.Reconcile(scope, new[] { Page("a", "one", notebook: "local") })[0].PageKey;

			// flux is reconciled together with local, so the old identity is still unclaimed
			var result = provider.Reconcile(scope, new[] { Page("a-moved", "one", notebook: "flux") });

			Assert.AreEqual(key, result[0].PageKey);
			Assert.AreEqual(ResolutionKind.MovedNotebook, result[0].Kind);
			Assert.AreEqual("flux", provider.Read(key).NotebookKey);
		}


		[TestMethod]
		public void Reconcile_NotebookOutOfScope_IsLeftAlone()
		{
			provider.Reconcile(new[] { "a", "b" }, new[] { Page("x", "one", notebook: "a"), Page("y", "two", notebook: "b") });

			provider.Reconcile(new[] { "a" }, new[] { Page("x", "one", notebook: "a") });

			Assert.IsFalse(provider.ReadAll().Any(r => r.IsMissing), "notebook b was not scanned so its page is not missing");
		}


		[TestMethod]
		public void Reconcile_PageDeleted_BecomesMissingNotDeleted()
		{
			provider.Reconcile(Nb, new[] { Page("a", "one"), Page("b", "two") });

			provider.Reconcile(Nb, new[] { Page("a", "one") });

			var rows = provider.ReadAll();
			Assert.AreEqual(2, rows.Count);
			Assert.IsNotNull(rows.Single(r => r.PageID == "b").MissingSince);
			Assert.IsNull(rows.Single(r => r.PageID == "a").MissingSince);
		}


		[TestMethod]
		public void Reconcile_MissingPageReappearsWithSameID_ClearsMissing()
		{
			provider.Reconcile(Nb, new[] { Page("a", "one") });
			provider.Reconcile(Nb, new PageRef[0]);
			Assert.IsTrue(provider.ReadAll().Single().IsMissing);

			provider.Reconcile(Nb, new[] { Page("a", "one") });

			Assert.IsFalse(provider.ReadAll().Single().IsMissing);
		}


		[TestMethod]
		public void Reconcile_MissingFromAnotherNotebook_CanStillBeClaimed()
		{
			var key = provider.Reconcile(new[] { "a" }, new[] { Page("x", "one", notebook: "a") })[0].PageKey;
			provider.Reconcile(new[] { "a" }, new PageRef[0]);   // page leaves notebook a

			// a later scan of a different notebook finds it there
			var result = provider.Reconcile(new[] { "b" }, new[] { Page("y", "one", notebook: "b") });

			Assert.AreEqual(key, result[0].PageKey);
		}


		[TestMethod]
		public void Reconcile_Changes_AreWrittenBack()
		{
			var key = provider.Reconcile(Nb, new[] { Page("a", "one", modified: "2026-02-01T00:00:00.000Z") })[0].PageKey;

			provider.Reconcile(Nb, new[] { Page("a", "one", modified: "2026-09-09T00:00:00.000Z") });

			Assert.AreEqual("2026-09-09T00:00:00.000Z", provider.Read(key).Modified);
		}


		[TestMethod]
		public void Reconcile_EditedCreationDate_UpdatesCreatedAndAsksForRefresh()
		{
			var key = provider.Reconcile(Nb, new[] { Page("a", "one", created: "2026-08-13T13:49:30.000Z") })[0].PageKey;

			// the ID did not change but the notebook was reopened: both changed
			var result = provider.Reconcile(Nb, new[] { Page("b", "one", created: "2026-01-15T09:30:00.000Z") });

			Assert.AreEqual(key, result[0].PageKey);
			Assert.IsTrue(result[0].NeedsRefresh);
			Assert.AreEqual("2026-01-15T09:30:00.000Z", provider.Read(key).Created);
		}


		[TestMethod]
		public void PurgeMissing_RemovesOnlyPagesMissingLongerThanTheGracePeriod()
		{
			var keys = Keys(provider.Reconcile(Nb, new[] { Page("a", "one"), Page("b", "two"), Page("c", "three") }));
			provider.Reconcile(Nb, new[] { Page("a", "one") });   // b and c are now missing

			// backdate b so only it is older than the grace period
			Execute($"UPDATE identity_page SET missingSince = '2000-01-01T00:00:00.0000Z' WHERE pageKey = {keys[1]}");

			var purged = provider.PurgeMissing(TimeSpan.FromDays(1));

			CollectionAssert.AreEqual(new[] { keys[1] }, purged.ToArray());
			CollectionAssert.AreEquivalent(new[] { keys[0], keys[2] }, provider.ReadAll().Select(r => r.PageKey).ToArray());
		}


		[TestMethod]
		public void PurgeMissing_NeverTouchesPresentPages()
		{
			provider.Reconcile(Nb, new[] { Page("a", "one") });

			Assert.AreEqual(0, provider.PurgeMissing(TimeSpan.Zero).Count);
			Assert.AreEqual(1, provider.ReadAll().Count);
		}


		[TestMethod]
		public void Keys_AreNeverReusedAfterAPurge()
		{
			var first = provider.Reconcile(Nb, new[] { Page("a", "one") })[0].PageKey;
			provider.Reconcile(Nb, new PageRef[0]);
			Execute("UPDATE identity_page SET missingSince = '2000-01-01T00:00:00.0000Z'");
			provider.PurgeMissing(TimeSpan.FromDays(1));
			Assert.AreEqual(0, provider.ReadAll().Count);

			var next = provider.Reconcile(Nb, new[] { Page("b", "two") })[0].PageKey;

			Assert.IsTrue(next > first, "a reused key would attach old index rows to a different page");
		}


		[TestMethod]
		public void ReadByPageID_FindsTheCurrentPage()
		{
			var key = provider.Reconcile(Nb, new[] { Page("a", "one"), Page("b", "two") })[1].PageKey;

			Assert.AreEqual(key, provider.ReadByPageID("b").PageKey);
			Assert.IsNull(provider.ReadByPageID("nope"));
		}


		[TestMethod]
		public void ReadByPageID_AfterReopen_FindsByTheNewIDOnly()
		{
			provider.Reconcile(Nb, new[] { Page("old", "one") });
			var key = provider.Reconcile(Nb, new[] { Page("new", "one") })[0].PageKey;

			Assert.AreEqual(key, provider.ReadByPageID("new").PageKey);
			Assert.IsNull(provider.ReadByPageID("old"), "the old ID is gone once the identity is rehomed");
		}


		[TestMethod]
		public void Reconcile_SkippedSection_IsNotTreatedAsDeleted()
		{
			// a locked section cannot be listed, so its pages are absent from the page list
			var pages = new[]
			{
				Page("a", "open page", section: "/open"),
				Page("b", "locked page", section: "/locked")
			};
			provider.Reconcile(Nb, pages);

			var skip = new[] { PageIdentityKeys.SectionScope("nb", "/locked") };
			provider.Reconcile(Nb, new[] { Page("a", "open page", section: "/open") }, skip);

			Assert.IsFalse(provider.ReadAll().Any(r => r.IsMissing));
		}


		[TestMethod]
		public void Reconcile_SectionNotSkipped_IsTreatedAsDeleted()
		{
			provider.Reconcile(Nb, new[] { Page("a", "one", section: "/open"), Page("b", "two", section: "/gone") });

			provider.Reconcile(Nb, new[] { Page("a", "one", section: "/open") });

			Assert.AreEqual(1, provider.ReadAll().Count(r => r.IsMissing));
		}


		[TestMethod]
		public void Read_UnknownKey_ReturnsNull()
		{
			Assert.IsNull(provider.Read(12345));
		}


		[TestMethod]
		public void NotebookKey_PrefersPathAndIgnoresCaseAndTrailingSlash()
		{
			Assert.AreEqual("https://x/y", PageIdentityKeys.NotebookKey("HTTPS://x/y/", "Name"));
			Assert.AreEqual("name", PageIdentityKeys.NotebookKey(null, "Name"));
			Assert.AreEqual("name", PageIdentityKeys.NotebookKey("  ", "Name"));
		}


		[TestMethod]
		public void SectionKey_IncludesGroupPath()
		{
			Assert.AreEqual("/FluxTop/Group A/More Group/Section 3",
				PageIdentityKeys.SectionKey(new[] { "FluxTop", "Group A", "More Group" }, "Section 3"));
			Assert.AreEqual("/save", PageIdentityKeys.SectionKey(null, "save"));
		}


		[TestMethod]
		public void Catalog_NewerThanThisBuildKnows_IsLeftAloneAndStillUsable()
		{
			// another build, newer than this one, has already bumped the version
			Execute("UPDATE identity_schema SET version = 99");

			using var second = new PageIdentityProvider(connection);

			Assert.AreEqual(99, Convert.ToInt32(Scalar("SELECT version FROM identity_schema WHERE schemaID = 0")));

			var result = second.Reconcile(Nb, new[] { Page("a", "one") });
			Assert.AreEqual(1, result.Count);
		}


		[TestMethod]
		public void Catalog_FreshDatabase_IsAtVersionOne()
		{
			Assert.AreEqual(1, Convert.ToInt32(Scalar("SELECT version FROM identity_schema WHERE schemaID = 0")));
		}


		[TestMethod]
		public void Catalog_ReopenedOverAnExistingOne_KeepsItsRows()
		{
			provider.Reconcile(Nb, new[] { Page("a", "one") });

			// a second provider over the same connection sees the existing catalog
			var rows = new PageIdentityProvider(connection).ReadAll();

			Assert.AreEqual(1, rows.Count);
		}


		[TestMethod]
		public void Reconcile_TwoConnectionsAtOnce_GiveEveryNewPageExactlyOneKey()
		{
			var file = Path.Combine(Path.GetTempPath(), $"identity-{Guid.NewGuid():N}.db");
			var connectionString = $"Data Source={file};Default Timeout=30";

			try
			{
				using var c1 = new SQLiteConnection(connectionString);
				c1.Open();
				using var c2 = new SQLiteConnection(connectionString);
				c2.Open();

				using var p1 = new PageIdentityProvider(c1);
				using var p2 = new PageIdentityProvider(c2);

				var pages = Enumerable.Range(0, 60)
					.Select(i => Page($"p{i}", $"title {i}"))
					.ToArray();

				// the add-in and the tray both finding the same new pages at the same moment
				var first = Task.Run(() => p1.Reconcile(Nb, pages));
				var second = Task.Run(() => p2.Reconcile(Nb, pages));
				Task.WaitAll(first, second);

				Assert.AreEqual(60, p1.ReadAll().Count, "one record per page, not one per connection");
				CollectionAssert.AreEqual(Keys(first.Result), Keys(second.Result));
			}
			finally
			{
				foreach (var f in new[] { file, file + "-wal", file + "-shm", file + "-journal" })
				{
					try { File.Delete(f); } catch (IOException) { }
				}
			}
		}


		[TestMethod]
		public void Reconcile_Failure_RollsBackEverythingItDid()
		{
			// a page ID of null violates NOT NULL after other pages were already inserted
			var pages = new[]
			{
				Page("good", "one"),
				new PageRef(null, "nb", "/s", "bad", "2026-01-01T00:00:00.000Z", Modified)
			};

			Assert.ThrowsException<SQLiteException>(() => provider.Reconcile(Nb, pages));

			Assert.AreEqual(0, provider.ReadAll().Count, "the good page must not survive a failed pass");

			// and the connection is usable again, not stuck inside a transaction
			Assert.AreEqual(1, provider.Reconcile(Nb, new[] { Page("good", "one") }).Count);
		}
	}
}
