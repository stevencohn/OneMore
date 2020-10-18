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
		private int precision;


		public Processor(Table table)
		{
			this.table = table;
			logger = Logger.Current;
		}


		public void Execute(IEnumerable<TableCell> cells)
		{
			var calculator = new Calculator();
			calculator.ProcessSymbol += ResolveCellReference;

			foreach (var cell in cells)
			{
				var formula = cell.GetMeta("omfx");
				if (formula == null)
				{
					logger.WriteLine($"Cell {cell.Coordinates} is missing its formula");
					continue;
				}

				var parts = formula.Split(';');
				if (parts[0] == "0") // Version 0
				{
					Execute(cell, parts[1], parts[2], parts[3]);
					continue;
				}

				if (parts.Length < 3 || !Enum.TryParse<FormulaFormat>(parts[1], true, out var format))
				{
					logger.WriteLine($"Cell {cell.Coordinates} has bad function {formula}");
					continue;
				}

				formula = parts[2];

				precision = 0;

				try
				{
					var result = calculator.Execute(formula);

					Report(cell, result, format);
				}
				catch (Exception exc)
				{
					logger.WriteLine($"Error calculating {cell.Coordinates} formula '{formula}'", exc);
					throw;
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
					precision = Math.Max(value.ToString().Length - ((int)value).ToString().Length - 1, precision);
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


		private void Report(TableCell cell, double result, FormulaFormat format)
		{
			string text = string.Empty;
			switch (format)
			{
				case FormulaFormat.Currency:
					text = $"{result:C}";
					break;

				case FormulaFormat.Number:
					text = result.ToString($"N{precision}");
					break;

				case FormulaFormat.Percentage:
					text = $"{(result / 100):P}";
					break;
			}

			cell.SetContent(text);

			//logger.WriteLine($"Cell {cell.Coordinates} calculated result = ({text})");
		}


		#region Version0_obsolete

		private void Execute(TableCell cell, string range, string func, string form)
		{
			if (!Enum.TryParse<FormulaRangeType>(range, true, out var rangeType))
				return;

			if (!Enum.TryParse<FormulaFunction>(func, true, out var function))
				return;

			if (!Enum.TryParse<FormulaFormat>(form, true, out var format))
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
