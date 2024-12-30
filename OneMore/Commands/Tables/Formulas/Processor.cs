//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	internal class Processor : Loggable
	{
		private readonly Table table;
		private int maxdec;
		private List<TagDef> tags;


		public Processor(Table table)
		{
			this.table = table;
			maxdec = 0;
		}


		public void Execute(IEnumerable<TableCell> cells)
		{
			var calculator = new Calculator();
			calculator.SetVariable("tablecols", table.ColumnCount);
			calculator.SetVariable("tablerows", table.RowCount);

			calculator.GetCellValue += GetCellValue;

			foreach (var cell in cells)
			{
				var formula = new Formula(cell);
				if (!formula.Valid)
				{
					logger.WriteLine($"cell {cell.Coordinates} is missing its formula");
					continue;
				}

				try
				{
					calculator.SetVariable("col", cell.ColNum);
					calculator.SetVariable("row", cell.RowNum);

					var result = calculator.Compute(formula.Expression);

					Report(cell, formula, result);
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error calculating {cell.Coordinates} formula '{formula}'", exc);
					UI.MoreMessageBox.ShowError(null, exc.Message);
				}
			}
		}


		private void GetCellValue(object sender, GetCellValueEventArgs e)
		{
			var cell = table.GetCell(e.Name.ToUpper());
			if (cell is null)
			{
				return;
			}

			var text = cell.GetText().Trim()
				.Replace(AddIn.Culture.NumberFormat.CurrencySymbol, string.Empty)
				.Replace(AddIn.Culture.NumberFormat.PercentSymbol, string.Empty);

			// common case is double
			if (double.TryParse(text, out var dvalue)) // Culture-specific user input?!
			{
				maxdec = Math.Max(dvalue.ToString().Length - ((int)dvalue).ToString().Length - 1, maxdec);

				e.Value = dvalue.ToString();
				return;
			}

			if (TimeSpan.TryParse(text, AddIn.Culture, out var tvalue))
			{
				// timespans are returned as milliseconds, to be converted
				// back to formatted strings by the Report() method
				e.Value = tvalue.TotalMilliseconds.ToString();
				return;
			}

			// has a todo checkbox? If so then the comparison is limited to the checkbox
			// and WILL NOT fall thru to a string comparison!
			var tagx = cell.Root.Descendants().FirstOrDefault(d => d.Name.LocalName == "Tag");
			if (tagx != null)
			{
				var index = tagx.Attribute("index").Value;
				if (index != null)
				{
					tags ??= DiscoverToDoTags();

					var tag = tags.Find(t => t.Index == index);
					if (tag != null)
					{
						if (tag.IsToDo())
						{
							e.Value = (tagx.Attribute("completed").Value == "true").ToString();
							return;
						}
					}
				}
			}

			// can text be interpereted as a boolean?
			if (bool.TryParse(text, out var bvalue))
			{
				e.Value = bvalue.ToString();
				return;
			}

			// treat it as a string
			e.Value = text;
		}


		private List<TagDef> DiscoverToDoTags()
		{
			var pageElement = table.Root.Ancestors().FirstOrDefault(e => e.Name.LocalName == "Page");
			if (pageElement == null)
			{
				return new List<TagDef>();
			}

			var page = new Page(pageElement);
			return TagMapper.GetTagDefs(page).Where(d => d.IsToDo()).ToList();
		}


		private void Report(TableCell cell, Formula formula, double result)
		{
			var dplaces = formula.Version >= 2 ? formula.DecimalPlaces : maxdec;

			var text = string.Empty;
			switch (formula.Format)
			{
				case FormulaFormat.Currency:
					text = result.ToString($"C{dplaces}", AddIn.Culture);
					break;

				case FormulaFormat.Number:
					text = result.ToString($"N{dplaces}", AddIn.Culture);
					break;

				case FormulaFormat.Percentage:
					text = (result / 100).ToString($"P{dplaces}", AddIn.Culture);
					break;

				case FormulaFormat.Time:
					var span = TimeSpan.FromMilliseconds(result);
					text = span.ToString();
					break;
			}

			cell.SetContent(text);

			//logger.WriteLine(
			//	$"Cell {cell.Coordinates} calculated {result}, " +
			//	$"formatted \"{text}\" for culture {AddIn.Culture.Name}");
		}
	}
}
