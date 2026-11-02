//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Formats the values of the selected cells, or of the entire columns containing them,
	/// as numbers, percents, currency, dates, or times. A header row is never formatted.
	/// </summary>
	internal class FormatCellsCommand : Command
	{
		public FormatCellsCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var guard = EnterOnce();
			if (guard is null) { return; }

			await using var one = new OneNote(out var page, out var ns);

			// first selected cell is the anchor used to locate the table; filtering on
			// selected=all avoids the parent table of a selected nested table
			var anchor = page.Root.Descendants(ns + "Cell")
				.Elements(ns + "OEChildren").Elements(ns + "OE")
				.Elements(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all")
				.Select(e => e.Parent.Parent.Parent)
				.FirstOrDefault();

			if (anchor is null)
			{
				ShowInfo(Resx.FormatCellsCommand_NoSelection);
				return;
			}

			var table = new Table(anchor.FirstAncestor(ns + "Table"));
			var selected = table.GetSelectedCells(out _).ToList();

			// RowNum and ColNum are 1-based; the header row is never formatted
			var firstRow = table.HasHeaderRow ? 1 : 0;

			var selectedCells = selected
				.Where(c => c.RowNum - 1 >= firstRow)
				.ToList();

			var columns = selected
				.Select(c => c.ColNum - 1)
				.Distinct()
				.OrderBy(c => c)
				.ToList();

			var columnCells = new List<TableCell>();
			for (var r = firstRow; r < table.RowCount; r++)
			{
				var row = table[r];
				foreach (var column in columns)
				{
					columnCells.Add(row[column]);
				}
			}

			using var dialog = new FormatCellsDialog(
				selectedCells.Select(c => c.GetText()).ToList(),
				columnCells.Select(c => c.GetText()).ToList(),
				columns.Count);

			if (dialog.ShowDialog(owner) != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}

			var format = dialog.Format;
			var cells = format.Scope == FormatScope.EntireColumns ? columnCells : selectedCells;

			var updated = false;

			foreach (var cell in cells)
			{
				// blank and unparseable cells are left alone
				var text = CellFormatter.Format(cell.GetText(), format, AddIn.Locale);
				if (text is not null)
				{
					cell.SetContent(cell, text);
					updated = true;
				}
			}

			if (updated)
			{
				await one.Update(page);
			}
		}
	}
}
