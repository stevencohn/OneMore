//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using System;
	using System.Data;
	using System.Data.SQLite;
	using Resx = Properties.Resources;


	internal class FavoritesProvider : DatabaseProvider
	{
		private static readonly string Domain = "favorites";
		private static readonly string PrimaryTable = "favorite";

		// version 3 adds the page key and the notebook and section keys; see Upgrade2to3
		private const int CurrentVersion = 3;


		/// <summary>
		/// Initialize this provider, opening the standard database
		/// </summary>
		public FavoritesProvider()
			: base()
		{
			OpenDatabase();

			if (!CatalogExists())
			{
				RefreshDataSchema(Domain, Resx.FavoritesDB);
				MigrateFavoritesFile();
			}
			else
			{
				UpgradeCatalog();
			}
		}


		#region UpgradeCatalog
		/// <summary>
		/// Reads the current favorites_schema version and runs any pending incremental
		/// upgrades, following the same pattern as HashtagProvider.UpgradeCatalog.
		/// </summary>
		private void UpgradeCatalog()
		{
			// a database that predates versioning has no favorites_schema table at all
			var version = ReadSchemaVersion("favorites_schema", "schemaID", missing: 1);

			if (IsNewerThanKnown(Domain, version, CurrentVersion))
			{
				return;
			}

			if (version == 1)
			{
				version = Upgrade1to2(con);
			}

			if (version == 2)
			{
				version = Upgrade2to3();
			}
		}


		/// <summary>
		/// Adds the keys that identify a target by what it is, not by the IDs OneNote regenerates:
		/// the key of a page in the identity catalog, and the keys of a notebook and a section. The
		/// columns start empty and are filled in later, when OneNote is available, so this is pure
		/// SQL. The unique indexes on the old ID columns are kept, and key-based ones are added:
		/// the same page, or the same notebook, section or section group, cannot be a favorite
		/// twice even after its IDs have changed.
		/// </summary>
		private int Upgrade2to3()
		{
			var version = 3;
			logger.WriteLine($"upgrading favorites catalog to version {version}");
			using var indent = logger.Indent();

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;

			// the add-in and the tray can both open this catalog; take the write lock first and
			// look at the version again under it, so the second one finds the first one done
			try
			{
				cmd.CommandText = "BEGIN IMMEDIATE";
				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error starting favorites upgrade transaction", exc);
				return 2;
			}

			try
			{
				var current = ReadSchemaVersion("favorites_schema", "schemaID", missing: 2);
				if (current != 2)
				{
					cmd.CommandText = "ROLLBACK";
					cmd.ExecuteNonQuery();
					return current;
				}

				foreach (var (column, type) in new[]
				{
					("pageKey", "INTEGER"), ("notebookKey", "TEXT"), ("sectionKey", "TEXT")
				})
				{
					if (ColumnExists(con, PrimaryTable, column))
					{
						logger.WriteLine($"table favorite already has column {column}");
					}
					else
					{
						logger.WriteLine($"adding {column} column to favorite table");
						cmd.CommandText = $"ALTER TABLE favorite ADD COLUMN {column} {type}";
						cmd.ExecuteNonQuery();
					}
				}

				cmd.CommandText =
					"CREATE UNIQUE INDEX IF NOT EXISTS idx_favorite_target_pagekey " +
					"ON favorite(pageKey) WHERE pageKey IS NOT NULL";
				cmd.ExecuteNonQuery();

				cmd.CommandText =
					"CREATE UNIQUE INDEX IF NOT EXISTS idx_favorite_target_container " +
					"ON favorite(notebookKey, COALESCE(sectionKey, ''), COALESCE(kind, 'section')) " +
					"WHERE pageKey IS NULL AND notebookKey IS NOT NULL";
				cmd.ExecuteNonQuery();

				if (!UpsertSchemaVersion(cmd, "favorites_schema", "schemaID", version))
				{
					cmd.CommandText = "ROLLBACK";
					cmd.ExecuteNonQuery();
					return 2;
				}

				cmd.CommandText = "COMMIT";
				cmd.ExecuteNonQuery();
				return version;
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error upgrading favorites catalog to version {version}", exc);

				try
				{
					cmd.CommandText = "ROLLBACK";
					cmd.ExecuteNonQuery();
				}
				catch (Exception rollbackExc)
				{
					// never mask the error that caused the rollback
					logger.WriteLine("error rolling back favorites upgrade", rollbackExc);
				}

				return 2;
			}
		}

		/// <summary>
		/// Introduces favorites_schema versioning and adds the "kind" column, used to
		/// distinguish notebook and section-group favorites in the Favorites menu.
		/// </summary>
		private int Upgrade1to2(SQLiteConnection con)
		{
			var version = 2;
			logger.WriteLine($"upgrading favorites catalog to version {version}");
			using var indent = logger.Indent();

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			using var transaction = con.BeginTransaction();

			try
			{
				logger.WriteLine("creating table favorites_schema");
				cmd.CommandText =
					"CREATE TABLE IF NOT EXISTS favorites_schema " +
					"(schemaID INTEGER PRIMARY KEY UNIQUE NOT NULL, version NUMERIC (12) UNIQUE NOT NULL)";
				cmd.ExecuteNonQuery();

				logger.WriteLine("adding kind column to favorite table");
				cmd.CommandText = "ALTER TABLE favorite ADD COLUMN kind TEXT";
				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				transaction.Rollback();
				logger.WriteLine("error upgrading favorites catalog to version 2", exc);
				return 1;
			}

			if (!UpgradeSchemaVersion(cmd, transaction, version))
			{
				return 1;
			}

			try
			{
				transaction.Commit();
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error committing changes for version {version}", exc);
				return 1;
			}

			return version;
		}


		private bool UpgradeSchemaVersion(SQLiteCommand cmd, SQLiteTransaction transaction, int version)
		{
			try
			{
				logger.WriteLine($"updating favorites_schema version v{version}");

				// upsert, not a plain UPDATE like HashtagProvider's version: unlike
				// hashtag_scanner, favorites_schema has no guaranteed pre-existing row
				// on a database that predates this versioning scheme
				cmd.CommandText =
					"INSERT INTO favorites_schema (schemaID, version) VALUES (0, @v) " +
					"ON CONFLICT(schemaID) DO UPDATE SET version = @v";
				cmd.Parameters.AddWithValue("@v", version);
				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				logger.End();
				logger.WriteLine($"error updating favorites_schema version v{version}", exc);
				transaction.Rollback();
				return false;
			}

			return true;
		}
		#endregion UpgradeCatalog


		/// <summary>
		/// Test-only constructor that builds the favorites schema directly on an
		/// already-open connection (e.g. an in-memory SQLite database), bypassing the
		/// standard AppData-backed database file and the legacy file migration.
		/// </summary>
		internal FavoritesProvider(SQLiteConnection connection)
		{
			con = connection;

			if (TableExists(PrimaryTable))
			{
				UpgradeCatalog();
			}
			else
			{
				RefreshDataSchema(Domain, Resx.FavoritesDB);
			}
		}


		public static bool CatalogExists()
		{
			return CatalogExists(PrimaryTable);
		}


		public bool DropCatalog()
		{
			return DropCatalog(Domain, Resx.FavoritesDB);
		}


		/// <summary>
		/// Temporary migration from old XML file to DB
		/// </summary>
		private void MigrateFavoritesFile()
		{
			var filer = new FavoritesFileProvider();
			var favs = filer.LoadFavorites();

			if (favs.Count == 0)
			{
				return;
			}

			// are they alphabetical or custom sorted?

			var custom = false;
			if (favs.Count > 1)
			{
				var previous = favs[0].Name;
				for (var i = 1; i < favs.Count; i++)
				{
					// we're going backwards so compare < 0
					if (favs[i].Name.CompareTo(previous) < 0)
					{
						custom = true;
						break;
					}

					previous = favs[i].Name;
				}
			}

			// migrate...

			var count = 0;

			foreach (var fav in favs)
			{
				var favorite = new Favorite
				{
					Name = fav.Name,
					Location = fav.Location,
					Uri = fav.Uri,
					NotebookID = fav.NotebookID,
					SectionID = fav.ObjectID,
					SortOrder = custom ? fav.Index : 0
				};

				if (favorite.Uri.Contains("page-id"))
				{
					// duplicates SectionID, but we only care about one of them at a time
					favorite.PageID = fav.ObjectID;
				}

				if (WriteFavorite(favorite))
				{
					count++;
				}
			}

			logger.WriteLine($"migrated {count} of {favs.Count} items from {Resx.FavoritesFilename}");
		}


		/// <summary>
		/// Creates a new, empty favorites folder.
		/// </summary>
		/// <param name="name">The folder name, must be unique</param>
		/// <returns>The new folderID, or 0 if the folder could not be created</returns>
		public int CreateFolder(string name)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "INSERT INTO favorites_folder (name) VALUES (@n)";
			cmd.Parameters.AddWithValue("@n", name);

			try
			{
				cmd.ExecuteNonQuery();

				cmd.CommandText = "SELECT last_insert_rowid()";
				cmd.Parameters.Clear();
				var folderID = (long)cmd.ExecuteScalar();

				logger.WriteLine($"created favorites folder {name} ({folderID})");
				return (int)folderID;
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error creating favorites folder {name}", exc);
				return 0;
			}
		}


		/// <summary>
		/// Deletes the specified favorite.
		/// </summary>
		/// <param name="favoriteID">The ID of the favorite record to delete.</param>
		/// <returns>True if successful</returns>
		public bool DeleteFavorite(int favoriteID)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.Parameters.AddWithValue("@id", favoriteID);
			cmd.CommandText = "DELETE FROM favorite WHERE favoriteID = @id";

			try
			{
				cmd.ExecuteNonQuery();
				logger.WriteLine($"delete favorite {favoriteID}");
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error deleting favorite {favoriteID}", exc);
				return false;
			}

			return true;
		}


		/// <summary>
		/// Deletes the specified folder and all favorites within it.
		/// </summary>
		/// <param name="folderID">The ID of the folder to delete.</param>
		/// <returns>True if successful</returns>
		public bool DeleteFolder(int folderID)
		{
			using var transaction = con.BeginTransaction();

			try
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandType = CommandType.Text;
					cmd.CommandText = "DELETE FROM favorite WHERE folderID = @id";
					cmd.Parameters.AddWithValue("@id", folderID);
					cmd.ExecuteNonQuery();
				}

				using (var cmd = con.CreateCommand())
				{
					cmd.CommandType = CommandType.Text;
					cmd.CommandText = "DELETE FROM favorites_folder WHERE folderID = @id";
					cmd.Parameters.AddWithValue("@id", folderID);
					cmd.ExecuteNonQuery();
				}

				transaction.Commit();
				logger.WriteLine($"deleted favorites folder {folderID}");
			}
			catch (Exception exc)
			{
				transaction.Rollback();
				logger.WriteLine($"error deleting favorites folder {folderID}", exc);
				return false;
			}

			return true;
		}


		/// <summary>
		/// Read the full collection of favorites. The collection consists of top-level
		/// folder and favorites. Folders contain favorites but do not contain other folders.
		/// </summary>
		/// <returns>A FavoritesCollection representing the entire favorites data set.</returns>
		public FavoritesCollection ReadFavorites()
		{
			var collection = new FavoritesCollection();

			using var cmd = con.CreateCommand();

			// single query, full tree - we allow only one level of folders
			cmd.CommandText =
@"
SELECT
  o.folderID,
  o.name AS folderName,
  f.favoriteID,
  f.name,
  f.alias,
  f.location,
  f.uri,
  f.notebookID,
  f.sectionID,
  f.pageID,
  f.kind,
  f.sortOrder,
  f.pageKey,
  f.notebookKey,
  f.sectionKey
FROM favorites_folder o
LEFT JOIN favorite f ON f.folderID = o.folderID
UNION ALL
SELECT
  0 as folderID,
  null as folderName,
  f.favoriteID,
  f.name,
  f.alias,
  f.location,
  f.uri,
  f.notebookID,
  f.sectionID,
  f.pageID,
  f.kind,
  f.sortOrder,
  f.pageKey,
  f.notebookKey,
  f.sectionKey
FROM favorite f
WHERE f.folderID = 0
ORDER BY folderName COLLATE NOCASE NULLS LAST, sortOrder, name COLLATE NOCASE;
";

			try
			{
				var currentFolderID = 0;
				IFavoritesFolder folder = collection;

				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var folderID = reader.GetInt32(0);
					if (folderID == 0)
					{
						folder = collection;
					}
					else if (folderID != currentFolderID)
					{
						folder = new FavoritesFolder
						{
							FolderID = folderID,
							Name = reader.GetString(1)
						};

						collection.Folders.Add((FavoritesFolder)folder);
						currentFolderID = folderID;
					}

					if (reader.IsDBNull(2))
					{
						// empty folder; the LEFT JOIN produced a row with no matching favorite
						continue;
					}

					folder.Items.Add(Map(reader, folderID));
				}
			}
			catch (Exception exc)
			{
				ReportError("error reading favorites", cmd, exc);
			}

			return collection;
		}


		// reads a favorite from a row of the shape both queries here return: folder ID and name
		// first, then the favorite's own columns from index 2
		private static Favorite Map(SQLiteDataReader reader, int folderID)
		{
			return new Favorite
			{
				ID = reader.GetInt32(2),
				FolderID = folderID,
				Name = reader.GetString(3),
				Alias = reader.IsDBNull(4) ? null : reader.GetString(4),
				Location = reader.GetString(5),
				Uri = reader.GetString(6),
				NotebookID = reader.GetString(7),
				SectionID = reader.GetString(8),
				PageID = reader.IsDBNull(9) ? null : reader.GetString(9),
				Kind = reader.IsDBNull(10) ? null : reader.GetString(10),
				SortOrder = reader.GetInt32(11),
				PageKey = reader.IsDBNull(12) ? null : reader.GetInt64(12),
				NotebookKey = reader.IsDBNull(13) ? null : reader.GetString(13),
				SectionKey = reader.IsDBNull(14) ? null : reader.GetString(14)
			};
		}


		/// <summary>
		/// Reads one favorite.
		/// </summary>
		/// <param name="favoriteID">The ID of the favorite record</param>
		/// <returns>The favorite, or null if there is none</returns>
		public Favorite ReadFavorite(int favoriteID)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText =
				"SELECT COALESCE(f.folderID, 0), NULL, f.favoriteID, f.name, f.alias, f.location, " +
				"f.uri, f.notebookID, f.sectionID, f.pageID, f.kind, f.sortOrder, " +
				"f.pageKey, f.notebookKey, f.sectionKey " +
				"FROM favorite f WHERE f.favoriteID = @id";

			cmd.Parameters.AddWithValue("@id", favoriteID);

			try
			{
				using var reader = cmd.ExecuteReader();
				return reader.Read() ? Map(reader, reader.GetInt32(0)) : null;
			}
			catch (Exception exc)
			{
				ReportError("error reading favorite", cmd, exc);
				return null;
			}
		}


		/// <summary>
		/// Saves what was found about where a favorite points now: its keys, IDs, link and location.
		/// The name, alias, folder and sort order are the user's and are left alone, so that a save made
		/// in the background can never overwrite a change just made in the manage dialog; see
		/// UpdateFavorite for those.
		/// </summary>
		/// <param name="favorite">The favorite to update, identified by favorite.ID</param>
		/// <returns>True if successful</returns>
		public bool UpdateTarget(Favorite favorite)
		{
			return UpdateTarget(favorite, out _);
		}


		/// <summary>
		/// Saves what was found about where a favorite points now.
		/// </summary>
		/// <param name="favorite">The favorite to update, identified by favorite.ID</param>
		/// <param name="duplicate">True if the update failed because another favorite already
		/// points at the same target, which can happen when two favorites turn out to be the
		/// same page once their keys are known</param>
		/// <returns>True if successful</returns>
		public bool UpdateTarget(Favorite favorite, out bool duplicate)
		{
			duplicate = false;

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText =
				"UPDATE favorite SET location = @l, uri = @u, notebookID = @b, " +
				"sectionID = @s, pageID = @g, pageKey = @pk, notebookKey = @nk, sectionKey = @sk " +
				"WHERE favoriteID = @id";

			cmd.Parameters.AddWithValue("@l", favorite.Location);
			cmd.Parameters.AddWithValue("@u", favorite.Uri);
			cmd.Parameters.AddWithValue("@b", favorite.NotebookID);
			cmd.Parameters.AddWithValue("@s", favorite.SectionID);
			cmd.Parameters.AddWithValue("@g", (object)favorite.PageID ?? DBNull.Value);
			cmd.Parameters.AddWithValue("@pk", favorite.PageKey.HasValue ? favorite.PageKey.Value : DBNull.Value);
			cmd.Parameters.AddWithValue("@nk", (object)favorite.NotebookKey ?? DBNull.Value);
			cmd.Parameters.AddWithValue("@sk", (object)favorite.SectionKey ?? DBNull.Value);
			cmd.Parameters.AddWithValue("@id", favorite.ID);

			try
			{
				cmd.ExecuteNonQuery();
				logger.Verbose($"updated target of favorite {favorite.ID}");
				return true;
			}
			catch (Exception exc)
			{
				if (exc is SQLiteException && exc.Message.Contains("UNIQUE constraint failed"))
				{
					duplicate = true;
					logger.WriteLine($"favorite {favorite.ID} points at the same target as another");
				}
				else
				{
					logger.WriteLine($"error updating target of favorite {favorite.ID}", exc);
				}

				return false;
			}
		}

		/// <summary>
		/// Renames an existing favorites folder.
		/// </summary>
		/// <param name="folderID">The ID of the folder to rename.</param>
		/// <param name="name">The new folder name, must be unique.</param>
		/// <returns>True if successful</returns>
		public bool RenameFolder(int folderID, string name)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "UPDATE favorites_folder SET name = @n WHERE folderID = @id";
			cmd.Parameters.AddWithValue("@n", name);
			cmd.Parameters.AddWithValue("@id", folderID);

			try
			{
				cmd.ExecuteNonQuery();
				logger.WriteLine($"renamed favorites folder {folderID} to {name}");
				return true;
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error renaming favorites folder {folderID}", exc);
				return false;
			}
		}


		/// <summary>
		/// Updates the alias, folder assignment, and sort order of an existing favorite.
		/// </summary>
		/// <param name="favorite">The favorite to update, identified by favorite.ID</param>
		/// <returns>True if successful</returns>
		public bool UpdateFavorite(Favorite favorite)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;

			cmd.CommandText =
				"UPDATE favorite SET name = @n, alias = @a, folderID = @f, sortOrder = @o " +
				"WHERE favoriteID = @id";

			cmd.Parameters.Clear();
			cmd.Parameters.Add("@n", DbType.String);
			cmd.Parameters.Add("@a", DbType.String);
			cmd.Parameters.Add("@f", DbType.Int32);
			cmd.Parameters.Add("@o", DbType.Int32);
			cmd.Parameters.Add("@id", DbType.Int32);

			object alias = string.IsNullOrWhiteSpace(favorite.Alias) ? DBNull.Value : favorite.Alias;

			cmd.Parameters["@n"].Value = favorite.Name;
			cmd.Parameters["@a"].Value = alias;
			cmd.Parameters["@f"].Value = favorite.FolderID;
			cmd.Parameters["@o"].Value = favorite.SortOrder;
			cmd.Parameters["@id"].Value = favorite.ID;

			try
			{
				cmd.ExecuteNonQuery();
				logger.Verbose($"updated favorite {favorite.ID}");
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error updating favorite {favorite.ID}", exc);
				return false;
			}

			return true;
		}


		/// <summary>
		/// Gets the sort order value that would place a new favorite after every
		/// existing favorite in the given folder (0 for the root/top-level list).
		/// </summary>
		/// <param name="folderID">The folder to scope the lookup to, or 0 for the root</param>
		/// <returns>The next sort order value for the given folder</returns>
		public int GetNextSortOrder(int folderID)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText =
				"SELECT COALESCE(MAX(sortOrder), -1) + 1 FROM favorite WHERE folderID = @f";

			cmd.Parameters.AddWithValue("@f", folderID);

			try
			{
				return Convert.ToInt32(cmd.ExecuteScalar());
			}
			catch (Exception exc)
			{
				logger.WriteLine("error computing next favorite sort order", exc);
				return 0;
			}
		}


		/// <summary>
		/// Records the given favorite.
		/// </summary>
		/// <param name="favorite">A Favorite to save</param>
		/// <returns>True if successful</returns>
		public bool WriteFavorite(Favorite favorite)
		{
			return WriteFavorite(favorite, out _);
		}


		/// <summary>
		/// Records the given favorite.
		/// </summary>
		/// <param name="favorite">A Favorite to save</param>
		/// <param name="duplicate">
		/// True if the write failed because a favorite already exists for the same pageID,
		/// or, for section/section-group favorites, the same sectionID
		/// </param>
		/// <returns>True if successful</returns>
		public bool WriteFavorite(Favorite favorite, out bool duplicate)
		{
			duplicate = false;

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;

			cmd.CommandText = "INSERT INTO favorite " +
				"(folderID, name, alias, location, uri, notebookID, sectionID, pageID, kind, sortOrder, " +
				"pageKey, notebookKey, sectionKey) " +
				"VALUES (@f, @n, @a, @l, @u, @b, @s, @g, @k, @o, @pk, @nk, @sk)";

			cmd.Parameters.Clear();
			cmd.Parameters.Add("@f", DbType.Int32);
			cmd.Parameters.Add("@n", DbType.String);
			cmd.Parameters.Add("@a", DbType.String);
			cmd.Parameters.Add("@l", DbType.String);
			cmd.Parameters.Add("@u", DbType.String);
			cmd.Parameters.Add("@b", DbType.String);
			cmd.Parameters.Add("@s", DbType.String);
			cmd.Parameters.Add("@g", DbType.String);
			cmd.Parameters.Add("@k", DbType.String);
			cmd.Parameters.Add("@o", DbType.Int32);
			cmd.Parameters.Add("@pk", DbType.Int64);
			cmd.Parameters.Add("@nk", DbType.String);
			cmd.Parameters.Add("@sk", DbType.String);

			logger.Verbose($"writing favorite {favorite.Location}");

			object alias = string.IsNullOrWhiteSpace(favorite.Alias) ? DBNull.Value : favorite.Alias;

			cmd.Parameters["@f"].Value = favorite.FolderID;
			cmd.Parameters["@n"].Value = favorite.Name;
			cmd.Parameters["@a"].Value = alias;
			cmd.Parameters["@l"].Value = favorite.Location;
			cmd.Parameters["@u"].Value = favorite.Uri;
			cmd.Parameters["@b"].Value = favorite.NotebookID;
			cmd.Parameters["@s"].Value = favorite.SectionID;
			cmd.Parameters["@g"].Value = favorite.PageID;
			cmd.Parameters["@k"].Value = favorite.Kind;
			cmd.Parameters["@o"].Value = favorite.SortOrder;
			cmd.Parameters["@pk"].Value = favorite.PageKey.HasValue ? favorite.PageKey.Value : DBNull.Value;
			cmd.Parameters["@nk"].Value = (object)favorite.NotebookKey ?? DBNull.Value;
			cmd.Parameters["@sk"].Value = (object)favorite.SectionKey ?? DBNull.Value;

			try
			{
				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				var targetID = favorite.PageID ?? favorite.SectionID;

				if (exc is SQLiteException && exc.Message.Contains("UNIQUE constraint failed"))
				{
					duplicate = true;
					logger.WriteLine($"duplicate favorite {favorite.Location} on {targetID}");
				}
				else
				{
					logger.WriteLine($"error writing favorite {favorite.Location} on {targetID}");
					logger.WriteLine(exc);
				}

				return false;
			}

			return true;
		}
	}
}
