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
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


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
						UIHelper.ShowInfo(manager.Window, Resx.FormulaCommand_SelectOne);
						return;
					}

					var table = new Table(anchor.FirstAncestor(ns + "Table"));
					var cells = table.GetSelectedCells().ToList();

					var rangeType = InferRangeType(cells);
					if (rangeType == FormulaRangeType.Rectangular)
					{
						UIHelper.ShowInfo(manager.Window, Resx.FormulaCommand_Linear);
						return;
					}

					using (var dialog = new Dialogs.FormulaDialog())
					{
						// display selected cell names
						dialog.SetCellNames(
							string.Join(", ", cells.Select(c => c.Coordinates))); // + $" ({rangeType})");

						var cell = cells.First();

						// display formula of first cell if any
						var fx = cell.GetMeta("omfx");
						if (fx != null)
						{
							var parts = fx.Split(';');
							if (parts.Length == 3)
							{
								if (Enum.TryParse<FormulaFormat>(parts[1], true, out var format))
								{
									dialog.Format = format;
								}

								dialog.Formula = parts[2];
							}
						}


						var tagIndex = page.GetTagIndex("140");
						if (!string.IsNullOrEmpty(tagIndex))
						{
							if (cell.HasTag(tagIndex))
							{
								dialog.Tagged = true;
							}
						}

						if (dialog.ShowDialog(owner) != DialogResult.OK)
						{
							return;
						}

						if (dialog.Tagged)
						{
							tagIndex = page.AddTag("140", "Calculated");
						}

						StoreFormula(cells, dialog.Formula, dialog.Format, rangeType, tagIndex);

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
			IEnumerable<TableCell> cells, 
			string formula, FormulaFormat format, FormulaRangeType rangeType, string tagIndex)
		{
			if (rangeType == FormulaRangeType.Single)
			{
				var cell = cells.First();
				cell.SetMeta("omfx", $"1;{format};{formula}");
				//logger.WriteLine($"Cell {cells.First().Coordinates} stored formula '{formula}'");

				if (!string.IsNullOrEmpty(tagIndex))
				{
					cell.SetTag(tagIndex);
				}
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

				if (!string.IsNullOrEmpty(tagIndex))
				{
					cell.SetTag(tagIndex);
				}

				offset++;
			}
		}
	}
}
