//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Helpers
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using System.Data.SQLite;


	[TestClass]
	public class DatabaseProviderTests
	{
		// the shared helpers are protected, so expose them through a minimal provider
		private sealed class Probe : DatabaseProvider
		{
			public Probe(SQLiteConnection connection)
			{
				con = connection;
			}

			public bool Table(string name) => TableExists(name);
			public bool Column(string table, string column) => ColumnExists(con, table, column);
			public int Version(string table, string key, int missing) => ReadSchemaVersion(table, key, missing);
			public bool Newer(int found, int known) => IsNewerThanKnown("test", found, known);

			public bool Upsert(string table, string key, int version)
			{
				using var cmd = con.CreateCommand();
				return UpsertSchemaVersion(cmd, table, key, version);
			}
		}


		private SQLiteConnection connection;
		private Probe provider;


		[TestInitialize]
		public void Setup()
		{
			connection = new SQLiteConnection("Data Source=:memory:");
			connection.Open();
			provider = new Probe(connection);
		}


		[TestCleanup]
		public void Teardown()
		{
			provider?.Dispose();
			connection?.Dispose();
		}


		private void Execute(string sql)
		{
			using var cmd = connection.CreateCommand();
			cmd.CommandText = sql;
			cmd.ExecuteNonQuery();
		}


		[TestMethod]
		public void TableExists_ReportsExistingAndMissingTables()
		{
			Execute("CREATE TABLE present (x INTEGER)");

			Assert.IsTrue(provider.Table("present"));
			Assert.IsFalse(provider.Table("absent"));
		}


		[TestMethod]
		public void TableExists_IgnoresViewsAndIndexes()
		{
			Execute("CREATE TABLE t (x INTEGER)");
			Execute("CREATE VIEW v AS SELECT x FROM t");
			Execute("CREATE INDEX i ON t (x)");

			Assert.IsFalse(provider.Table("v"));
			Assert.IsFalse(provider.Table("i"));
		}


		[TestMethod]
		public void ColumnExists_FindsColumnsCaseInsensitively()
		{
			Execute("CREATE TABLE t (pageKey INTEGER, title TEXT)");

			Assert.IsTrue(provider.Column("t", "title"));
			Assert.IsTrue(provider.Column("t", "PAGEKEY"));
			Assert.IsFalse(provider.Column("t", "missing"));
		}


		[TestMethod]
		public void ColumnExists_SeesAColumnAfterAlterTable()
		{
			Execute("CREATE TABLE t (a INTEGER)");
			Assert.IsFalse(provider.Column("t", "b"));

			Execute("ALTER TABLE t ADD COLUMN b TEXT");

			Assert.IsTrue(provider.Column("t", "b"));
		}


		[TestMethod]
		public void ReadSchemaVersion_TableMissing_ReturnsTheDefault()
		{
			Assert.AreEqual(1, provider.Version("no_such_schema", "schemaID", missing: 1));
		}


		[TestMethod]
		public void ReadSchemaVersion_TableWithoutRow_ReturnsTheDefault()
		{
			Execute("CREATE TABLE s (schemaID INTEGER PRIMARY KEY, version NUMERIC)");

			Assert.AreEqual(1, provider.Version("s", "schemaID", missing: 1));
		}


		[TestMethod]
		public void ReadSchemaVersion_ReadsTheStoredVersion()
		{
			Execute("CREATE TABLE s (schemaID INTEGER PRIMARY KEY, version NUMERIC)");
			Execute("INSERT INTO s (schemaID, version) VALUES (0, 7)");

			Assert.AreEqual(7, provider.Version("s", "schemaID", missing: 1));
		}


		[TestMethod]
		public void UpsertSchemaVersion_CreatesTheRowThenUpdatesIt()
		{
			Execute("CREATE TABLE s (schemaID INTEGER PRIMARY KEY UNIQUE NOT NULL, version NUMERIC UNIQUE NOT NULL)");

			Assert.IsTrue(provider.Upsert("s", "schemaID", 2));
			Assert.AreEqual(2, provider.Version("s", "schemaID", missing: 0));

			Assert.IsTrue(provider.Upsert("s", "schemaID", 3));
			Assert.AreEqual(3, provider.Version("s", "schemaID", missing: 0));
		}


		[TestMethod]
		public void UpsertSchemaVersion_TableMissing_ReturnsFalse()
		{
			Assert.IsFalse(provider.Upsert("no_such_schema", "schemaID", 2));
		}


		[TestMethod]
		public void IsNewerThanKnown_OnlyWhenTheFoundVersionIsHigher()
		{
			Assert.IsTrue(provider.Newer(found: 6, known: 5));
			Assert.IsFalse(provider.Newer(found: 5, known: 5));
			Assert.IsFalse(provider.Newer(found: 4, known: 5));
		}
	}
}
