//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Backup
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Backups;
	using System;
	using System.Data.SQLite;
	using System.IO;
	using System.Threading;


	[TestClass]
	public class DatabaseBackupTests
	{
		private string folder;


		[TestInitialize]
		public void Setup()
		{
			folder = Path.Combine(Path.GetTempPath(), $"OneMoreBackupTests-{Guid.NewGuid():N}");
			Directory.CreateDirectory(folder);
		}


		[TestCleanup]
		public void Cleanup()
		{
			SQLiteConnection.ClearAllPools();
			if (Directory.Exists(folder))
			{
				Directory.Delete(folder, true);
			}
		}


		[TestMethod]
		public void IsDueWhenNeverBackedUp()
		{
			var options = new BackupOptions { Enabled = true };
			Assert.IsTrue(DatabaseBackup.IsDue(options, DateTime.UtcNow));
		}


		[TestMethod]
		public void IsDueHonorsInterval()
		{
			var last = new DateTime(2026, 1, 31, 12, 0, 0, DateTimeKind.Utc);

			var daily = new BackupOptions { Interval = BackupInterval.Daily, LastBackup = last };
			Assert.IsFalse(DatabaseBackup.IsDue(daily, last.AddHours(23)));
			Assert.IsTrue(DatabaseBackup.IsDue(daily, last.AddDays(1)));

			var weekly = new BackupOptions { Interval = BackupInterval.Weekly, LastBackup = last };
			Assert.IsFalse(DatabaseBackup.IsDue(weekly, last.AddDays(6)));
			Assert.IsTrue(DatabaseBackup.IsDue(weekly, last.AddDays(7)));

			// one calendar month after Jan 31 is Feb 28
			var monthly = new BackupOptions { Interval = BackupInterval.Monthly, LastBackup = last };
			Assert.IsFalse(DatabaseBackup.IsDue(monthly, last.AddDays(27)));
			Assert.IsTrue(DatabaseBackup.IsDue(monthly, last.AddMonths(1)));
		}


		[TestMethod]
		public void PruneKeepsNewestAndIgnoresOtherFiles()
		{
			string[] names =
			{
				"OneMore-20260101-010101.db",
				"OneMore-20260102-010101.db",
				"OneMore-20260103-010101.db",
				"OneMore-20260104-010101.db"
			};

			foreach (var name in names)
			{
				File.WriteAllText(Path.Combine(folder, name), "x");
			}

			File.WriteAllText(Path.Combine(folder, "notes.txt"), "x");
			File.WriteAllText(Path.Combine(folder, "Other-20250101-010101.db"), "x");

			DatabaseBackup.Prune(folder, 2);

			Assert.IsFalse(File.Exists(Path.Combine(folder, names[0])));
			Assert.IsFalse(File.Exists(Path.Combine(folder, names[1])));
			Assert.IsTrue(File.Exists(Path.Combine(folder, names[2])));
			Assert.IsTrue(File.Exists(Path.Combine(folder, names[3])));
			Assert.IsTrue(File.Exists(Path.Combine(folder, "notes.txt")));
			Assert.IsTrue(File.Exists(Path.Combine(folder, "Other-20250101-010101.db")));
		}


		[TestMethod]
		public void CopyIncludesWalContent()
		{
			var source = Path.Combine(folder, "source.db");
			var target = Path.Combine(folder, "copies");
			Directory.CreateDirectory(target);

			// keep this connection open so rows remain in the -wal file
			using (var con = new SQLiteConnection($"Data source={source}"))
			{
				con.Open();
				Execute(con, "PRAGMA journal_mode=WAL");
				Execute(con, "CREATE TABLE t (id INTEGER)");
				Execute(con, "INSERT INTO t VALUES (42)");

				var path = DatabaseBackup.Copy(source, target, CancellationToken.None);

				Assert.IsNotNull(path);
				Assert.IsTrue(File.Exists(path));
				Assert.AreEqual(0, Directory.GetFiles(target, "*.tmp").Length);

				using var copy = new SQLiteConnection($"Data source={path}");
				copy.Open();
				using var cmd = copy.CreateCommand();
				cmd.CommandText = "SELECT id FROM t";
				Assert.AreEqual(42L, cmd.ExecuteScalar());
			}
		}


		[TestMethod]
		public void CopyCanceledLeavesNothingBehind()
		{
			var source = Path.Combine(folder, "source.db");
			var target = Path.Combine(folder, "copies");
			Directory.CreateDirectory(target);

			using (var con = new SQLiteConnection($"Data source={source}"))
			{
				con.Open();
				Execute(con, "CREATE TABLE t (id INTEGER)");
			}

			using var cancel = new CancellationTokenSource();
			cancel.Cancel();

			var path = DatabaseBackup.Copy(source, target, cancel.Token);

			Assert.IsNull(path);
			Assert.AreEqual(0, Directory.GetFiles(target).Length);
		}


		private static void Execute(SQLiteConnection con, string sql)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandText = sql;
			cmd.ExecuteNonQuery();
		}
	}
}
