//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using River.OneMoreAddIn.Models;
	using System.Text.RegularExpressions;


	/// <summary>
	/// Shifts the cell references of a formula for the nth selected cell, so a formula written
	/// for the first cell can be applied to the rest of a selected row or column.
	/// </summary>
	internal static class FormulaShifter
	{
		private static readonly Regex CellRegex =
			new Regex(@"\b(?<c>[a-zA-Z]{1,3})(?<r>\d{1,3})\b");


		/// <summary>
		/// Reduces an expression to its shape by rewriting every cell reference as an offset
		/// relative to the hosting cell, ignoring case and whitespace. Formulas that are
		/// equivalent after shifting have equal normalized forms.
		/// </summary>
		public static string Normalize(string expression, string hostCoordinates)
		{
			if (string.IsNullOrEmpty(expression))
			{
				return string.Empty;
			}

			var hostCol = 0;
			var hostRow = 0;
			var host = CellRegex.Match(hostCoordinates ?? string.Empty);
			if (host.Success)
			{
				hostCol = TableCell.LettersToIndex(host.Groups["c"].Value);
				hostRow = int.Parse(host.Groups["r"].Value);
			}

			var normalized = CellRegex.Replace(expression, match =>
			{
				var col = TableCell.LettersToIndex(match.Groups["c"].Value) - hostCol;
				var row = int.Parse(match.Groups["r"].Value) - hostRow;
				return $"@{col},{row}";
			});

			return Regex.Replace(normalized, @"\s+", string.Empty).ToLowerInvariant();
		}


		/// <summary>
		/// Returns the expression with every cell reference moved by the given offset.
		/// Column selections shift the column letters; row selections shift the row numbers.
		/// </summary>
		public static string Shift(string expression, int offset, TableSelectionRange rangeType)
		{
			if (offset == 0 || string.IsNullOrEmpty(expression))
			{
				return expression;
			}

			return CellRegex.Replace(expression, match =>
			{
				var col = match.Groups["c"].Value;
				var row = match.Groups["r"].Value;

				if (rangeType == TableSelectionRange.Columns)
				{
					var shifted = TableCell.IndexToLetters(TableCell.LettersToIndex(col) + offset);

					// IndexToLetters is upper case; keep the case the user typed
					col = col == col.ToLowerInvariant() ? shifted.ToLowerInvariant() : shifted;
				}
				else
				{
					row = (int.Parse(row) + offset).ToString();
				}

				return $"{col}{row}";
			});
		}
	}
}
