//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

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

		private readonly SQLiteConnection con;
		private readonly string timestamp;
		private bool disposed;


		/// <summary>
		/// Initialize this provider, opening the standard database
		/// </summary>
		public HashtagProvider()
		{
			var path = Path.Combine(
				PathHelper.GetAppDataPath(), Resources.DatabaseFilename);

			var refresh = !File.Exists(path);

			con = new SQLiteConnection($"Data source={path}");
			con.Open();

			if (refresh)
			{
				RefreshDatabase();
			}
			else
			{
				UpgradeDatabase();
			}

			timestamp = DateTime.Now.ToZuluString();
		}


		public static bool DatabaseExists()
		{
			return File.Exists(Path.Combine(
				PathHelper.GetAppDataPath(), Resources.DatabaseFilename));
		}


		public static void DeleteDatabase()
		{
			var path = Path.Combine(
				PathHelper.GetAppDataPath(), Resources.DatabaseFilename);

			if (File.Exists(path))
			{
				try
				{
					Logger.Current.WriteLine("deleting hashtag database");
					File.Delete(path);
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine("error deleting hashtag database", exc);
				}
			}
		}


		public bool DropDatabase()
		{
			int Drop(string type, IEnumerable<string> names)
			{
				using var cmd = con.CreateCommand();
				cmd.CommandText = $"DROP {type} IF EXISTS @n";
				cmd.CommandType = CommandType.Text;
				var count = 0;

				foreach (var name in names)
				{
					try
					{
						logger.WriteLine($"dropping {type} {name}");
						cmd.Parameters["@n"].Value = name;
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
				count += Drop("view", list.Select(e => e.Item2));
			}

			list = entities.Where(e => e.Item1 == "INDEX");
			if (list.Any())
			{
				count += Drop("index", list.Select(e => e.Item2));
			}

			list = entities.Where(e => e.Item1 == "TABLE");
			if (list.Any())
			{
				// there is one foreign key but tables will be dropped in the right order
				count += Drop("table", list.Select(e => e.Item2));
			}

			if (count != entities.Count())
			{
				logger.WriteLine("error dropping hashtag database, see errors above");
				return false;
			}

			try
			{
				using var cmd = con.CreateCommand();
				cmd.CommandText = "VACUUM";
				cmd.ExecuteNonQuery();

				transaction.Commit();
				logger.WriteLine("hashtag database drop done");
			}
			catch (Exception exc)
			{
				logger.WriteLine("error dropping db", exc);
				transaction.Rollback();
				return false;
			}

			return true;
		}


		private void RefreshDatabase()
		{
			logger.WriteLine("building hashtag database");
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
						ReportError("error building database", cmd, exc);
						throw;
					}
				}
			}

			try
			{
				transaction.Commit();
				logger.WriteLine("hashtag database done");
			}
			catch (Exception exc)
			{
				logger.WriteLine("error building db", exc);
				throw;
			}
		}


		#region UpgradeDatabase
		private void UpgradeDatabase()
		{
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
		}


		private int Upgrade1to2(SQLiteConnection con)
		{
			logger.WriteLine("upgrading database to version 2");
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

			if (!UpgradeSchemaVersion(cmd, transaction, 2))
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
				logger.WriteLine("error committing changes for version 2", exc);
				return 0;
			}

			logger.End();

			// new version
			return 2;
		}

		private int Upgrade2to3(SQLiteConnection con)
		{
			logger.WriteLine("upgrading database to version 3");
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

			if (!UpgradeSchemaVersion(cmd, transaction, 3))
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
				logger.WriteLine("error committing changes for version 3", exc);
				return 0;
			}

			logger.End();

			// new version
			return 3;
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
		#endregion UpgradeDatabase


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
		public List<string> ReadKnownNotebookIDs()
		{
			var list = new List<string>();

			using var cmd = con.CreateCommand();
			cmd.CommandText = "SELECT notebookID FROM hashtag_notebook";

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

			sql = $"{sql} ORDER BY t.lastModified LIMIT 5";
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
				"WHERE p.pageID = @p";

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
		public IEnumerable<string> ReadTagNames(string notebookID = null, string sectionID = null)
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

			return tags;
		}


		/// <summary>
		/// Returns a collection of Hashtag instances with the specified name across all pages
		/// </summary>
		/// <param name="criteria">The user-entered search criteria, optional wildcards</param>
		/// <returns>A collection of Hashtags</returns>
		public Hashtags SearchTags(
			string criteria, out string parsed, string notebookID = null, string sectionID = null)
		{
			var parameters = new List<SQLiteParameter>();

			var builder = new StringBuilder();
			builder.Append("SELECT t.tag, t.moreID, p.pageID, p.titleID, t.objectID, ");
			builder.Append("p.notebookID, p.sectionID, t.lastModified, t.snippet, p.path, p.name ");
			builder.Append("FROM hashtag t ");
			builder.Append("JOIN hashtag_page p ON t.moreID = p.moreID ");

			if (!string.IsNullOrWhiteSpace(sectionID))
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

			var query = new HashtagQueryBuilder("g.tags");
			var where = query.BuildFormattedWhereClause(criteria, out parsed);
			builder.Append(where);

			builder.Append(" ORDER BY p.path, p.name, t.tag");
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
						tag.HierarchyPath = reader[9] is DBNull ? null : reader.GetString(9);
						tag.PageTitle = reader[10] is DBNull ? null : reader.GetString(10);
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
			cmd.CommandText = "SELECT count(1) " +
				"FROM hashtag_page WHERE moreID = @mid AND pageID <> @pid";

			cmd.CommandType = CommandType.Text;
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
			cmd.CommandText = "UPDATE hashtag " +
				"SET snippet = @c, lastModified = @s " +
				"WHERE tag = @t AND moreID = @m";

			cmd.CommandType = CommandType.Text;
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
		public void WriteNotebook(string notebookID, string name)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandText = "REPLACE INTO hashtag_notebook " +
				"(notebookID, name) VALUES (@nid, @nam)";

			cmd.Parameters.AddWithValue("@nid", notebookID);
			cmd.Parameters.AddWithValue("@nam", name);

			try
			{
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
		public void WriteTags(Hashtags tags)
		{
			using var tagcmd = con.CreateCommand();
			tagcmd.CommandText = "INSERT INTO hashtag " +
				"(tag, moreID, objectID, snippet, lastModified) VALUES (@t, @m, @o, @c, @s)";

			tagcmd.CommandType = CommandType.Text;
			tagcmd.Parameters.Add("@t", DbType.String);
			tagcmd.Parameters.Add("@m", DbType.String);
			tagcmd.Parameters.Add("@o", DbType.String);
			tagcmd.Parameters.Add("@c", DbType.String);
			tagcmd.Parameters.Add("@s", DbType.String);

			using var transaction = con.BeginTransaction();
			foreach (var tag in tags)
			{
				logger.Verbose($"writing tag {tag.Tag}");

				tagcmd.Parameters["@t"].Value = tag.Tag;
				tagcmd.Parameters["@m"].Value = tag.MoreID;
				tagcmd.Parameters["@o"].Value = tag.ObjectID;
				tagcmd.Parameters["@c"].Value = tag.Snippet;
				tagcmd.Parameters["@s"].Value = tag.LastModified;

				try
				{
					tagcmd.ExecuteNonQuery();
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

			try
			{
				transaction.Commit();
			}
			catch (Exception exc)
			{
				ReportError("error writing tags", tagcmd, exc);
			}
		}


		private void ReportError(string msg, SQLiteCommand cmd, Exception exc)
		{
			// provider currently only deals with strings as input so quote everything...

			var sql = Regex.Replace(cmd.CommandText, "@[a-z]+",
				m => cmd.Parameters.Contains(m.Value)
					? $"'{cmd.Parameters[m.Value]}'"
					: m.Value
			);

			logger.WriteLine("error writing tags");
			logger.WriteLine(sql);
			logger.WriteLine(exc);
		}
	}
}
