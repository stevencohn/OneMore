//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	internal class Processor
	{
		private readonly ILogger logger;
		private readonly Table table;
		private int maxdec;
		private List<TagDef> tags;


		public Processor(Table table)
		{
			this.table = table;
			maxdec = 0;

			logger = Logger.Current;
		}


		public void Execute(IEnumerable<TableCell> cells)
		{
			var calculator = new Calculator();
			calculator.ProcessSymbol += ResolveCellReference;

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
					var result = calculator.Execute(formula.Expression);

					Report(cell, formula, result);
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error calculating {cell.Coordinates} formula '{formula}'", exc);
					UIHelper.ShowError(exc.Message);
				}
			}
		}


		private void ResolveCellReference(object sender, SymbolEventArgs e)
		{
			var cell = table.GetCell(e.Name.ToUpper());
			if (cell == null)
			{
				e.Status = SymbolStatus.UndefinedSymbol;
				return;
			}

			var text = cell.GetText().Trim()
				.Replace(AddIn.Culture.NumberFormat.CurrencySymbol, string.Empty)
				.Replace(AddIn.Culture.NumberFormat.PercentSymbol, string.Empty);

			// common case is double
			if (double.TryParse(text, out var dvalue))
			{
				maxdec = Math.Max(dvalue.ToString().Length - ((int)dvalue).ToString().Length - 1, maxdec);

				e.SetResult(dvalue);
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
					if (tags == null)
					{
						tags = DiscoverTags();
					}

					var tag = tags.FirstOrDefault(t => t.Index == index);
					if (tag != null)
					{
						if (tag.IsToDo())
						{
							e.SetResult(tagx.Attribute("completed").Value == "true");
							return;
						}
					}
				}
			}

			// can text be interpereted as a boolean?
			if (bool.TryParse(text, out var bvalue))
			{
				e.SetResult(bvalue);
				return;
			}

			// treat it as a string
			e.SetResult(text);
		}


		private List<TagDef> DiscoverTags()
		{
			var pageElement = table.Root.Ancestors().FirstOrDefault(e => e.Name.LocalName == "Page");
			if (pageElement == null)
			{
				return new List<TagDef>();
			}

			var page = new Page(pageElement);
			var map = page.GetTagDefMap();
			var list = map.Where(m => m.TagDef.IsToDo()).Select(m => m.TagDef).ToList();
			return list;
		}


		private void Report(TableCell cell, Formula formula, double result)
		{
			int dplaces = formula.Version >= 2 ? formula.DecimalPlaces : maxdec;

			string text = string.Empty;
			switch (formula.Format)
			{
				case FormulaFormat.Currency:
					text = result.ToString($"C{dplaces}", AddIn.Culture);
					break;

				case FormulaFormat.Number:
					text = result.ToString($"N{dplaces}", AddIn.Culture);
					break;

				case FormulaFormat.Percentage:
					text = (result / 100).ToString("P", AddIn.Culture);
					break;
			}

			cell.SetContent(text);

			//logger.WriteLine(
			//	$"Cell {cell.Coordinates} calculated {result}, " +
			//	$"formatted \"{text}\" for culture {AddIn.Culture.Name}");
		}
	}
}
