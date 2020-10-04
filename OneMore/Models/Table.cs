//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Xml.Linq;


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
		/// Gets the rows in this table.
		/// </summary>
		public IEnumerable<TableRow> Rows => rows;


		/// <summary>
		/// Adds a column definition to the table
		/// </summary>
		/// <param name="width">Required width of the column</param>
		/// <param name="locked">True if the column width is locked</param>
		public void AddColumn(float width, bool locked = false)
		{
			var column = new XElement(ns + "Column",
				new XAttribute("index", numCells),
				new XAttribute("width", width.ToString("0.0#"))
				);

			if (locked)
			{
				column.Add(new XAttribute("isLocked", "true"));
			}

			columns.Add(column);
			numCells++;
		}


		/// <summary>
		/// Adds a new blank row to the table
		/// </summary>
		/// <returns>A TableRow that can be used to add content</returns>
		public TableRow AddRow()
		{
			var row = new TableRow(ns, numCells);
			rows.Add(row);

			var last = columns.NodesAfterSelf().OfType<XElement>()
				.Where(e => e.Name.LocalName == "Row")
				.LastOrDefault();

			if (last == null)
			{
				columns.AddAfterSelf(row.Root);
			}
			else
			{
				last.AddAfterSelf(row.Root);
			}

			return row;
		}


		public void SetColumnWidth(int index, float width)
		{
			var column = columns.Elements(ns + "Column").Skip(index)?.FirstOrDefault();
			if (column != null)
			{
				column.SetAttributeValue("width", width.ToString("0.0#"));
				column.SetAttributeValue("isLocked", "true");
			}
		}
	}
}
