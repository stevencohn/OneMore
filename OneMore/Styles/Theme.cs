//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Styles
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	internal class Theme
	{
		private readonly XElement root;
		private readonly XNamespace ns;
		private readonly string name;
		private readonly bool dark;


		/// <summary>
		/// Initialize a new instance from the given theme XML.
		/// </summary>
		public Theme(XElement root, string key)
		{
			Key = key;

			this.root = root;
			ns = root.GetDefaultNamespace();

			if (!root.GetAttributeValue("name", out name, key))
			{
				root.Add(new XAttribute("name", name));
			}

			root.GetAttributeValue("dark", out dark, false);
		}


		public string Key { get; private set; }


		public string Name => name;


		public bool Dark => dark;


		public int GetCount()
		{
			return root.Elements(ns + "Style").Count();
		}


		public string GetName(int index)
		{
			return root.Elements(ns + "Style")
				.ElementAt(index)
				.Attribute("name").Value;
		}


		public List<StyleRecord> GetRecords()
		{
			return root.Elements(ns + "Style")
				.ToList()
				.ConvertAll(e => new StyleRecord(e));
		}


		public Style GetStyle(int index)
		{
			return new Style(new StyleRecord(
				root.Elements(ns + "Style").ElementAt(index)
				));
		}


		public List<Style> GetStyles()
		{
			return GetRecords().ToList().ConvertAll(e => new Style(e));
		}
	}
}
