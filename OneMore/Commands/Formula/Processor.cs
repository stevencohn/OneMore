//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Formula
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	internal class Processor
	{
		private readonly ILogger logger;
		private readonly Table table;
		private int maxdec;


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
				var formula = new Formula(cell.GetMeta("omfx"));
				if (!formula.Valid)
				{
					logger.WriteLine($"Cell {cell.Coordinates} is missing its formula");
					continue;
				}

				if (formula.Version == 0)
				{
					Execute(cell, formula.Range, formula.Expression, formula.Format);
					continue;
				}

				try
				{
					var result = calculator.Execute(formula.Expression);

					Report(cell, formula, result);
				}
				catch (Exception exc)
				{
					logger.WriteLine($"Error calculating {cell.Coordinates} formula '{formula}'", exc);
					UIHelper.ShowError(exc.Message);
				}
			}
		}


		private void ResolveCellReference(object sender, SymbolEventArgs e)
		{
			var cell = table.GetCell(e.Name.ToUpper());
			if (cell != null)
			{
				var text = cell.GetContent()
					.Replace("$", string.Empty)
					.Replace("%", string.Empty);

				if (double.TryParse(text, out var value))
				{
					maxdec = Math.Max(value.ToString().Length - ((int)value).ToString().Length - 1, maxdec);

					e.Result = value;
					e.Status = SymbolStatus.OK;
				}
				else
					e.Status = SymbolStatus.None;
			}
			else
			{
				e.Status = SymbolStatus.UndefinedSymbol;
			}
		}


		private void Report(TableCell cell, Formula formula, double result)
		{
			int dplaces = formula.Version >= 2 ? formula.Version : maxdec;

			string text = string.Empty;
			switch (formula.Format)
			{
				case FormulaFormat.Currency:
					text = result.ToString($"C{dplaces}");
					break;

				case FormulaFormat.Number:
					text = result.ToString($"N{dplaces}");
					break;

				case FormulaFormat.Percentage:
					text = $"{(result / 100):P}";
					break;
			}

			cell.SetContent(text);

			//logger.WriteLine($"Cell {cell.Coordinates} calculated result = ({text})");
		}


		#region Version0_obsolete

		private void Execute(TableCell cell, string range, string func, FormulaFormat format)
		{
			if (!Enum.TryParse<FormulaRangeType>(range, true, out var rangeType))
				return;

			if (!Enum.TryParse<FormulaFunction>(func, true, out var function))
				return;

			var values = CollectValues(cell.Root, rangeType);
			var result = Calculate(values, function);

			string text = string.Empty;
			switch (format)
			{
				case FormulaFormat.Currency:
					text = $"{result:C}";
					break;

				case FormulaFormat.Number:
					// show only max precision from all values
					var prec = values.Max(v => Math.Max(v.ToString().Length - ((int)v).ToString().Length - 1, 0));
					text = result.ToString($"N{prec}");
					break;

				case FormulaFormat.Percentage:
					text = $"{(result / 100):P}";
					break;
			}

			cell.SetContent(text);
		}


		private List<decimal> CollectValues(XElement cell, FormulaRangeType rangeType)
		{
			var values = new List<decimal>();
			var ns = cell.GetNamespaceOfPrefix("one");

			if (rangeType == FormulaRangeType.Rows)
			{
				var index = cell.ElementsBeforeSelf(ns + "Cell").Count();
				foreach (var row in cell.Parent.ElementsBeforeSelf(ns + "Row"))
				{
					var value = ReadCell(row.Elements(ns + "Cell").ElementAt(index), ns);
					if (value != null)
					{
						values.Add((decimal)value);
					}
				}
			}
			else
			{
				foreach (var element in cell.ElementsBeforeSelf(ns + "Cell"))
				{
					var value = ReadCell(element, ns);
					if (value != null)
					{
						values.Add((decimal)value);
					}
				}
			}

			return values;
		}


		private decimal? ReadCell(XElement cell, XNamespace ns)
		{
			var children = cell.Elements(ns + "OEChildren");
			if (children?.Count() == 1)
			{
				var child = children.First().Elements(ns + "OE");
				if (child?.Count() == 1)
				{
					var text = child.First().Value;
					if (decimal.TryParse(text, out decimal value))
					{
						return value;
					}
				}
			}

			return null;
		}


		public decimal Calculate(List<decimal> values, FormulaFunction function)
		{
			decimal result = 0.0M;

			switch (function)
			{
				case FormulaFunction.Average:
					result = values.Average();
					break;

				case FormulaFunction.Max:
					result = values.Max();
					break;

				case FormulaFunction.Median:
					result = values.Median();
					break;

				case FormulaFunction.Min:
					result = values.Min();
					break;

				case FormulaFunction.Mode:
					result = values.Mode();
					break;

				case FormulaFunction.Range:
					result = values.Max() - values.Min();
					break;

				case FormulaFunction.StandardDeviation:
					result = values.StandardDeviation();
					break;

				case FormulaFunction.Sum:
					result = values.Sum();
					break;

				case FormulaFunction.Variance:
					result = values.Variance();
					break;
			}

			return result;
		}

		#endregion Version0_obsolete
	}
}
