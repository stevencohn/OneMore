//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Collections.Generic;
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
			cells = new List<TableCell>();

			for (int i = 0; i < numCells; i++)
			{
				var cell = new TableCell(ns);
				cells.Add(cell);
				Root.Add(cell.Root);
			}
		}


		/// <summary>
		/// Gets the table cells in this row.
		/// </summary>
		public IEnumerable<TableCell> Cells => cells;
	}
}
