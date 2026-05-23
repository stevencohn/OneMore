//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>Wraps a one:Outline element — the top-level content container on a page.</summary>
	internal sealed class OutlineNode : OneNoteNode
	{
		private OEChildrenNode root;


		internal OutlineNode(XElement el) : base(el) { }


		/// <summary>Creates a new empty Outline element.</summary>
		public static OutlineNode Create()
		{
			var outline = E("Outline", E("OEChildren"));
			return new OutlineNode(outline);
		}


		/// <summary>The root OEChildren of this outline.</summary>
		public OEChildrenNode Root
		{
			get
			{
				if (root is null)
				{
					var oecEl = el.Element(NS + "OEChildren");
					if (oecEl is null)
					{
						oecEl = E("OEChildren");
						el.Add(oecEl);
					}
					root = new OEChildrenNode(oecEl);
				}
				return root;
			}
		}


		/// <summary>All paragraphs in this outline, DFS order including nested items.</summary>
		public IEnumerable<OENode> AllParagraphs() => Root.AllDescendants();


		/// <summary>All tables anywhere in this outline.</summary>
		public IEnumerable<TableNode> AllTables() => Root.Descendants<TableNode>();


		/// <summary>All images anywhere in this outline.</summary>
		public IEnumerable<ImageNode> AllImages() => Root.Descendants<ImageNode>();


		/// <summary>All nodes of type T in this outline, DFS order.</summary>
		public IEnumerable<T> Descendants<T>() where T : OneNoteNode => Root.Descendants<T>();


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Canvas position (one:Position child element)

		/// <summary>Horizontal position on the page canvas in points.</summary>
		public double X
		{
			get => GetPositionAttr("x");
			set => SetPositionAttr("x", value);
		}


		/// <summary>Vertical position on the page canvas in points.</summary>
		public double Y
		{
			get => GetPositionAttr("y");
			set => SetPositionAttr("y", value);
		}


		/// <summary>Z-order (stacking depth). Higher values appear on top.</summary>
		public int Z
		{
			get => (int)(GetPositionAttr("z"));
			set => SetPositionAttr("z", value);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Canvas size (one:Size child element)

		/// <summary>Width of the outline bounding box in points.</summary>
		public double Width
		{
			get => GetSizeAttr("width");
			set => SetSizeAttr("width", value);
		}


		/// <summary>Height of the outline bounding box in points.</summary>
		public double Height
		{
			get => GetSizeAttr("height");
			set => SetSizeAttr("height", value);
		}


		/// <summary>True when the user has explicitly resized this outline.</summary>
		public bool IsSizeSetByUser
		{
			get => el.Element(NS + "Size")?.Attribute("isSetByUser")?.Value == "true";
			set
			{
				EnsureSize().SetAttributeValue("isSetByUser", value ? "true" : "false");
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Meta

		/// <summary>All Meta name/value pairs on this outline.</summary>
		public IEnumerable<MetaNode> AllMeta
			=> el.Elements(NS + "Meta").Select(e => new MetaNode(e));


		/// <summary>Returns the content of the named Meta entry, or null if absent.</summary>
		public string GetMeta(string name)
			=> AllMeta.FirstOrDefault(m => m.Name == name)?.Content;


		/// <summary>Sets or creates the named Meta entry.</summary>
		public void SetMeta(string name, string content)
		{
			var existing = el.Elements(NS + "Meta")
				.FirstOrDefault(e => e.Attribute("name")?.Value == name);
			if (existing is not null)
			{
				existing.SetAttributeValue("content", content ?? string.Empty);
				return;
			}

			// Meta must appear after Position/Size but before OEChildren per PageObject schema
			var meta = MetaNode.Create(name, content).Element;
			var lastMeta = el.Elements(NS + "Meta").LastOrDefault();
			if (lastMeta is not null)
				lastMeta.AddAfterSelf(meta);
			else
			{
				var oec = el.Element(NS + "OEChildren");
				if (oec is not null) oec.AddBeforeSelf(meta);
				else el.Add(meta);
			}
		}


		/// <summary>Removes the named Meta entry if present.</summary>
		public void RemoveMeta(string name)
		{
			el.Elements(NS + "Meta")
				.Where(e => e.Attribute("name")?.Value == name)
				.Remove();
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Helpers

		private double GetPositionAttr(string attr)
		{
			var v = el.Element(NS + "Position")?.Attribute(attr)?.Value;
			return double.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
				? d : 0;
		}


		private void SetPositionAttr(string attr, double value)
		{
			var pos = el.Element(NS + "Position");
			if (pos is null)
			{
				pos = E("Position",
					new XAttribute("x", "0"),
					new XAttribute("y", "0"),
					new XAttribute("z", "0"));
				el.AddFirst(pos);
			}
			pos.SetAttributeValue(attr, value.ToString("F2", CultureInfo.InvariantCulture));
		}


		private double GetSizeAttr(string attr)
		{
			var v = el.Element(NS + "Size")?.Attribute(attr)?.Value;
			return double.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
				? d : 0;
		}


		private void SetSizeAttr(string attr, double value)
		{
			EnsureSize().SetAttributeValue(attr,
				value.ToString("F2", CultureInfo.InvariantCulture));
		}


		private XElement EnsureSize()
		{
			var size = el.Element(NS + "Size");
			if (size is null)
			{
				size = E("Size",
					new XAttribute("width", "0"),
					new XAttribute("height", "0"),
					new XAttribute("isSetByUser", "false"));
				// Size comes after Position, before OEChildren
				var pos = el.Element(NS + "Position");
				if (pos is not null) pos.AddAfterSelf(size);
				else el.AddFirst(size);
			}
			return size;
		}
	}
}
