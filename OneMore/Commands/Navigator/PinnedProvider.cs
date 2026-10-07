//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Newtonsoft.Json;
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SQLite;
	using HistoryRecord = OneNote.HierarchyInfo;


	/// <summary>
	/// A page or paragraph on the reading list, with the keys that identify it across notebook
	/// reopens, moves and renames; see Design - Page Identity.
	/// </summary>
	internal sealed class PinnedItem
	{
		public int ID { get; set; }

		/// <summary>The user's order, lowest first.</summary>
		public int SortOrder { get; set; }

		/// <summary>What OneNote reported when the page was pinned, or when it was last healed.</summary>
		public HistoryRecord Info { get; set; }

		/// <summary>The key of the page in the identity catalog, or null if it is not known yet.</summary>
		public long? PageKey { get; set; }

		public string NotebookKey { get; set; }

		public string SectionKey { get; set; }
	}


	/// <summary>
	/// Stores the reading list in the local OneMore.db, as favorites are. Unlike Navigator.json it
	/// does not roam, which matters because page keys only mean something on the machine whose
	/// identity catalog made them.
	/// </summary>
	internal class PinnedProvider : DatabaseProvider
	{
		private const string Domain = "pinned";
		private const string PrimaryTable = "pinned";
		private const string SchemaTable = "pinned_schema";

		// recorded in pinned_schema so a later change to the table has a version to upgrade from
		private const int CurrentVersion = 1;

		private static readonly string DDL =
			"CREATE TABLE IF NOT EXISTS pinned (" +
			"pinnedID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, " +
			"sortOrder INTEGER NOT NULL DEFAULT 0, " +
			"name TEXT NOT NULL, path TEXT, notebookID TEXT, sectionID TEXT, titleID TEXT, " +
			"pageID TEXT NOT NULL, objectID TEXT, link TEXT, color TEXT, sectionGroups TEXT, " +
			"pageKey INTEGER, notebookKey TEXT, sectionKey TEXT);\n" +
			"CREATE UNIQUE INDEX IF NOT EXISTS idx_pinned_page " +
			"ON pinned(pageID, COALESCE(objectID, ''));\n" +
			"CREATE UNIQUE INDEX IF NOT EXISTS idx_pinned_pagekey " +
			"ON pinned(pageKey, COALESCE(objectID, '')) WHERE pageKey IS NOT NULL;\n" +
			"CREATE TABLE IF NOT EXISTS pinned_schema (" +
			"schemaID INTEGER PRIMARY KEY UNIQUE NOT NULL, " +
			"version NUMERIC (12) UNIQUE NOT NULL, " +
			"migrated INTEGER NOT NULL DEFAULT 0);\n" +
			"INSERT OR IGNORE INTO pinned_schema (schemaID, version, migrated) " +
			"VALUES (0, " + CurrentVersion + ", 0);";

		private const string Columns =
			"pinnedID, sortOrder, name, path, notebookID, sectionID, titleID, pageID, objectID, " +
			"link, color, sectionGroups, pageKey, notebookKey, sectionKey";


		/// <summary>
		/// Initialize this provider, opening the standard database.
		/// </summary>
		public PinnedProvider()
			: base()
		{
			OpenDatabase();
			EnsureSchema();
		}


		/// <summary>
		/// Test-only constructor that builds the schema on an already-open connection, such as
		/// an in-memory database.
		/// </summary>
		internal PinnedProvider(SQLiteConnection connection)
		{
			con = connection;
			EnsureSchema();
		}


		// The schema table is the marker, not the primary table: the statements are all
		// IF NOT EXISTS, so a database that has the table from before it was versioned just
		// gains the schema row, and its reading list is imported again, harmlessly.
		private void EnsureSchema()
		{
			if (!TableExists(SchemaTable))
			{
				RefreshDataSchema(Domain, DDL);
			}
		}


		public static bool CatalogExists()
		{
			return CatalogExists(PrimaryTable);
		}


		public bool DropCatalog()
		{
			return DropCatalog(Domain, DDL);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Migration...

		/// <summary>
		/// Determines whether this machine has already copied the reading list out of
		/// Navigator.json. That file roams, so every machine copies it once.
		/// </summary>
		public bool IsMigrated()
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT migrated FROM pinned_schema WHERE schemaID = 0";

			var value = cmd.ExecuteScalar();
			return value is not null and not DBNull && Convert.ToInt32(value) != 0;
		}


		/// <summary>
		/// Copies a reading list from Navigator.json, in order, and records that it was done. All
		/// or nothing; an item that is already present is skipped.
		/// </summary>
		/// <param name="records">The pinned list as stored in Navigator.json</param>
		/// <param name="stamp">Finds the keys of a record, or returns null</param>
		/// <returns>The number copied, or -1 if it could not be done and should be tried again</returns>
		public int Import(IReadOnlyList<HistoryRecord> records, Func<HistoryRecord, PinnedItem> stamp)
		{
			using var transaction = con.BeginTransaction();

			try
			{
				if (IsMigrated())
				{
					transaction.Rollback();
					return 0;
				}

				var count = 0;
				for (var i = 0; i < records.Count; i++)
				{
					if (string.IsNullOrEmpty(records[i].PageId))
					{
						continue;
					}

					var item = stamp?.Invoke(records[i]) ?? new PinnedItem { Info = records[i] };
					item.SortOrder = i;

					if (Insert(item, out _))
					{
						count++;
					}
				}

				using var cmd = con.CreateCommand();
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "UPDATE pinned_schema SET migrated = 1 WHERE schemaID = 0";
				cmd.ExecuteNonQuery();

				transaction.Commit();
				logger.WriteLine($"migrated {count} of {records.Count} reading list items to the database");
				return count;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error migrating the reading list", exc);

				try
				{
					transaction.Rollback();
				}
				catch (Exception rollbackExc)
				{
					logger.WriteLine("error rolling back reading list migration", rollbackExc);
				}

				return -1;
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Read...

		/// <summary>
		/// Reads the whole reading list in the user's order.
		/// </summary>
		public List<PinnedItem> ReadAll()
		{
			var items = new List<PinnedItem>();

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = $"SELECT {Columns} FROM pinned ORDER BY sortOrder, pinnedID";

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				items.Add(Read(reader));
			}

			return items;
		}


		/// <summary>
		/// Finds the item for a page, or for a paragraph of a page.
		/// </summary>
		public PinnedItem Find(string pageID, string objectID)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = $"SELECT {Columns} FROM pinned " +
				"WHERE pageID = @g AND COALESCE(objectID, '') = @o";

			cmd.Parameters.AddWithValue("@g", pageID);
			cmd.Parameters.AddWithValue("@o", objectID ?? string.Empty);

			using var reader = cmd.ExecuteReader();
			return reader.Read() ? Read(reader) : null;
		}


		private static PinnedItem Read(SQLiteDataReader reader)
		{
			string Text(int ordinal)
			{
				return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
			}

			var info = new HistoryRecord
			{
				Name = Text(2),
				Path = Text(3),
				NotebookId = Text(4),
				SectionId = Text(5),
				TitleId = Text(6),
				PageId = Text(7),
				ObjectId = Text(8),
				Link = Text(9),
				Color = Text(10)
			};

			var groups = Text(11);
			if (!string.IsNullOrEmpty(groups))
			{
				try
				{
					info.SectionGroups = JsonConvert.DeserializeObject<List<string>>(groups) ?? new();
				}
				catch (JsonException)
				{
					// leave the list empty; it is only a hint for display
				}
			}

			return new PinnedItem
			{
				ID = reader.GetInt32(0),
				SortOrder = reader.GetInt32(1),
				Info = info,
				PageKey = reader.IsDBNull(12) ? null : reader.GetInt64(12),
				NotebookKey = Text(13),
				SectionKey = Text(14)
			};
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Write...

		/// <summary>
		/// Adds an item at its sort order; see <see cref="GetNextSortOrder"/> to add it last.
		/// </summary>
		/// <param name="item">The item to add; its ID is set when it is added</param>
		/// <param name="duplicate">True if the page, or paragraph, is already on the list</param>
		public bool Insert(PinnedItem item, out bool duplicate)
		{
			duplicate = false;

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "INSERT INTO pinned " +
				"(sortOrder, name, path, notebookID, sectionID, titleID, pageID, objectID, link, " +
				"color, sectionGroups, pageKey, notebookKey, sectionKey) " +
				"VALUES (@so, @n, @p, @b, @s, @t, @g, @o, @l, @c, @sg, @pk, @nk, @sk); " +
				"SELECT last_insert_rowid()";

			Bind(cmd, item, includeOrder: true);

			try
			{
				item.ID = Convert.ToInt32(cmd.ExecuteScalar());
				return true;
			}
			catch (Exception exc)
			{
				if (exc is SQLiteException && exc.Message.Contains("UNIQUE constraint failed"))
				{
					duplicate = true;
				}
				else
				{
					logger.WriteLine($"error pinning {item.Info.Path}", exc);
				}

				return false;
			}
		}


		/// <summary>
		/// Gets the sort order that puts an item last.
		/// </summary>
		public int GetNextSortOrder()
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT COALESCE(MAX(sortOrder), -1) + 1 FROM pinned";
			return Convert.ToInt32(cmd.ExecuteScalar());
		}


		/// <summary>
		/// Saves what was found about where an item points now. The name and the sort order are
		/// the user's and are left alone, so a save made in the background can never overwrite a
		/// change made in the Navigator.
		/// </summary>
		/// <param name="item">The item to update, identified by item.ID</param>
		/// <param name="duplicate">True if the update failed because another item already points at
		/// the same target, which can happen when two items turn out to be the same page</param>
		public bool UpdateTarget(PinnedItem item, out bool duplicate)
		{
			duplicate = false;

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText =
				"UPDATE pinned SET path = @p, notebookID = @b, sectionID = @s, titleID = @t, " +
				"pageID = @g, objectID = @o, link = @l, color = @c, sectionGroups = @sg, " +
				"pageKey = @pk, notebookKey = @nk, sectionKey = @sk WHERE pinnedID = @id";

			Bind(cmd, item, includeOrder: false);
			cmd.Parameters.AddWithValue("@id", item.ID);

			try
			{
				cmd.ExecuteNonQuery();
				logger.Verbose($"updated target of pinned item {item.ID}");
				return true;
			}
			catch (Exception exc)
			{
				if (exc is SQLiteException && exc.Message.Contains("UNIQUE constraint failed"))
				{
					duplicate = true;
					logger.WriteLine($"pinned item {item.ID} points at the same target as another");
				}
				else
				{
					logger.WriteLine($"error updating target of pinned item {item.ID}", exc);
				}

				return false;
			}
		}


		/// <summary>
		/// Removes an item from the list.
		/// </summary>
		public bool Delete(string pageID, string objectID)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "DELETE FROM pinned WHERE pageID = @g AND COALESCE(objectID, '') = @o";

			cmd.Parameters.AddWithValue("@g", pageID);
			cmd.Parameters.AddWithValue("@o", objectID ?? string.Empty);

			return cmd.ExecuteNonQuery() > 0;
		}


		/// <summary>
		/// Makes the list match the given records, in the given order: items are reordered, ones
		/// not given are removed, and ones not already present are added. All or nothing.
		/// </summary>
		public void Replace(IReadOnlyList<HistoryRecord> records)
		{
			using var transaction = con.BeginTransaction();

			try
			{
				var existing = ReadAll();
				var keep = new HashSet<int>();

				for (var i = 0; i < records.Count; i++)
				{
					var record = records[i];
					var item = existing.Find(e =>
						e.Info.PageId == record.PageId &&
						(e.Info.ObjectId ?? string.Empty) == (record.ObjectId ?? string.Empty));

					if (item is null)
					{
						Insert(new PinnedItem { Info = record, SortOrder = i }, out _);
						continue;
					}

					keep.Add(item.ID);

					using var cmd = con.CreateCommand();
					cmd.CommandType = CommandType.Text;
					cmd.CommandText = "UPDATE pinned SET sortOrder = @so, name = @n WHERE pinnedID = @id";
					cmd.Parameters.AddWithValue("@so", i);
					cmd.Parameters.AddWithValue("@n", record.Name ?? item.Info.Name);
					cmd.Parameters.AddWithValue("@id", item.ID);
					cmd.ExecuteNonQuery();
				}

				foreach (var item in existing)
				{
					if (!keep.Contains(item.ID))
					{
						Delete(item.Info.PageId, item.Info.ObjectId);
					}
				}

				transaction.Commit();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error saving the reading list", exc);
				transaction.Rollback();
				throw;
			}
		}


		private static void Bind(SQLiteCommand cmd, PinnedItem item, bool includeOrder)
		{
			var info = item.Info;

			if (includeOrder)
			{
				cmd.Parameters.AddWithValue("@so", item.SortOrder);
				cmd.Parameters.AddWithValue("@n", info.Name ?? string.Empty);
			}

			cmd.Parameters.AddWithValue("@p", (object)info.Path ?? DBNull.Value);
			cmd.Parameters.AddWithValue("@b", (object)info.NotebookId ?? DBNull.Value);
			cmd.Parameters.AddWithValue("@s", (object)info.SectionId ?? DBNull.Value);
			cmd.Parameters.AddWithValue("@t", (object)info.TitleId ?? DBNull.Value);
			cmd.Parameters.AddWithValue("@g", info.PageId);
			cmd.Parameters.AddWithValue("@o", string.IsNullOrEmpty(info.ObjectId) ? DBNull.Value : info.ObjectId);
			cmd.Parameters.AddWithValue("@l", (object)info.Link ?? DBNull.Value);
			cmd.Parameters.AddWithValue("@c", (object)info.Color ?? DBNull.Value);
			cmd.Parameters.AddWithValue("@sg", info.SectionGroups is { Count: > 0 }
				? JsonConvert.SerializeObject(info.SectionGroups)
				: DBNull.Value);
			cmd.Parameters.AddWithValue("@pk", item.PageKey.HasValue ? item.PageKey.Value : DBNull.Value);
			cmd.Parameters.AddWithValue("@nk", (object)item.NotebookKey ?? DBNull.Value);
			cmd.Parameters.AddWithValue("@sk", (object)item.SectionKey ?? DBNull.Value);
		}
	}
}
