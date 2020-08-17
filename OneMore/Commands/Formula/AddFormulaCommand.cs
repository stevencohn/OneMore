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

		private FormulaDirection direction;
		private FormulaFormat format;
		private FormulaFunction function;


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
					direction = InferDirection(cells);
					if (direction != FormulaDirection.Error)
					{
						using (var dialog = new FormulaDialog())
						{
							dialog.Direction = direction;

							var diaresult = dialog.ShowDialog(owner);
							if (diaresult == DialogResult.OK)
							{
								direction = dialog.Direction;
								format = dialog.Format;
								function = dialog.Function;

								var calculator = new Calculator(ns, direction, function, format);

								foreach (var cell in cells)
								{
									var values = calculator.CollectValues(cell);
									var result = calculator.Calculate(values);
									calculator.ReportResult(cell, result);
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
	}
}
