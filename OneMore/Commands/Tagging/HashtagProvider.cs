//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S1133 // Deprecated code should be removed

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Properties;
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SQLite;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;


	/// <summary>
	/// Reads, Writes, manages a data store of Hashtags.
	/// </summary>
	internal class HashtagProvider : Loggable, IDisposable
	{
		private const int ScannerID = 0;

		private readonly string timestamp;

		private SQLiteConnection con;
		private bool disposed;


		/// <summary>
		/// Initialize this provider, opening the standard database
		/// </summary>
		public HashtagProvider()
		{
			if (CatalogExists())
			{
				UpgradeCatalog();
			}
			else
			{
				RefreshCatalog();
			}

			timestamp = DateTime.Now.ToZuluString();
		}


		public static bool CatalogExists()
		{
			var path = Path.Combine(
				PathHelper.GetAppDataPath(), Resources.DatabaseFilename);

			if (!File.Exists(Path.Combine(path)))
			{
				return false;
			}

			var con = new SQLiteConnection($"Data source={path}");
			con.Open();

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT COUNT(1) FROM sqlite_master " +
				"WHERE type = 'table' AND name = 'hashtag_scanner'";

			var count = 0;
			try
			{
				using var reader = cmd.ExecuteReader();
				if (reader.Read())
				{
					count = reader.GetInt32(0);
				}
			}
			catch (Exception exc)
			{
				ReportError("error reading scanner version", cmd, exc);
				return false;
			}

			return count > 0;
		}


		private void OpenDatabase()
		{
			if (con is null)
			{
				var path = Path.Combine(
					PathHelper.GetAppDataPath(), Resources.DatabaseFilename);

				con = new SQLiteConnection($"Data source={path}");
				con.Open();
			}
		}


		public bool DropCatalog()
		{
			int Drop(string type, IEnumerable<string> names)
			{
				using var cmd = con.CreateCommand();
				cmd.CommandType = CommandType.Text;

				var count = 0;

				foreach (var name in names)
				{
					logger.WriteLine($"dropping {type} {name}");

					// Cannot use named parameters here because they would be interpreted as
					// literal quoted strings rather than direct names like @myview. So use
					// dynamic SQL. The possibility of SQL injection is quite low because
					// these names come from an embedded resx in this assembly.
					cmd.CommandText = $"DROP {type} IF EXISTS {name}";

					try
					{
						cmd.ExecuteNonQuery();
						count++;
					}
					catch (Exception exc)
					{
						ReportError($"error dropping {type} {name}", cmd, exc);
					}
				}

				return count;
			}

			var path = Path.Combine(
				PathHelper.GetAppDataPath(), Resources.DatabaseFilename);

			if (!File.Exists(path))
			{
				return true;
			}

			var pattern = new Regex(@"CREATE ([^\s]+) IF NOT EXISTS ([^\s]+)",
				RegexOptions.Compiled);

			var entities = Regex.Split(Resources.HashtagsDB, @"\r\n|\n\r|\n")
				.AsEnumerable()
				.Select(d => pattern.Match(d))
				.Where(m => m.Success)
				.Select(m => (m.Groups[1].Value, m.Groups[2].Value));

			if (!entities.Any())
			{
				return true;
			}

			using var transaction = con.BeginTransaction();
			var count = 0;

			IEnumerable<(string, string)> list;
			list = entities.Where(e => e.Item1 == "VIEW");
			if (list.Any())
			{
				count += Drop("view", list.Select(e => e.Item2).ToList());
			}

			list = entities.Where(e => e.Item1 == "INDEX");
			if (list.Any())
			{
				count += Drop("index", list.Select(e => e.Item2).ToList());
			}

			list = entities.Where(e => e.Item1 == "TABLE");
			if (list.Any())
			{
				// there is one foreign key but tables will be dropped in the right order
				count += Drop("table", list.Select(e => e.Item2).ToList());
			}

			if (count != entities.Count())
			{
				logger.WriteLine("error dropping hashtag catalog, see errors above");
				return false;
			}

			try
			{
				transaction.Commit();

				// vacuum must be done outside a transaction
				using var cmd = con.CreateCommand();
				cmd.CommandText = "VACUUM";
				cmd.ExecuteNonQuery();

				logger.WriteLine("hashtag catalog drop done");
			}
			catch (Exception exc)
			{
				logger.WriteLine("error dropping catalog", exc);
				transaction.Rollback();
				return false;
			}

			return true;
		}


		private void RefreshCatalog()
		{
			logger.WriteLine("building hashtag catalog");

			OpenDatabase();

			using var transaction = con.BeginTransaction();

			var ddl = Regex.Split(Resources.HashtagsDB, @"\r\n|\n\r|\n");
			foreach (var line in ddl)
			{
				var sql = line.Trim();
				if (!string.IsNullOrWhiteSpace(sql) && !sql.StartsWith("--"))
				{
					using var cmd = con.CreateCommand();

					try
					{
						cmd.CommandText = sql;
						cmd.CommandType = CommandType.Text;
						cmd.ExecuteNonQuery();
					}
					catch (Exception exc)
					{
						ReportError("error building catalog", cmd, exc);
						throw;
					}
				}
			}

			try
			{
				transaction.Commit();
				logger.WriteLine("hashtag catalog done");
			}
			catch (Exception exc)
			{
				logger.WriteLine("error building db", exc);
				throw;
			}
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
		}


		private int Upgrade1to2(SQLiteConnection con)
		{
			var version = 2;
			logger.WriteLine($"upgrading hashtag catalog to version {version}");
			logger.Start();

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
				logger.End();
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
				logger.End();
				logger.WriteLine($"error committing changes for version {version}", exc);
				return 0;
			}

			logger.End();
			return version;
		}


		private int Upgrade2to3(SQLiteConnection con)
		{
			var version = 3;
			logger.WriteLine($"upgrading hashtag catalog to version {version}");
			logger.Start();

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
				logger.End();
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
				logger.End();
				logger.WriteLine($"error committing changes for version {version}", exc);
				return 0;
			}

			logger.End();
			return version;
		}


		private int Upgrade3to4(SQLiteConnection con)
		{
			int version = 4;
			logger.WriteLine($"upgrading hashtag catalog to version {version}");
			logger.Start();

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
				logger.End();
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
				logger.End();
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
				logger.End();
				logger.WriteLine($"error committing changes for version {version}", exc);
				return 0;
			}

			logger.End();
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


		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					con?.Dispose();
				}

				disposed = true;
			}
		}


		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// Deletes pages that used to have tags but no longer do by comparing the recorded
		/// pages against the list of knownIDs and deleting any records no longer in that list.
		/// </summary>
		/// <param name="knownIDs"></param>
		public void DeletePhantoms(List<string> knownIDs, string sectionID, string sectionPath)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT moreID, pageID FROM hashtag_page WHERE sectionID = @sid";
			cmd.Parameters.AddWithValue("@sid", sectionID);

			using var tagcmd = con.CreateCommand();
			tagcmd.CommandType = CommandType.Text;
			tagcmd.CommandText =
				"DELETE FROM hashtag WHERE moreID IN " +
				"(SELECT DISTINCT moreID FROM hashtag_page WHERE pageID = @pid)";
			tagcmd.Parameters.Add("@pid", DbType.String);

			using var pagcmd = con.CreateCommand();
			pagcmd.CommandType = CommandType.Text;
			pagcmd.CommandText = "DELETE FROM hashtag_page WHERE pageID = @pid";
			pagcmd.Parameters.Add("@pid", DbType.String);

			using var transaction = con.BeginTransaction();
			var count = 0;

			try
			{
				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var pageID = reader.GetString(1);
					if (!knownIDs.Contains(pageID))
					{
						tagcmd.Parameters["@pid"].Value = pageID;
						tagcmd.ExecuteNonQuery();

						pagcmd.Parameters["@pid"].Value = pageID;
						pagcmd.ExecuteNonQuery();
						count++;
					}
				}

				if (count > 0)
				{
					transaction.Commit();
					logger.WriteLine($"deleted {count} phantom pages from {sectionPath}");
				}
			}
			catch (Exception exc)
			{
				transaction.Rollback();
				logger.WriteLine("error deleting phantom pages", exc);
			}
		}


		/// <summary>
		/// Deletes the specified tags
		/// </summary>
		/// <param name="tags">A collection of Hashtags</param>
		[Obsolete("Was used as part of original tag resolution logic")]
		public void DeleteTags(Hashtags tags)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandText = "DELETE FROM hashtag WHERE tag = @t AND moreID = @m";
			cmd.CommandType = CommandType.Text;
			cmd.Parameters.Add("@t", DbType.String);
			cmd.Parameters.Add("@m", DbType.String);

			using var transaction = con.BeginTransaction();
			foreach (var tag in tags)
			{
				logger.Verbose($"deleting tag {tag.Tag}");

				cmd.Parameters["@t"].Value = tag.Tag;
				cmd.Parameters["@m"].Value = tag.MoreID;

				try
				{
					cmd.ExecuteNonQuery();
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error deleting tag {tag.Tag} on {tag.MoreID}", exc);
				}
			}

			try
			{
				transaction.Commit();

				CleanupPages();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error deleting tags", exc);
			}
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

			cmd.CommandText = "SELECT notebookID, name, lastModified FROM hashtag_notebook";

			try
			{
				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					list.Add(new HashtagNotebook
					{
						NotebookID = reader.GetString(0),
						Name = reader.GetString(1),
						LastModified = reader.GetString(2)
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
		/// <param name="pageID">The ID of the page</param>
		/// <returns>A collection of Hashtags</returns>
		public Hashtags ReadPageTags(string pageID)
		{
			var sql =
				"SELECT t.tag, t.moreID, p.pageID, p.titleID, t.objectID, " +
				"p.notebookID, p.sectionID, t.lastModified " +
				"FROM hashtag t " +
				"JOIN hashtag_page p ON p.moreID = t.moreID " +
				"WHERE p.pageID = @p " +
				"ORDER BY t.documentOrder";

			return ReadTags(sql,
				new SQLiteParameter[] { new("@p", pageID) }
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

			sql = $"{sql} ORDER BY 1";
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
			string criteria, bool caseSensitive,
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

			builder.Append("JOIN page_hashtags g ON g.moreID = p.moreID ");

			var query = new HashtagQueryBuilder("g.tags", caseSensitive);
			var where = query.BuildFormattedWhereClause(criteria, out parsed);
			builder.Append(where);

			builder.Append(" ORDER BY p.path, p.name, t.documentOrder");
			var sql = builder.ToString();

			logger.Verbose(sql);

			var tags = ReadTags(sql, parameters.ToArray());

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


		private Hashtags ReadTags(string sql, SQLiteParameter[] parameters = null)
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

					if (reader.FieldCount > 7 && sql.Contains("snippet"))
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
		/// Determines if the moreID matches the pageID, otherwise this might be coming
		/// from a new duplicate or copy of an existing page so need to generate a new moreID
		/// </summary>
		/// <param name="pageID"></param>
		/// <param name="moreID"></param>
		/// <returns></returns>
		public bool UniqueMoreID(string pageID, string moreID)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT count(1) " +
				"FROM hashtag_page WHERE moreID = @mid AND pageID <> @pid";

			cmd.Parameters.AddWithValue("@mid", moreID);
			cmd.Parameters.AddWithValue("@pid", pageID);

			var unique = false;
			try
			{
				using var reader = cmd.ExecuteReader();
				if (reader.Read())
				{
					var count = reader.GetInt32(0);
					unique = count == 0;
				}
			}
			catch (Exception exc)
			{
				ReportError("error validating moreID", cmd, exc);
				return false;
			}

			return unique;
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
				ReportError("error updating snippets", cmd, exc);
			}
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

				cmd.CommandText = "REPLACE INTO hashtag_notebook " +
					"(notebookID, name, lastModified) VALUES (@nid, @nam, @mod)";

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
		/// Records the timestamp value that was initialized at construction of this class
		/// instance
		/// </summary>
		public void WriteScanTime()
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
		/// Records the given tags.
		/// </summary>
		/// <param name="tags">A collection of Hashtags</param>
		public void WriteTags(string pageID, Hashtags tags)
		{
			using var transaction = con.BeginTransaction();

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;

			// first purge all existing tags for page...

			cmd.CommandText = "DELETE FROM HASHTAG WHERE moreID = " +
				"(SELECT moreID FROM hashtag_page WHERE pageID = @p);";

			cmd.Parameters.AddWithValue("@p", pageID);

			try
			{
				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				transaction.Rollback();
				logger.WriteLine($"error deleting tags {pageID}", exc);
				return;
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
					}
				}
			}

			CleanupPages();

			try
			{
				transaction.Commit();
			}
			catch (Exception exc)
			{
				ReportError("error writing tags", cmd, exc);
			}
		}


		private static void ReportError(string msg, SQLiteCommand cmd, Exception exc)
		{
			// provider currently only deals with strings as input so quote everything...

			var sql = Regex.Replace(cmd.CommandText, "@[a-z]+",
				m => cmd.Parameters.Contains(m.Value)
					? $"'{cmd.Parameters[m.Value].Value}'"
					: m.Value
			);

			var logger = Logger.Current;
			logger.WriteLine(msg);
			logger.WriteLine(sql);
			logger.WriteLine(exc);
		}
	}
}
