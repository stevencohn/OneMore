//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
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
		private enum FillType
		{
			None,
			Number,
			Alpha,
			Date
		}


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


				System.Diagnostics.Debugger.Launch();

				var table = new Table(anchor.FirstAncestor(ns + "Table"));
				var cells = table.GetSelectedCells(out var range).ToList();

				var updated = false;
				var action = (FillCells)args[0];

				switch (action)
				{
					case FillCells.CopyAcross:
					case FillCells.CopyDown:
						updated = Copy(table, cells, action);
						break;

					case FillCells.FillAcross:
					case FillCells.FillDown:
						break;
				}

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


		private FillType DetectCellType(string value)
		{
			var flags = NumberStyles.AllowCurrencySymbol |
				NumberStyles.AllowDecimalPoint |
				NumberStyles.AllowThousands;

			if (decimal.TryParse(value, flags, CultureInfo.CurrentCulture, out _))
			{
				return FillType.Number;
			}

			if (Regex.Match(value, "^[a-z]+$|^[A-Z]+$").Success)
				return FillType.Alpha;

			if (DateTime.TryParse(value, out _))
				return FillType.Date;

			return FillType.None;
		}
	}
}
