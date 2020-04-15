//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// Helper class to construct a table row and add cell content
	/// </summary>
	internal class TableRow
	{
		private readonly XNamespace ns;


		/// <summary>
		/// Initialize a new row with cells and an optional shading color
		/// </summary>
		/// <param name="ns"></param>
		/// <param name="numCells"></param>
		/// <param name="shadingColor"></param>
		public TableRow(XNamespace ns, int numCells, string shadingColor = null)
		{
			this.ns = ns;

			Root = new XElement(ns + "Row");

			for (int i = 0; i < numCells; i++)
			{
				var cell = new XElement(ns + "Cell");
				if (shadingColor != null)
				{
					cell.Add(new XAttribute("shadingColor", shadingColor));
				}

				Root.Add(cell);
			}
		}


		/// <summary>
		/// Gest the root element of the row
		/// </summary>
		public XElement Root { get; }


		/// <summary>
		/// Adds a table as the content of the specified cell
		/// </summary>
		/// <param name="index">The cell index</param>
		/// <param name="table">The table to add</param>
		public void SetCellContent(int index, Table table)
		{
			SetCellContent(index, table.Root);
		}


		/// <summary>
		/// Convenience routine to set the cell content to the given text.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="text"></param>
		public void SetCellContent(int index, string text)
		{
			SetCellContent(index, new XElement(ns + "T", new XCData(text)));
		}


		/// <summary>
		/// Sets the contents of the indexed cell to the given content
		/// </summary>
		/// <param name="index"></param>
		/// <param name="content"></param>
		public void SetCellContent(int index, XElement content)
		{
			var cell = Root.Elements(ns + "Cell").ElementAt(index);
			if (cell != null)
			{
				// ensure the content is properly wrapped
				var name = content.Name.LocalName;
				if (name != "OEChildren")
				{
					if (name == "OE")
					{
						content = new XElement(ns + "OEChildren", content);
					}
					else
					{
						content = new XElement(ns + "OEChildren", new XElement(ns + "OE", content));
					}
				}

				// set cell contents
				if (cell.HasElements)
				{
					cell.Element(ns + "OEChildren").ReplaceWith(content);
				}
				else
				{
					cell.Add(content);
				}
			}
		}


		/// <summary>
		/// Sets the shaidng of the specified cell
		/// </summary>
		/// <param name="index"></param>
		/// <param name="shading"></param>
		public void SetCellShading(int index, string shading)
		{
			var cell = Root.Elements(ns + "Cell").ElementAt(index);
			if (cell != null)
			{
				var attr = cell.Attribute("shadingColor");
				if (attr == null)
				{
					cell.Add(new XAttribute("shadingColor", shading));
				}
				else
				{
					attr.Value = shading;
				}
			}
		}
	}
}
