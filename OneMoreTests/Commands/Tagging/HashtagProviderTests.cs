//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Tagging
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Identity;
	using System.Collections.Generic;
	using System.Data.SQLite;
	using System.Linq;


	[TestClass]
	public class HashtagProviderTests
	{
		private const string Epoch = "0001-01-01T00:00:00.0000Z";
		private const string Stamp = "2026-02-01T00:00:00.000Z";

		private SQLiteConnection connection;
		private HashtagProvider provider;


		[TestInitialize]
		public void Setup()
		{
			connection = new SQLiteConnection("Data Source=:memory:");
			connection.Open();
			provider = new HashtagProvider(connection);
		}


		[TestCleanup]
		public void Teardown()
		{
			provider?.Dispose();
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


		private static Hashtag Tag(string tag, string moreID, string objectID, int order = 0)
		{
			return new Hashtag
			{
				Tag = tag,
				MoreID = moreID,
				PageID = "{page-" + moreID + "}",
				ObjectID = objectID,
				Snippet = "text " + tag,
				DocumentOrder = order,
				LastModified = Stamp
			};
		}


		// records a page with tags, as a scan does
		private void Record(string moreID, string pageID, string notebookID = "{nb}",
			string sectionID = "{sec}", string path = "/Notes/Inbox", string title = "A page",
			params string[] tags)
		{
			var list = new Hashtags();
			for (var i = 0; i < tags.Length; i++)
			{
				list.Add(Tag(tags[i], moreID, "{obj-" + i + "}", i));
			}

			provider.WritePageInfo(moreID, pageID, "{title}", notebookID, sectionID, path, title);
			Assert.IsTrue(provider.WriteTags(moreID, list));
		}


		private static HashtagPageInfo Info(string moreID, string pageID, string notebookID = "{nb}",
			string sectionID = "{sec}", string path = "/Notes/Inbox", string title = "A page")
		{
			return new HashtagPageInfo
			{
				MoreID = moreID,
				PageID = pageID,
				NotebookID = notebookID,
				SectionID = sectionID,
				Path = path,
				Name = title
			};
		}


		#region Upgrade 5 to 6
		// a catalog as version 5 left it: tags and pages under omPageID stamps
		private void MakeVersion5()
		{
			Execute("UPDATE hashtag_scanner SET version = 5, scanTime = '2026-09-01T00:00:00.0000Z'");
			Execute("INSERT INTO hashtag_page (moreID, pageID, titleID, notebookID, sectionID, path, name) " +
				"VALUES ('6f037c477bea4d8fa0a0366e9b5e3a89', '{old}', '{t}', '{nb}', '{sec}', '/Notes/Inbox', 'Old')");
			Execute("INSERT INTO hashtag (tag, moreID, objectID, snippet, documentOrder, lastModified) " +
				"VALUES ('#old', '6f037c477bea4d8fa0a0366e9b5e3a89', '{o}', 's', 0, '2026-01-01T00:00:00.000Z')");
			Execute("INSERT INTO hashtag_notebook (notebookID, name, included, lastModified) " +
				"VALUES ('{nb}', 'Notes', 0, '2026-08-01T00:00:00.000Z')");
		}


		[TestMethod]
		public void NewCatalog_IsVersion6()
		{
			Assert.AreEqual(6L, System.Convert.ToInt64(Scalar("SELECT version FROM hashtag_scanner")));
		}


		[TestMethod]
		public void Upgrade5to6_ClearsTagsAndPages_ResetsScanTime_KeepsNotebooks()
		{
			MakeVersion5();

			using var upgraded = new HashtagProvider(connection);

			Assert.AreEqual(6L, System.Convert.ToInt64(Scalar("SELECT version FROM hashtag_scanner")));
			Assert.AreEqual(0L, System.Convert.ToInt64(Scalar("SELECT count(*) FROM hashtag")));
			Assert.AreEqual(0L, System.Convert.ToInt64(Scalar("SELECT count(*) FROM hashtag_page")));
			Assert.AreEqual(Epoch, (string)Scalar("SELECT scanTime FROM hashtag_scanner"));

			// the user's choice to exclude a notebook survives
			Assert.AreEqual(1L, System.Convert.ToInt64(Scalar("SELECT count(*) FROM hashtag_notebook")));
			Assert.AreEqual(0L, System.Convert.ToInt64(
				Scalar("SELECT included FROM hashtag_notebook WHERE notebookID = '{nb}'")));
		}


		[TestMethod]
		public void Upgrade5to6_CatalogOpenedAgainAfterUpgrade_KeepsTagsWrittenSince()
		{
			MakeVersion5();
			using var first = new HashtagProvider(connection);

			// the first build to upgrade then rebuilds some tags under page keys
			Record("42", "{new}", tags: "#new");

			// opening the catalog again, as another process does, must not wipe them (the in-lock
			// re-check that covers two openers racing at version 5 is not exercised here)
			using var second = new HashtagProvider(connection);

			Assert.AreEqual(1L, System.Convert.ToInt64(Scalar("SELECT count(*) FROM hashtag WHERE moreID = '42'")));
		}


		[TestMethod]
		public void CatalogNewerThanKnown_IsLeftAlone()
		{
			Record("42", "{p}", tags: "#keep");
			Execute("UPDATE hashtag_scanner SET version = 7");

			using var again = new HashtagProvider(connection);

			Assert.AreEqual(7L, System.Convert.ToInt64(Scalar("SELECT version FROM hashtag_scanner")));
			Assert.AreEqual(1L, System.Convert.ToInt64(Scalar("SELECT count(*) FROM hashtag")));
		}
		#endregion Upgrade 5 to 6


		#region Tags by page key
		[TestMethod]
		public void WriteTags_ThenReadPageTags_ByKey()
		{
			Record("42", "{p}", tags: new[] { "#one", "#two" });

			var tags = provider.ReadPageTags("42");

			Assert.AreEqual(2, tags.Count);
			Assert.AreEqual("#one", tags[0].Tag);
			Assert.AreEqual("#two", tags[1].Tag);
			Assert.AreEqual("42", tags[0].MoreID);
			Assert.AreEqual("{p}", tags[0].PageID);
		}


		[TestMethod]
		public void ReadPageTags_AreFoundByKey_NotByPageID()
		{
			Record("42", "{old-id}", tags: "#one");

			// OneNote regenerated the page ID; the key still finds the tags
			provider.RefreshPageInfo(new[] { Info("42", "{new-id}") });

			Assert.AreEqual(1, provider.ReadPageTags("42").Count);
			Assert.AreEqual("{new-id}", provider.ReadPageTags("42")[0].PageID);
		}


		[TestMethod]
		public void WriteTags_ReplacesOnlyThatPage()
		{
			Record("42", "{a}", tags: new[] { "#one", "#two" });
			Record("43", "{b}", tags: "#other");

			var replacement = new Hashtags { Tag("#three", "42", "{obj-9}") };
			Assert.IsTrue(provider.WriteTags("42", replacement));

			Assert.AreEqual(1, provider.ReadPageTags("42").Count);
			Assert.AreEqual("#three", provider.ReadPageTags("42")[0].Tag);
			Assert.AreEqual(1, provider.ReadPageTags("43").Count);
		}


		[TestMethod]
		public void WriteTags_Empty_RemovesTagsAndThePageRecord()
		{
			Record("42", "{a}", tags: "#one");

			Assert.IsTrue(provider.WriteTags("42", new Hashtags()));

			Assert.AreEqual(0, provider.ReadPageTags("42").Count);
			Assert.AreEqual(0, provider.ReadTaggedPages().Count);
		}
		#endregion Tags by page key


		#region Refresh, delete
		[TestMethod]
		public void ReadTaggedPages_ByKey()
		{
			Record("42", "{a}", path: "/Notes/Inbox", title: "First", tags: "#one");
			Record("43", "{b}", path: "/Notes/Done", title: "Second", tags: "#two");

			var pages = provider.ReadTaggedPages();

			Assert.AreEqual(2, pages.Count);
			Assert.AreEqual("{a}", pages["42"].PageID);
			Assert.AreEqual("/Notes/Done", pages["43"].Path);
			Assert.AreEqual("Second", pages["43"].Name);
		}


		[TestMethod]
		public void RefreshPageInfo_UpdatesLocation_KeepsTagsAndTitleID()
		{
			Record("42", "{old}", notebookID: "{nb-old}", sectionID: "{sec-old}",
				path: "/Notes/Inbox", title: "Before", tags: new[] { "#one", "#two" });

			var count = provider.RefreshPageInfo(new[]
			{
				Info("42", "{new}", "{nb-new}", "{sec-new}", "/Notes/Group/Done", "After")
			});

			Assert.AreEqual(1, count);

			var page = provider.ReadTaggedPages()["42"];
			Assert.AreEqual("{new}", page.PageID);
			Assert.AreEqual("{nb-new}", page.NotebookID);
			Assert.AreEqual("{sec-new}", page.SectionID);
			Assert.AreEqual("/Notes/Group/Done", page.Path);
			Assert.AreEqual("After", page.Name);
			Assert.AreEqual("{title}", page.TitleID);
			Assert.AreEqual(2, provider.ReadPageTags("42").Count);
		}


		[TestMethod]
		public void RefreshPageInfo_UnknownKey_IsIgnored()
		{
			Assert.AreEqual(0, provider.RefreshPageInfo(new[] { Info("999", "{x}") }));
			Assert.AreEqual(0, provider.ReadTaggedPages().Count);
		}


		[TestMethod]
		public void RefreshPageInfo_Nothing_ReturnsZero()
		{
			Assert.AreEqual(0, provider.RefreshPageInfo(new List<HashtagPageInfo>()));
		}


		[TestMethod]
		public void DeleteTags_RemovesTagsAndPage_OfTheGivenKeysOnly()
		{
			Record("42", "{a}", tags: new[] { "#one", "#two" });
			Record("43", "{b}", tags: "#keep");

			var count = provider.DeleteTags(new[] { "42", "7" });

			Assert.AreEqual(1, count);
			Assert.AreEqual(0, provider.ReadPageTags("42").Count);
			Assert.AreEqual(1, provider.ReadPageTags("43").Count);
			Assert.AreEqual(1, provider.ReadTaggedPages().Count);
		}


		[TestMethod]
		public void DeleteTags_Nothing_ReturnsZero()
		{
			Assert.AreEqual(0, provider.DeleteTags(new string[0]));
		}
		#endregion Refresh, delete


		#region ReconcileNotebooks
		private void Notebook(string id, string name, int included = 1, string modified = "")
		{
			Execute($"INSERT INTO hashtag_notebook (notebookID, name, included, lastModified) " +
				$"VALUES ('{id}', '{name}', {included}, '{modified}')");
		}


		private static IReadOnlyList<(string ID, string Name)> Open(params (string, string)[] books)
		{
			return books;
		}


		private List<string> NotebookIDs()
		{
			var ids = new List<string>();
			using var cmd = connection.CreateCommand();
			cmd.CommandText = "SELECT notebookID FROM hashtag_notebook ORDER BY notebookID";
			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				ids.Add(reader.GetString(0));
			}

			return ids;
		}


		[TestMethod]
		public void ReconcileNotebooks_ReopenedNotebook_TakesTheNewID_KeepsItsChoices()
		{
			Notebook("{old}", "Local", included: 0, modified: Stamp);

			var changed = provider.ReconcileNotebooks(Open(("{new}", "Local")));

			Assert.AreEqual(1, changed);
			CollectionAssert.AreEqual(new[] { "{new}" }, NotebookIDs());

			var known = provider.ReadKnownNotebooks().Single();
			Assert.IsFalse(known.Included);
			Assert.AreEqual(Stamp, known.LastModified);
		}


		[TestMethod]
		public void ReconcileNotebooks_BothRecorded_MergesIntoOne_ExclusionWins()
		{
			// what a reopen left behind: the old record and a new one, as seen in practice
			Notebook("{old}", "Local", included: 0, modified: Stamp);
			Notebook("{new}", "Local", included: 1, modified: "");

			provider.ReconcileNotebooks(Open(("{new}", "Local")));

			CollectionAssert.AreEqual(new[] { "{new}" }, NotebookIDs());

			var known = provider.ReadKnownNotebooks().Single();
			Assert.IsFalse(known.Included);
			Assert.AreEqual(Stamp, known.LastModified);
		}


		[TestMethod]
		public void ReconcileNotebooks_Included_StaysIncluded()
		{
			Notebook("{old}", "Local", included: 1, modified: Stamp);
			Notebook("{new}", "Local", included: 1, modified: "");

			provider.ReconcileNotebooks(Open(("{new}", "Local")));

			Assert.IsTrue(provider.ReadKnownNotebooks().Single().Included);
		}


		[TestMethod]
		public void ReconcileNotebooks_NameMatchIgnoresCase()
		{
			Notebook("{old}", "Local", included: 0);

			provider.ReconcileNotebooks(Open(("{new}", "LOCAL")));

			CollectionAssert.AreEqual(new[] { "{new}" }, NotebookIDs());
		}


		[TestMethod]
		public void ReconcileNotebooks_ClosedNotebook_IsKept()
		{
			Notebook("{closed}", "Archive", included: 0);
			Notebook("{old}", "Local");

			provider.ReconcileNotebooks(Open(("{new}", "Local")));

			CollectionAssert.AreEqual(new[] { "{closed}", "{new}" }, NotebookIDs());
		}


		[TestMethod]
		public void ReconcileNotebooks_TwoOpenNotebooksWithTheSameName_AreLeftAlone()
		{
			Notebook("{old}", "Notes", included: 0);

			var changed = provider.ReconcileNotebooks(Open(("{a}", "Notes"), ("{b}", "Notes")));

			Assert.AreEqual(0, changed);
			CollectionAssert.AreEqual(new[] { "{old}" }, NotebookIDs());
		}


		[TestMethod]
		public void ReconcileNotebooks_NothingToDo_ChangesNothing()
		{
			Notebook("{a}", "Local", included: 0, modified: Stamp);

			Assert.AreEqual(0, provider.ReconcileNotebooks(Open(("{a}", "Local"))));
			Assert.AreEqual(0, provider.ReconcileNotebooks(Open(("{z}", "Other"))));
			CollectionAssert.AreEqual(new[] { "{a}" }, NotebookIDs());
		}
		#endregion ReconcileNotebooks
	}


	[TestClass]
	public class HashtagScannerPolicyTests
	{
		private const string Before = "2026-09-01T00:00:00.000Z";
		private const string After = "2026-10-01T00:00:00.000Z";
		private const string LastScan = "2026-09-15T00:00:00.000Z";


		[TestMethod]
		public void UnchangedKnownPage_IsNotScanned()
		{
			Assert.IsFalse(HashtagScanner.NeedsScan(false, Before, LastScan, ResolutionKind.Known, false));
			Assert.IsFalse(HashtagScanner.NeedsScan(false, Before, LastScan, ResolutionKind.SameLocation, false));
		}


		[TestMethod]
		public void ChangedPage_IsScanned()
		{
			Assert.IsTrue(HashtagScanner.NeedsScan(false, After, LastScan, ResolutionKind.Known, false));
		}


		[TestMethod]
		public void ForceThru_ScansEverything()
		{
			Assert.IsTrue(HashtagScanner.NeedsScan(true, Before, LastScan, ResolutionKind.Known, false));
		}


		[TestMethod]
		public void PageNewToTheCatalog_IsScanned_EvenWhenOld()
		{
			// a page moved in from a notebook never scanned has no tags recorded for it
			Assert.IsTrue(HashtagScanner.NeedsScan(false, Before, LastScan, ResolutionKind.New, false));
		}


		[TestMethod]
		public void WeaklyMatchedPage_IsScanned()
		{
			// same notebook, section and title but a different creation time: it may not be the
			// page the tags were recorded for
			Assert.IsTrue(HashtagScanner.NeedsScan(false, Before, LastScan, ResolutionKind.SameTitle, false));
		}


		[TestMethod]
		public void MovedPage_WithoutTags_IsNotScanned()
		{
			Assert.IsFalse(HashtagScanner.NeedsScan(false, Before, LastScan, ResolutionKind.MovedSection, false));
			Assert.IsFalse(HashtagScanner.NeedsScan(false, Before, LastScan, ResolutionKind.MovedNotebook, false));
		}


		[TestMethod]
		public void TaggedPageWithANewPageID_IsScanned_EvenWhenUnchanged()
		{
			// a reopen or a move gives the page a new ID and regenerates the IDs of its title and
			// paragraphs, which the tags record to navigate to; found only by reading the page
			Assert.IsTrue(HashtagScanner.NeedsScan(false, Before, LastScan, ResolutionKind.SameLocation, true));
			Assert.IsTrue(HashtagScanner.NeedsScan(false, Before, LastScan, ResolutionKind.MovedSection, true));
			Assert.IsTrue(HashtagScanner.NeedsScan(false, Before, LastScan, ResolutionKind.MovedNotebook, true));
		}


		[TestMethod]
		public void NeverScanned_ScansEverything()
		{
			Assert.IsTrue(HashtagScanner.NeedsScan(false, Before, "0001-01-01T00:00:00.0000Z", ResolutionKind.Known, false));
		}
	}
}
