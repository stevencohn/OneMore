//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Properties;
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SQLite;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;


	/// <summary>
	/// Base class for database providers, which encapsulate the common lifecycle and helper
	/// functions needed by specific providers.
	/// </summary>
	internal abstract class DatabaseProvider : Loggable, IDisposable
	{
		protected SQLiteConnection con;
		protected bool disposed;

		protected static readonly string path = Path.Combine(
			PathHelper.GetAppDataPath(), Resources.DatabaseFilename);


		protected DatabaseProvider()
		{
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


		protected static bool CatalogExists(string primaryTableName)
		{
			if (!File.Exists(path))
			{
				return false;
			}

			using var con = new SQLiteConnection($"Data source={path}");
			con.Open();

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT COUNT(1) FROM sqlite_master " +
				"WHERE type = 'table' AND name = @tableName";

			cmd.Parameters.AddWithValue("@tableName", primaryTableName);

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


		protected bool DropCatalog(string domain, string DDL)
		{
			int Drop(string type, IEnumerable<string> names)
			{
				using var cmd = con.CreateCommand();
				cmd.CommandType = CommandType.Text;

				var count = 0;

				foreach (var name in names)
				{
					logger.WriteLine($"dropping {domain} {type} {name}");

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

			if (!File.Exists(path))
			{
				return true;
			}

			var pattern = new Regex(@"CREATE(?: UNIQUE)? ([^\s]+) IF NOT EXISTS ([^\s]+)",
				RegexOptions.Compiled);

			var entities = Regex.Split(DDL, @"\r\n|\n\r|\n")
				.AsEnumerable()
				.Select(d => pattern.Match(d))
				.Where(m => m.Success)
				.Select(m => (m.Groups[1].Value, m.Groups[2].Value))
				.ToList();

			if (!entities.Any())
			{
				return true;
			}

			using var transaction = con.BeginTransaction();
			var count = 0;

			IEnumerable<(string, string)> list;

			list = entities.Where(e => e.Item1 == "TRIGGER");
			if (list.Any())
			{
				count += Drop("trigger", list.Select(e => e.Item2).ToList());
			}

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

			if (count != entities.Count(e => 
				e.Item1 == "TRIGGER" || e.Item1 == "VIEW" || e.Item1 == "INDEX" || e.Item1 == "TABLE"))
			{
				logger.WriteLine($"error dropping {domain} schema, see errors above");
				transaction.Rollback();
				return false;
			}

			try
			{
				transaction.Commit();

				// vacuum must be done outside a transaction
				using var cmd = con.CreateCommand();
				cmd.CommandText = "VACUUM";
				cmd.ExecuteNonQuery();

				logger.WriteLine($"{domain} schema drop done");
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error dropping {domain} schema", exc);
				transaction.Rollback();
				return false;
			}

			return true;
		}


		protected void OpenDatabase()
		{
			if (con is null)
			{
				con = new SQLiteConnection($"Data source={path}");
				con.Open();

				// WAL mode lets readers proceed concurrently with the scanner's write
				// transactions; NORMAL synchronous is the safe companion to WAL;
				// busy_timeout lets a conflicting writer back off instead of failing immediately
				using var pragma = con.CreateCommand();
				pragma.CommandText =
					"PRAGMA journal_mode=WAL;" +
					"PRAGMA synchronous=NORMAL;" +
					"PRAGMA busy_timeout=3000;" +
					"PRAGMA temp_store=MEMORY;";
				pragma.ExecuteNonQuery();
			}
		}


		protected void RefreshDataSchema(string domain, string DDL)
		{
			logger.WriteLine($"building {domain} database schema");

			OpenDatabase();

			using var transaction = con.BeginTransaction();

			var ddl = Regex.Split(DDL, @"\r\n|\n\r|\n");
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
						ReportError($"error building {domain} schema", cmd, exc);
						throw;
					}
				}
			}

			try
			{
				transaction.Commit();
				logger.WriteLine($"refresh {domain} schema done");
			}
			catch (Exception exc)
			{
				transaction.Rollback();
				logger.WriteLine($"error building {domain} schema", exc);
				throw;
			}
		}


		protected static void ReportError(string msg, SQLiteCommand cmd, Exception exc)
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
