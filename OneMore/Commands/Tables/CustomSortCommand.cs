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
	/// Sorts the rows of the current table by one or more columns, each with its own
	/// direction and data type.
	/// </summary>
	internal class CustomSortCommand : Command
	{
		public CustomSortCommand()
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
				ShowInfo(Resx.CustomSortCommand_NoSelection);
				return;
			}

			var table = new Table(anchor.FirstAncestor(ns + "Table"));

			var allRows = table.Rows.ToList();
			var skip = table.HasHeaderRow ? 1 : 0;
			var dataRows = allRows.Skip(skip).ToList();
			if (dataRows.Count < 2)
			{
				ShowInfo(Resx.CustomSortCommand_NoRows);
				return;
			}

			var columnCount = table.ColumnCount;
			var text = dataRows
				.Select(r => r.Cells.Select(c => c.GetText()).ToArray())
				.ToList();

			var names = GetColumnNames(table, allRows, columnCount);
			var types = new List<SortDataType>();
			for (var c = 0; c < columnCount; c++)
			{
				var column = c;
				types.Add(TableSorter.Detect(
					text.Select(t => column < t.Length ? t[column] : null), AddIn.Locale));
			}

			using var dialog = new CustomSortDialog(names, types);
			if (dialog.ShowDialog(owner) != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}

			var order = TableSorter.Sort(text, dialog.Rules, AddIn.Locale);

			// detach all data rows, then re-append in sorted order; AddRow appends after the
			// last remaining row (the header) or after the Columns element
			var roots = dataRows.Select(r => r.Root).ToList();
			foreach (var root in roots)
			{
				root.Remove();
			}

			foreach (var index in order)
			{
				table.AddRow(roots[index]);
			}

			await one.Update(page);
		}


		private static List<string> GetColumnNames(
			Table table, List<TableRow> rows, int columnCount)
		{
			var header = table.HasHeaderRow ? rows[0].Cells.ToList() : null;
			var names = new List<string>();

			for (var c = 0; c < columnCount; c++)
			{
				var name = header is not null && c < header.Count
					? header[c].GetText()?.Trim()
					: null;

				names.Add(string.IsNullOrEmpty(name)
					? string.Format(Resx.CustomSortDialog_ColumnName, c + 1)
					: name);
			}

			return names;
		}
	}
}
