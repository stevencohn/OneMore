//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Tables.FillCellModels;
	using River.OneMoreAddIn.Models;
	using System;
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


		public FillCellsCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out ns))
			{
				// Find first selected cell as anchor point to locate table ; By filtering on
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

				var action = (FillCells)args[0];

				var updated = action == FillCells.CopyAcross || action == FillCells.CopyDown
					? Copy(table, cells, action)
					: Fill(table, cells, action);

				if (updated)
				{
					await one.Update(page);
				}
			}
		}


		private bool Copy(Table table, List<TableCell> cells, FillCells action)
		{
			// RowNum and ColNum are 1-based so must shift them to be 0-based
			var minCol = cells.Min(c => c.ColNum) - 1;
			var maxCol = cells.Max(c => c.ColNum) - 1;
			var minRow = cells.Min(c => c.RowNum) - 1;
			var maxRow = cells.Max(c => c.RowNum) - 1;

			var updated = false;

			if (action == FillCells.CopyAcross)
			{
				if (maxCol == minCol)
				{
					maxCol = table.ColumnCount - 1;
				}

				for (int ri = minRow; ri <= maxRow; ri++)
				{
					var row = table[ri];

					var content = row[minCol].GetContent().Clone();
					content.Descendants(ns + "T")
						.Where(e => e.Attribute("selected")?.Value == "all")
						.ToList()
						.ForEach((e) => { e.Attribute("selected").Remove(); });

					for (int ci = minCol + 1; ci <= maxCol; ci++)
					{
						row[ci].SetContent(content);
						updated = true;
					}
				}
			}
			else
			{
				if (maxRow == minRow)
				{
					maxRow = table.RowCount - 1;
				}

				for (int ci = minCol; ci <= maxCol; ci++)
				{
					var content = table[minRow][ci].GetContent().Clone();
					content.Descendants(ns + "T")
						.Where(e => e.Attribute("selected")?.Value == "all")
						.ToList()
						.ForEach((e) => { e.Attribute("selected").Remove(); });

					for (int ri = minRow + 1; ri <= maxRow; ri++)
					{
						table[ri][ci].SetContent(content);
						updated = true;
					}
				}
			}

			return updated;
		}


		private bool Fill(Table table, List<TableCell> cells, FillCells action)
		{
			System.Diagnostics.Debugger.Launch();


			// RowNum and ColNum are 1-based so must shift them to be 0-based
			var minCol = cells.Min(c => c.ColNum) - 1;
			var maxCol = cells.Max(c => c.ColNum) - 1;
			var minRow = cells.Min(c => c.RowNum) - 1;
			var maxRow = cells.Max(c => c.RowNum) - 1;

			var updated = false;

			if (action == FillCells.FillAcross)
			{
				if (maxCol == minCol)
				{
					maxCol = table.ColumnCount - 1;
				}

				if (maxCol == minCol)
				{
					// no room to fill
					return updated;
				}

				IFiller filler;
				var amount = 1;

				if (maxCol == minCol + 1)
				{
					// only one cell to fill, no pattern so increment by 1
					filler = GetFiller(table[minRow][minCol]);
				}
				else
				{
					// determine if there is a repeatable pattern to increment
					amount = Analyze(table[minRow][minCol], table[minRow][minCol + 1], out filler);
				}

				if (amount == 0)
				{
					UIHelper.ShowInfo("nothign to fill");
					return false;
				}

				for (int ri = minRow; ri <= maxRow; ri++)
				{
					var row = table[ri];
					for (int ci = minCol + 1; ci <= maxCol; ci++)
					{
						var content = filler.Increment(amount);
						row[ci].SetContent(content);
						updated = true;
					}
				}
			}
			else
			{
				if (maxRow == minRow)
				{
					maxRow = table.RowCount - 1;
				}

				if (maxRow == minRow)
				{
					// no room to fill
					return updated;
				}

				IFiller filler;
				var amount = 1;

				if (maxRow == minRow + 1)
				{
					// only one cell to fill, no pattern so increment by 1
					filler = GetFiller(table[minRow][minCol]);
				}
				else
				{
					// determine if there is a repeatable pattern to increment
					amount = Analyze(table[minRow][minCol], table[minRow][minCol + 1], out filler);
				}

				if (amount == 0)
				{
					UIHelper.ShowInfo("nothing to fill");
					return false;
				}

				for (int ci = minCol; ci <= maxCol; ci++)
				{
					for (int ri = minRow + 1; ri <= maxRow; ri++)
					{
						var content = filler.Increment(amount);
						table[ri][ci].SetContent(content);
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

			if (filler.Type == FillType.Date)
			{
				return ((DateFiller)filler2).Value.Subtract(((DateFiller)filler).Value).Days;
			}

			if (filler.Type == FillType.AlphaNumeric)
			{
				return ((AlphaNumericFiller)filler2).Value - ((AlphaNumericFiller)filler).Value;
			}

			// numeric
			return (int)(((NumberFiller)filler2).Value - ((NumberFiller)filler).Value);
		}


		private IFiller GetFiller(TableCell cell)
		{
			var value = cell.GetText();

			if (NumberFiller.CanParse(value))
			{
				return new NumberFiller(value);
			}

			if (DateFiller.CanParse(value))
			{
				return new DateFiller(value);
			}

			if (AlphaNumericFiller.CanParse(value))
			{
				return new AlphaNumericFiller(value);
			}

			return null;
		}
	}
}
