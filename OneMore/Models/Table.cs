//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// Helper class to construct a OneNote table
	/// </summary>
	internal class Table : CommonAttributes
	{
		private readonly XNamespace ns;
		private readonly XElement columns;
		private readonly string shadingColor;
		private int numCells;


		/// <summary>
		/// Initialize a new instance with an optional shading color
		/// </summary>
		/// <param name="ns"></param>
		/// <param name="shadingColor"></param>
		public Table(XNamespace ns, string shadingColor = null)
		{
			this.ns = ns;
			this.shadingColor = shadingColor;

			numCells = 0;
			columns = new XElement(ns + "Columns");
			root = new XElement(ns + "Table", columns);
		}


		public Table(XElement root)
		{
			this.root = root;
			ns = root.GetDefaultNamespace();
			columns = root.Element(ns + "Columns");
			numCells = columns.Elements(ns + "Column").Count();
		}


		/// <summary>
		/// Gest the root element of the table
		/// </summary>
		public XElement Root { get { return root; } }


		/// <summary>
		/// Adds a column definition to the table
		/// </summary>
		/// <param name="width">Required width of the column</param>
		/// <param name="locked">True if the column width is locked</param>
		public void AddColumn(float width, bool locked = false)
		{
			var column = new XElement(ns + "Column",
				new XAttribute("index", numCells)
				);

			column.Add(new XAttribute("width", width.ToString("0.0#")));

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
			var row = new TableRow(ns, numCells, shadingColor);

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


		/// <summary>
		/// Enables or disables border visiblity for the table
		/// </summary>
		/// <param name="visible"></param>
		public void SetBordersVisible(bool visible)
		{
			var attr = root.Attribute("bordersVisible");
			if (attr == null)
			{
				root.Add(new XAttribute("bordersVisible", visible.ToString().ToLower()));
			}
			else
			{
				attr.Value = visible.ToString().ToLower();
			}
		}


		/// <summary>
		/// Sets the overall shading for the table
		/// </summary>
		/// <param name="color"></param>
		public void SetShading(string color)
		{
			var cells = root.Elements(ns + "Row").Elements(ns + "Cell");
			if (cells?.Any() == true)
			{
				foreach (var cell in cells)
				{
					var attr = cell.Attribute("shadingColor");
					if (attr == null)
					{
						cell.Add(new XAttribute("shadingColor", color));
					}
					else
					{
						attr.Value = color;
					}
				}
			}
		}
	}
}
