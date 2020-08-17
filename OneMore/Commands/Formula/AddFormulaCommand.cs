//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class AddFormulaCommand : Command
	{
		private ApplicationManager manager;
		private Page page;
		private XNamespace ns;


		public AddFormulaCommand() : base()
		{
		}


		public void Execute()
		{
			using (manager = new ApplicationManager())
			{
				page = new Page(manager.CurrentPage());
				ns = page.Namespace;

				var cells = page.Root.Descendants(ns + "Cell")?
					.Select(o => new { cell = o, selected = o.Attribute("selected")?.Value })
					.Where(o => o.selected == "all" || o.selected == "partial")
					.Select(o => o.cell);

				if (cells != null && cells.Count() > 0)
				{
					var direction = InferDirection(cells);
					if (direction != FormulaDirection.Error)
					{
						using (var dialog = new FormulaDialog())
						{
							dialog.Direction = direction;

							var result = dialog.ShowDialog(owner);
							if (result == DialogResult.OK)
							{
								Calculate(cells, dialog.Direction, dialog.Format, dialog.Function);

								manager.UpdatePageContent(page.Root);
							}
						}
					}
				}
			}
		}


		private FormulaDirection InferDirection(IEnumerable<XElement> cells)
		{
			var rows = new List<XElement>();
			foreach (var cell in cells)
			{
				if (!rows.Contains(cell.Parent))
				{
					rows.Add(cell.Parent);
				}
			}

			// either all cells must share the same row (e.g. last row selected)
			// or all cells must be from different rows (e.g. last col selected)

			var direction = FormulaDirection.Vertical;
			var rowsCount = rows.Count();
			var cellsCount = cells.Count();

			if (cellsCount == 1)
			{
				// one cell selected in last column, not last row?
				var cell = cells.First();
				if (cell.ElementsAfterSelf().Count() == 0 &&		// right
					cell.Parent.ElementsAfterSelf().Count() > 0)	// below
				{
					direction = FormulaDirection.Horizontal;
				}
			}
			else if (rowsCount == cellsCount && rowsCount > 1)
			{
				// all selected cells in one column
				direction = FormulaDirection.Horizontal;
			}
			else if (!(rowsCount == 1 || rowsCount == cellsCount))
			{
				// multiple rows and columns (rectangular selection)
				direction = FormulaDirection.Error;
			}

			if (direction == FormulaDirection.Error)
			{
				UIHelper.ShowMessage(manager.Window,
					"Select one or more cells from the same row or the same column");
			}

			return direction;
		}


		private void Calculate(IEnumerable<XElement> cells,
			FormulaDirection direction, FormulaFormat format, FormulaFunction function)
		{
			foreach (var cell in cells)
			{
				var values = CollectValues(cell, direction);
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
						result = values.CalculateMedian();
						break;

					case FormulaFunction.Min:
						result = values.Min();
						break;

					case FormulaFunction.Mode:
						result = values.CalculateMode();
						break;

					case FormulaFunction.Range:
						result = values.Max() - values.Min();
						break;

					case FormulaFunction.StandardDeviation:
						result = values.CalculateStandardDeviation();
						break;

					case FormulaFunction.Sum:
						result = values.Sum();
						break;

					case FormulaFunction.Variance:
						result = values.CalculateVariance();
						break;
				}

				ReportResult(cell, result, format);
			}
		}


		private List<decimal> CollectValues(XElement cell, FormulaDirection direction)
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


		private void ReportResult(XElement cell, decimal result, FormulaFormat format)
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
					new XElement(ns + "T", report)
					)
				);

			if (cell.HasElements)
			{
				cell.Element(ns + "OEChildren").ReplaceWith(content);
			}
			else
			{
				cell.Add(content);
			}
		}
	}
}
