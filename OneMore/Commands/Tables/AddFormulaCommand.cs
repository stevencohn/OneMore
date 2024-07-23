﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Tables.Formulas;
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Applies a formula to one or more selected cells in a table. A formula can consist of
	/// basic mathematical operators, parenthesis, and most math functions such as abs, sum,
	/// average, sin, etc.
	/// </summary>
	/// <remarks>
	/// If you use cell references in your formula and you've selected more than one cell then
	/// OneMore will automatically increment the references relative to each seleted cell. For
	/// example, if you select cells A10, B10, and C10 and enter the formula sum(A1:A9) then that
	/// will apply to A10, sum(B1:B9) will apply to B10, and sum(C1:C9) will apply to C10.
	/// 
	/// Formula processing is not recursive.This means that if cell A1 has a formula "A2+1" and
	/// cell A2 has a formula "1+1", then when A1 is calculated, it will not force A2 to be
	/// recalculated. Instead, each cell is calculated in order, top-down and left-to-right
	/// across the table.
	/// </remarks>
	internal class AddFormulaCommand : Command
	{
		public const string BoltSymbol = "140";


		public AddFormulaCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out var ns);

			if (!page.ConfirmBodyContext())
			{
				ShowInfo(Resx.FormulaCommand_SelectOne);
				return;
			}

			// Find first selected cell as anchor point to locate table into which
			// the formula should be inserted; By filtering on selected=all, we avoid
			// including the parent table of a selected nested table.

			var anchor = page.Root.Descendants(ns + "Cell")
				// first dive down to find the selected T
				.Elements(ns + "OEChildren").Elements(ns + "OE")
				.Elements(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all")
				// now move back up to the Cell
				.Select(e => e.Parent.Parent.Parent)
				.FirstOrDefault();

			if (anchor == null)
			{
				ShowInfo(Resx.FormulaCommand_SelectOne);
				return;
			}

			var table = new Table(anchor.FirstAncestor(ns + "Table"));
			var cells = table.GetSelectedCells(out var range).ToList();

			if (range == TableSelectionRange.Rectangular)
			{
				ShowInfo(Resx.FormulaCommand_Linear);
				return;
			}

			using var dialog = new FormulaDialog();

			// display selected cell names
			if (cells.Count == 1)
			{
				dialog.SetCellNames(cells[0].Coordinates);
				dialog.SetResultRow(cells[0].RowNum);
			}
			else
			{
				dialog.SetCellNames(
					$"{cells[0].Coordinates} - {cells[cells.Count - 1].Coordinates}");

				if (range == TableSelectionRange.Rows)
				{
					dialog.SetResultRow(cells[cells.Count - 1].RowNum);
				}
			}

			var cell = cells[0];

			// display formula of first cell if any
			var formula = new Formula(cell);
			if (formula.Valid)
			{
				dialog.Format = formula.Format;
				dialog.Formula = formula.Expression;
				dialog.DecimalPlaces = formula.DecimalPlaces;
			}

			var tagIndex = page.GetTagDefIndex(BoltSymbol);
			if (!string.IsNullOrEmpty(tagIndex))
			{
				if (cell.HasTag(tagIndex))
				{
					dialog.Tagged = true;
				}
				else
				{
					tagIndex = null;
				}
			}

			if (dialog.ShowDialog(owner) != DialogResult.OK)
			{
				return;
			}

			if (dialog.Tagged)
			{
				tagIndex = page.AddTagDef(BoltSymbol, Resx.AddFormulaCommand_Calculated);
			}

			StoreFormula(table, cells,
				dialog.Formula, dialog.Format, dialog.DecimalPlaces,
				range, tagIndex);

			var processor = new Processor(table);
			processor.Execute(cells);

			await one.Update(page);
		}


		private void StoreFormula(
			Table table, IEnumerable<TableCell> cells,
			string expression, FormulaFormat format, int dplaces,
			TableSelectionRange rangeType, string tagIndex)
		{
			if (rangeType == TableSelectionRange.Single)
			{
				var cell = cells.First();

				cell.SetMeta("omfx", new Formula(format, dplaces, expression).ToString());
				if (!string.IsNullOrEmpty(tagIndex))
				{
					cell.SetTag(tagIndex);
				}

				return;
			}

			var regex = new Regex(Processor.OffsetPattern);

			int offset = 0;
			foreach (var cell in cells)
			{
				var builder = new StringBuilder(expression);
				if (offset > 0)
				{
					var matches = regex.Matches(expression);
					foreach (Match match in matches)
					{
						string col;
						string row;

						if (rangeType == TableSelectionRange.Columns)
						{
							col = TableCell.IndexToLetters(
								TableCell.LettersToIndex(match.Groups["c"].Value) + offset);

							row = match.Groups["r"].Value;
						}
						else
						{
							col = match.Groups["c"].Value;

							var r = int.Parse(match.Groups["r"].Value);
							row = match.Groups["o"].Success
								? $"-{(table.RowCount - r + offset)}"
								: (r + offset).ToString();
						}

						builder.Replace(match.Value, $"{col}{row}", match.Index, match.Length);
					}
				}

				cell.SetMeta("omfx", new Formula(format, dplaces, builder.ToString()).ToString());

				if (!string.IsNullOrEmpty(tagIndex))
				{
					cell.SetTag(tagIndex);
				}

				offset++;
			}
		}
	}
}
