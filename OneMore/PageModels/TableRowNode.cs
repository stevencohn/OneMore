//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>Wraps a one:Row element within a table.</summary>
	internal sealed class TableRowNode : OneNoteNode
	{
		internal TableRowNode(XElement el) : base(el) { }


		/// <summary>Creates a new row element with the specified number of empty cells.</summary>
		public static TableRowNode Create(int columnCount)
		{
			var row = E("Row");
			for (int i = 0; i < columnCount; i++)
				row.Add(TableCellNode.Create().Element);
			return new TableRowNode(row);
		}


		/// <summary>Cells in document order.</summary>
		public IReadOnlyList<TableCellNode> Cells
			=> el.Elements(NS + "Cell")
				.Select(e => new TableCellNode(e))
				.ToList();


		/// <summary>Number of cells (columns) in this row.</summary>
		public int CellCount => el.Elements(NS + "Cell").Count();


		/// <summary>Returns the cell at the given zero-based column index.</summary>
		public TableCellNode Cell(int column)
			=> new TableCellNode(el.Elements(NS + "Cell").ElementAt(column));


		/// <summary>Appends a new empty cell to this row and returns it.</summary>
		public TableCellNode AddCell()
		{
			var cell = TableCellNode.Create();
			el.Add(cell.Element);
			return cell;
		}


		/// <summary>Removes the cell at the given zero-based column index.</summary>
		public void RemoveCell(int column)
		{
			var cells = el.Elements(NS + "Cell").ToList();
			if (column >= 0 && column < cells.Count)
				cells[column].Remove();
		}
	}
}
