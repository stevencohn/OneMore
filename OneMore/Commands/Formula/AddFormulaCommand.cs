//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using River.OneMoreAddIn.Models;


	internal class AddFormulaCommand : Command
	{
		private ApplicationManager manager;


		public AddFormulaCommand() : base()
		{
		}


		public void Execute()
		{
			using (manager = new ApplicationManager())
			{
				var page = new Page(manager.CurrentPage());
				var ns = page.Namespace;

				// find all selected cells into which a formula should be inserted
				var cells = page.Root.Descendants(ns + "Cell")?
					.Select(o => new { cell = o, selected = o.Attribute("selected")?.Value })
					.Where(o => o.selected == "all" || o.selected == "partial")
					.Select(o => o.cell);

				if (cells != null && cells.Any())
				{
					var calculator = new Calculator(ns, 
						FormulaDirection.Vertical, FormulaFunction.Sum, FormulaFormat.Number);

					if (calculator.Direction != FormulaDirection.Error)
					{
						using (var dialog = new Dialogs.FormulaDialog())
						{
							dialog.Direction = InferDirection(cells);

							if (cells.Count() == 1)
							{
								// if cell already contains a formula, use its paramters
								var formula = cells.First().Descendants(ns + "Meta")
									.Where(e => e.Attribute("name").Value == "omfx")
									.Select(e => e.Attribute("content").Value).FirstOrDefault();

								if (!string.IsNullOrEmpty(formula))
								{
									calculator.ParseFormula(formula);
									dialog.Direction = calculator.Direction;
									dialog.Format = calculator.Format;
									dialog.Function = calculator.Function;
								}
							}

							var diaresult = dialog.ShowDialog(owner);
							if (diaresult == DialogResult.OK)
							{
								calculator.Direction = dialog.Direction;
								calculator.Format = dialog.Format;
								calculator.Function = dialog.Function;

								foreach (var cell in cells)
								{
									var values = calculator.CollectValues(cell);
									var result = calculator.Calculate(values);
									calculator.ReportResult(cell, result, values);
								}

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
			var rowsCount = rows.Count;
			var cellsCount = cells.Count();

			if (cellsCount == 1)
			{
				// one cell selected in last column, not last row?
				var cell = cells.First();
				if (!cell.ElementsAfterSelf().Any() &&		// right
					cell.Parent.ElementsAfterSelf().Any())	// below
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
					Properties.Resources.AddFormula_linearMessage);
			}

			return direction;
		}
	}
}
