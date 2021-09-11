//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Tables.FillCellModels;
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal enum FillCells
	{
		CopyAcross,
		CopyDown,
		FillAcross,
		FillDown
	}


	internal class FillCellsCommand : Command
	{
		private XNamespace ns;
		private int minCol, maxCol;
		private int minRow, maxRow;


		public FillCellsCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out ns))
			{
				// Find first selected cell as anchor point to locate table; by filtering on
				// selected=all, we avoid including the parent table of a selected nested table.

				var anchor = page.Root.Descendants(ns + "Cell")
					// dive down to find the selected T within a table cell
					.Elements(ns + "OEChildren")
					.Elements(ns + "OE")
					.Elements(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					// now move back up to the Cell
					.Select(e => e.Parent.Parent.Parent)
					.FirstOrDefault();

				if (anchor == null)
				{
					UIHelper.ShowInfo(one.Window, Resx.InsertCellsCommand_NoSelection);
					return;
				}

				var table = new Table(anchor.FirstAncestor(ns + "Table"));
				var cells = table.GetSelectedCells(out var range).ToList();

				// RowNum and ColNum are 1-based so must shift them to be 0-based
				minCol = cells.Min(c => c.ColNum) - 1;
				maxCol = cells.Max(c => c.ColNum) - 1;
				minRow = cells.Min(c => c.RowNum) - 1;
				maxRow = cells.Max(c => c.RowNum) - 1;

				var updated = false;

				var action = (FillCells)args[0];
				switch (action)
				{
					case FillCells.CopyAcross: updated = CopyAcross(table, cells); break;
					case FillCells.CopyDown: updated = CopyDown(table, cells); break;
					case FillCells.FillAcross: updated = FillAcross(table, cells); break;
					case FillCells.FillDown: updated = FillDown(table, cells); break;
				}

				if (updated)
				{
					await one.Update(page);
				}
			}
		}


		private bool CopyAcross(Table table, List<TableCell> cells)
		{
			if (maxCol == minCol)
			{
				maxCol = table.ColumnCount - 1;
			}

			var updated = false;
			for (int ri = minRow; ri <= maxRow; ri++)
			{
				var filler = new GenericFiller(table[ri][minCol]);
				for (int ci = minCol + 1; ci <= maxCol; ci++)
				{
					table[ri][ci].SetContent(filler.Cell, filler.Value);
					updated = true;
				}
			}

			return updated;
		}


		private bool CopyDown(Table table, List<TableCell> cells)
		{
			if (maxRow == minRow)
			{
				maxRow = table.RowCount - 1;
			}

			var updated = false;
			for (int ci = minCol; ci <= maxCol; ci++)
			{
				var filler = new GenericFiller(table[minRow][ci]);
				for (int ri = minRow + 1; ri <= maxRow; ri++)
				{
					table[ri][ci].SetContent(filler.Cell, filler.Value);
					updated = true;
				}
			}

			return updated;
		}


		private bool FillAcross(Table table, List<TableCell> cells)
		{
			if (maxCol == minCol)
			{
				maxCol = table.ColumnCount - 1;
			}

			if (maxCol == minCol)
			{
				// no room to fill
				return false;
			}

			var updated = false;
			for (int ri = minRow; ri <= maxRow; ri++)
			{
				IFiller filler;
				var amount = 1;

				if (maxCol == minCol + 1)
				{
					// only one cell to fill, no pattern so increment by 1
					filler = GetFiller(table[ri][minCol]);
				}
				else
				{
					// determine if there is a repeatable pattern to increment
					amount = Analyze(table[ri][minCol], table[ri][minCol + 1], out filler);
				}

				if (amount > 0)
				{
					for (int ci = minCol + 1; ci <= maxCol; ci++)
					{
						var text = filler.Increment(amount);
						table[ri][ci].SetContent(filler.Cell, text);
						updated = true;
					}
				}
			}

			return updated;
		}


		private bool FillDown(Table table, List<TableCell> cells)
		{
			if (maxRow == minRow)
			{
				maxRow = table.RowCount - 1;
			}

			if (maxRow == minRow)
			{
				// no room to fill
				return false;
			}

			var updated = false;
			for (int ci = minCol; ci <= maxCol; ci++)
			{
				IFiller filler;
				var amount = 1;

				if (maxRow == minRow + 1)
				{
					// only one cell to fill, no pattern so increment by 1
					filler = GetFiller(table[minRow][ci]);
				}
				else
				{
					// determine if there is a repeatable pattern to increment
					amount = Analyze(table[minRow][ci], table[minRow + 1][ci], out filler);
				}

				if (amount > 0)
				{
					for (int ri = minRow + 1; ri <= maxRow; ri++)
					{
						var text = filler.Increment(amount);
						table[ri][ci].SetContent(filler.Cell, text);
						updated = true;
					}
				}
			}

			return updated;
		}


		private int Analyze(TableCell first, TableCell second, out IFiller filler)
		{
			filler = GetFiller(first);
			if (filler == null)
			{
				return 0;
			}

			var filler2 = GetFiller(second);
			if (filler2 == null)
			{
				return 1;
			}

			if (filler2.Type != filler.Type)
			{
				return 0;
			}

			// filler..filler2 is presumed progression
			return filler2.Subtract(filler);
		}


		private IFiller GetFiller(TableCell cell)
		{
			var value = cell.GetText(true);

			if (NumberFiller.CanParse(value))
			{
				return new NumberFiller(cell);
			}

			if (DateFiller.CanParse(value))
			{
				return new DateFiller(cell);
			}

			if (AlphaNumericFiller.CanParse(value))
			{
				return new AlphaNumericFiller(cell);
			}

			return null;
		}
	}
}
