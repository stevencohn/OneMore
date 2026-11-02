//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SQLite;
	using Resx = Properties.Resources;


	/// <summary>
	/// Reads and writes the form control lists and settings kept in OneMore.db.
	/// </summary>
	internal class FormControlsProvider : DatabaseProvider
	{
		public const string DateFormatKey = "dateFormat";
		public const string DefaultDateFormat = "dddd, MMMM d, yyyy";

		private const string Domain = "formcontrols";
		private const string PrimaryTable = "form_list";
		private const string SchemaTable = "formcontrols_schema";
		private const int CurrentVersion = 1;


		/// <summary>
		/// Initialize this provider, opening the standard database
		/// </summary>
		public FormControlsProvider()
			: base()
		{
			OpenDatabase();
			EnsureCatalog();
		}


		/// <summary>
		/// Initialize this provider with an already-open connection, bypassing the standard
		/// database file; used by unit tests with an in-memory database.
		/// </summary>
		internal FormControlsProvider(SQLiteConnection connection)
		{
			con = connection;
			EnsureCatalog();
		}


		private void EnsureCatalog()
		{
			if (!TableExists(PrimaryTable))
			{
				RefreshDataSchema(Domain, Resx.FormControlsDB);

				using var cmd = con.CreateCommand();
				UpsertSchemaVersion(cmd, SchemaTable, "schemaID", CurrentVersion);
			}
			else
			{
				// no upgrades yet; just log a catalog written by a newer release
				var version = ReadSchemaVersion(SchemaTable, "schemaID", missing: CurrentVersion);
				IsNewerThanKnown(Domain, version, CurrentVersion);
			}
		}


		/// <summary>
		/// Reads all lists, alphabetically, with their items in the order the user arranged them.
		/// </summary>
		public List<FormList> ReadLists()
		{
			var lists = new List<FormList>();
			var map = new Dictionary<long, FormList>();

			using (var cmd = con.CreateCommand())
			{
				cmd.CommandText = "SELECT listID, name FROM form_list ORDER BY name COLLATE NOCASE";
				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var list = new FormList(reader.GetString(1));
					map.Add(reader.GetInt64(0), list);
					lists.Add(list);
				}
			}

			using (var cmd = con.CreateCommand())
			{
				cmd.CommandText = "SELECT listID, text FROM form_list_item ORDER BY sortOrder, itemID";
				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					if (map.TryGetValue(reader.GetInt64(0), out var list))
					{
						list.Items.Add(reader.GetString(1));
					}
				}
			}

			return lists;
		}


		/// <summary>
		/// Reads the items of the named list; empty if there is no such list.
		/// </summary>
		public List<string> ReadItems(string listName)
		{
			var items = new List<string>();

			using var cmd = con.CreateCommand();
			cmd.CommandText =
				"SELECT i.text FROM form_list_item i " +
				"JOIN form_list l ON l.listID = i.listID " +
				"WHERE l.name = @name COLLATE NOCASE ORDER BY i.sortOrder, i.itemID";

			cmd.Parameters.AddWithValue("@name", listName);

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				items.Add(reader.GetString(0));
			}

			return items;
		}


		/// <summary>
		/// Replaces all lists with the given lists in a single transaction.
		/// </summary>
		public bool WriteLists(IEnumerable<FormList> lists)
		{
			try
			{
				BeginImmediate();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error starting form lists transaction", exc);
				return false;
			}

			try
			{
				using var cmd = con.CreateCommand();
				cmd.CommandType = CommandType.Text;

				// items are removed explicitly so we don't depend on foreign key enforcement
				cmd.CommandText = "DELETE FROM form_list_item";
				cmd.ExecuteNonQuery();
				cmd.CommandText = "DELETE FROM form_list";
				cmd.ExecuteNonQuery();

				var order = 0;
				foreach (var list in lists)
				{
					cmd.CommandText =
						"INSERT INTO form_list (name, sortOrder) VALUES (@name, @order); " +
						"SELECT last_insert_rowid()";

					cmd.Parameters.Clear();
					cmd.Parameters.AddWithValue("@name", list.Name);
					cmd.Parameters.AddWithValue("@order", order++);
					var listID = (long)cmd.ExecuteScalar();

					var itemOrder = 0;
					foreach (var item in list.Items)
					{
						cmd.CommandText =
							"INSERT INTO form_list_item (listID, text, sortOrder) " +
							"VALUES (@listID, @text, @order)";

						cmd.Parameters.Clear();
						cmd.Parameters.AddWithValue("@listID", listID);
						cmd.Parameters.AddWithValue("@text", item);
						cmd.Parameters.AddWithValue("@order", itemOrder++);
						cmd.ExecuteNonQuery();
					}
				}

				Commit();
				return true;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error writing form lists", exc);
				Rollback();
				return false;
			}
		}


		/// <summary>
		/// Reads a form control setting, returning the default if it has not been set.
		/// </summary>
		public string ReadSetting(string key, string defaultValue)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandText = "SELECT value FROM form_setting WHERE key = @key";
			cmd.Parameters.AddWithValue("@key", key);

			var value = cmd.ExecuteScalar() as string;
			return string.IsNullOrEmpty(value) ? defaultValue : value;
		}


		/// <summary>
		/// Writes a form control setting
		/// </summary>
		public bool WriteSetting(string key, string value)
		{
			try
			{
				using var cmd = con.CreateCommand();
				cmd.CommandText =
					"INSERT INTO form_setting (key, value) VALUES (@key, @value) " +
					"ON CONFLICT(key) DO UPDATE SET value = excluded.value";

				cmd.Parameters.AddWithValue("@key", key);
				cmd.Parameters.AddWithValue("@value", value);
				cmd.ExecuteNonQuery();
				return true;
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error writing form setting {key}", exc);
				return false;
			}
		}
	}
}
