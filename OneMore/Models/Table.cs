//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Xml.Linq;


	internal enum TableSelectionRange
	{
		Single,
		Columns,
		Rows,
		Rectangular
	}


	/// <summary>
	/// Helper class to construct a OneNote table
	/// </summary>
	internal class Table : TableProperties
	{
		private readonly XElement columns;
		private readonly List<TableRow> rows;
		private int numCells;


		/// <summary>
		/// Initialize a new instance with an optional shading color
		/// </summary>
		/// <param name="ns"></param>
		/// <param name="shadingColor"></param>
		public Table(XNamespace ns) : base(ns)
		{
			columns = new XElement(ns + "Columns");
			Root = new XElement(ns + "Table", columns);

			rows = new List<TableRow>();
			numCells = 0;
		}


		public Table(XNamespace ns, int rows, int cols) : this(ns)
		{
			for (int i = 0; i < cols; i++)
			{
				AddColumn(1f, false);
			}

			for (int i = 0; i < rows; i++)
			{
				AddRow();
			}
		}


		public Table(XElement root) : base(root)
		{
			columns = root.Element(ns + "Columns");
			numCells = columns.Elements(ns + "Column").Count();

			rows = new List<TableRow>();
			var elements = root.Elements(ns + "Row");
			if (elements?.Any() == true)
			{
				int r = 1;
				foreach (var element in elements)
				{
					rows.Add(new TableRow(element, r));
					r++;
				}
			}
		}


		/// <summary>
		/// Initialize a new table using another table as a template to describe the number
		/// and width of columns.
		/// </summary>
		/// <param name="other">The other table to base this new table on</param>
		public Table(Table other) : this(other.ns)
		{
			BordersVisible = other.BordersVisible;
			foreach (var col in other.columns.Elements())
			{
				columns.Add(new XElement(col));
				numCells++;
			}
		}


		public bool BordersVisible
		{
			get { return GetBooleanAttribute("bordersVisible"); }
			set { SetAttribute("bordersVisible", value.ToString().ToLower()); }
		}


		public bool HasHeaderRow
		{
			get { return GetBooleanAttribute("hasHeaderRow"); }
			set { SetAttribute("hasHeaderRow", value.ToString().ToLower()); }
		}


		/// <summary>
		/// Get the number of columns in the table.
		/// </summary>
		public int ColumnCount => columns.Elements().Count();


		/// <summary>
		/// Get the number of rows in the table.
		/// </summary>
		public int RowCount => rows.Count;


		/// <summary>
		/// Gets the rows in this table.
		/// </summary>
		public IEnumerable<TableRow> Rows => rows;


		/// <summary>
		/// Gets the indexed row.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public TableRow this[int i] => rows[i];


		/// <summary>
		/// Adds a column definition to the table. If the table has any rows then append
		/// each row with a new cell
		/// </summary>
		/// <param name="width">Required width of the column</param>
		/// <param name="locked">True if the column width is locked</param>
		public void AddColumn(float width, bool locked = false)
		{
			var column = new XElement(ns + "Column",
				new XAttribute("index", numCells),
				new XAttribute("width", width.ToString("0.0#", CultureInfo.InvariantCulture))
				);

			if (locked)
			{
				column.Add(new XAttribute("isLocked", "true"));
			}

			columns.Add(column);
			numCells++;

			if (rows.Any())
			{
				foreach (var row in rows)
				{
					row.AddCell();
				}
			}
		}


		/// <summary>
		/// Adds a new blank row to the table
		/// </summary>
		/// <returns>A TableRow that can be used to add content</returns>
		public TableRow AddRow()
		{
			var row = new TableRow(ns, numCells);
			AddRow(row);
			return row;
		}


		/// <summary>
		/// Adds a new row to the table from the given one:Row element
		/// </summary>
		/// <param name="root">The one:Row element to ad</param>
		/// <param name="rownum">The index of this row in the table</param>
		public TableRow AddRow(XElement root)
		{
			var row = new TableRow(root, rows.Count);
			AddRow(row);
			return row;
		}


		private void AddRow(TableRow row)
		{
			rows.Add(row);

			var last = columns.NodesAfterSelf().OfType<XElement>()
				.LastOrDefault(e => e.Name.LocalName == "Row");

			if (last == null)
			{
				columns.AddAfterSelf(row.Root);
			}
			else
			{
				last.AddAfterSelf(row.Root);
			}
		}


		public void FixColumns(bool locked)
		{
			var isLocked = locked.ToString().ToLower();
			foreach (var column in columns.Elements(ns + "Column"))
			{
				column.SetAttributeValue("isLocked", isLocked);
			}
		}


		public TableCell GetCell(string coord)
		{
			return rows.SelectMany(
				r => r.Cells.Where(e => e.Coordinates == coord), (row, cell) => cell)
				.FirstOrDefault();
		}


		/// <summary>
		/// Returns a collection of selected table cells in row/col order, all cells
		/// in row 1, followed by all cells in row 2, followed by all cells in row 3...
		/// </summary>
		/// <param name="range">
		/// The selection shape: Rows/vertical, Columns/horizontal, or Rectangular
		/// </param>
		/// <returns>An enumerable collection of TableCell</returns>
		public IEnumerable<TableCell> GetSelectedCells(out TableSelectionRange range)
		{
			var selections =
				from r in rows
				let cells = r.Cells
				from c in cells
				where c.Selected == Selection.all || c.Selected == Selection.partial
				select c;

			range = InferRangeType(selections);

			return selections;
		}


		private TableSelectionRange InferRangeType(IEnumerable<TableCell> cells)
		{
			if (cells.Count() == 1)
			{
				return TableSelectionRange.Single;
			}

			var col = -1;
			var row = -1;

			// cells should be in order (row/col), from A1, B1, .. A2, B2, .. A3, B3, ..
			foreach (var cell in cells)
			{
				// record first column and then notice when there are multiple cols selected
				if (col < 0)
					col = cell.ColNum;
				else if (col != int.MaxValue && col != cell.ColNum)
					col = int.MaxValue;

				// record first row and then notice when there are multiple rows selected
				if (row < 0)
					row = cell.RowNum;
				else if (row != int.MaxValue && row != cell.RowNum)
					row = int.MaxValue;

				// if both multi cols and multi rows then must be rectangular
				if (col == int.MaxValue && row == int.MaxValue)
					break;
			}

			if (col == int.MaxValue && row == int.MaxValue)
			{
				return TableSelectionRange.Rectangular;
			}
			else if (col == int.MaxValue)
			{
				// horizontal selection, multiple columns in a single row
				return TableSelectionRange.Columns;
			}

			// vertical selection, multiple rows in a single column
			return TableSelectionRange.Rows;
		}


		public void SetColumnWidth(int index, float width)
		{
			var column = columns.Elements(ns + "Column").Skip(index)?.FirstOrDefault();
			if (column != null)
			{
				column.SetAttributeValue("width", width.ToString("0.0#", CultureInfo.InvariantCulture));
				column.SetAttributeValue("isLocked", "true");
			}
		}
	}
}
