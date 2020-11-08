//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class InsertCellsCommand : Command
	{

		public InsertCellsCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{
				// Find first selected cell as anchor point to locate table ; By filtering on
				// selected=all, we avoid including the parent table of a selected nested table.

				var anchor = page.Root.Descendants(ns + "Cell")
					// first dive down to find the selected T
					.Elements(ns + "OEChildren").Elements(ns + "OE")
					.Elements(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					// now move back up to the Cell
					.Select(e => e.Parent.Parent.Parent)
					.FirstOrDefault();

				if (anchor == null)
				{
					UIHelper.ShowInfo(one.Window, Resx.FormulaCommand_SelectOne);
					return;
				}

				var table = new Table(anchor.FirstAncestor(ns + "Table"));
				var cells = table.GetSelectedCells(out var range).ToList();

				var shiftDown = true;
				var numCells = 1;

				using (var dialog = new InsertCellsDialog())
				{
					if (dialog.ShowDialog(owner) != DialogResult.OK)
					{
						return;
					}

					shiftDown = dialog.ShiftDown;
					numCells = dialog.NumCells;
				}

				if (shiftDown)
				{
					ShiftDown(table, cells, numCells);
				}
				else
				{
					ShiftRight(table, cells, numCells);
				}

				one.Update(page);
			}
		}


		private void ShiftDown(Table table, List<TableCell> cells, int numCells)
		{

			System.Diagnostics.Debugger.Launch();


			// RowNum and ColNum are 1-based so must adjust them to be 0-based

			var minCol = cells.Min(c => c.ColNum) - 1;
			var maxCol = cells.Max(c => c.ColNum) - 1;
			var minRow = cells.Min(c => c.RowNum) - 1;
			var maxRow = cells.Max(c => c.RowNum) - 1;

			// if rectangular then only move rectangle
			if (minRow == maxRow)
			{
				// find last row in columns that contains visible text
				var found = false;
				maxRow = table.Rows.Count() - 1;
				while (maxRow > minRow && !found)
				{
					for (var i = minCol; i <= maxCol; i++)
					{
						if (!string.IsNullOrEmpty(table[maxRow][i].GetText()))
						{
							found = true;
							break;
						}
					}

					if (!found)
					{
						maxRow--;
					}
				}
			}

			// add rows to make room for all visible text
			int overflow = (maxRow + 1) + numCells;
			if (overflow > table.Rows.Count())
			{
				for (var i = 0; i < maxRow - table.Rows.Count(); i++) table.AddRow();
			}

			int offset = numCells - 1;

			// start one row below
			for (var r = maxRow + 1; r > minRow; r--)
			{
				for (var c = minCol; c <= maxCol; c++)
				{
					var source = table[r - 1 + offset][c];

					table[r + offset][c].SetContent(source.GetContent());
					source.SetContent(string.Empty);
				}
			}
		}


		private void ShiftRight(Table table, List<TableCell> cells, int numCells)
		{
			//
		}
	}
}
