//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

#define xEnableUpgrade

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Properties;
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SQLite;
	using System.IO;
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
#if EnableUpgrade
			else
			{
				UpgradeDatabase();
			}
#endif

			timestamp = DateTime.Now.ToZuluString();
		}


		public static void DeleteDatabase()
		{
			var path = Path.Combine(
				PathHelper.GetAppDataPath(), Resources.DatabaseFilename);

			if (File.Exists(path))
			{
				try
				{
					File.Delete(path);
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine("error deleting hashtag database", exc);
				}
			}
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
					try
					{
						using var cmd = con.CreateCommand();
						cmd.CommandText = sql;
						cmd.CommandType = CommandType.Text;
						cmd.ExecuteNonQuery();
					}
					catch (Exception exc)
					{
						logger.WriteLine($"error executing sql {sql}", exc);
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
#if EnableUpgrade
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
				logger.WriteLine("error reading scanner version", exc);
				return;
			}

			// upgrade incrementally...

			if (version == 1)
			{
				version = Upgrade1to2(con);
			}

			//if (version == 2)
			//{
			//	version = Upgrade2to3(con);
			//}
		}


		private int Upgrade1to2(SQLiteConnection con)
		{
			logger.WriteLine("upgrading database to version 2");
			using var cmd = con.CreateCommand();

			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "ALTER TABLE hashtag_page ADD COLUMN token NUMBER DEFAULT 0";

			using var transaction = con.BeginTransaction();

			try
			{
				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error upgrading hashtag_page v2", exc);
				return 0;
			}

			try
			{
				cmd.CommandText = "CREATE INDEX IF NOT EXISTS IDX_token ON hashtag_page (token)";
				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error upgrading hashtag_page v2", exc);
				transaction.Rollback();
				return 0;
			}

			try
			{
				cmd.CommandText = "UPDATE hashtag_scanner " +
					$"SET version = 2 WHERE scannerID = {ScannerID}";

				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error upgrading hashtag_page v2", exc);
				transaction.Rollback();
				return 0;
			}

			try
			{
				transaction.Commit();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error committing hashtag_page v2", exc);
				return 0;
			}

			return 2;
		}
#endif
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
		/// Compares the recorded pages against the list of knownIDs and deletes any
		/// records no longer in that list.
		/// </summary>
		/// <param name="knownIDs"></param>
		public void DeletePhantoms(List<string> knownIDs, string sectionID, string sectionPath)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT moreID, pageID FROM hashtag_page WHERE sectionID = @sid";
			cmd.Parameters.AddWithValue("@sid", sectionID);

			using var delcmd = con.CreateCommand();
			delcmd.CommandType = CommandType.Text;
			delcmd.CommandText = "DELETE FROM hashtag_page WHERE pageID = @pid";
			delcmd.Parameters.Add("@pid", DbType.String);

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
						delcmd.Parameters["@pid"].Value = pageID;
						delcmd.ExecuteNonQuery();
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
				logger.WriteLine("error cleaning up pages", exc);
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
				logger.WriteLine("error reading last scan time", exc);
			}

			return DateTime.MinValue.ToZuluString();
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
				logger.WriteLine("error reading distinct tags", exc);
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
				logger.WriteLine("error reading distinct tags", exc);
			}

			return tags;
		}


		/// <summary>
		/// Returns a collection of Hashtag instances with the specified name across all pages
		/// </summary>
		/// <param name="wildcard">The name of the tag to search for, optionally with wildcards</param>
		/// <returns>A collection of Hashtags</returns>
		public Hashtags SearchTags(
			string wildcard, string notebookID = null, string sectionID = null)
		{
			var parameters = new List<SQLiteParameter>();

			var sql =
				"SELECT t.tag, t.moreID, p.pageID, p.titleID, t.objectID, " +
				"p.notebookID, p.sectionID, t.lastModified, t.snippet, p.path, p.name " +
				"FROM hashtag t " +
				"JOIN hashtag_page p " +
				"ON t.moreID = p.moreID ";

			if (!string.IsNullOrWhiteSpace(sectionID))
			{
				sql = $"{sql} AND p.sectionID = @sid ";
				parameters.Add(new("sid", sectionID));
			}
			else if (!string.IsNullOrWhiteSpace(notebookID))
			{
				sql = $"{sql} AND p.notebookID = @nid ";
				parameters.Add(new("nid", notebookID));
			}

			parameters.Add(new("@t", wildcard));

			sql = $"{sql} WHERE t.tag LIKE @t " +
				"ORDER BY p.path, p.name, t.tag";

			return ReadTags(sql, parameters.ToArray());
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
						TitleID = reader.GetString(3),
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
				logger.WriteLine("error reading tags", exc);
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
				logger.WriteLine("error updating snippets", exc);
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
				logger.WriteLine("error writing last scan time", exc);
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
				logger.WriteLine("error writing page info", exc);
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
					logger.WriteLine($"error writing tag {tag.Tag} on {tag.PageID}", exc);
				}
			}

			try
			{
				transaction.Commit();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error writing tags", exc);
			}
		}
	}
}
