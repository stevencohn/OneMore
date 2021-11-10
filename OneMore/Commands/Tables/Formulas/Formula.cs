//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Text;


	/// <summary>
	/// Manage formula versioning, such as reading, writing, upgrading
	/// </summary>
	internal class Formula
	{

		private const int CurrentVersion = 2;


		/// <summary>
		/// Indicates if this parsed formula is valid
		/// </summary>
		public bool Valid { get; private set; }


		/// <summary>
		/// Gets or sets the formula version, should be 0 through CurrentVersion
		/// </summary>
		public int Version { get; private set; }


		/// <summary>
		/// Gets or sets the formula format.
		/// </summary>
		public FormulaFormat Format { get; set; }


		/// <summary>
		/// Gets or sets the decimal places to report.
		/// </summary>
		public int DecimalPlaces { get; set; }


		/// <summary>
		/// Gets or sets the formula expression.
		/// </summary>
		public string Expression { get; set; }


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		/// <summary>
		/// Initialize and new formula with the given properties. Used for formula creation
		/// </summary>
		/// <param name="format"></param>
		/// <param name="dplaces"></param>
		/// <param name="expression"></param>
		public Formula(FormulaFormat format, int dplaces, string expression)
		{
			Version = CurrentVersion;
			Format = format;
			DecimalPlaces = dplaces;
			Expression = expression;
			Valid = true;
		}


		/// <summary>
		/// Initialize a formula by parsing the given meta string. Used for existing formulas.
		/// </summary>
		/// <param name="meta"></param>
		public Formula(TableCell cell)
		{
			Valid = false;

			var meta = cell.GetMeta("omfx");
			if (string.IsNullOrEmpty(meta))
			{
				return;
			}

			var parts = meta.Split(';');

			// version is always first part
			if (!int.TryParse(parts[0], out var version))
			{
				return;
			}

			Version = version;

			if (version > CurrentVersion)
			{
				return;
			}

			switch (version)
			{
				case 0:
					// version 0;range;function;format
					Upgrade0(cell, parts);
					break;

				case 1:
					// version 1;format;expression
					ParseFormat(parts[1]);
					Expression = parts[2];
					break;

				case 2:
					// version 2;format;dplaces;expression
					ParseFormat(parts[1]);
					ParseDecimalPlaces(parts[2]);
					Expression = parts[3];
					break;
			}

			Valid = !string.IsNullOrEmpty(Expression);
		}


		private void Upgrade0(TableCell cell, string[] parts)
		{
			// 0;range;function;format

			if (!Enum.TryParse<FormulaFunction>(parts[2], true, out var func))
			{
				Valid = false;
				return;
			}

			var builder = new StringBuilder();

			if (func == FormulaFunction.StandardDeviation)
			{
				builder.Append("stdev(");
			}
			else
			{
				builder.Append(func.ToString().ToLower());
				builder.Append("(");
			}

			if (parts[1] == "Rows")
			{
				var col = cell.ColNum.ToAlphabetic();
				builder.Append(col);
				builder.Append(1);
				builder.Append(":");
				builder.Append(col);
				builder.Append(cell.RowNum - 1);
			}
			else
			{
				builder.Append("A");
				builder.Append(cell.RowNum);
				builder.Append(":");
				builder.Append((cell.ColNum -1).ToAlphabetic());
				builder.Append(cell.RowNum);
			}

			builder.Append(")");
			Expression = builder.ToString();

			ParseFormat(parts[3]);
		}


		private void ParseDecimalPlaces(string places)
		{
			if (int.TryParse(places, out var value))
			{
				DecimalPlaces = value;
			}

		}


		private void ParseFormat(string format)
		{
			if (Enum.TryParse<FormulaFormat>(format, true, out var value))
			{
				Format = value;
			}
		}


		/// <summary>
		/// Serializes the formula properties as a string for storing in a meta tag.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return $"{CurrentVersion};{Format};{DecimalPlaces};{Expression}";
		}
	}
}
