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

		private readonly string timestamp;


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

			timestamp = DateTime.Now.ToZuluString();
		}


		public static bool CatalogExists()
		{
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


		private int Upgrade4to5(SQLiteConnection con)
		{
			int version = 5;
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
					"ADD COLUMN included INTEGER NOT NULL DEFAULT 1 CHECK(included IN(0, 1))";

				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				transaction.Rollback();
				logger.End();
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


		/// <summary>
		/// Deletes pages that used to have tags but no longer do by comparing the recorded
		/// pages against the list of knownIDs and deleting any records no longer in that list.
		/// </summary>
		/// <param name="knownIDs"></param>
		public void DeletePhantoms(List<string> knownIDs, string sectionID, string sectionPath)
		{
			// HashSet for O(1) membership vs O(n) List.Contains
			var knownSet = new HashSet<string>(knownIDs, StringComparer.Ordinal);

			// Phase 1: identify phantoms without holding a write transaction
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT moreID, pageID FROM hashtag_page WHERE sectionID = @sid";
			cmd.Parameters.AddWithValue("@sid", sectionID);

			var phantomIDs = new List<string>();
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var pageID = reader.GetString(1);
					if (!knownSet.Contains(pageID))
					{
						phantomIDs.Add(pageID);
					}
				}
			}

			if (phantomIDs.Count == 0)
			{
				return;
			}

			// Phase 2: two batch DELETEs — 2 round-trips regardless of N
			var paramNames = string.Join(",",
				Enumerable.Range(0, phantomIDs.Count).Select(i => $"@p{i}"));

			using var tagcmd = con.CreateCommand();
			tagcmd.CommandType = CommandType.Text;
			tagcmd.CommandText =
				"DELETE FROM hashtag WHERE moreID IN " +
				$"(SELECT DISTINCT moreID FROM hashtag_page WHERE pageID IN ({paramNames}))";

			using var pagcmd = con.CreateCommand();
			pagcmd.CommandType = CommandType.Text;
			pagcmd.CommandText = $"DELETE FROM hashtag_page WHERE pageID IN ({paramNames})";

			// SQLite's default SQLITE_MAX_VARIABLE_NUMBER is 32766. Even a pathological section with
			// "hundreds" of stale pages is well within that limit. No chunking is needed.

			for (var i = 0; i < phantomIDs.Count; i++)
			{
				tagcmd.Parameters.AddWithValue($"@p{i}", phantomIDs[i]);
				pagcmd.Parameters.AddWithValue($"@p{i}", phantomIDs[i]);
			}

			// PRAGMA foreign_keys is not enabled in DatabaseProvider.cs, so cascade delete does
			// not fire automatically.Deleting hashtag rows before hashtag_page rows(the current
			// order) must be preserved.          
				
			using var transaction = con.BeginTransaction();
			try
			{
				tagcmd.ExecuteNonQuery();
				pagcmd.ExecuteNonQuery();
				transaction.Commit();
				logger.WriteLine($"deleted {phantomIDs.Count} phantom pages from {sectionPath}");
			}
			catch (Exception exc)
			{
				transaction.Rollback();
				logger.WriteLine("error deleting phantom pages", exc);
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
				transaction.Rollback();
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
		/// 
		/// </summary>
		/// <param name="notebookID"></param>
		/// <param name="included"></param>
		public void WriteNotebookInclusion(string notebookID, bool included)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandText = "REPLACE INTO hashtag_notebook " +
				"(notebookID, name, lastModified) VALUES (@nid, @nam, @mod)";

			cmd.Parameters.Clear();
			cmd.Parameters.AddWithValue("@nid", notebookID);
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
		public bool WriteTags(string pageID, Hashtags tags)
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
