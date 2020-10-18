//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Commands.Formula;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;


	internal class AddFormulaCommand : Command
	{
		private ApplicationManager manager;


		public AddFormulaCommand() : base()
		{
		}


		public void Execute()
		{
			try
			{
				using (manager = new ApplicationManager())
				{
					var page = new Page(manager.CurrentPage());
					var ns = page.Namespace;

					// find first selected cell as anchor point to locate table
					var anchor = page.Root.Descendants(ns + "Cell")?
						.Select(o => new { cell = o, selected = o.Attribute("selected")?.Value })
						.Where(o => o.selected == "all" || o.selected == "partial")
						.Select(o => o.cell)
						.FirstOrDefault();

					if (anchor == null)
					{
						UIHelper.ShowMessage(manager.Window, "Select one or more table cells");
						return;
					}

					var table = new Table(anchor.Ancestors(ns + "Table").First());
					var cells = table.GetSelectedCells().ToList();

					var rangeType = InferRangeType(cells);
					if (rangeType == FormulaRangeType.Rectangular)
					{
						UIHelper.ShowMessage(manager.Window,
							"Selected cells must be in the same row or the same column");

						return;
					}

					using (var dialog = new Dialogs.FormulaDialog())
					{
						// display selected cell names
						dialog.SetCellNames(
							string.Join(", ", cells.Select(c => c.Coordinates)) + $" ({rangeType})");

						// display formula of first cell if any
						var fx = cells.First().GetMeta("omfx");
						if (fx != null)
						{
							var parts = fx.Split(';');
							if (parts.Length == 3)
							{
								if (!Enum.TryParse<FormulaFormat>(parts[1], out var format))
								{
									dialog.Format = format;
								}

								dialog.Formula = parts[2];
							}
						}

						if (dialog.ShowDialog(owner) != DialogResult.OK)
						{
							return;
						}

						StoreFormula(cells, dialog.Formula, dialog.Format, rangeType);

						var processor = new Processor(table);
						processor.Execute(cells);

						manager.UpdatePageContent(page.Root);
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("AddFormula", exc);
			}
		}


		private FormulaRangeType InferRangeType(IEnumerable<TableCell> cells)
		{
			if (cells.Count() == 1)
			{
				return FormulaRangeType.Single;
			}

			var col = -1;
			var row = -1;
			foreach (var cell in cells)
			{
				if (col < 0)
					col = cell.ColNum;
				else if (col != int.MaxValue && col != cell.ColNum)
					col = int.MaxValue;

				if (row < 0)
					row = cell.RowNum;
				else if (row != int.MaxValue && row != cell.RowNum)
					row = int.MaxValue;

				if (col == int.MaxValue && row == int.MaxValue)
					break;
			}

			if (col == int.MaxValue && row == int.MaxValue)
			{
				return FormulaRangeType.Rectangular;
			}
			else if (col == int.MaxValue)
			{
				return FormulaRangeType.Columns;
			}

			return FormulaRangeType.Rows;
		}


		private void StoreFormula(
			IEnumerable<TableCell> cells, string formula, FormulaFormat format, FormulaRangeType rangeType)
		{
			if (rangeType == FormulaRangeType.Single)
			{
				cells.First().SetMeta("omfx", $"1;{format};{formula}");
				//logger.WriteLine($"Cell {cells.First().Coordinates} stored formula '{formula}'");
				return;
			}

			var regex = new Regex(@"([a-zA-Z]{1,3})(\d{1,3})");

			int offset = 0;
			foreach (var cell in cells)
			{
				var builder = new StringBuilder(formula);
				if (offset > 0)
				{
					var matches = regex.Matches(formula);
					foreach (Match match in matches)
					{
						string col;
						string row;

						if (rangeType == FormulaRangeType.Columns)
						{
							col = TableCell.IndexToLetters(
								TableCell.LettersToIndex(match.Groups[1].Value) + offset);

							row = match.Groups[2].Value;
						}
						else
						{
							col = match.Groups[1].Value;
							row = (int.Parse(match.Groups[2].Value) + offset).ToString();
						}

						builder.Replace(match.Value, $"{col}{row}", match.Index, match.Length);
					}
				}

				cell.SetMeta("omfx", $"1;{format};{builder}");
				//logger.WriteLine($"Cell {cell.Coordinates} stored formula '{builder}'");

				offset++;
			}
		}
	}
}
