//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using River.OneMoreAddIn.Models;
	using System.Text.RegularExpressions;


	/// <summary>
	/// Corrects formulas whose cells have moved since the formula was written, for example
	/// after rows or columns were inserted or deleted in the table.
	/// </summary>
	/// <remarks>
	/// Each formula records the coordinates of its host cell (the anchor). The difference between
	/// the anchor and the cell's current coordinates tells how far the cell has moved. Since the
	/// point of insertion is unknown, references that share the anchor's row or column are
	/// assumed to have moved with the formula cell; all other references are left untouched and
	/// counted as needing review.
	/// </remarks>
	internal static class FormulaFixer
	{
		// A1 or A1:B2; a trailing "(" means a function name such as log10(, not a reference
		private static readonly Regex RangeRegex = new(
			@"\b(?<a>(?<ac>[a-zA-Z]{1,3})(?<ar>\d{1,3})\b(?!\s*\())(?::(?<b>(?<bc>[a-zA-Z]{1,3})(?<br>\d{1,3})\b))?",
			RegexOptions.Compiled);

		private static readonly Regex AnchorRegex = new(
			@"^(?<c>[a-zA-Z]{1,3})(?<r>\d{1,3})$", RegexOptions.Compiled);


		/// <summary>
		/// Fixes the formula of every cell in the table that has moved from its anchor, and
		/// stamps an anchor on formulas that do not have one yet.
		/// </summary>
		/// <param name="table">The table to fix</param>
		/// <param name="review">The number of formulas with references that could not be
		/// determined and may need manual review</param>
		/// <returns>The number of formulas whose expression was changed</returns>
		public static int Fix(Table table, out int review)
		{
			var fixedCount = 0;
			review = 0;

			foreach (var row in table.Rows)
			{
				foreach (var cell in row.Cells)
				{
					if (string.IsNullOrEmpty(cell.GetMeta("omfx")))
					{
						continue;
					}

					var formula = new Formula(cell);
					if (!formula.Valid)
					{
						continue;
					}

					if (formula.Version < 3 || !TryParseAnchor(formula.Anchor, out var arow, out var acol))
					{
						// legacy formula; assume it is correct where it sits and pin it
						formula.Anchor = cell.Coordinates;
						Store(cell, formula);
						continue;
					}

					var dr = cell.RowNum - arow;
					var dc = cell.ColNum - acol;

					if (dr == 0 && dc == 0)
					{
						continue;
					}

					var expression = Shift(formula.Expression, arow, acol, dr, dc, out var unsure);
					if (unsure)
					{
						review++;
					}

					if (expression != formula.Expression)
					{
						fixedCount++;
					}

					formula.Expression = expression;
					formula.Anchor = cell.Coordinates;
					Store(cell, formula);
				}
			}

			return fixedCount;
		}


		/// <summary>
		/// Shifts the cell references of an expression that belongs to a cell that moved from
		/// (arow, acol) by (dr, dc).
		/// </summary>
		/// <param name="expression">The original expression</param>
		/// <param name="arow">The 1-based row of the original anchor</param>
		/// <param name="acol">The 1-based column of the original anchor</param>
		/// <param name="dr">The number of rows the cell moved</param>
		/// <param name="dc">The number of columns the cell moved</param>
		/// <param name="review">True if some reference could not be determined</param>
		/// <returns>The corrected expression</returns>
		public static string Shift(
			string expression, int arow, int acol, int dr, int dc, out bool review)
		{
			var unsure = false;

			var result = RangeRegex.Replace(expression, match =>
			{
				var aCol = TableCell.LettersToIndex(match.Groups["ac"].Value);
				var aRow = int.Parse(match.Groups["ar"].Value);

				var bCol = aCol;
				var bRow = aRow;
				var ranged = match.Groups["b"].Success;
				if (ranged)
				{
					bCol = TableCell.LettersToIndex(match.Groups["bc"].Value);
					bRow = int.Parse(match.Groups["br"].Value);
				}

				// a range moves as a unit if either end shares the anchor's row or column
				var related =
					aRow == arow || aCol == acol ||
					(ranged && (bRow == arow || bCol == acol));

				if (!related)
				{
					unsure = true;
					return match.Value;
				}

				var nac = aCol + dc;
				var nar = aRow + dr;
				var nbc = bCol + dc;
				var nbr = bRow + dr;

				if (nac < 1 || nar < 1 || nbc < 1 || nbr < 1)
				{
					unsure = true;
					return match.Value;
				}

				var text = $"{TableCell.IndexToLetters(nac)}{nar}";
				if (ranged)
				{
					text = $"{text}:{TableCell.IndexToLetters(nbc)}{nbr}";
				}

				return text;
			});

			review = unsure;
			return result;
		}


		private static bool TryParseAnchor(string anchor, out int row, out int col)
		{
			row = 0;
			col = 0;

			if (string.IsNullOrEmpty(anchor))
			{
				return false;
			}

			var match = AnchorRegex.Match(anchor);
			if (!match.Success)
			{
				return false;
			}

			col = TableCell.LettersToIndex(match.Groups["c"].Value);
			row = int.Parse(match.Groups["r"].Value);
			return true;
		}


		private static void Store(TableCell cell, Formula formula)
		{
			cell.SetMeta("omfx", formula.ToString());
		}
	}
}
