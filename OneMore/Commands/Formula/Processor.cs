//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	internal class Processor
	{
		private readonly XNamespace ns;


		public Processor(XNamespace ns,
			FormulaDirection direction, FormulaFunction function, FormulaFormat format)
		{
			Direction = direction;
			Function = function;
			Format = format;
			this.ns = ns;
		}


		public Processor(XNamespace ns, string formula)
		{
			this.ns = ns;
			ParseFormula(formula);
		}


		public void ParseFormula(string formula)
		{
			var parts = formula.Split(';');

			// version
			if (parts[0] == "0")
			{
				//content="0;Horizontal;Number;Sum"
				Direction = (FormulaDirection)Enum.Parse(typeof(FormulaDirection), parts[1]);
				Format = (FormulaFormat)Enum.Parse(typeof(FormulaFormat), parts[2]);
				Function = (FormulaFunction)Enum.Parse(typeof(FormulaFunction), parts[3]);
			}
		}


		public FormulaDirection Direction { get; set; }

		public FormulaFormat Format { get; set; }

		public FormulaFunction Function { get; set; }


		public List<decimal> CollectValues(XElement cell)
		{
			var values = new List<decimal>();

			if (Direction == FormulaDirection.Vertical)
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
	}
}
