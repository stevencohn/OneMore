//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Resx = Properties.Resources;

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


		public void Execute(IEnumerable<TableCell> cells, UI.ProgressDialog progress = null)
		{
			var calculator = new Calculator();
			calculator.SetVariable("tablecols", table.ColumnCount);
			calculator.SetVariable("tablerows", table.RowCount);

			LoadVariables(calculator);

			calculator.GetCellValue += GetCellValue;

			foreach (var cell in cells)
			{
				progress?.SetMessage(string.Format(Resx.FormulaCommand_Progress, cell.Coordinates));

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


		private static void LoadVariables(Calculator calculator)
		{
			using var provider = new VariableProvider();
			var variables = provider.ReadVariables();
			foreach (var variable in variables)
			{
				calculator.SetVariable(variable.Name, variable.Value);
			}
		}


		private void GetCellValue(object sender, GetCellValueEventArgs e)
		{
			var cell = table.GetCell(e.Name.ToUpper());
			if (cell is null)
			{
				return;
			}

			var text = cell.GetText().Trim();

			// common case is double
			if (TryParseNumber(text, out var dvalue))
			{
				var formatted = dvalue.ToString(CultureInfo.InvariantCulture);
				var whole = ((int)dvalue).ToString(CultureInfo.InvariantCulture);
				maxdec = Math.Max(formatted.Length - whole.Length - 1, maxdec);

				e.Value = formatted;
				return;
			}

			if (TimeSpan.TryParse(text, AddIn.Locale, out var tvalue))
			{
				// timespans are returned as milliseconds, to be converted
				// back to formatted strings by the Report() method
				e.Value = tvalue.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
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


		/// <summary>
		/// Parses a table cell's displayed text as a number, tolerant of it having
		/// been formatted under a different culture (currency symbol, decimal
		/// separator) than the one active during this recalculation - e.g. after
		/// a Language change.
		/// </summary>
		private static bool TryParseNumber(string text, out double value)
		{
			// strip currency/percent symbols regardless of which culture produced
			// them - a referenced cell's formula result may have been formatted
			// under a different Language/Locale than the one active now
			var stripped = new string(text.Where(c =>
				char.GetUnicodeCategory(c) != UnicodeCategory.CurrencySymbol &&
				c != '%').ToArray()).Trim();

			// a separator followed by only 1-2 digits can only be a decimal point -
			// no real thousands-grouping convention uses groups smaller than 3
			// digits - so resolve that specific ambiguity ourselves instead of
			// depending on whichever locale happens to be active; treat any other
			// separator characters earlier in the string as thousands decorations
			var last = Math.Max(stripped.LastIndexOf('.'), stripped.LastIndexOf(','));
			if (last >= 0 && stripped.Length - last - 1 is > 0 and <= 2)
			{
				var whole = stripped.Substring(0, last).Replace(".", string.Empty).Replace(",", string.Empty);
				var normalized = $"{whole}.{stripped.Substring(last + 1)}";

				if (double.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
				{
					return true;
				}
			}

			return
				double.TryParse(stripped, NumberStyles.Number, AddIn.Locale, out value) ||
				double.TryParse(stripped, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
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
					text = result.ToString($"C{dplaces}", AddIn.Locale);
					break;

				case FormulaFormat.Number:
					text = result.ToString($"N{dplaces}", AddIn.Locale);
					break;

				case FormulaFormat.Percentage:
					text = (result / 100).ToString($"P{dplaces}", AddIn.Locale);
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
