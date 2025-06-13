//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Properties;
	using System;
	using System.Data.SQLite;
	using System.IO;
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


		protected void OpenDatabase()
		{
			if (con is null)
			{
				con = new SQLiteConnection($"Data source={path}");
				con.Open();
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
