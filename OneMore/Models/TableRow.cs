//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// Helper class to construct a table row and add cell content
	/// </summary>
	internal class TableRow : TableProperties
	{
		private readonly List<TableCell> cells;


		/// <summary>
		/// Initialize a new row with cells and an optional shading color
		/// </summary>
		/// <param name="ns"></param>
		/// <param name="numCells"></param>
		/// <param name="shadingColor"></param>
		public TableRow(XNamespace ns, int numCells) : base(ns)
		{
			Root = new XElement(ns + "Row");
			this.ns = ns;

			cells = new List<TableCell>();

			for (int i = 0; i < numCells; i++)
			{
				var cell = new TableCell(ns);
				cells.Add(cell);
				Root.Add(cell.Root);
			}
		}


		public TableRow(XElement root, int rownum) : base(root)
		{
			cells = new List<TableCell>();

			var elements = root.Elements(ns + "Cell");
			if (elements?.Any() == true)
			{
				var c = 1;
				foreach (var element in elements)
				{
					var cell = new TableCell(element)
					{
						ColNum = c,
						RowNum = rownum,
						Coordinates = $"{TableCell.IndexToLetters(c)}{rownum}"
					};

					cells.Add(cell);
					c++;
				}
			}
		}


		/// <summary>
		/// Gets the table cells in this row.
		/// </summary>
		public IEnumerable<TableCell> Cells => cells;


		/// <summary>
		/// Gets the indexed cell.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public TableCell this[int i] => cells[i];


		/// <summary>
		/// Gets the row number, position in the table, starting at 1
		/// </summary>
		public int RowNum { get; private set; }


		/// <summary>
		/// Adds a new cell to this row and returns the cell
		/// </summary>
		/// <returns></returns>
		public TableCell AddCell()
		{
			var cell = new TableCell(ns);
			cells.Add(cell);
			Root.Add(cell.Root);
			return cell;
		}


		/// <summary>
		/// Set the background shading color of all cells in the current row
		/// </summary>
		/// <param name="color"></param>
		public void SetShading(string color)
		{
			foreach (var cell in cells)
			{
				cell.ShadingColor = color;
			}
		}
	}
}
