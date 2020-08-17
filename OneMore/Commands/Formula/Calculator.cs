//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	internal class Calculator
	{
		private readonly FormulaDirection direction;
		private readonly FormulaFunction function;
		private readonly FormulaFormat format;
		private readonly XNamespace ns;


		public Calculator(XNamespace ns,
			FormulaDirection direction, FormulaFunction function, FormulaFormat format)
		{
			this.direction = direction;
			this.function = function;
			this.format = format;
			this.ns = ns;
		}


		public List<decimal> CollectValues(XElement cell)
		{
			var values = new List<decimal>();

			if (direction == FormulaDirection.Vertical)
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


		public void ReportResult(XElement cell, decimal result)
		{
			string report = string.Empty;
			switch (format)
			{
				case FormulaFormat.Currency:
					report = $"{result:C}";
					break;

				case FormulaFormat.Number:
					report = $"{result:N2}";
					break;

				case FormulaFormat.Percentage:
					report = $"{(result / 100):P}";
					break;
			}

			var content = new XElement(ns + "OEChildren",
				new XElement(ns + "OE",
					new XElement(ns + "Meta",
						new XAttribute("name", "omfx"),
						new XAttribute("content", $"0;{direction};{format};{function}")
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
