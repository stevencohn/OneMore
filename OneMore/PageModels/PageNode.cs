//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// Wraps the root one:Page element. Entry point for the PageModels DOM.
	///
	/// Load from XML:   var page = PageNode.Parse(xmlString);
	/// Save back:       string xml = page.ToXml();
	/// </summary>
	internal sealed class PageNode : OneNoteNode
	{
		private QuickStyleIndex quickStyles;
		private TagDefIndex tagDefs;


		private PageNode(XElement el) : base(el) { }


		/// <summary>Parses OneNote XML as returned by GetPageContent.</summary>
		public static PageNode Parse(string xml)
			=> new PageNode(XElement.Parse(xml));


		/// <summary>Wraps an already-parsed root element.</summary>
		public static PageNode FromElement(XElement root)
			=> new PageNode(root);


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Page metadata

		public string PageId => Attr("ID");

		public string Title
		{
			get => TitleNode?.Text ?? string.Empty;
			set
			{
				var t = TitleNode;
				if (t is null)
				{
					var titleEl = E("Title", E("OE", E("T", new XCData(value ?? string.Empty))));
					var firstContent = el.Elements()
						.FirstOrDefault(e => e.Name.LocalName != "TagDef"
							&& e.Name.LocalName != "QuickStyleDef"
							&& e.Name.LocalName != "Meta"
							&& e.Name.LocalName != "PageSettings");
					if (firstContent is not null)
						firstContent.AddBeforeSelf(titleEl);
					else
						el.Add(titleEl);
				}
				else
				{
					t.Text = value ?? string.Empty;
				}
			}
		}


		public int PageLevel
		{
			get => AttrInt("pageLevel") ?? 1;
			set => AttrInt("pageLevel", value);
		}


		/// <summary>
		/// Page layout settings (background color, RTL, size, margins, rule lines).
		/// Creates the element on first access if absent.
		/// </summary>
		public PageSettingsNode PageSettings
		{
			get
			{
				var settingsEl = el.Element(NS + "PageSettings");
				if (settingsEl is null)
				{
					var node = PageSettingsNode.Create();
					// PageSettings appears before Title in the schema
					var title = el.Element(NS + "Title");
					if (title is not null) title.AddBeforeSelf(node.Element);
					else el.AddFirst(node.Element);
					return node;
				}
				return new PageSettingsNode(settingsEl);
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Page-level Meta

		/// <summary>All Meta name/value pairs directly on the page element.</summary>
		public IEnumerable<MetaNode> AllMeta
			=> el.Elements(NS + "Meta").Select(e => new MetaNode(e));


		/// <summary>Returns the content of the named Meta entry, or null if absent.</summary>
		public string GetMeta(string name)
			=> AllMeta.FirstOrDefault(m => m.Name == name)?.Content;


		/// <summary>Sets or creates the named Meta entry on the page.</summary>
		public void SetMeta(string name, string content)
		{
			var existing = el.Elements(NS + "Meta")
				.FirstOrDefault(e => e.Attribute("name")?.Value == name);
			if (existing is not null)
				existing.SetAttributeValue("content", content ?? string.Empty);
			else
				el.Add(MetaNode.Create(name, content).Element);
		}


		/// <summary>Removes the named Meta entry if present.</summary>
		public void RemoveMeta(string name)
		{
			el.Elements(NS + "Meta")
				.Where(e => e.Attribute("name")?.Value == name)
				.Remove();
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Indices

		/// <summary>Access to the quick style definitions on this page.</summary>
		public QuickStyleIndex QuickStyles
			=> quickStyles ??= new QuickStyleIndex(el);


		/// <summary>Access to the tag definitions on this page.</summary>
		public TagDefIndex TagDefs
			=> tagDefs ??= new TagDefIndex(el);


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Structure

		public TitleNode TitleNode
		{
			get
			{
				var titleEl = el.Element(NS + "Title");
				return titleEl is null ? null : new TitleNode(titleEl);
			}
		}


		/// <summary>All top-level Outline elements on the page.</summary>
		public IEnumerable<OutlineNode> Outlines
			=> el.Elements(NS + "Outline").Select(e => new OutlineNode(e));


		/// <summary>Appends a new empty outline and returns it.</summary>
		public OutlineNode AddOutline()
		{
			var outline = OutlineNode.Create();
			el.Add(outline.Element);
			return outline;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Page-wide traversal

		/// <summary>All paragraphs on the page in document order (DFS).</summary>
		public IEnumerable<OENode> AllParagraphs()
			=> Outlines.SelectMany(o => o.AllParagraphs());


		/// <summary>All tables on the page.</summary>
		public IEnumerable<TableNode> AllTables()
			=> Outlines.SelectMany(o => o.AllTables());


		/// <summary>All images on the page.</summary>
		public IEnumerable<ImageNode> AllImages()
			=> Outlines.SelectMany(o => o.AllImages());


		/// <summary>All nodes of type T on the page, DFS order across all outlines.</summary>
		public IEnumerable<T> Descendants<T>() where T : OneNoteNode
			=> Outlines.SelectMany(o => o.Descendants<T>());


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Serialization

		/// <summary>Serializes back to XML for UpdatePageContent.</summary>
		public string ToXml()
			=> el.ToString(SaveOptions.DisableFormatting);
	}
}
