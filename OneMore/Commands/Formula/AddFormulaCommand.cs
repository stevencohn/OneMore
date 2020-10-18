//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Commands.Formula;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;


	internal class AddFormulaCommand : Command
	{
		private ApplicationManager manager;


		public AddFormulaCommand() : base()
		{
		}


		public void Execute()
		{
			try
			{
				using (manager = new ApplicationManager())
				{
					var page = new Page(manager.CurrentPage());
					var ns = page.Namespace;

					// find first selected cell as anchor point to locate table
					var anchor = page.Root.Descendants(ns + "Cell")?
						.Select(o => new { cell = o, selected = o.Attribute("selected")?.Value })
						.Where(o => o.selected == "all" || o.selected == "partial")
						.Select(o => o.cell)
						.FirstOrDefault();

					if (anchor == null)
					{
						UIHelper.ShowMessage(manager.Window, "Select one or more table cells");
						return;
					}

					var table = new Table(anchor.Ancestors(ns + "Table").First());
					var cells = table.GetSelectedCells();

					var direction = InferDirection(cells);
					if (direction == FormulaDirection.Rectangular)
					{
						UIHelper.ShowMessage(manager.Window, "Selected cells must be in the same row or the same column");
						return;
					}

					using (var dialog = new Dialogs.FormulaDialog())
					{
						dialog.SetCellNames(
							string.Join(", ", cells.Select(c => c.Coordinates)) + $" ({direction})");

						if (dialog.ShowDialog(owner) != DialogResult.OK)
						{
							return;
						}
					}

					/*
					var processor = new Processor(ns,
						FormulaDirection.Vertical, FormulaFunction.Sum, FormulaFormat.Number);

					if (processor.Direction != FormulaDirection.Error)
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
									processor.ParseFormula(formula);
									dialog.Direction = processor.Direction;
									dialog.Format = processor.Format;
									dialog.Function = processor.Function;
								}
							}

							var diaresult = dialog.ShowDialog(owner);
							if (diaresult == DialogResult.OK)
							{
								processor.Direction = dialog.Direction;
								processor.Format = dialog.Format;
								processor.Function = dialog.Function;

								foreach (var cell in cells)
								{
									var values = processor.CollectValues(cell);
									var result = processor.Calculate(values);
									processor.ReportResult(cell, result, values);
								}

								manager.UpdatePageContent(page.Root);
							}
						}
					}
					*/
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("AddFormula", exc);
			}
		}


		private FormulaDirection InferDirection(IEnumerable<TableCell> cells)
		{
			if (cells.Count() == 1)
			{
				return FormulaDirection.Single;
			}

			var col = -1;
			var row = -1;
			foreach (var cell in cells)
			{
				if (col < 0)
					col = cell.ColNum;
				else if (col != int.MaxValue && col != cell.ColNum)
					col = int.MaxValue;

				if (row < 0)
					row = cell.RowNum;
				else if (row != int.MaxValue && row != cell.RowNum)
					row = int.MaxValue;
			}

			if (col == int.MaxValue && row == int.MaxValue)
			{
				return FormulaDirection.Rectangular;
			}
			else if (col == int.MaxValue)
			{
				return FormulaDirection.Vertical;
			}

			return FormulaDirection.Horizontal;
		}
	}
}
