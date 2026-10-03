//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SQLite;
	using System.Linq;
	using System.Text;
	using River.OneMoreAddIn.Properties;


	/// <summary>
	/// Reads, Writes, manages a data store of Hashtags.
	/// </summary>
	internal class HashtagProvider : DatabaseProvider
	{
		private const int ScannerID = 0;

		// version 6 keys tags by page key instead of by the omPageID stamp; see Upgrade5to6
		private const int CurrentVersion = 6;


		/// <summary>
		/// Initialize this provider, opening the standard database
		/// </summary>
		public HashtagProvider()
			: base()
		{
			if (CatalogExists())
			{
				UpgradeCatalog();
			}
			else
			{
				RefreshDataSchema("hashtag", Resources.HashtagsDB);
			}
		}


		/// <summary>
		/// Initialize this provider over an open connection, such as an in-memory database.
		/// </summary>
		internal HashtagProvider(SQLiteConnection connection)
		{
			con = connection;

			if (TableExists("hashtag_scanner"))
			{
				UpgradeCatalog();
			}
			else
			{
				RefreshDataSchema("hashtag", Resources.HashtagsDB);
			}
		}


		public static bool CatalogExists()		{
			return CatalogExists("hashtag_scanner");
		}


		public bool DropCatalog()
		{
			return DropCatalog("hashtag", Resources.HashtagsDB);
		}


		#region UpgradeCatalog
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell",
			"S1854:Unused assignments should be removed", Justification = "<Pending>")]
		private void UpgradeCatalog()
		{
			OpenDatabase();

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = $"SELECT version FROM hashtag_scanner WHERE scannerID = {ScannerID}";

			var version = 0;
			try
			{
				using var reader = cmd.ExecuteReader();
				if (reader.Read())
				{
					version = reader.GetInt32(0);
				}
			}
			catch (Exception exc)
			{
				ReportError("error reading scanner version", cmd, exc);
				return;
			}

			if (IsNewerThanKnown("hashtag", version, CurrentVersion))
			{
				return;
			}

			// upgrade incrementally...

			if (version == 1)
			{
				version = Upgrade1to2(con);
			}

			if (version == 2)
			{
				version = Upgrade2to3(con);
			}

			if (version == 3)
			{
				version = Upgrade3to4(con);
			}

			if (version == 4)
			{
				version = Upgrade4to5(con);
			}

			if (version == 5)
			{
				version = Upgrade5to6(con);
			}
		}


		private int Upgrade1to2(SQLiteConnection con)
		{
			var version = 2;
			logger.WriteLine($"upgrading hashtag catalog to version {version}");
			using var indent = logger.Indent();

			using var cmd = con.CreateCommand();
			using var transaction = con.BeginTransaction();

			try
			{
				logger.WriteLine("creating view page_hashtags");
				cmd.CommandType = CommandType.Text;
				cmd.CommandText =
					"CREATE VIEW IF NOT EXISTS page_hashtags (moreID, tags) AS SELECT " +
					"t.moreID, group_concat(DISTINCT(t.tag)) AS tags FROM hashtag t GROUP BY t.moreID;";

				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error creating view hashtag_hashtags", exc);
				return 0;
			}

			if (!UpgradeSchemaVersion(cmd, transaction, version))
			{
				return 0;
			}

			try
			{
				transaction.Commit();
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error committing changes for version {version}", exc);
				return 0;
			}

			return version;
		}


		private int Upgrade2to3(SQLiteConnection con)
		{
			var version = 3;
			logger.WriteLine($"upgrading hashtag catalog to version {version}");
			using var indent = logger.Indent();

			using var cmd = con.CreateCommand();
			using var transaction = con.BeginTransaction();

			try
			{
				logger.WriteLine("creating table hashtag_notebook");
				cmd.CommandType = CommandType.Text;
				cmd.CommandText =
					"CREATE TABLE IF NOT EXISTS hashtag_notebook " +
					"(notebookID TEXT PRIMARY KEY, name TEXT)";

				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error creating table hashtag_notebook", exc);
				return 0;
			}

			if (!UpgradeSchemaVersion(cmd, transaction, version))
			{
				return 0;
			}

			try
			{
				transaction.Commit();
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error committing changes for version {version}", exc);
				return 0;
			}

			return version;
		}


		private int Upgrade3to4(SQLiteConnection con)
		{
			int version = 4;
			logger.WriteLine($"upgrading hashtag catalog to version {version}");
			using var indent = logger.Indent();

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;

			using var transaction = con.BeginTransaction();

			try
			{
				logger.WriteLine("updating table hashtag_notebook");

				cmd.CommandText =
					"ALTER TABLE hashtag_notebook " +
					"ADD COLUMN lastModified TEXT NOT NULL default('')";

				cmd.ExecuteNonQuery();

				cmd.CommandText =
					"UPDATE hashtag_notebook AS nb SET lastModified = COALESCE(" +
					"(SELECT MAX(t.lastModified) " +
					"FROM hashtag_notebook n " +
					"JOIN hashtag_page p ON p.notebookID = n.notebookID " +
					"JOIN hashtag t ON t.moreID = p.moreID " +
					"WHERE n.notebookID = nb.notebookID " +
					"GROUP BY n.notebookID), '')";

				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				transaction.Rollback();
				logger.WriteLine("error updating table hashtag_notebook", exc);
				return 0;
			}

			try
			{
				logger.WriteLine("updating table hashtag");

				cmd.CommandText =
					"CREATE TABLE hashtag_v4 " +
					"(tag TEXT NOT NULL, moreID TEXT NOT NULL, objectID TEXT NOT NULL, " +
					"snippet TEXT, documentOrder INTEGER DEFAULT (0), lastModified TEXT NOT NULL, " +
					"PRIMARY KEY (tag, objectID), " +
					"CONSTRAINT FK_moreID FOREIGN KEY (moreID) REFERENCES hashtag_page (moreID) " +
					"ON DELETE CASCADE)";

				cmd.ExecuteNonQuery();

				cmd.CommandText =
					"INSERT INTO hashtag_v4 (tag, moreID, objectID, snippet, lastModified) " +
					"SELECT tag, moreID, objectID, snippet, lastModified " +
					"FROM hashtag";

				cmd.ExecuteNonQuery();

				cmd.CommandText = "DROP INDEX IDX_moreID";
				cmd.ExecuteNonQuery();

				cmd.CommandText = "DROP INDEX IDX_tag";
				cmd.ExecuteNonQuery();

				cmd.CommandText = "DROP TABLE hashtag";
				cmd.ExecuteNonQuery();

				cmd.CommandText = "DROP VIEW page_hashtags";
				cmd.ExecuteNonQuery();

				cmd.CommandText = "ALTER TABLE hashtag_v4 RENAME TO hashtag";
				cmd.ExecuteNonQuery();

				cmd.CommandText = "CREATE INDEX IDX_moreID ON hashtag(moreID)";
				cmd.ExecuteNonQuery();

				cmd.CommandText = "CREATE INDEX IDX_tag ON hashtag(tag)";
				cmd.ExecuteNonQuery();

				cmd.CommandText = "CREATE VIEW IF NOT EXISTS page_hashtags (moreID, tags) AS " +
					"SELECT t.moreID, group_concat(DISTINCT(t.tag)) AS tags " +
					"FROM hashtag t GROUP BY t.moreID";

				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				transaction.Rollback();
				logger.WriteLine("error updating table hashtag", exc);
				return 0;
			}

			if (!UpgradeSchemaVersion(cmd, transaction, version))
			{
				return 0;
			}

			try
			{
				transaction.Commit();
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error committing changes for version {version}", exc);
				return 0;
			}

			return version;
		}


		private int Upgrade4to5(SQLiteConnection con)
		{
			int version = 5;
			logger.WriteLine($"upgrading hashtag catalog to version {version}");
			using var indent = logger.Indent();

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;

			using var transaction = con.BeginTransaction();

			try
			{
				if (ColumnExists(con, "hashtag_notebook", "included"))
				{
					logger.WriteLine("table hashtag_notebook already has column included");
				}
				else
				{
					logger.WriteLine("updating table hashtag_notebook");

					cmd.CommandText =
						"ALTER TABLE hashtag_notebook " +
						"ADD COLUMN included INTEGER NOT NULL DEFAULT 1 CHECK(included IN(0, 1))";

					cmd.ExecuteNonQuery();
				}
			}
			catch (Exception exc)
			{
				transaction.Rollback();
				logger.WriteLine("error updating table hashtag_notebook", exc);
				return 0;
			}

			if (!UpgradeSchemaVersion(cmd, transaction, version))
			{
				return 0;
			}

			try
			{
				transaction.Commit();
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error committing changes for version {version}", exc);
				return 0;
			}

			return version;
		}


		// Version 6 identifies a page by its page key from the identity catalog, held as text in
		// the moreID columns, instead of by an omPageID stamp written into the page. Tags recorded
		// under the old stamps will never be matched again, and they are derived from page text, so
		// they are cleared and the scan time reset so the next scan rebuilds them. hashtag_notebook
		// is not touched, so the user's notebook choices survive.
		private int Upgrade5to6(SQLiteConnection con)
		{
			int version = 6;
			logger.WriteLine($"upgrading hashtag catalog to version {version}");
			using var indent = logger.Indent();

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;

			// the add-in and the tray can both open this catalog at once; take the write lock first
			// and check the version again under it, so whichever comes second does not clear the
			// tags the first has already rebuilt
			try
			{
				cmd.CommandText = "BEGIN IMMEDIATE";
				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error starting hashtag upgrade transaction", exc);
				return 0;
			}

			try
			{
				cmd.CommandText = $"SELECT version FROM hashtag_scanner WHERE scannerID = {ScannerID}";
				var current = Convert.ToInt32(cmd.ExecuteScalar());
				if (current != 5)
				{
					logger.WriteLine($"hashtag catalog is already version {current}");
					cmd.CommandText = "ROLLBACK";
					cmd.ExecuteNonQuery();
					return current;
				}

				logger.WriteLine("clearing tags recorded under page stamps");

				cmd.CommandText = "DELETE FROM hashtag";
				cmd.ExecuteNonQuery();

				cmd.CommandText = "DELETE FROM hashtag_page";
				cmd.ExecuteNonQuery();

				cmd.CommandText =
					$"UPDATE hashtag_scanner SET scanTime = '0001-01-01T00:00:00.0000Z' WHERE scannerID = {ScannerID}";

				cmd.ExecuteNonQuery();

				logger.WriteLine($"updating hashtag_scanner version v{version}");
				cmd.CommandText =
					$"UPDATE hashtag_scanner SET version = {version} WHERE scannerID = {ScannerID}";

				cmd.ExecuteNonQuery();

				cmd.CommandText = "COMMIT";
				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error upgrading hashtag catalog to version {version}", exc);

				try
				{
					cmd.CommandText = "ROLLBACK";
					cmd.ExecuteNonQuery();
				}
				catch (Exception rollbackExc)
				{
					// never mask the error that caused the rollback
					logger.WriteLine("error rolling back hashtag upgrade", rollbackExc);
				}

				return 0;
			}

			return version;
		}

		private bool UpgradeSchemaVersion(
			SQLiteCommand cmd, SQLiteTransaction transaction, int version)
		{
			try
			{
				logger.WriteLine($"updating hashtag_scanner version v{version}");
				cmd.CommandText =
					$"UPDATE hashtag_scanner SET version = {version} WHERE scannerID = {ScannerID}";

				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				logger.End();
				logger.WriteLine($"error updating hashtag_scanner version v{version}", exc);
				transaction.Rollback();
				return false;
			}

			return true;
		}
		#endregion UpgradeCatalog


		/// <summary>
		/// Returns the last saved scan time
		/// </summary>
		/// <returns>A collection of Hashtags</returns>
		public string ReadScanTime()
		{
			using var cmd = con.CreateCommand();
			cmd.CommandText =
				$"SELECT version, scanTime FROM hashtag_scanner WHERE scannerID = {ScannerID}";

			try
			{
				using var reader = cmd.ExecuteReader();
				if (reader.Read())
				{
					return reader.GetString(1);
				}
			}
			catch (Exception exc)
			{
				ReportError("error reading last scan time", cmd, exc);
			}

			return DateTime.MinValue.ToZuluString();
		}


		/// <summary>
		/// Returns a list of known notebook IDs scanned thus far that contain tags
		/// </summary>
		/// <returns>A collection of strings</returns>
		public HashtagNotebooks ReadKnownNotebooks()
		{
			var list = new HashtagNotebooks();

			using var cmd = con.CreateCommand();

			cmd.CommandText = "SELECT notebookID, name, included, lastModified FROM hashtag_notebook";

			try
			{
				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					list.Add(new HashtagNotebook
					{
						NotebookID = reader.GetString(0),
						Name = reader.GetString(1),
						Included = reader.GetInt32(2) == 1,
						LastModified = reader.GetString(3)
					});
				}
			}
			catch (Exception exc)
			{
				ReportError("error reading known notebooks", cmd, exc);
			}

			return list;
		}


		/// <summary>
		/// Returns a collection of the latest tag names.
		/// </summary>
		/// <returns>A list of strings</returns>
		public IEnumerable<string> ReadLatestTagNames(
			string notebookID = null, string sectionID = null)
		{
			var tags = new List<string>();
			using var cmd = con.CreateCommand();

			var sql = "SELECT DISTINCT t.tag FROM hashtag t";
			if (!string.IsNullOrWhiteSpace(sectionID))
			{
				sql = $"{sql} JOIN hashtag_page p ON p.moreID = t.moreID AND p.sectionID = @sid";
				cmd.Parameters.AddWithValue("@sid", sectionID);
			}
			else if (!string.IsNullOrWhiteSpace(notebookID))
			{
				sql = $"{sql} JOIN hashtag_page p ON p.moreID = t.moreID AND p.notebookID = @nid";
				cmd.Parameters.AddWithValue("@nid", notebookID);
			}

			sql = $"{sql} ORDER BY t.lastModified DESC LIMIT 5";
			cmd.CommandText = sql;

			try
			{
				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					tags.Add(reader.GetString(0));
				}
			}
			catch (Exception exc)
			{
				ReportError("error reading list of tags", cmd, exc);
			}

			return tags;
		}


		/// <summary>
		/// Returns a collection of tags on the specified page
		/// </summary>
		/// <param name="moreID">The page key, as text</param>
		/// <returns>A collection of Hashtags</returns>
		public Hashtags ReadPageTags(string moreID)
		{
			var sql =
				"SELECT t.tag, t.moreID, p.pageID, p.titleID, t.objectID, " +
				"p.notebookID, p.sectionID, t.lastModified " +
				"FROM hashtag t " +
				"JOIN hashtag_page p ON p.moreID = t.moreID " +
				"WHERE t.moreID = @m " +
				"ORDER BY t.documentOrder";

			return ReadTags(sql,
				new SQLiteParameter[] { new("@m", moreID) }
				);
		}


		/// <summary>
		/// Returns a list of known notebook IDs scanned thus far that contain tags
		/// </summary>
		/// <returns>A collection of strings</returns>
		public List<string> ReadTaggedNotebookIDs()
		{
			var list = new List<string>();

			using var cmd = con.CreateCommand();
			cmd.CommandText =
				"SELECT DISTINCT(notebookID), SUBSTR(path, 0, INSTR(SUBSTR(path,2),'/')+1) " +
				"FROM hashtag_page";

			try
			{
				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					list.Add(reader.GetString(0));
				}
			}
			catch (Exception exc)
			{
				ReportError("error reading tagged notebooks", cmd, exc);
			}

			return list;
		}


		/// <summary>
		/// Returns a collection of all unique tag names.
		/// </summary>
		/// <returns>A list of strings</returns>
		public IEnumerable<string> ReadTagNames(
			string notebookID = null, string sectionID = null, string moreID = null)
		{
			var tags = new List<string>();
			using var cmd = con.CreateCommand();

			var sql = "SELECT DISTINCT t.tag FROM hashtag t";

			if (!string.IsNullOrWhiteSpace(moreID))
			{
				sql = $"{sql} JOIN hashtag_page p ON p.moreID = t.moreID AND t.moreID = @mid";
				cmd.Parameters.AddWithValue("@mid", moreID);
			}
			else if (!string.IsNullOrWhiteSpace(sectionID))
			{
				sql = $"{sql} JOIN hashtag_page p ON p.moreID = t.moreID AND p.sectionID = @sid";
				cmd.Parameters.AddWithValue("@sid", sectionID);
			}
			else if (!string.IsNullOrWhiteSpace(notebookID))
			{
				sql = $"{sql} JOIN hashtag_page p ON p.moreID = t.moreID AND p.notebookID = @nid";
				cmd.Parameters.AddWithValue("@nid", notebookID);
			}

			// Note, no need to ORDER BY here because we're going to .OrderBy() below...

			cmd.CommandText = sql;

			try
			{
				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					tags.Add(reader.GetString(0));
				}
			}
			catch (Exception exc)
			{
				ReportError("error reading list of tag names", cmd, exc);
			}

			return tags.OrderBy(s => s.Replace("#", "").ToLower());
		}


		/// <summary>
		/// Returns a collection of Hashtag instances with the specified name across all pages
		/// </summary>
		/// <param name="criteria">The user-entered search criteria, optional wildcards</param>
		/// <returns>A collection of Hashtags</returns>
		public Hashtags SearchTags(
			string criteria, bool caseSensitive, bool allTags,
			out string parsed,
			string notebookID = null, string sectionID = null, string moreID = null)
		{
			var parameters = new List<SQLiteParameter>();

			var builder = new StringBuilder();
			builder.Append("SELECT t.tag, t.moreID, p.pageID, p.titleID, t.objectID, ");
			builder.Append("p.notebookID, p.sectionID, t.lastModified, t.snippet, ");
			builder.Append("t.documentOrder, p.path, p.name ");
			builder.Append("FROM hashtag t ");
			builder.Append("JOIN hashtag_page p ON t.moreID = p.moreID ");

			if (!string.IsNullOrWhiteSpace(moreID))
			{
				builder.Append("AND t.moreID = @mid ");
				parameters.Add(new("mid", moreID));
			}
			else if (!string.IsNullOrWhiteSpace(sectionID))
			{
				builder.Append("AND p.sectionID = @sid ");
				parameters.Add(new("sid", sectionID));
			}
			else if (!string.IsNullOrWhiteSpace(notebookID))
			{
				builder.Append("AND p.notebookID = @nid ");
				parameters.Add(new("nid", notebookID));
			}

			HashtagQueryBuilder query;
			if (allTags)
			{
				builder.Append("JOIN page_hashtags g ON g.moreID = p.moreID ");
				query = new HashtagQueryBuilder("g.tags", caseSensitive);
			}
			else
			{
				query = new HashtagQueryBuilder("t.tag", caseSensitive);
			}

			var where = query.BuildFormattedWhereClause(criteria, out parsed);
			builder.Append(where);

			builder.Append(" ORDER BY p.path, p.name, t.documentOrder");
			var sql = builder.ToString();

			logger.Verbose(sql);

			var tags = ReadTags(sql, parameters.ToArray(), includeSnippetCols: true);

			// don't highlight everything, otherwise there's no use!
			if (criteria != "*" && criteria != "%")
			{
				// mark direct hits; others are just additional tags on the page
				var pattern = query.GetMatchingPattern(parsed);
				foreach (var tag in tags)
				{
					tag.DirectHit = pattern.IsMatch(tag.Snippet);
				}
			}

			return tags;
		}


		private Hashtags ReadTags(
			string sql, SQLiteParameter[] parameters = null, bool includeSnippetCols = false)
		{
			var tags = new Hashtags();
			using var cmd = con.CreateCommand();
			cmd.CommandText = sql;

			if (parameters != null && parameters.Length > 0)
			{
				cmd.Parameters.AddRange(parameters);
			}

			try
			{
				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var tag = new Hashtag
					{
						Tag = reader.GetString(0),
						MoreID = reader.GetString(1),
						PageID = reader.GetString(2),
						TitleID = reader[3] is DBNull ? null : reader.GetString(3),
						ObjectID = reader.GetString(4),
						NotebookID = reader.GetString(5),
						SectionID = reader.GetString(6),
						LastModified = reader.GetString(7)
					};

					if (includeSnippetCols)
					{
						tag.Snippet = reader[8] is DBNull ? null : reader.GetString(8);
						tag.DocumentOrder = reader[9] is DBNull ? 0 : reader.GetInt32(9);
						tag.HierarchyPath = reader[10] is DBNull ? null : reader.GetString(10);
						tag.PageTitle = reader[11] is DBNull ? null : reader.GetString(11);
					}

					tags.Add(tag);
				}
			}
			catch (Exception exc)
			{
				ReportError("error reading tags", cmd, exc);
			}

			return tags;
		}


		/// <summary>
		/// Records the given tags.
		/// </summary>
		/// <param name="tags">A collection of Hashtags</param>
		public void UpdateSnippet(Hashtags tags)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "UPDATE hashtag " +
				"SET snippet = @c, lastModified = @s " +
				"WHERE tag = @t AND moreID = @m";

			cmd.Parameters.Add("@c", DbType.String);
			cmd.Parameters.Add("@s", DbType.String);
			cmd.Parameters.Add("@t", DbType.String);
			cmd.Parameters.Add("@m", DbType.String);

			using var transaction = con.BeginTransaction();
			foreach (var tag in tags)
			{
				logger.Verbose($"updating tag {tag.Tag}");

				cmd.Parameters["@c"].Value = tag.Snippet;
				cmd.Parameters["@s"].Value = tag.LastModified;
				cmd.Parameters["@t"].Value = tag.Tag;
				cmd.Parameters["@m"].Value = tag.MoreID;

				try
				{

					cmd.ExecuteNonQuery();
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error updating tag {tag.Tag} on {tag.PageID}", exc);
				}
			}

			try
			{
				transaction.Commit();
			}
			catch (Exception exc)
			{
				transaction.Rollback();
				ReportError("error updating snippets", cmd, exc);
			}
		}


		/// <summary>
		/// Reads where every page that has tags is recorded, keyed by page key.
		/// </summary>
		public Dictionary<string, HashtagPageInfo> ReadTaggedPages()
		{
			var pages = new Dictionary<string, HashtagPageInfo>(StringComparer.Ordinal);

			using var cmd = con.CreateCommand();
			cmd.CommandText =
				"SELECT moreID, pageID, titleID, notebookID, sectionID, path, name FROM hashtag_page";

			try
			{
				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var info = new HashtagPageInfo
					{
						MoreID = reader.GetString(0),
						PageID = reader.GetString(1),
						TitleID = reader[2] is DBNull ? null : reader.GetString(2),
						NotebookID = reader.GetString(3),
						SectionID = reader.GetString(4),
						Path = reader[5] is DBNull ? null : reader.GetString(5),
						Name = reader[6] is DBNull ? null : reader.GetString(6)
					};

					pages[info.MoreID] = info;
				}
			}
			catch (Exception exc)
			{
				ReportError("error reading tagged pages", cmd, exc);
			}

			return pages;
		}


		/// <summary>
		/// Updates where pages with tags are recorded, after a notebook was reopened or a page was
		/// moved or renamed. The tags themselves are untouched, and so is the title paragraph ID,
		/// which only a scan of the page can supply.
		/// </summary>
		/// <returns>The number of pages updated</returns>
		public int RefreshPageInfo(IReadOnlyCollection<HashtagPageInfo> pages)
		{
			if (pages.Count == 0)
			{
				return 0;
			}

			using var transaction = con.BeginTransaction();

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText =
				"UPDATE hashtag_page " +
				"SET pageID = @pid, notebookID = @nid, sectionID = @sid, path = @pth, name = @nam " +
				"WHERE moreID = @mid";

			cmd.Parameters.Add("@pid", DbType.String);
			cmd.Parameters.Add("@nid", DbType.String);
			cmd.Parameters.Add("@sid", DbType.String);
			cmd.Parameters.Add("@pth", DbType.String);
			cmd.Parameters.Add("@nam", DbType.String);
			cmd.Parameters.Add("@mid", DbType.String);

			var count = 0;

			try
			{
				foreach (var page in pages)
				{
					cmd.Parameters["@pid"].Value = page.PageID;
					cmd.Parameters["@nid"].Value = page.NotebookID;
					cmd.Parameters["@sid"].Value = page.SectionID;
					cmd.Parameters["@pth"].Value = page.Path;
					cmd.Parameters["@nam"].Value = page.Name;
					cmd.Parameters["@mid"].Value = page.MoreID;
					count += cmd.ExecuteNonQuery();
				}

				transaction.Commit();
			}
			catch (Exception exc)
			{
				transaction.Rollback();
				ReportError("error refreshing page info", cmd, exc);
				return 0;
			}

			return count;
		}


		/// <summary>
		/// Deletes all tags, and the page record, of the given pages, such as pages the identity
		/// catalog has purged because they were missing for too long.
		/// </summary>
		/// <param name="moreIDs">Page keys, as text</param>
		/// <returns>The number of pages that had tags</returns>
		public int DeleteTags(IEnumerable<string> moreIDs)
		{
			var keys = moreIDs.ToList();
			if (keys.Count == 0)
			{
				return 0;
			}

			using var transaction = con.BeginTransaction();

			// PRAGMA foreign_keys is not enabled, so the cascade does not fire; delete the tags first
			using var tagcmd = con.CreateCommand();
			tagcmd.CommandText = "DELETE FROM hashtag WHERE moreID = @m";
			tagcmd.Parameters.Add("@m", DbType.String);

			using var pagcmd = con.CreateCommand();
			pagcmd.CommandText = "DELETE FROM hashtag_page WHERE moreID = @m";
			pagcmd.Parameters.Add("@m", DbType.String);

			var count = 0;

			try
			{
				foreach (var key in keys)
				{
					tagcmd.Parameters["@m"].Value = key;
					pagcmd.Parameters["@m"].Value = key;
					tagcmd.ExecuteNonQuery();
					count += pagcmd.ExecuteNonQuery();
				}

				transaction.Commit();
			}
			catch (Exception exc)
			{
				transaction.Rollback();
				ReportError("error deleting tags of purged pages", tagcmd, exc);
				return 0;
			}

			return count;
		}


		/// <summary>
		/// Brings the known notebooks up to date after OneNote regenerated notebook IDs, by
		/// matching a notebook that is now open to a recorded notebook of the same name whose ID is
		/// no longer open. The user's choice to exclude a notebook always survives: if either record
		/// excluded it, the merged record is excluded.
		/// </summary>
		/// <param name="open">The ID and name of every notebook that is open now</param>
		/// <returns>The number of recorded notebooks that were merged or renumbered</returns>
		/// <remarks>
		/// A name that more than one open notebook has is skipped, because it cannot say which
		/// recorded notebook belongs to which. Recorded notebooks that are not open are kept, so a
		/// closed notebook is recognized when it is opened again.
		/// </remarks>
		public int ReconcileNotebooks(IReadOnlyList<(string ID, string Name)> open)
		{
			var recorded = new List<(string ID, string Name, int Included, string LastModified)>();

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT notebookID, name, included, lastModified FROM hashtag_notebook";

			try
			{
				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					recorded.Add((
						reader.GetString(0),
						reader[1] is DBNull ? string.Empty : reader.GetString(1),
						reader.GetInt32(2),
						reader.GetString(3)));
				}
			}
			catch (Exception exc)
			{
				ReportError("error reading notebooks to reconcile", cmd, exc);
				return 0;
			}

			var openIDs = new HashSet<string>(open.Select(o => o.ID), StringComparer.Ordinal);
			var plan = new List<(string ID, string Name, int Included, string LastModified, List<string> Remove)>();

			foreach (var notebook in open)
			{
				if (open.Count(o => string.Equals(o.Name, notebook.Name, StringComparison.OrdinalIgnoreCase)) != 1)
				{
					continue;
				}

				var stale = recorded
					.Where(r => !openIDs.Contains(r.ID) &&
						string.Equals(r.Name, notebook.Name, StringComparison.OrdinalIgnoreCase))
					.ToList();

				if (stale.Count == 0)
				{
					continue;
				}

				var current = recorded.FindIndex(r => r.ID == notebook.ID);

				// exclusion wins, and keep the latest scan time so a reopened notebook is not
				// treated as never scanned
				var included = stale.Min(r => r.Included);
				var lastModified = stale.Max(r => r.LastModified);
				if (current >= 0)
				{
					included = Math.Min(included, recorded[current].Included);
					if (string.CompareOrdinal(recorded[current].LastModified, lastModified) > 0)
					{
						lastModified = recorded[current].LastModified;
					}
				}

				plan.Add((notebook.ID, notebook.Name, included, lastModified,
					stale.Select(r => r.ID).ToList()));
			}

			if (plan.Count == 0)
			{
				return 0;
			}

			using var transaction = con.BeginTransaction();

			var changed = 0;

			try
			{
				foreach (var item in plan)
				{
					cmd.Parameters.Clear();

					cmd.CommandText = "DELETE FROM hashtag_notebook WHERE notebookID = @nid";
					cmd.Parameters.Add("@nid", DbType.String);
					foreach (var id in item.Remove)
					{
						cmd.Parameters["@nid"].Value = id;
						changed += cmd.ExecuteNonQuery();
					}

					cmd.Parameters.Clear();
					cmd.CommandText =
						"INSERT INTO hashtag_notebook (notebookID, name, included, lastModified) " +
						"VALUES (@nid, @nam, @inc, @mod) " +
						"ON CONFLICT(notebookID) DO UPDATE SET " +
						"name = @nam, included = @inc, lastModified = @mod";

					cmd.Parameters.AddWithValue("@nid", item.ID);
					cmd.Parameters.AddWithValue("@nam", item.Name);
					cmd.Parameters.AddWithValue("@inc", item.Included);
					cmd.Parameters.AddWithValue("@mod", item.LastModified);
					cmd.ExecuteNonQuery();
				}

				transaction.Commit();
			}
			catch (Exception exc)
			{
				transaction.Rollback();
				ReportError("error reconciling notebooks", cmd, exc);
				return 0;
			}

			return changed;
		}

		/// <summary>
		/// Records a notebook instance; used to capture "known" notebooks
		/// </summary>
		public void WriteNotebook(string notebookID, string name, bool modified)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandText = "SELECT lastModified FROM hashtag_notebook WHERE notebookID = @nid";
			cmd.Parameters.AddWithValue("@nid", notebookID);

			try
			{
				var lastModified = string.Empty;
				if (modified)
				{
					lastModified = DateTime.Now.ToZuluString();
				}
				else
				{
					using var reader = cmd.ExecuteReader();
					while (reader.Read())
					{
						lastModified = reader.GetString(0);
					}
				}

				cmd.CommandText =
					"INSERT INTO hashtag_notebook (notebookID, name, included, lastModified) " +
					"VALUES (@nid, @nam, 1, @mod) " +
					"ON CONFLICT(notebookID) DO UPDATE SET name = @nam, lastModified = @mod";

				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@nid", notebookID);
				cmd.Parameters.AddWithValue("@nam", name);
				cmd.Parameters.AddWithValue("@mod", lastModified);

				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				ReportError("error writing notebook", cmd, exc);
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="notebookID"></param>
		/// <param name="included"></param>
		public void WriteNotebookInclusion(string notebookID, string name, bool included)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandText =
				"INSERT INTO hashtag_notebook (notebookID, name, included, lastModified) " +
				"VALUES (@nid, @nam, @inc, '') " +
				"ON CONFLICT(notebookID) DO UPDATE SET included = @inc";
			cmd.Parameters.AddWithValue("@nid", notebookID);
			cmd.Parameters.AddWithValue("@nam", name);
			cmd.Parameters.AddWithValue("@inc", included ? 1 : 0);

			try
			{
				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				ReportError("error updating notebook", cmd, exc);
			}
		}


		/// <summary>
		/// Records given info for the specified page.
		/// </summary>
		/// <param name="moreID">The assigned ID to the page</param>
		/// <param name="pageID">The OneNote ID of the page</param>
		/// <param name="titleID">The OneNote ID of the title paragraph</param>
		/// <param name="notebookID">The ID of the notebook</param>
		/// <param name="sectionID">The ID of the section</param>
		/// <param name="path">The hierarchy path of the page</param>
		/// <param name="title">The title (name) of the page</param>
		public void WritePageInfo(
			string moreID, string pageID, string titleID,
			string notebookID, string sectionID, string path, string title)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandText = "REPLACE INTO hashtag_page " +
				"(moreID, pageID, titleID, notebookID, sectionID, path, name) " +
				"VALUES (@mid, @pid, @tid, @nid, @sid, @pth, @nam)";

			cmd.Parameters.AddWithValue("@mid", moreID);
			cmd.Parameters.AddWithValue("@pid", pageID);
			cmd.Parameters.AddWithValue("@tid", titleID);
			cmd.Parameters.AddWithValue("@nid", notebookID);
			cmd.Parameters.AddWithValue("@sid", sectionID);
			cmd.Parameters.AddWithValue("@pth", path);
			cmd.Parameters.AddWithValue("@nam", title);

			try
			{
				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				ReportError("error writing page info", cmd, exc);
			}
		}


		/// <summary>
		/// Records the given timestamp as the time of the most recently completed scan
		/// </summary>
		/// <param name="timestamp">
		/// The Zulu-formatted timestamp captured at the start of the scan cycle
		/// </param>
		public void WriteScanTime(string timestamp)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandText = "UPDATE hashtag_scanner SET scanTime = @d WHERE scannerID = 0";
			cmd.Parameters.AddWithValue("@d", timestamp);

			try
			{
				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				ReportError("error writing scan time", cmd, exc);
			}
		}


		/// <summary>
		/// Replaces all of the tags recorded for a page with the given tags.
		/// </summary>
		/// <param name="moreID">The page key, as text</param>
		/// <param name="tags">A collection of Hashtags</param>
		public bool WriteTags(string moreID, Hashtags tags)
		{
			using var transaction = con.BeginTransaction();

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;

			// first purge all existing tags for page...

			cmd.CommandText = "DELETE FROM hashtag WHERE moreID = @m";

			cmd.Parameters.AddWithValue("@m", moreID);

			try
			{
				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				transaction.Rollback();
				logger.WriteLine($"error deleting tags {moreID}", exc);
				return false;
			}

			// now add (re-add) newly discovered tags for page, reestablishing doc order...

			if (tags.Any())
			{
				cmd.CommandText = "INSERT INTO hashtag " +
					"(tag, moreID, objectID, snippet, documentOrder, lastModified) " +
					"VALUES (@t, @m, @o, @c, @d, @s)";

				cmd.Parameters.Clear();
				cmd.Parameters.Add("@t", DbType.String);
				cmd.Parameters.Add("@m", DbType.String);
				cmd.Parameters.Add("@o", DbType.String);
				cmd.Parameters.Add("@c", DbType.String);
				cmd.Parameters.Add("@d", DbType.Int32);
				cmd.Parameters.Add("@s", DbType.String);

				foreach (var tag in tags)
				{
					logger.Verbose($"writing tag {tag.Tag}");

					cmd.Parameters["@t"].Value = tag.Tag;
					cmd.Parameters["@m"].Value = tag.MoreID;
					cmd.Parameters["@o"].Value = tag.ObjectID;
					cmd.Parameters["@c"].Value = tag.Snippet;
					cmd.Parameters["@d"].Value = tag.DocumentOrder;
					cmd.Parameters["@s"].Value = tag.LastModified;

					try
					{
						cmd.ExecuteNonQuery();
					}
					catch (Exception exc)
					{
						logger.WriteLine($"error writing tag {tag.Tag} on {tag.PageID}");
						logger.WriteLine($"error moreID=[{tag.MoreID}]");
						logger.WriteLine($"error objectID=[{tag.ObjectID}]");
						logger.WriteLine($"error Snippet=[{tag.Snippet}]");
						logger.WriteLine($"error lastModified=[{tag.LastModified}]");
						logger.WriteLine(exc);
						transaction.Rollback();
						return false;
					}
				}
			}

			try
			{
				transaction.Commit();
			}
			catch (Exception exc)
			{
				transaction.Rollback();
				ReportError("error writing tags", cmd, exc);
				return false;
			}

			CleanupPages();
			return true;
		}


		private void CleanupPages()
		{
			// as tags are deleted from a page, that page may be left dangling in the
			// hashtag_page table; this cleans up those orphaned records

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "DELETE FROM hashtag_page WHERE moreID IN (" +
				"SELECT P.moreID " +
				"FROM hashtag_page P " +
				"LEFT OUTER JOIN hashtag T " +
				"ON T.moreID = P.moreID WHERE T.tag IS NULL)";

			try
			{
				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				ReportError("error cleaning up pages", cmd, exc);
			}
		}
	}
}
