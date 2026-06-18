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
		}


		/// <summary>
		/// Test-only constructor that builds the favorites schema directly on an
		/// already-open connection (e.g. an in-memory SQLite database), bypassing the
		/// standard AppData-backed database file and the legacy file migration.
		/// </summary>
		internal FavoritesProvider(SQLiteConnection connection)
		{
			con = connection;
			RefreshDataSchema(Domain, Resx.FavoritesDB);
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
  f.sortOrder
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
  f.sortOrder
FROM favorite f
WHERE f.folderID = 0
ORDER BY folderName NULLS LAST, sortOrder, name;
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

					var favorite = new Favorite
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
						SortOrder = reader.GetInt32(10)
					};

					folder.Items.Add(favorite);
				}
			}
			catch (Exception exc)
			{
				ReportError("error reading favorites", cmd, exc);
			}

			return collection;
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
				"UPDATE favorite SET alias = @a, folderID = @f, sortOrder = @o " +
				"WHERE favoriteID = @id";

			cmd.Parameters.Clear();
			cmd.Parameters.Add("@a", DbType.String);
			cmd.Parameters.Add("@f", DbType.Int32);
			cmd.Parameters.Add("@o", DbType.Int32);
			cmd.Parameters.Add("@id", DbType.Int32);

			object alias = string.IsNullOrWhiteSpace(favorite.Alias) ? DBNull.Value : favorite.Alias;

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
				"(folderID, name, alias, location, uri, notebookID, sectionID, pageID, sortOrder) " +
				"VALUES (@f, @n, @a, @l, @u, @b, @s, @g, @o)";

			cmd.Parameters.Clear();
			cmd.Parameters.Add("@f", DbType.Int32);
			cmd.Parameters.Add("@n", DbType.String);
			cmd.Parameters.Add("@a", DbType.String);
			cmd.Parameters.Add("@l", DbType.String);
			cmd.Parameters.Add("@u", DbType.String);
			cmd.Parameters.Add("@b", DbType.String);
			cmd.Parameters.Add("@s", DbType.String);
			cmd.Parameters.Add("@g", DbType.String);
			cmd.Parameters.Add("@o", DbType.Int32);

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
			cmd.Parameters["@o"].Value = favorite.SortOrder;

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
