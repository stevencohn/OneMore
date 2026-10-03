//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Layouts
{
	using System;
	using System.Data;
	using System.Data.SQLite;
	using Resx = Properties.Resources;


	internal class LayoutsProvider : DatabaseProvider
	{
		private static readonly string Domain = "layouts";
		private static readonly string PrimaryTable = "layout";

		// version 2 adds layouts_schema, which layouts never had, and the page key of a window
		private const int CurrentVersion = 2;


		/// <summary>
		/// Initialize this provider, opening the standard database
		/// </summary>
		public LayoutsProvider()
			: base()
		{
			OpenDatabase();

			if (!CatalogExists())
			{
				RefreshDataSchema(Domain, Resx.LayoutsDB);
			}
			else
			{
				UpgradeCatalog();
			}
		}


		/// <summary>
		/// Test-only constructor that builds the favorites schema directly on an
		/// already-open connection (e.g. an in-memory SQLite database), bypassing the
		/// standard AppData-backed database file and the legacy file migration.
		/// </summary>
		internal LayoutsProvider(SQLiteConnection connection)
		{
			con = connection;

			if (TableExists(PrimaryTable))
			{
				UpgradeCatalog();
			}
			else
			{
				RefreshDataSchema(Domain, Resx.LayoutsDB);
			}
		}


		#region UpgradeCatalog
		private void UpgradeCatalog()
		{
			// a database that predates versioning has no layouts_schema table at all
			var version = ReadSchemaVersion("layouts_schema", "schemaID", missing: 1);

			if (IsNewerThanKnown(Domain, version, CurrentVersion))
			{
				return;
			}

			if (version == 1)
			{
				version = Upgrade1to2();
			}
		}


		/// <summary>
		/// Introduces layouts_schema versioning, which layouts never had, and adds the key of the
		/// page of a window. The column starts empty and is filled in later, when OneNote is
		/// available, so this is pure SQL. The unique index on the page ID is kept and a key-based
		/// one is added: the same page cannot be in a layout twice even after its ID has changed.
		/// </summary>
		private int Upgrade1to2()
		{
			var version = 2;
			logger.WriteLine($"upgrading layouts catalog to version {version}");
			using var indent = logger.Indent();

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;

			// the add-in and the tray can both open this catalog; take the write lock first and look
			// at the version again under it, so the second one finds the first one done
			try
			{
				cmd.CommandText = "BEGIN IMMEDIATE";
				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error starting layouts upgrade transaction", exc);
				return 1;
			}

			try
			{
				if (ReadSchemaVersion("layouts_schema", "schemaID", missing: 1) != 1)
				{
					cmd.CommandText = "ROLLBACK";
					cmd.ExecuteNonQuery();
					return ReadSchemaVersion("layouts_schema", "schemaID", missing: 1);
				}

				cmd.CommandText =
					"CREATE TABLE IF NOT EXISTS layouts_schema " +
					"(schemaID INTEGER PRIMARY KEY UNIQUE NOT NULL, version NUMERIC (12) UNIQUE NOT NULL)";
				cmd.ExecuteNonQuery();

				if (ColumnExists(con, "layout_window", "pageKey"))
				{
					logger.WriteLine("table layout_window already has column pageKey");
				}
				else
				{
					logger.WriteLine("adding pageKey column to layout_window table");
					cmd.CommandText = "ALTER TABLE layout_window ADD COLUMN pageKey INTEGER";
					cmd.ExecuteNonQuery();
				}

				cmd.CommandText =
					"CREATE UNIQUE INDEX IF NOT EXISTS idx_layouts_target_pagekey " +
					"ON layout_window (layoutID, pageKey) WHERE pageKey IS NOT NULL";
				cmd.ExecuteNonQuery();

				if (!UpsertSchemaVersion(cmd, "layouts_schema", "schemaID", version))
				{
					cmd.CommandText = "ROLLBACK";
					cmd.ExecuteNonQuery();
					return 1;
				}

				cmd.CommandText = "COMMIT";
				cmd.ExecuteNonQuery();
				return version;
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error upgrading layouts catalog to version {version}", exc);

				try
				{
					cmd.CommandText = "ROLLBACK";
					cmd.ExecuteNonQuery();
				}
				catch (Exception rollbackExc)
				{
					// never mask the error that caused the rollback
					logger.WriteLine("error rolling back layouts upgrade", rollbackExc);
				}

				return 1;
			}
		}
		#endregion UpgradeCatalog


		public static bool CatalogExists()
		{
			return CatalogExists(PrimaryTable);
		}


		public bool DropCatalog()
		{
			return DropCatalog(Domain, Resx.LayoutsDB);
		}


		/// <summary>
		/// Creates a new, empty layout.
		/// </summary>
		/// <param name="name">The layout name, must be unique</param>
		/// <returns>The new layoutID, or 0 if the layout could not be created</returns>
		public int CreateLayout(string name)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "INSERT INTO layout (name) VALUES (@n)";
			cmd.Parameters.AddWithValue("@n", name);

			try
			{
				cmd.ExecuteNonQuery();

				cmd.CommandText = "SELECT last_insert_rowid()";
				cmd.Parameters.Clear();
				var layoutID = (long)cmd.ExecuteScalar();

				logger.WriteLine($"created layout {name} ({layoutID})");
				return (int)layoutID;
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error creating layout {name}", exc);
				return 0;
			}
		}


		/// <summary>
		/// Deletes the specified layout window.
		/// </summary>
		/// <param name="windowID">The ID of the window record to delete.</param>
		/// <returns>True if successful</returns>
		public bool DeleteWindow(int windowID)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.Parameters.AddWithValue("@id", windowID);
			cmd.CommandText = "DELETE FROM layout_window WHERE windowID = @id";

			try
			{
				cmd.ExecuteNonQuery();
				logger.WriteLine($"deleted layout window {windowID}");
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error deleting layout window {windowID}", exc);
				return false;
			}

			return true;
		}


		/// <summary>
		/// Deletes the specified layout and all windows within it.
		/// </summary>
		/// <param name="layoutID">The ID of the layout to delete.</param>
		/// <returns>True if successful</returns>
		public bool DeleteLayout(int layoutID)
		{
			using var transaction = con.BeginTransaction();

			try
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandType = CommandType.Text;
					cmd.CommandText = "DELETE FROM layout_window WHERE layoutID = @id";
					cmd.Parameters.AddWithValue("@id", layoutID);
					cmd.ExecuteNonQuery();
				}

				using (var cmd = con.CreateCommand())
				{
					cmd.CommandType = CommandType.Text;
					cmd.CommandText = "DELETE FROM layout WHERE layoutID = @id";
					cmd.Parameters.AddWithValue("@id", layoutID);
					cmd.ExecuteNonQuery();
				}

				transaction.Commit();
				logger.WriteLine($"deleted layout {layoutID}");
			}
			catch (Exception exc)
			{
				transaction.Rollback();
				logger.WriteLine($"error deleting layout {layoutID}", exc);
				return false;
			}

			return true;
		}


		/// <summary>
		/// Read the full collection of layouts.
		/// </summary>
		/// <returns>A LayoutsCollection representing the entire layouts data set.</returns>
		public LayoutsCollection ReadLayouts()
		{
			var collection = new LayoutsCollection();

			using var cmd = con.CreateCommand();

			// single query, full tree - LEFT JOIN so layouts with no windows yet are
			// still included rather than silently dropped
			cmd.CommandText =
@"
SELECT l.layoutID, l.name AS layoutName, w.windowID, w.name, w.alias,
  w.location, w.uri, w.notebookID, w.sectionID, w.pageID, w.zOrder,
  w.device, w.winLeft, w.winTop, w.winRight, w.winBottom, w.pageKey
FROM layout l
LEFT JOIN layout_window w ON w.layoutID = l.layoutID
ORDER BY layoutName, w.zOrder;
";

			try
			{
				var currentLayoutID = -1;
				Layout layout = null;

				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var layoutID = reader.GetInt32(0);
					if (layoutID != currentLayoutID)
					{
						layout = new Layout
						{
							LayoutID = layoutID,
							Name = reader.GetString(1)
						};

						collection.Layouts.Add(layout);
						currentLayoutID = layoutID;
					}

					if (reader.IsDBNull(2))
					{
						// empty layout; the LEFT JOIN produced a row with no matching window
						continue;
					}

					var window = new LayoutWindow
					{
						ID = reader.GetInt32(2),
						LayoutID = layoutID,
						Name = reader.GetString(3),
						Alias = reader.IsDBNull(4) ? null : reader.GetString(4),
						Location = reader.GetString(5),
						Uri = reader.GetString(6),
						NotebookID = reader.GetString(7),
						SectionID = reader.GetString(8),
						PageID = reader.IsDBNull(9) ? null : reader.GetString(9),
						ZOrder = reader.GetInt32(10),
						Device = reader.IsDBNull(11) ? null : reader.GetString(11),
						WinLeft = reader.IsDBNull(12) ? (int?)null : reader.GetInt32(12),
						WinTop = reader.IsDBNull(13) ? (int?)null : reader.GetInt32(13),
						WinRight = reader.IsDBNull(14) ? (int?)null : reader.GetInt32(14),
						WinBottom = reader.IsDBNull(15) ? (int?)null : reader.GetInt32(15),
						PageKey = reader.IsDBNull(16) ? (long?)null : reader.GetInt64(16)
					};

					layout.Windows.Add(window);
				}
			}
			catch (Exception exc)
			{
				ReportError("error reading layouts", cmd, exc);
			}

			return collection;
		}


		/// <summary>
		/// Renames an existing favorites layout.
		/// </summary>
		/// <param name="layoutID">The ID of the layout to rename.</param>
		/// <param name="name">The new layout name, must be unique.</param>
		/// <returns>True if successful</returns>
		public bool RenameLayout(int layoutID, string name)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "UPDATE layout SET name = @n WHERE layoutID = @id";
			cmd.Parameters.AddWithValue("@n", name);
			cmd.Parameters.AddWithValue("@id", layoutID);

			try
			{
				cmd.ExecuteNonQuery();
				logger.WriteLine($"renamed layout {layoutID} to {name}");
				return true;
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error renaming layout {layoutID}", exc);
				return false;
			}
		}


		/// <summary>
		/// Updates the alias, layout assignment, and z-order of an existing layout window.
		/// </summary>
		/// <param name="window">The layout window to update, identified by windowID</param>
		/// <returns>True if successful</returns>
		public bool UpdateWindow(LayoutWindow window)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;

			cmd.CommandText =
				"UPDATE layout_window SET name = @n, alias = @a, location = @l, layoutID = @f, zOrder = @o " +
				"WHERE windowID = @id";

			cmd.Parameters.Clear();
			cmd.Parameters.Add("@n", DbType.String);
			cmd.Parameters.Add("@a", DbType.String);
			cmd.Parameters.Add("@l", DbType.String);
			cmd.Parameters.Add("@f", DbType.Int32);
			cmd.Parameters.Add("@o", DbType.Int32);
			cmd.Parameters.Add("@id", DbType.Int32);

			object alias = string.IsNullOrWhiteSpace(window.Alias) ? DBNull.Value : window.Alias;

			cmd.Parameters["@n"].Value = window.Name;
			cmd.Parameters["@a"].Value = alias;
			cmd.Parameters["@l"].Value = window.Location;
			cmd.Parameters["@f"].Value = window.LayoutID;
			cmd.Parameters["@o"].Value = window.ZOrder;
			cmd.Parameters["@id"].Value = window.ID;

			try
			{
				cmd.ExecuteNonQuery();
				logger.Verbose($"updated layout window {window.ID}");
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error updating layout window {window.ID}", exc);
				return false;
			}

			return true;
		}


		/// <summary>
		/// Saves what was found about where a window's page is now: its key, IDs, link and location.
		/// The name, alias, layout and z-order are the user's and are left alone, so a save made in
		/// the background can never overwrite a change just made in the manage dialog; see
		/// UpdateWindow for those.
		/// </summary>
		/// <param name="window">The window to update, identified by window.ID</param>
		/// <param name="duplicate">True if the update failed because the same page is already in
		/// the layout under another window, which can happen when two windows turn out to be the
		/// same page once their keys are known</param>
		/// <returns>True if successful</returns>
		public bool UpdateTarget(LayoutWindow window, out bool duplicate)
		{
			duplicate = false;

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText =
				"UPDATE layout_window SET location = @l, uri = @u, notebookID = @b, " +
				"sectionID = @s, pageID = @g, pageKey = @pk WHERE windowID = @id";

			cmd.Parameters.AddWithValue("@l", window.Location);
			cmd.Parameters.AddWithValue("@u", window.Uri);
			cmd.Parameters.AddWithValue("@b", window.NotebookID);
			cmd.Parameters.AddWithValue("@s", window.SectionID);
			cmd.Parameters.AddWithValue("@g", window.PageID);
			cmd.Parameters.AddWithValue("@pk", window.PageKey.HasValue ? window.PageKey.Value : DBNull.Value);
			cmd.Parameters.AddWithValue("@id", window.ID);

			try
			{
				cmd.ExecuteNonQuery();
				logger.Verbose($"updated target of layout window {window.ID}");
				return true;
			}
			catch (Exception exc)
			{
				if (exc is SQLiteException && exc.Message.Contains("UNIQUE constraint failed"))
				{
					duplicate = true;
					logger.WriteLine($"layout window {window.ID} is the same page as another in its layout");
				}
				else
				{
					logger.WriteLine($"error updating target of layout window {window.ID}", exc);
				}

				return false;
			}
		}


		/// <summary>
		/// Records the given layout window.
		/// </summary>
		/// <param name="window">A window to save</param>
		/// <returns>True if successful</returns>
		public bool WriteWindow(LayoutWindow window)
		{
			return WriteWindow(window, out _);
		}


		/// <summary>
		/// Records the given layout window.
		/// </summary>
		/// <param name="window">A window to save</param>
		/// <param name="duplicate">
		/// True if the write failed because a window already exists for the same pageID,
		/// or, for section/section-group favorites, the same sectionID
		/// </param>
		/// <returns>True if successful</returns>
		public bool WriteWindow(LayoutWindow window, out bool duplicate)
		{
			duplicate = false;

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;

			cmd.CommandText = "INSERT INTO layout_window " +
				"(layoutID, name, alias, location, uri, notebookID, sectionID, pageID, zOrder, " +
				"device, winLeft, winTop, winRight, winBottom, pageKey) " +
				"VALUES (@f, @n, @a, @l, @u, @b, @s, @g, @z, @d, @wl, @wt, @wr, @wb, @pk)";

			cmd.Parameters.Clear();
			cmd.Parameters.Add("@f", DbType.Int32);
			cmd.Parameters.Add("@n", DbType.String);
			cmd.Parameters.Add("@a", DbType.String);
			cmd.Parameters.Add("@l", DbType.String);
			cmd.Parameters.Add("@u", DbType.String);
			cmd.Parameters.Add("@b", DbType.String);
			cmd.Parameters.Add("@s", DbType.String);
			cmd.Parameters.Add("@g", DbType.String);
			cmd.Parameters.Add("@z", DbType.Int32);
			cmd.Parameters.Add("@d", DbType.String);
			cmd.Parameters.Add("@wl", DbType.Int32);
			cmd.Parameters.Add("@wt", DbType.Int32);
			cmd.Parameters.Add("@wr", DbType.Int32);
			cmd.Parameters.Add("@wb", DbType.Int32);
			cmd.Parameters.Add("@pk", DbType.Int64);

			logger.Verbose($"writing layout window {window.Location}");

			object alias = string.IsNullOrWhiteSpace(window.Alias) ? DBNull.Value : window.Alias;
			object device = string.IsNullOrWhiteSpace(window.Device) ? DBNull.Value : window.Device;

			cmd.Parameters["@f"].Value = window.LayoutID;
			cmd.Parameters["@n"].Value = window.Name;
			cmd.Parameters["@a"].Value = alias;
			cmd.Parameters["@l"].Value = window.Location;
			cmd.Parameters["@u"].Value = window.Uri;
			cmd.Parameters["@b"].Value = window.NotebookID;
			cmd.Parameters["@s"].Value = window.SectionID;
			cmd.Parameters["@g"].Value = window.PageID;
			cmd.Parameters["@z"].Value = window.ZOrder;
			cmd.Parameters["@d"].Value = device;
			cmd.Parameters["@wl"].Value = (object)window.WinLeft ?? DBNull.Value;
			cmd.Parameters["@wt"].Value = (object)window.WinTop ?? DBNull.Value;
			cmd.Parameters["@wr"].Value = (object)window.WinRight ?? DBNull.Value;
			cmd.Parameters["@wb"].Value = (object)window.WinBottom ?? DBNull.Value;
			cmd.Parameters["@pk"].Value = window.PageKey.HasValue ? window.PageKey.Value : DBNull.Value;

			try
			{
				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				var targetID = window.PageID ?? window.SectionID;

				if (exc is SQLiteException && exc.Message.Contains("UNIQUE constraint failed"))
				{
					duplicate = true;
					logger.WriteLine($"duplicate layout window {window.Location} on {targetID}");
				}
				else
				{
					logger.WriteLine($"error writing layout window {window.Location} on {targetID}");
					logger.WriteLine(exc);
				}

				return false;
			}

			return true;
		}
	}
}
