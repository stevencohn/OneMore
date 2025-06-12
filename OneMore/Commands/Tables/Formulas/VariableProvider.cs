//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using River.OneMoreAddIn.Properties;
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;
	using System.Text.RegularExpressions;

	internal class VariableProvider : DatabaseProvider
	{
		private const string tableName = "variable";


		public VariableProvider()
			: base()
		{
		}


		private void BuildCatalog()
		{
			logger.WriteLine("building variables catalog");

			OpenDatabase();

			using var transaction = con.BeginTransaction();

			var ddl = Regex.Split(Resources.VariablesDB, @"\r\n|\n\r|\n");
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
				logger.WriteLine("variables catalog done");
			}
			catch (Exception exc)
			{
				logger.WriteLine("error building db", exc);
				throw;
			}
		}


		public List<Variable> ReadVariables()
		{
			var variables = new List<Variable>();

			if (!CatalogExists())
			{
				return variables;
			}

			using (var cmd = con.CreateCommand())
			{
				cmd.CommandText = $"SELECT name, value FROM {tableName} ORDER BY name";
				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var name = reader.GetString(0);
					var value = reader.GetDouble(1);
					variables.Add(new Variable(name, value));
				}
			}

			return variables;
		}


		public void WriteVariables(IEnumerable<Variable> variables)
		{
			if (!CatalogExists())
			{
				BuildCatalog();
			}

			using var transaction = con.BeginTransaction();

			using var cmd = con.CreateCommand();
			try
			{
				cmd.CommandText = "DELETE FROM variable";
				cmd.CommandType = CommandType.Text;
				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				ReportError("error deleting variables", cmd, exc);
				throw;
			}

			using var cmd2 = con.CreateCommand();
			try
			{
				cmd2.CommandText = "INSERT INTO variable (name, value) VALUES (@n, @v)";

				cmd2.Parameters.Clear();
				cmd2.Parameters.Add("@n", DbType.String);
				cmd2.Parameters.Add("@v", DbType.Double);

				foreach (var variable in variables)
				{
					logger.Verbose($"writing variable {variable.Name}");

					cmd2.Parameters["@n"].Value = variable.Name;
					cmd2.Parameters["@v"].Value = variable.Value;

					try
					{
						cmd2.ExecuteNonQuery();
					}
					catch (Exception exc)
					{
						logger.WriteLine("error saving variable", exc);
					}
				}

				transaction.Commit();
			}
			catch (Exception exc)
			{
				ReportError("error saving variables", cmd2, exc);
				transaction.Rollback();
			}
		}


		private bool CatalogExists()
		{
			if (!File.Exists(path))
			{
				return false;
			}

			OpenDatabase();

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT COUNT(1) FROM sqlite_master " +
				$"WHERE type = 'table' AND name = '{tableName}'";

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
	}
}
