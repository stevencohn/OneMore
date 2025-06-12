//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;


	internal class VariableProvider : DatabaseProvider
	{
		private const string tableName = "variable";


		public VariableProvider()
			: base()
		{
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
				cmd.CommandText = $"SELECT name, value FROM {tableName}";
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
