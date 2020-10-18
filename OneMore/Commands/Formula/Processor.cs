//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Formula
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;


	internal class Processor
	{
		private readonly Table table;
		private int precision;
		private ILogger logger;


		public Processor(Table table)
		{
			this.table = table;
			logger = Logger.Current;
		}


		public void Execute(IEnumerable<TableCell> cells)
		{
			var calculator = new Calculator();
			calculator.ProcessSymbol += LookupCellValue;

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

				if (parts.Length < 3 || !Enum.TryParse<FormulaFormat>(parts[1], out var format))
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


		private void LookupCellValue(object sender, SymbolEventArgs e)
		{
			var cell = table.GetCell(e.Name.ToUpper());
			if (cell != null)
			{
				if (double.TryParse(cell.GetContent(), out var value))
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

			logger.WriteLine($"Cell {cell.Coordinates} calculated result = ({text})");
		}


		// ----------
		// Version 0

		private void Execute(TableCell cell, string range, string func, string form)
		{
			if (!Enum.TryParse<FormulaRangeType>(range, out var rangeType))
				return;

			if (!Enum.TryParse<FormulaFunction>(func, out var function))
				return;

			if (!Enum.TryParse<FormulaFormat>(form, out var format))
				return;

			//
		}
		/*
		private List<decimal> CollectValues(XElement cell)
		{
			var values = new List<decimal>();

			if (Direction == FormulaRangeType.Rows)
			{
				var index = cell.ElementsBeforeSelf(ns + "Cell").Count();
				foreach (var row in cell.Parent.ElementsBeforeSelf(ns + "Row"))
				{
					var value = ReadCell(row.Elements(ns + "Cell").ElementAt(index));
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
					var value = ReadCell(element);
					if (value != null)
					{
						values.Add((decimal)value);
					}
				}
			}

			return values;
		}


		public decimal Calculate(List<decimal> values)
		{
			decimal result = 0.0M;

			switch (Function)
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


		private decimal? ReadCell(XElement cell)
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


		public void ReportResult(XElement cell, decimal result, List<decimal> values)
		{
			string report = string.Empty;
			switch (Format)
			{
				case FormulaFormat.Currency:
					report = $"{result:C}";
					break;

				case FormulaFormat.Number:
					// show only max precision from all values
					var precision = values.Max(v => Math.Max(v.ToString().Length - ((int)v).ToString().Length - 1, 0));
					report = result.ToString("N" + precision.ToString());
					break;

				case FormulaFormat.Percentage:
					report = $"{(result / 100):P}";
					break;
			}

			var content = new XElement(ns + "OEChildren",
				new XElement(ns + "OE",
					new XElement(ns + "Meta",
						new XAttribute("name", "omfx"),
						new XAttribute("content", $"0;{Direction};{Format};{Function}")
						),
					new XElement(ns + "T", report)
					)
				);

			if (cell.HasElements)
			{
				cell.Descendants().Remove();
				cell.Add(content);
			}
			else
			{
				cell.Add(content);
			}
		}
		*/
	}
}
