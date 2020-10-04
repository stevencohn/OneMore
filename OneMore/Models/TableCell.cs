//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Xml.Linq;


	internal class TableCell : TableProperties
	{

		public TableCell(XNamespace ns) : base(ns)
		{
			Root = new XElement(ns + "Cell",
				new XElement(ns + "OEChildren",
					new XElement(ns + "OE",
						new XElement(ns + "T", new XCData(string.Empty))
				)));
		}


		public TableCell(XElement element) : base(element)
		{
		}


		/// <summary>
		/// Gets or sets the background shading color of this cell.
		/// </summary>
		public string ShadingColor
		{
			get { return GetAttribute("shadingColor"); }
			set { SetAttribute("shadingColor", value); }
		}


		/// <summary>
		/// Adds the given table as the content of this cell.
		/// </summary>
		/// <param name="table">The table to add</param>
		public void SetContent(Table table)
		{
			SetContent(table.Root);
		}


		/// <summary>
		/// Adds the given text as the content of this cell.
		/// </summary>
		/// <param name="text">The text to add</param>
		public void SetContent(string text)
		{
			SetContent(new XElement(ns + "T", new XCData(text)));
		}
		
		
		/// <summary>
		/// Sets the contents of the cell to the given content
		/// </summary>
		/// <param name="content"></param>
		public void SetContent(XElement content)
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
			if (Root.HasElements)
			{
				Root.Element(ns + "OEChildren").ReplaceWith(content);
			}
			else
			{
				Root.Add(content);
			}
		}
	}
}
