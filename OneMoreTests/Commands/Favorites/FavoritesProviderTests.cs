//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Favorites
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands.Favorites;
	using System.Data.SQLite;
	using System.Linq;


	[TestClass]
	public class FavoritesProviderTests
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


		[TestMethod]
		public void WriteFavorite_ReadFavorites_RoundTrip_LeavesAliasNullWhenNotSupplied()
		{
			using var provider = new FavoritesProvider(connection);

			var written = provider.WriteFavorite(new Favorite
			{
				Name = "Page1",
				Location = "Notebook1/Section1/Page1",
				Uri = "onenote:#Page1",
				NotebookID = "nb-1",
				SectionID = "sec-1",
				PageID = "page-1"
			});

			Assert.IsTrue(written, "WriteFavorite should succeed");

			var collection = provider.ReadFavorites();
			Assert.AreEqual(1, collection.Items.Count);

			var favorite = collection.Items[0];
			Assert.AreEqual("Page1", favorite.Name);
			Assert.IsNull(favorite.Alias, "Alias should remain null when not supplied, not default to Name");
			Assert.AreEqual("Notebook1/Section1/Page1", favorite.Location);
			Assert.AreEqual("onenote:#Page1", favorite.Uri);
			Assert.AreEqual("nb-1", favorite.NotebookID);
			Assert.AreEqual("sec-1", favorite.SectionID);
			Assert.AreEqual("page-1", favorite.PageID);
		}


		[TestMethod]
		public void ReadFavorites_RootItems_OrderedBySortOrderThenName()
		{
			using var provider = new FavoritesProvider(connection);

			bool Write(string name, int sortOrder) => provider.WriteFavorite(new Favorite
			{
				Name = name,
				Location = $"Notebook1/{name}",
				Uri = $"onenote:#{name}",
				NotebookID = "nb-1",
				SectionID = $"sec-{name}",
				SortOrder = sortOrder
			});

			Write("Charlie", 0);
			Write("Alpha", 0);
			Write("Bravo", 1);

			var collection = provider.ReadFavorites();

			CollectionAssert.AreEqual(
				new[] { "Alpha", "Charlie", "Bravo" },
				collection.Items.Select(f => f.Name).ToList());
		}


		[TestMethod]
		public void ReadFavorites_SectionLevelFavorite_NullPageId_DoesNotThrow()
		{
			// Regression test: section/notebook-level favorites have no PageID, and the
			// reader must treat that column as nullable rather than calling GetString on it.
			using var provider = new FavoritesProvider(connection);

			provider.WriteFavorite(new Favorite
			{
				Name = "Section1",
				Location = "Notebook1/Section1",
				Uri = "onenote:#Section1",
				NotebookID = "nb-1",
				SectionID = "sec-1",
				PageID = null
			});

			var collection = provider.ReadFavorites();

			Assert.AreEqual(1, collection.Items.Count);
			Assert.IsNull(collection.Items[0].PageID);
		}


		[TestMethod]
		public void DeleteFavorite_RemovesRow()
		{
			using var provider = new FavoritesProvider(connection);

			provider.WriteFavorite(new Favorite
			{
				Name = "Page1",
				Location = "Notebook1/Section1/Page1",
				Uri = "onenote:#Page1",
				NotebookID = "nb-1",
				SectionID = "sec-1",
				PageID = "page-1"
			});

			var favoriteId = provider.ReadFavorites().Items[0].ID;

			var deleted = provider.DeleteFavorite(favoriteId);
			Assert.IsTrue(deleted);

			Assert.AreEqual(0, provider.ReadFavorites().Items.Count);
		}


		[TestMethod]
		public void WriteFavorite_ViolatesNotNullConstraint_ReturnsFalseInsteadOfThrowing()
		{
			using var provider = new FavoritesProvider(connection);

			var written = provider.WriteFavorite(new Favorite
			{
				Name = null, // name is NOT NULL in the schema
				Location = "Notebook1/Section1/Page1",
				Uri = "onenote:#Page1",
				NotebookID = "nb-1",
				SectionID = "sec-1"
			});

			Assert.IsFalse(written);
			Assert.AreEqual(0, provider.ReadFavorites().Items.Count);
		}


		[TestMethod]
		public void WriteFavorite_DuplicatePage_ReturnsFalseWithDuplicateTrue()
		{
			using var provider = new FavoritesProvider(connection);

			var written1 = provider.WriteFavorite(new Favorite
			{
				Name = "Page1",
				Location = "Notebook1/Section1/Page1",
				Uri = "onenote:#Page1",
				NotebookID = "nb-1",
				SectionID = "sec-1",
				PageID = "page-1"
			});

			Assert.IsTrue(written1, "first write should succeed");

			var written = provider.WriteFavorite(new Favorite
			{
				Name = "Page1",
				Location = "Notebook1/Section1/Page1",
				Uri = "onenote:#Page1",
				NotebookID = "nb-1",
				SectionID = "sec-1",
				PageID = "page-1"
			}, out var duplicate);

			Assert.IsFalse(written, "duplicate page favorite should fail to write");
			Assert.IsTrue(duplicate, "failure should be reported as a duplicate");
			Assert.AreEqual(1, provider.ReadFavorites().Items.Count);
		}


		[TestMethod]
		public void WriteFavorite_DuplicateSection_ReturnsFalseWithDuplicateTrue()
		{
			// section/section-group favorites have a null PageID; uniqueness must still
			// be enforced on notebookID + sectionID in that case
			using var provider = new FavoritesProvider(connection);

			var written1 = provider.WriteFavorite(new Favorite
			{
				Name = "Section1",
				Location = "Notebook1/Section1",
				Uri = "onenote:#Section1",
				NotebookID = "nb-1",
				SectionID = "sec-1",
				PageID = null
			});

			Assert.IsTrue(written1, "first write should succeed");

			var written2 = provider.WriteFavorite(new Favorite
			{
				Name = "Section1",
				Location = "Notebook1/Section1",
				Uri = "onenote:#Section1",
				NotebookID = "nb-1",
				SectionID = "sec-1",
				PageID = null
			}, out var duplicate);

			Assert.IsFalse(written2, "duplicate section favorite should fail to write");
			Assert.IsTrue(duplicate, "failure should be reported as a duplicate");
			Assert.AreEqual(1, provider.ReadFavorites().Items.Count);
		}


		[TestMethod]
		public void WriteFavorite_SamePageDifferentNotebookAndSection_StillTreatedAsDuplicate()
		{
			// pageID alone identifies a page favorite; notebookID/sectionID may legitimately
			// differ (e.g. stale metadata) but should not bypass duplicate detection
			using var provider = new FavoritesProvider(connection);

			var written1 = provider.WriteFavorite(new Favorite
			{
				Name = "Page1",
				Location = "Notebook1/Section1/Page1",
				Uri = "onenote:#Page1",
				NotebookID = "nb-1",
				SectionID = "sec-1",
				PageID = "page-1"
			});

			Assert.IsTrue(written1, "first write should succeed");

			var written2 = provider.WriteFavorite(new Favorite
			{
				Name = "Page1",
				Location = "Notebook2/Section2/Page1",
				Uri = "onenote:#Page1",
				NotebookID = "nb-2",
				SectionID = "sec-2",
				PageID = "page-1"
			}, out var duplicate);

			Assert.IsFalse(written2, "same pageID under a different notebook/section should fail to write");
			Assert.IsTrue(duplicate, "failure should be reported as a duplicate");
			Assert.AreEqual(1, provider.ReadFavorites().Items.Count);
		}


		[TestMethod]
		public void WriteFavorite_SameSectionDifferentNotebook_StillTreatedAsDuplicate()
		{
			// sectionID alone identifies a section/section-group favorite when pageID is null
			using var provider = new FavoritesProvider(connection);

			var written1 = provider.WriteFavorite(new Favorite
			{
				Name = "Section1",
				Location = "Notebook1/Section1",
				Uri = "onenote:#Section1",
				NotebookID = "nb-1",
				SectionID = "sec-1",
				PageID = null
			});

			Assert.IsTrue(written1, "first write should succeed");

			var written2 = provider.WriteFavorite(new Favorite
			{
				Name = "Section1",
				Location = "Notebook2/Section1",
				Uri = "onenote:#Section1",
				NotebookID = "nb-2",
				SectionID = "sec-1",
				PageID = null
			}, out var duplicate);

			Assert.IsFalse(written2, "same sectionID under a different notebook should fail to write");
			Assert.IsTrue(duplicate, "failure should be reported as a duplicate");
			Assert.AreEqual(1, provider.ReadFavorites().Items.Count);
		}


		[TestMethod]
		public void CreateFolder_ReturnsID_ReadableViaReadFavorites()
		{
			using var provider = new FavoritesProvider(connection);

			var folderID = provider.CreateFolder("Folder1");
			Assert.IsTrue(folderID > 0, "CreateFolder should return a positive folderID");

			provider.WriteFavorite(new Favorite
			{
				FolderID = folderID,
				Name = "Page1",
				Location = "Notebook1/Section1/Page1",
				Uri = "onenote:#Page1",
				NotebookID = "nb-1",
				SectionID = "sec-1",
				PageID = "page-1"
			});

			var collection = provider.ReadFavorites();
			Assert.AreEqual(1, collection.Folders.Count);
			Assert.AreEqual("Folder1", collection.Folders[0].Name);
			Assert.AreEqual(1, collection.Folders[0].Items.Count);
		}


		[TestMethod]
		public void CreateFolder_EmptyFolder_ReadFavoritesDoesNotThrow()
		{
			// Regression test: an empty folder's LEFT JOIN produces a row with no matching
			// favorite, and the reader must not try to materialize a Favorite from it.
			using var provider = new FavoritesProvider(connection);

			provider.CreateFolder("Folder1");

			var collection = provider.ReadFavorites();
			Assert.AreEqual(1, collection.Folders.Count);
			Assert.AreEqual("Folder1", collection.Folders[0].Name);
			Assert.AreEqual(0, collection.Folders[0].Items.Count);
		}


		[TestMethod]
		public void CreateFolder_DuplicateName_ReturnsZero()
		{
			using var provider = new FavoritesProvider(connection);

			var first = provider.CreateFolder("Folder1");
			Assert.IsTrue(first > 0);

			var second = provider.CreateFolder("Folder1");
			Assert.AreEqual(0, second, "Duplicate folder name should fail to create");
		}


		[TestMethod]
		public void RenameFolder_ChangesNameReadableViaReadFavorites()
		{
			using var provider = new FavoritesProvider(connection);

			var folderID = provider.CreateFolder("Folder1");

			var renamed = provider.RenameFolder(folderID, "Renamed");
			Assert.IsTrue(renamed);

			var collection = provider.ReadFavorites();
			Assert.AreEqual("Renamed", collection.Folders[0].Name);
		}


		[TestMethod]
		public void RenameFolder_DuplicateName_ReturnsFalse()
		{
			using var provider = new FavoritesProvider(connection);

			provider.CreateFolder("Folder1");
			var folderID = provider.CreateFolder("Folder2");

			var renamed = provider.RenameFolder(folderID, "Folder1");
			Assert.IsFalse(renamed, "Renaming to a duplicate name should fail");
		}


		[TestMethod]
		public void DeleteFolder_RemovesFolderAndItsFavorites()
		{
			using var provider = new FavoritesProvider(connection);

			var folderID = provider.CreateFolder("Folder1");

			provider.WriteFavorite(new Favorite
			{
				FolderID = folderID,
				Name = "Page1",
				Location = "Notebook1/Section1/Page1",
				Uri = "onenote:#Page1",
				NotebookID = "nb-1",
				SectionID = "sec-1",
				PageID = "page-1"
			});

			provider.WriteFavorite(new Favorite
			{
				FolderID = folderID,
				Name = "Page2",
				Location = "Notebook1/Section1/Page2",
				Uri = "onenote:#Page2",
				NotebookID = "nb-1",
				SectionID = "sec-1",
				PageID = "page-2"
			});

			var deleted = provider.DeleteFolder(folderID);
			Assert.IsTrue(deleted);

			var collection = provider.ReadFavorites();
			Assert.AreEqual(0, collection.Folders.Count);
			Assert.AreEqual(0, collection.Items.Count);
		}


		[TestMethod]
		public void UpdateFavorite_ChangesAliasFolderAndSortOrder_RoundTrips()
		{
			using var provider = new FavoritesProvider(connection);

			provider.WriteFavorite(new Favorite
			{
				Name = "Page1",
				Location = "Notebook1/Section1/Page1",
				Uri = "onenote:#Page1",
				NotebookID = "nb-1",
				SectionID = "sec-1",
				PageID = "page-1"
			});

			var favorite = provider.ReadFavorites().Items[0];
			var folderID = provider.CreateFolder("Folder1");

			favorite.Alias = "MyPage";
			favorite.FolderID = folderID;
			favorite.SortOrder = 3;

			var updated = provider.UpdateFavorite(favorite);
			Assert.IsTrue(updated);

			var reread = provider.ReadFavorites().Folders[0].Items[0];
			Assert.AreEqual("MyPage", reread.Alias);
			Assert.AreEqual(folderID, reread.FolderID);
			Assert.AreEqual(3, reread.SortOrder);
		}


		[TestMethod]
		public void UpdateFavorite_AliasClearedToNull_StaysNullOnReread()
		{
			using var provider = new FavoritesProvider(connection);

			provider.WriteFavorite(new Favorite
			{
				Name = "Page1",
				Alias = "Custom",
				Location = "Notebook1/Section1/Page1",
				Uri = "onenote:#Page1",
				NotebookID = "nb-1",
				SectionID = "sec-1",
				PageID = "page-1"
			});

			var favorite = provider.ReadFavorites().Items[0];
			favorite.Alias = null;

			var updated = provider.UpdateFavorite(favorite);
			Assert.IsTrue(updated);

			var reread = provider.ReadFavorites().Items[0];
			Assert.IsNull(reread.Alias, "clearing the alias should leave it null, not revert to Name");
		}
	}
}
