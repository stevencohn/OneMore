//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>Wraps a one:Cell element within a table row.</summary>
	internal sealed class TableCellNode : OneNoteNode
	{
		private OEChildrenNode content;


		internal TableCellNode(XElement el) : base(el) { }


		/// <summary>Creates a new empty cell element.</summary>
		public static TableCellNode Create()
			=> new TableCellNode(E("Cell", E("OEChildren", E("OE", E("T", new XCData(string.Empty))))));


		/// <summary>Background shading color (#RRGGBB or named color).</summary>
		public string ShadingColor
		{
			get => Attr("shadingColor");
			set => Attr("shadingColor", value);
		}


		/// <summary>
		/// The OEChildren node that holds this cell's content. Created on demand if absent.
		/// </summary>
		public OEChildrenNode Content
		{
			get
			{
				if (content is null)
				{
					var oecEl = el.Elements(NS + "OEChildren").FirstOrDefault();
					if (oecEl is null)
					{
						oecEl = E("OEChildren");
						el.Add(oecEl);
					}
					content = new OEChildrenNode(oecEl);
				}
				return content;
			}
		}


		/// <summary>Convenience: plain text of all paragraphs in this cell, joined by newline.</summary>
		public string PlainText
			=> string.Join("\n", Content.Select(oe => oe.PlainText));


		/// <summary>Convenience: replaces all content with a single paragraph of plain text.</summary>
		public void SetText(string text)
		{
			var oec = el.Elements(NS + "OEChildren").FirstOrDefault();
			if (oec is null)
			{
				oec = E("OEChildren");
				el.Add(oec);
			}
			oec.ReplaceNodes(E("OE", E("T", new XCData(text ?? string.Empty))));
			content = null; // invalidate cached wrapper
		}
	}
}
