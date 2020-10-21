//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Linq;
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
		/// Gets or sets the tabular coordinate name of the cell, A1, A2, etc.
		/// </summary>
		public string Coordinates { get; set; }

		public int ColNum { get; set; }


		public int RowNum { get; set; }


		/// <summary>
		/// Gets or sets the background shading color of this cell.
		/// </summary>
		public string ShadingColor
		{
			get { return GetAttribute("shadingColor"); }
			set { SetAttribute("shadingColor", value); }
		}


		/// <summary>
		/// Gets the value stored in this cell
		/// </summary>
		/// <returns>A string value</returns>
		public string GetContent()
		{
			var text = Root.Elements(ns + "OEChildren")
				.Elements(ns + "OE")
				.Elements(ns + "T")
				.FirstOrDefault();

			return text?.Value;
		}


		/// <summary>
		/// Gets the value of the named meta attribute
		/// </summary>
		/// <param name="name">The name of a meta attribute</param>
		/// <returns>The value of the meta attribute</returns>
		public string GetMeta(string name)
		{
			var element = Root.Elements(ns + "OEChildren")
				.Elements(ns + "OE")
				.Elements(ns + "Meta")
				.FirstOrDefault(e => e.Attribute("name").Value == name);
			
			return element?.Attribute("content")?.Value;
		}


		public bool HasTag(string index)
		{
			return Root.Elements(ns + "OEChildren")
				.Elements(ns + "OE")
				.Elements(ns + "Tag")
				.Any(e => e.Attribute("index").Value == index);
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
			// ensure the content is properly wrapped, while preserving existing
			// elements such as Meta that might have formula details

			var name = content.Name.LocalName;
			var oec = Root.Element(ns + "OEChildren");
			if (name == "OEChildren")
			{
				if (oec == null)
					Root.ReplaceNodes(content);
				else
					oec.ReplaceWith(content);

				return;
			}
			else if (oec == null)
			{
				oec = new XElement(ns + "OEChildren");
				Root.Add(oec);
			}

			var oe = oec.Element(ns + "OE");
			if (name == "OE")
			{
				if (oe == null)
					oec.ReplaceNodes(content);
				else
					oe.ReplaceWith(content);

				return;
			}
			else if (oe == null)
			{
				oe = new XElement(ns + "OE");
				oec.Add(oe);
			}

			var t = oe.Elements(ns + "T");

			if (t?.Any() == true)
			{
				t.Remove();
			}

			oe.Add(content);
		}


		/// <summary>
		/// Applies a meta attribute to the cell, used to contain formula definitions
		/// </summary>
		/// <param name="name">The meta name</param>
		/// <param name="value">The meta value</param>
		public void SetMeta(string name, string value)
		{
			if (GetContent() == null)
			{
				SetContent(string.Empty);
			}

			var parent = Root.Element(ns + "OEChildren").Element(ns + "OE");

			var element = parent.Elements(ns + "Meta")
				.FirstOrDefault(e => e.Attribute("name").Value == name);

			if (element == null)
			{
				// Meta must precede T
				parent.AddFirst(new XElement(ns + "Meta",
					new XAttribute("name", name),
					new XAttribute("content", value)
					));
			}
			else
			{
				element.SetAttributeValue("content", value);
			}
		}


		public void SetTag(string index)
		{
			//<one:Tag index="0" completed="true" disabled="false" />

			if (GetContent() == null)
			{
				SetContent(string.Empty);
			}

			var parent = Root.Element(ns + "OEChildren").Element(ns + "OE");

			var element = parent.Elements(ns + "Tag")
				.FirstOrDefault(e => e.Attribute("index").Value == index);

			if (element == null)
			{
				// Meta must precede T
				parent.AddFirst(new XElement(ns + "Tag",
					new XAttribute("index", index),
					new XAttribute("completed", "true"),
					new XAttribute("disabled", "false")
					));
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		/// <summary>
		/// Convert an index number to an alphabetic sequence, column index to column name
		/// </summary>
		/// <param name="index">A numeric index</param>
		/// <returns>A letter sequence A, B, C, ... ZZZ</returns>
		public static string IndexToLetters(int index)
		{
			string letters = string.Empty;
			int mod;
			int div = index;

			while (div > 0)
			{
				mod = (div - 1) % 26;
				letters = (char)(65 + mod) + letters;
				div = ((div - mod) / 26);
			}

			return letters;
		}


		/// <summary>
		/// Converts an alphabetic sequence to an index number, column name to column index
		/// </summary>
		/// <param name="letters">A letter sequence A, B, C, ... ZZZ</param>
		/// <returns>A numeric index</returns>
		public static int LettersToIndex(string letters)
		{
			letters = letters.ToUpper();
			int sum = 0;

			for (int i = 0; i < letters.Length; i++)
			{
				sum *= 26;
				sum += (letters[i] - 'A' + 1);
			}

			return sum;
		}
	}
}
