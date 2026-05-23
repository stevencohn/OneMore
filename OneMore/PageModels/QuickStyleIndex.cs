//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// Manages the QuickStyleDef elements on a Page. Provides lookup by index and by value,
	/// and the ability to find-or-create a style by its attribute values.
	/// </summary>
	internal sealed class QuickStyleIndex
	{
		private readonly XElement page;


		internal QuickStyleIndex(XElement root)
		{
			page = root;
		}


		private XNamespace NS => OneNoteNode.NS;


		/// <summary>All defined quick style indices, in document order.</summary>
		public IReadOnlyList<int> Indices
			=> page.Elements(NS + "QuickStyleDef")
				.Select(e => int.TryParse(e.Attribute("index")?.Value, out var i) ? i : -1)
				.Where(i => i >= 0)
				.ToList();


		/// <summary>Returns a typed wrapper for the QuickStyleDef at the given index, or null.</summary>
		public QuickStyleNode Get(int index)
		{
			var el = GetElement(index);
			return el is null ? null : new QuickStyleNode(el);
		}


		private XElement GetElement(int index)
			=> page.Elements(NS + "QuickStyleDef")
				.FirstOrDefault(e => e.Attribute("index")?.Value == index.ToString());


		/// <summary>
		/// Returns the index of an existing style whose name matches, or -1 if not found.
		/// </summary>
		public int FindByName(string name)
		{
			var el = page.Elements(NS + "QuickStyleDef")
				.FirstOrDefault(e => e.Attribute("name")?.Value == name);
			return el is null
				? -1
				: int.TryParse(el.Attribute("index")?.Value, out var i) ? i : -1;
		}


		/// <summary>
		/// Finds an existing QuickStyleDef matching the given attributes, or creates one.
		/// Returns the index of the matching or newly-created def.
		/// </summary>
		public int EnsureStyle(
			string name,
			string font,
			double fontSize,
			string fontColor = "automatic",
			string highlightColor = "automatic",
			string spaceBefore = "0.0",
			string spaceAfter = "0.0")
		{
			// look for an exact match by name first
			var existing = FindByName(name);
			if (existing >= 0) return existing;

			// allocate the next available index
			var nextIndex = page.Elements(NS + "QuickStyleDef")
				.Select(e => int.TryParse(e.Attribute("index")?.Value, out var i) ? i : -1)
				.DefaultIfEmpty(-1)
				.Max() + 1;

			var def = new XElement(NS + "QuickStyleDef",
				new XAttribute("index", nextIndex),
				new XAttribute("name", name),
				new XAttribute("font", font),
				new XAttribute("fontSize", $"{fontSize:F1}"),
				new XAttribute("fontColor", fontColor ?? "automatic"),
				new XAttribute("highlightColor", highlightColor ?? "automatic"),
				new XAttribute("spaceBefore", spaceBefore ?? "0.0"),
				new XAttribute("spaceAfter", spaceAfter ?? "0.0"));

			// QuickStyleDef elements appear after TagDef elements; insert after last existing one
			var last = page.Elements(NS + "QuickStyleDef").LastOrDefault()
				?? page.Elements(NS + "TagDef").LastOrDefault();
			if (last is not null)
				last.AddAfterSelf(def);
			else
				page.AddFirst(def);

			return nextIndex;
		}


		/// <summary>
		/// Applies a quick style index to an OE node by setting its quickStyleIndex attribute
		/// and removing any inline style override.
		/// </summary>
		public void Apply(OENode oe, int index)
		{
			oe.QuickStyleIndex = index;
			oe.InlineStyle = null;
		}


		/// <summary>
		/// Builds a StyleString from the attributes of the QuickStyleDef at the given index.
		/// Returns StyleString.Empty if the index is not found.
		/// </summary>
		/// <summary>
		/// Returns a StyleString representing the typographic properties of the def at the
		/// given index. Includes all formatting flags (bold, italic, etc.) as well as font,
		/// size, color, and spacing. Returns StyleString.Empty if the index is not found.
		/// </summary>
		public StyleString GetStyle(int index)
		{
			var node = Get(index);
			return node?.ToStyleString() ?? StyleString.Empty;
		}
	}
}
