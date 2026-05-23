//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>Wraps a one:Table element.</summary>
	internal sealed class TableNode : OneNoteNode
	{
		internal TableNode(XElement el) : base(el) { }


		/// <summary>Creates a new table with the given dimensions and uniform column width.</summary>
		public static TableNode Create(int rows, int columns, double columnWidthPx = 120)
		{
			var table = E("Table",
				new XAttribute("bordersVisible", "true"),
				E("Columns"));

			var cols = table.Element(NS + "Columns");
			for (int c = 0; c < columns; c++)
				cols.Add(new XElement("Column",
					new XAttribute("index", c),
					new XAttribute("width", columnWidthPx.ToString("F2"))));

			for (int r = 0; r < rows; r++)
				table.Add(TableRowNode.Create(columns).Element);

			return new TableNode(table);
		}


		public bool BordersVisible
		{
			get => AttrBool("bordersVisible") == true;
			set => AttrBool("bordersVisible", value);
		}


		public bool HasHeaderRow
		{
			get => AttrBool("hasHeaderRow") == true;
			set => AttrBool("hasHeaderRow", value);
		}


		/// <summary>Number of data rows.</summary>
		public int RowCount => el.Elements(NS + "Row").Count();


		/// <summary>Number of columns (from the Columns declaration).</summary>
		public int ColumnCount
			=> el.Element(NS + "Columns")?.Elements().Count() ?? 0;


		/// <summary>All rows in document order.</summary>
		public IReadOnlyList<TableRowNode> Rows
			=> el.Elements(NS + "Row")
				.Select(e => new TableRowNode(e))
				.ToList();


		/// <summary>All cells in row-major order.</summary>
		public IEnumerable<TableCellNode> AllCells()
			=> Rows.SelectMany(r => r.Cells);


		/// <summary>Returns the row at the given zero-based index.</summary>
		public TableRowNode Row(int row)
			=> new TableRowNode(el.Elements(NS + "Row").ElementAt(row));


		/// <summary>Returns the cell at the given zero-based row and column indices.</summary>
		public TableCellNode Cell(int row, int column)
			=> Row(row).Cell(column);


		/// <summary>
		/// Returns the cell using spreadsheet-style coordinates ("A1", "B3", etc.).
		/// Column is the letter(s), row is the 1-based number.
		/// </summary>
		public TableCellNode Cell(string coord)
		{
			if (string.IsNullOrEmpty(coord))
				throw new ArgumentException("coord cannot be empty", nameof(coord));

			int ci = 0, ri = -1;
			int i = 0;
			while (i < coord.Length && char.IsLetter(coord[i]))
			{
				ci = ci * 26 + (char.ToUpper(coord[i]) - 'A' + 1);
				i++;
			}

			if (i < coord.Length && int.TryParse(coord.Substring(i), out var row))
				ri = row - 1;

			if (ci < 1 || ri < 0)
				throw new ArgumentException($"Invalid coordinate: {coord}", nameof(coord));

			return Cell(ri, ci - 1);
		}


		/// <summary>Appends a new empty row and returns it.</summary>
		public TableRowNode AddRow()
		{
			var row = TableRowNode.Create(ColumnCount);
			el.Add(row.Element);
			return row;
		}


		/// <summary>Appends a new column with the given width to all rows.</summary>
		public void AddColumn(double widthPx = 120)
		{
			var cols = el.Element(NS + "Columns");
			var newIndex = cols?.Elements().Count() ?? 0;

			cols?.Add(new XElement("Column",
				new XAttribute("index", newIndex),
				new XAttribute("width", widthPx.ToString("F2"))));

			foreach (var row in el.Elements(NS + "Row"))
				row.Add(TableCellNode.Create().Element);
		}


		/// <summary>Removes the row at the given zero-based index.</summary>
		public void RemoveRow(int index)
		{
			var rows = el.Elements(NS + "Row").ToList();
			if (index >= 0 && index < rows.Count)
				rows[index].Remove();
		}


		/// <summary>Removes the column at the given zero-based index from all rows and the header.</summary>
		public void RemoveColumn(int index)
		{
			// remove from Columns header
			var cols = el.Element(NS + "Columns")?.Elements().ToList();
			if (cols != null && index < cols.Count)
			{
				cols[index].Remove();
				// re-index remaining columns
				var remaining = el.Element(NS + "Columns")!.Elements().ToList();
				for (int i = 0; i < remaining.Count; i++)
					remaining[i].SetAttributeValue("index", i);
			}

			// remove from each row
			foreach (var row in el.Elements(NS + "Row"))
			{
				var cells = row.Elements(NS + "Cell").ToList();
				if (index < cells.Count)
					cells[index].Remove();
			}
		}
	}
}
