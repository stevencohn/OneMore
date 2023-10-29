//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Properties;
	using System;
	using System.Data;
	using System.Data.SQLite;
	using System.IO;
	using System.Text.RegularExpressions;


	/// <summary>
	/// Reads, Writes, manages a data store of Hashtags.
	/// </summary>
	internal class HashtagProvider : Loggable, IDisposable
	{
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

			timestamp = DateTime.Now.ToZuluString();
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
				logger.WriteLine("error committing transaction", exc);
				throw;
			}
		}


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
		/// Deletes the specified tags
		/// </summary>
		/// <param name="tags">A collection of Hashtags</param>
		public void DeleteTags(Hashtags tags)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandText = "DELETE FROM hashtags WHERE tag = @t AND pageID = @p";
			cmd.CommandType = CommandType.Text;
			cmd.Parameters.Add("@t", DbType.String);
			cmd.Parameters.Add("@p", DbType.String);

			using var transaction = con.BeginTransaction();
			foreach (var tag in tags)
			{
				logger.WriteLine($"deleting tag {tag.Tag}");

				cmd.Parameters["@t"].Value = tag.Tag;
				cmd.Parameters["@p"].Value = tag.PageID;

				try
				{
					cmd.ExecuteNonQuery();
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error deleting tag {tag.Tag} on {tag.PageID}", exc);
				}
			}

			try
			{
				transaction.Commit();

				CleanupPages();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error committing transaction", exc);
			}
		}


		private void CleanupPages()
		{
			// as tags are deleted from a page, that page may be left dangling in the
			// hashtags_pages table; this cleans up those orphaned records

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "DELETE FROM hashtags_pages WHERE moreID IN (SELECT P.moreID FROM " +
				"hashtags_pages P LEFT OUTER JOIN hashtags T ON T.moreID = P.moreID WHERE T.tag IS NULL)";

			try
			{
				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error committing transaction", exc);
			}
		}


		/// <summary>
		/// Returns the last saved scan time
		/// </summary>
		/// <returns>A collection of Hashtags</returns>
		public string ReadLastScanTime()
		{
			using var cmd = con.CreateCommand();
			cmd.CommandText = "SELECT version, lastScan FROM hashtag_scanner WHERE scannerID = 0";

			try
			{
				using var reader = cmd.ExecuteReader();
				if (reader.Read())
				{
					// var version = reader.GetInt32(0); // upgrade?
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
		/// Returns a collection of tags on the specified page
		/// </summary>
		/// <param name="pageID">The ID of the page</param>
		/// <returns>A collection of Hashtags</returns>
		public Hashtags ReadPageTags(string pageID)
		{
			return ReadTags(
				"SELECT rowID, tag, moreID, pageID, objectID, lastScan FROM hashtags WHERE pageID = @p",
				new SQLiteParameter[] { new SQLiteParameter("@p", pageID) }
				);
		}


		/// <summary>
		/// Returns a collection of Hashtag instances with the specified name across all pages
		/// </summary>
		/// <param name="tag">The name of the tag to search for</param>
		/// <returns>A collection of Hashtags</returns>
		public Hashtags ReadTagsByName(string tag)
		{
			return ReadTags(
				"SELECT rowID, tag, moreID, pageID, objectID, lastScan FROM hashtags WHERE tag = @p",
				new SQLiteParameter[] { new SQLiteParameter("@p", tag) }
				);
		}


		private Hashtags ReadTags(string sql, SQLiteParameter[] parameters)
		{
			var tags = new Hashtags();
			using var cmd = con.CreateCommand();
			cmd.CommandText = sql;
			cmd.Parameters.AddRange(parameters);

			try
			{
				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					tags.Add(new Hashtag
					{
						RowID = reader.GetInt32(0),
						Tag = reader.GetString(1),
						MoreID = reader.GetString(2),
						PageID = reader.GetString(3),
						ObjectID = reader.GetString(4),
						LastScan = reader.GetString(5)
					});
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error reading tags", exc);
			}

			return tags;
		}


		/// <summary>
		/// Records the timestamp value that was initialized at construction of this class
		/// instance.
		/// </summary>
		public void WriteLastScanTime()
		{
			using var cmd = con.CreateCommand();
			cmd.CommandText = "UPDATE hashtag_scanner SET lastScan = @d WHERE scannerID = 0";
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
		/// <param name="path">The hierarchy path of the page</param>
		/// <param name="title">The title (name) of the page</param>
		public void WritePageInfo(string moreID, string path, string title)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandText = "REPLACE INTO hashtags_pages (moreID, path, name) VALUES (@m, @p, @n)";
			cmd.Parameters.AddWithValue("@m", moreID);
			cmd.Parameters.AddWithValue("@p", path);
			cmd.Parameters.AddWithValue("@n", title);

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
			using var cmd = con.CreateCommand();
			cmd.CommandText = "INSERT INTO hashtags " +
				"(tag, moreID, pageID, objectID, lastScan) VALUES (@t, @m, @p, @o, @s)";

			cmd.CommandType = CommandType.Text;
			cmd.Parameters.Add("@t", DbType.String);
			cmd.Parameters.Add("@m", DbType.String);
			cmd.Parameters.Add("@p", DbType.String);
			cmd.Parameters.Add("@o", DbType.String);
			cmd.Parameters.Add("@s", DbType.String);

			using var transaction = con.BeginTransaction();
			foreach (var tag in tags)
			{
				logger.WriteLine($"writing tag {tag.Tag}");

				cmd.Parameters["@t"].Value = tag.Tag;
				cmd.Parameters["@m"].Value = tag.MoreID;
				cmd.Parameters["@p"].Value = tag.PageID;
				cmd.Parameters["@o"].Value = tag.ObjectID;
				cmd.Parameters["@s"].Value = timestamp;

				try
				{
					cmd.ExecuteNonQuery();
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
				logger.WriteLine("error committing transaction", exc);
			}
		}
	}
}
