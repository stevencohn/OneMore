//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Favorites
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands.Favorites;
	using System;
	using System.Data.SQLite;
	using System.Linq;


	[TestClass]
	public class FavoritesKeysTests
	{
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
			connection.Dispose();
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


		// a catalog as version 2 left it: no keys, and the unique indexes on the ID columns only
		private void MakeVersion2()
		{
			Execute("CREATE TABLE favorites_folder (folderID INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL, UNIQUE(name))");
			Execute("CREATE TABLE favorite (favoriteID INTEGER PRIMARY KEY AUTOINCREMENT, folderID INTEGER REFERENCES favorites_folder(folderID) ON DELETE CASCADE, name TEXT NOT NULL, alias TEXT, location TEXT, uri TEXT NOT NULL, notebookID TEXT NOT NULL, sectionID TEXT NOT NULL, pageID TEXT, kind TEXT, sortOrder INTEGER NOT NULL DEFAULT 0)");
			Execute("CREATE TABLE favorites_schema (schemaID INTEGER PRIMARY KEY UNIQUE NOT NULL, version NUMERIC (12) UNIQUE NOT NULL)");
			Execute("CREATE UNIQUE INDEX idx_favorite_target_page ON favorite(pageID) WHERE pageID IS NOT NULL");
			Execute("CREATE UNIQUE INDEX idx_favorite_target_section ON favorite(sectionID) WHERE pageID IS NULL");
			Execute("INSERT INTO favorites_schema (schemaID, version) VALUES (0, 2)");
			Execute("INSERT INTO favorite (folderID, name, alias, location, uri, notebookID, sectionID, pageID, sortOrder) " +
				"VALUES (0, 'Old page', 'Mine', '/Notes/Inbox/Old page', 'onenote:#old', '{nb}', '{sec}', '{page}', 4)");
			Execute("INSERT INTO favorite (folderID, name, location, uri, notebookID, sectionID, sortOrder) " +
				"VALUES (0, 'Inbox', '/Notes/Inbox', 'onenote:#inbox', '{nb}', '{sec}', 5)");
		}


		private static Favorite Page(string name, string pageID, long? key = null,
			string notebookKey = null, string sectionKey = null)
		{
			return new Favorite
			{
				Name = name,
				Location = "/Notes/Inbox/" + name,
				Uri = "onenote:#" + name,
				NotebookID = "{nb}",
				SectionID = "{sec-" + name + "}",
				PageID = pageID,
				PageKey = key,
				NotebookKey = notebookKey,
				SectionKey = sectionKey
			};
		}


		private static Favorite Container(string name, string sectionID, string kind,
			string notebookKey, string sectionKey)
		{
			return new Favorite
			{
				Name = name,
				Location = "/Notes/" + name,
				Uri = sectionID,
				NotebookID = "{nb}",
				SectionID = sectionID,
				Kind = kind,
				NotebookKey = notebookKey,
				SectionKey = sectionKey
			};
		}


		#region Schema
		[TestMethod]
		public void FreshCatalog_IsVersion3_WithTheKeyColumns()
		{
			using var provider = new FavoritesProvider(connection);

			Assert.AreEqual(3L, Convert.ToInt64(Scalar("SELECT version FROM favorites_schema")));

			foreach (var column in new[] { "pageKey", "notebookKey", "sectionKey" })
			{
				Assert.AreEqual(1L, Convert.ToInt64(Scalar(
					$"SELECT count(*) FROM pragma_table_info('favorite') WHERE name = '{column}'")), column);
			}
		}


		[TestMethod]
		public void Version2Catalog_IsUpgraded_KeepingRowsAliasAndOrder()
		{
			MakeVersion2();

			using var provider = new FavoritesProvider(connection);

			Assert.AreEqual(3L, Convert.ToInt64(Scalar("SELECT version FROM favorites_schema")));

			var items = provider.ReadFavorites().Items;
			Assert.AreEqual(2, items.Count);
			Assert.AreEqual("Mine", items[0].Alias);
			Assert.AreEqual("{page}", items[0].PageID);
			Assert.IsTrue(items.All(f => f.PageKey is null && f.NotebookKey is null && f.SectionKey is null));
		}


		[TestMethod]
		public void Version2Catalog_AfterUpgrade_StillRejectsTheSameHandleIDs()
		{
			MakeVersion2();
			using var provider = new FavoritesProvider(connection);

			var same = Page("Another", "{page}");
			same.SectionID = "{other}";

			Assert.IsFalse(provider.WriteFavorite(same, out var duplicate));
			Assert.IsTrue(duplicate);
		}


		[TestMethod]
		public void UpgradedCatalog_CanBeOpenedAgain()
		{
			MakeVersion2();

			// the provider closes the connection it is given when it is disposed, so the first one
			// is left open, as another process's would be, while the second opens the same catalog
			var first = new FavoritesProvider(connection);
			Assert.IsNotNull(first);

			using var again = new FavoritesProvider(connection);

			Assert.AreEqual(3L, Convert.ToInt64(Scalar("SELECT version FROM favorites_schema")));
			Assert.AreEqual(2, again.ReadFavorites().Items.Count);
		}


		[TestMethod]
		public void CatalogNewerThanKnown_IsLeftAlone()
		{
			MakeVersion2();
			Execute("UPDATE favorites_schema SET version = 9");

			using var provider = new FavoritesProvider(connection);

			Assert.AreEqual(9L, Convert.ToInt64(Scalar("SELECT version FROM favorites_schema")));
			Assert.AreEqual(0L, Convert.ToInt64(Scalar(
				"SELECT count(*) FROM pragma_table_info('favorite') WHERE name = 'pageKey'")));
		}
		#endregion Schema


		#region Reading and writing keys
		[TestMethod]
		public void Keys_RoundTrip()
		{
			using var provider = new FavoritesProvider(connection);

			Assert.IsTrue(provider.WriteFavorite(Page("Keyed", "{p}", 42, "https://x/notes", "/Inbox")));

			var favorite = provider.ReadFavorites().Items.Single();
			Assert.AreEqual(42L, favorite.PageKey);
			Assert.AreEqual("https://x/notes", favorite.NotebookKey);
			Assert.AreEqual("/Inbox", favorite.SectionKey);
		}


		[TestMethod]
		public void WithoutKeys_TheyStayNull()
		{
			using var provider = new FavoritesProvider(connection);

			Assert.IsTrue(provider.WriteFavorite(Page("Legacy", "{p}")));

			var favorite = provider.ReadFavorites().Items.Single();
			Assert.IsNull(favorite.PageKey);
			Assert.IsNull(favorite.NotebookKey);
			Assert.IsNull(favorite.SectionKey);
		}


		[TestMethod]
		public void ReadFavorite_ReadsOneByID_OrNull()
		{
			using var provider = new FavoritesProvider(connection);
			provider.WriteFavorite(Page("One", "{1}", 1));
			provider.WriteFavorite(Page("Two", "{2}", 2));

			var id = provider.ReadFavorites().Items.Single(f => f.Name == "Two").ID;
			var two = provider.ReadFavorite(id);

			Assert.AreEqual("Two", two.Name);
			Assert.AreEqual(2L, two.PageKey);
			Assert.IsNull(provider.ReadFavorite(9999));
		}
		#endregion Reading and writing keys


		#region Duplicates
		[TestMethod]
		public void SamePageKey_IsADuplicate_EvenWithDifferentHandleIDs()
		{
			using var provider = new FavoritesProvider(connection);

			Assert.IsTrue(provider.WriteFavorite(Page("First", "{old-id}", 42)));

			// the page was favorited again after a reopen gave it new IDs
			Assert.IsFalse(provider.WriteFavorite(Page("Again", "{new-id}", 42), out var duplicate));
			Assert.IsTrue(duplicate);
		}


		[TestMethod]
		public void DifferentPageKeys_AreNotDuplicates()
		{
			using var provider = new FavoritesProvider(connection);

			Assert.IsTrue(provider.WriteFavorite(Page("A", "{a}", 1)));
			Assert.IsTrue(provider.WriteFavorite(Page("B", "{b}", 2)));
		}


		[TestMethod]
		public void SameSection_IsADuplicate_EvenWithDifferentHandleIDs()
		{
			using var provider = new FavoritesProvider(connection);

			Assert.IsTrue(provider.WriteFavorite(Container("Inbox", "{s1}", null, "nb", "/Inbox")));
			Assert.IsFalse(provider.WriteFavorite(
				Container("Inbox", "{s1-after-reopen}", null, "nb", "/Inbox"), out var duplicate));

			Assert.IsTrue(duplicate);
		}


		[TestMethod]
		public void ASectionAndASectionGroupOfTheSameName_AreDifferentFavorites()
		{
			using var provider = new FavoritesProvider(connection);

			Assert.IsTrue(provider.WriteFavorite(Container("Work", "{s}", null, "nb", "/Work")));
			Assert.IsTrue(provider.WriteFavorite(
				Container("Work", "{g}", Favorite.KindSectionGroup, "nb", "/Work")));
		}


		[TestMethod]
		public void ANotebookAndOneOfItsSections_AreDifferentFavorites()
		{
			using var provider = new FavoritesProvider(connection);

			Assert.IsTrue(provider.WriteFavorite(
				Container("Notes", "{nb}", Favorite.KindNotebook, "nb", null)));
			Assert.IsTrue(provider.WriteFavorite(Container("Inbox", "{s}", null, "nb", "/Inbox")));
		}


		[TestMethod]
		public void TheSameSectionNameInAnotherNotebook_IsADifferentFavorite()
		{
			using var provider = new FavoritesProvider(connection);

			Assert.IsTrue(provider.WriteFavorite(Container("Inbox", "{a}", null, "nb-one", "/Inbox")));
			Assert.IsTrue(provider.WriteFavorite(Container("Inbox", "{b}", null, "nb-two", "/Inbox")));
		}


		[TestMethod]
		public void LegacyContainersWithoutKeys_AreStillToldApartByTheirIDs()
		{
			using var provider = new FavoritesProvider(connection);

			Assert.IsTrue(provider.WriteFavorite(Container("A", "{a}", null, null, null)));
			Assert.IsTrue(provider.WriteFavorite(Container("B", "{b}", null, null, null)));
			Assert.IsFalse(provider.WriteFavorite(Container("A again", "{a}", null, null, null), out var duplicate));
			Assert.IsTrue(duplicate);
		}
		#endregion Duplicates


		#region UpdateTarget
		[TestMethod]
		public void UpdateTarget_SavesWhatWasFound_AndLeavesTheUsersChoicesAlone()
		{
			using var provider = new FavoritesProvider(connection);

			var legacy = Page("Old name", "{old-page}");
			legacy.Alias = "Mine";
			legacy.SortOrder = 7;
			legacy.FolderID = 0;
			provider.WriteFavorite(legacy);

			var stored = provider.ReadFavorites().Items.Single();

			stored.Name = "A name changed in memory";
			stored.Location = "/Notes/Archive/New name";
			stored.Uri = "onenote:#new";
			stored.NotebookID = "{nb-new}";
			stored.SectionID = "{sec-new}";
			stored.PageID = "{page-new}";
			stored.PageKey = 42;
			stored.NotebookKey = "https://x/notes";
			stored.SectionKey = "/Archive";

			// the user's name, alias and sort order must not be written by UpdateTarget even if they
			// changed in memory
			stored.Alias = "ignored";
			stored.SortOrder = 99;

			Assert.IsTrue(provider.UpdateTarget(stored));

			var read = provider.ReadFavorite(stored.ID);
			Assert.AreEqual("Old name", read.Name, "UpdateTarget never writes the name");
			Assert.AreEqual("/Notes/Archive/New name", read.Location);
			Assert.AreEqual("onenote:#new", read.Uri);
			Assert.AreEqual("{nb-new}", read.NotebookID);
			Assert.AreEqual("{sec-new}", read.SectionID);
			Assert.AreEqual("{page-new}", read.PageID);
			Assert.AreEqual(42L, read.PageKey);
			Assert.AreEqual("https://x/notes", read.NotebookKey);
			Assert.AreEqual("/Archive", read.SectionKey);

			Assert.AreEqual("Mine", read.Alias);
			Assert.AreEqual(7, read.SortOrder);
		}


		[TestMethod]
		public void UpdateTarget_TwoFavoritesTurnOutToBeTheSamePage_ReportsTheDuplicate()
		{
			using var provider = new FavoritesProvider(connection);
			provider.WriteFavorite(Page("First", "{a}", 42));
			provider.WriteFavorite(Page("Second", "{b}"));

			var second = provider.ReadFavorites().Items.Single(f => f.Name == "Second");
			second.PageKey = 42;

			Assert.IsFalse(provider.UpdateTarget(second, out var duplicate));
			Assert.IsTrue(duplicate);

			// the second one is left as it was, still unresolved
			Assert.IsNull(provider.ReadFavorite(second.ID).PageKey);
		}


		[TestMethod]
		public void UpdateTarget_UnknownFavorite_ChangesNothing()
		{
			using var provider = new FavoritesProvider(connection);
			provider.WriteFavorite(Page("One", "{a}", 1));

			var ghost = Page("Ghost", "{g}", 5);
			ghost.ID = 9999;

			Assert.IsTrue(provider.UpdateTarget(ghost));
			Assert.AreEqual(1, provider.ReadFavorites().Items.Count);
		}
		#endregion UpdateTarget
	}
}
