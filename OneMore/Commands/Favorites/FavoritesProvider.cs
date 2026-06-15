//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using System;
	using System.Data;
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
					SortOrder = fav.Index
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
  COALESCE(f.alias, f.name) AS alias,
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
  0 as folderName,
  f.favoriteID,
  f.name,
  COALESCE(f.alias, f.name) AS alias,
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

					var favorite = new Favorite
					{
						ID = reader.GetInt32(2),
						Name = reader.GetString(3),
						Alias = reader.GetString(4),
						Location = reader.GetString(5),
						Uri = reader.GetString(6),
						NotebookID = reader.GetString(7),
						SectionID = reader.GetString(8),
						PageID = reader.GetString(9),
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
		/// Records the given favorite.
		/// </summary>
		/// <param name="favorite">A Favorite to save</param>
		public bool WriteFavorite(Favorite favorite)
		{
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
				logger.WriteLine($"error writing favorite {favorite.Location} on {targetID}");
				logger.WriteLine(exc);
				return false;
			}

			return true;
		}
	}
}
