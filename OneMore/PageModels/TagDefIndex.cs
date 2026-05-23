//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// Manages the TagDef elements on a Page. Tag defs define the available note tags
	/// (e.g. To-Do, Important, Question) referenced by index from OE Tag elements.
	/// </summary>
	internal sealed class TagDefIndex
	{
		private readonly XElement page;


		internal TagDefIndex(XElement root)
		{
			page = root;
		}


		private XNamespace NS => OneNoteNode.NS;


		/// <summary>All tag def indices in document order.</summary>
		public IReadOnlyList<int> Indices
			=> page.Elements(NS + "TagDef")
				.Select(e => int.TryParse(e.Attribute("index")?.Value, out var i) ? i : -1)
				.Where(i => i >= 0)
				.ToList();


		/// <summary>Returns the TagDef element for the given index, or null.</summary>
		public XElement Get(int index)
			=> page.Elements(NS + "TagDef")
				.FirstOrDefault(e => e.Attribute("index")?.Value == index.ToString());


		/// <summary>Returns the index of the tag def whose name matches, or -1 if not found.</summary>
		public int FindByName(string name)
		{
			var el = page.Elements(NS + "TagDef")
				.FirstOrDefault(e => e.Attribute("name")?.Value == name);
			return el is null
				? -1
				: int.TryParse(el.Attribute("index")?.Value, out var i) ? i : -1;
		}


		/// <summary>
		/// Finds or creates a TagDef with the given name and returns its index.
		/// </summary>
		public int EnsureTag(string name, string type = "0", string symbol = "0")
		{
			var existing = FindByName(name);
			if (existing >= 0) return existing;

			var nextIndex = page.Elements(NS + "TagDef")
				.Select(e => int.TryParse(e.Attribute("index")?.Value, out var i) ? i : -1)
				.DefaultIfEmpty(-1)
				.Max() + 1;

			var def = new XElement(NS + "TagDef",
				new XAttribute("index", nextIndex),
				new XAttribute("name", name),
				new XAttribute("type", type),
				new XAttribute("symbol", symbol));

			var last = page.Elements(NS + "TagDef").LastOrDefault();
			if (last is not null)
				last.AddAfterSelf(def);
			else
				page.AddFirst(def);

			return nextIndex;
		}
	}
}
