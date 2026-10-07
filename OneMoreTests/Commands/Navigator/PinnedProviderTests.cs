//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Navigator
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using System.Collections.Generic;
	using System.Data.SQLite;
	using System.Linq;
	using HistoryRecord = River.OneMoreAddIn.OneNote.HierarchyInfo;


	[TestClass]
	public class PinnedProviderTests
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


		private static HistoryRecord Record(string pageID, string name, string objectID = null)
		{
			return new HistoryRecord
			{
				PageId = pageID,
				Name = name,
				Path = $"/N/S/{name}",
				Link = $"onenote:#{name}",
				SectionId = "s",
				NotebookId = "n",
				ObjectId = objectID,
				SectionGroups = new List<string> { "G1" }
			};
		}


		[TestMethod]
		public void Insert_ReadAll_RoundTrip()
		{
			using var provider = new PinnedProvider(connection);

			var item = new PinnedItem
			{
				Info = Record("p1", "One"),
				PageKey = 7,
				NotebookKey = "nk",
				SectionKey = "/S"
			};

			Assert.IsTrue(provider.Insert(item, out var duplicate));
			Assert.IsFalse(duplicate);
			Assert.IsTrue(item.ID > 0);

			var read = provider.ReadAll().Single();
			Assert.AreEqual("p1", read.Info.PageId);
			Assert.AreEqual("One", read.Info.Name);
			Assert.AreEqual("/N/S/One", read.Info.Path);
			Assert.AreEqual("G1", read.Info.SectionGroups.Single());
			Assert.AreEqual(7L, read.PageKey);
			Assert.AreEqual("nk", read.NotebookKey);
			Assert.AreEqual("/S", read.SectionKey);
			Assert.IsNull(read.Info.ObjectId);
		}


		[TestMethod]
		public void Insert_SamePage_IsDuplicate_ButAParagraphOfIt_IsNot()
		{
			using var provider = new PinnedProvider(connection);

			Assert.IsTrue(provider.Insert(new PinnedItem { Info = Record("p1", "One") }, out _));
			Assert.IsFalse(provider.Insert(new PinnedItem { Info = Record("p1", "One") }, out var duplicate));
			Assert.IsTrue(duplicate);

			Assert.IsTrue(provider.Insert(new PinnedItem { Info = Record("p1", "One", "o1") }, out _));
			Assert.AreEqual(2, provider.ReadAll().Count);
		}


		[TestMethod]
		public void Insert_SamePageKey_IsDuplicateEvenWithAnotherPageID()
		{
			using var provider = new PinnedProvider(connection);

			Assert.IsTrue(provider.Insert(
				new PinnedItem { Info = Record("p1", "One"), PageKey = 5 }, out _));

			Assert.IsFalse(provider.Insert(
				new PinnedItem { Info = Record("p2", "One"), PageKey = 5 }, out var duplicate));

			Assert.IsTrue(duplicate);
		}


		[TestMethod]
		public void ReadAll_IsInSortOrder()
		{
			using var provider = new PinnedProvider(connection);

			provider.Insert(new PinnedItem { Info = Record("p1", "One"), SortOrder = 2 }, out _);
			provider.Insert(new PinnedItem { Info = Record("p2", "Two"), SortOrder = 0 }, out _);
			provider.Insert(new PinnedItem { Info = Record("p3", "Three"), SortOrder = 1 }, out _);

			CollectionAssert.AreEqual(
				new[] { "p2", "p3", "p1" },
				provider.ReadAll().Select(i => i.Info.PageId).ToArray());

			Assert.AreEqual(3, provider.GetNextSortOrder());
		}


		[TestMethod]
		public void UpdateTarget_ChangesTheTarget_ButNotTheNameOrOrder()
		{
			using var provider = new PinnedProvider(connection);

			var item = new PinnedItem { Info = Record("p1", "One"), SortOrder = 4 };
			provider.Insert(item, out _);

			item.Info.PageId = "p9";
			item.Info.Link = "onenote:#new";
			item.Info.Name = "Renamed By Mistake";
			item.SortOrder = 99;
			item.PageKey = 12;

			Assert.IsTrue(provider.UpdateTarget(item, out _));

			var read = provider.ReadAll().Single();
			Assert.AreEqual("p9", read.Info.PageId);
			Assert.AreEqual("onenote:#new", read.Info.Link);
			Assert.AreEqual(12L, read.PageKey);
			Assert.AreEqual("One", read.Info.Name);
			Assert.AreEqual(4, read.SortOrder);
		}


		[TestMethod]
		public void UpdateTarget_ToATargetAlreadyPinned_IsDuplicate()
		{
			using var provider = new PinnedProvider(connection);

			var first = new PinnedItem { Info = Record("p1", "One") };
			var second = new PinnedItem { Info = Record("p2", "Two") };
			provider.Insert(first, out _);
			provider.Insert(second, out _);

			second.Info.PageId = "p1";

			Assert.IsFalse(provider.UpdateTarget(second, out var duplicate));
			Assert.IsTrue(duplicate);
		}


		[TestMethod]
		public void Delete_RemovesOnlyThatPageOrParagraph()
		{
			using var provider = new PinnedProvider(connection);

			provider.Insert(new PinnedItem { Info = Record("p1", "One") }, out _);
			provider.Insert(new PinnedItem { Info = Record("p1", "One", "o1") }, out _);

			Assert.IsTrue(provider.Delete("p1", "o1"));
			Assert.IsFalse(provider.Delete("p1", "o1"));

			Assert.IsNotNull(provider.Find("p1", null));
			Assert.IsNull(provider.Find("p1", "o1"));
		}


		[TestMethod]
		public void Replace_Reorders_Removes_AndAdds()
		{
			using var provider = new PinnedProvider(connection);

			provider.Insert(new PinnedItem { Info = Record("p1", "One"), SortOrder = 0, PageKey = 1 }, out _);
			provider.Insert(new PinnedItem { Info = Record("p2", "Two"), SortOrder = 1 }, out _);
			provider.Insert(new PinnedItem { Info = Record("p3", "Three"), SortOrder = 2 }, out _);

			provider.Replace(new List<HistoryRecord>
			{
				Record("p3", "Three"), Record("p4", "Four"), Record("p1", "One")
			});

			var items = provider.ReadAll();
			CollectionAssert.AreEqual(
				new[] { "p3", "p4", "p1" }, items.Select(i => i.Info.PageId).ToArray());

			// an item that stays keeps its keys
			Assert.AreEqual(1L, items.Single(i => i.Info.PageId == "p1").PageKey);
		}


		[TestMethod]
		public void Import_CopiesInOrder_Once()
		{
			using var provider = new PinnedProvider(connection);
			Assert.IsFalse(provider.IsMigrated());

			var records = new List<HistoryRecord>
			{
				Record("p1", "One"), Record("p2", "Two"), Record(null, "Broken"), Record("p1", "Dup")
			};

			var count = provider.Import(records, r => new PinnedItem { Info = r });

			// the record without an ID is skipped, and the repeat is a duplicate
			Assert.AreEqual(2, count);
			Assert.IsTrue(provider.IsMigrated());

			CollectionAssert.AreEqual(
				new[] { "p1", "p2" }, provider.ReadAll().Select(i => i.Info.PageId).ToArray());

			// a second import, as from another copy of the roamed file, does nothing
			Assert.AreEqual(0, provider.Import(new List<HistoryRecord> { Record("p9", "Nine") }, null));
			Assert.AreEqual(2, provider.ReadAll().Count);
		}


		private (long Rows, long Version, long Migrated) ReadSchemaRow()
		{
			using var cmd = connection.CreateCommand();
			cmd.CommandText = "SELECT COUNT(1), MAX(version), MAX(migrated) FROM pinned_schema";

			using var reader = cmd.ExecuteReader();
			reader.Read();
			return (reader.GetInt64(0), reader.GetInt64(1), reader.GetInt64(2));
		}


		[TestMethod]
		public void NewDatabase_HasOneSchemaRow_AtVersionOne_NotMigrated()
		{
			using var provider = new PinnedProvider(connection);

			Assert.AreEqual((1L, 1L, 0L), ReadSchemaRow());
		}


		[TestMethod]
		public void Import_MarksTheSchemaRowMigrated()
		{
			using var provider = new PinnedProvider(connection);
			provider.Import(new List<HistoryRecord> { Record("p1", "One") }, null);

			Assert.AreEqual((1L, 1L, 1L), ReadSchemaRow());
		}


		[TestMethod]
		public void ADatabaseFromBeforeTheSchemaTable_GainsIt_AndImportsWithoutFailing()
		{
			// the shape the earlier build made: the table and its state table, but no schema table
			// (not disposed: a provider owns its connection, and this one is shared)
			var first = new PinnedProvider(connection);
			first.Insert(new PinnedItem { Info = Record("p1", "One") }, out _);

			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = "DROP TABLE pinned_schema";
				cmd.ExecuteNonQuery();
			}

			using var provider = new PinnedProvider(connection);
			Assert.AreEqual((1L, 1L, 0L), ReadSchemaRow());
			Assert.IsFalse(provider.IsMigrated());

			var count = provider.Import(
				new List<HistoryRecord> { Record("p1", "One"), Record("p2", "Two") }, null);

			// the one already there is skipped, the new one is added
			Assert.AreEqual(1, count);
			Assert.AreEqual(2, provider.ReadAll().Count);
			Assert.IsTrue(provider.IsMigrated());
		}
	}
}
