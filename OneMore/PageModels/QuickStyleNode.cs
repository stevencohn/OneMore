//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System.Xml.Linq;


	/// <summary>
	/// Typed wrapper for a one:QuickStyleDef element. Exposes all attributes as properties
	/// so callers can read and mutate individual style settings without working with raw XML.
	/// Obtained via QuickStyleIndex.Get(index).
	/// </summary>
	internal sealed class QuickStyleNode : OneNoteNode
	{
		internal QuickStyleNode(XElement el) : base(el) { }


		/// <summary>Zero-based index; matches the quickStyleIndex attribute on OE elements.</summary>
		public int Index => AttrInt("index") ?? 0;


		/// <summary>
		/// Built-in name, e.g. "p" (body), "h1"–"h6" (headings), "PageTitle", "cite", "code".
		/// </summary>
		public string Name
		{
			get => Attr("name");
			set => Attr("name", value);
		}


		public string Font
		{
			get => Attr("font");
			set => Attr("font", value);
		}


		/// <summary>Font size in points (written as "11.0" in XML, no pt suffix).</summary>
		public double? FontSize
		{
			get => AttrDouble("fontSize");
			set => AttrDouble("fontSize", value);
		}


		/// <summary>
		/// Foreground color (#RRGGBB). Null when the attribute is absent or "automatic".
		/// Write null to restore automatic.
		/// </summary>
		public string FontColor
		{
			get { var v = Attr("fontColor"); return v == "automatic" ? null : v; }
			set => Attr("fontColor", value ?? "automatic");
		}


		/// <summary>Highlight/background color (#RRGGBB). Null when absent or "automatic".</summary>
		public string HighlightColor
		{
			get { var v = Attr("highlightColor"); return v == "automatic" ? null : v; }
			set => Attr("highlightColor", value ?? "automatic");
		}


		public bool? Bold
		{
			get => AttrBool("bold");
			set => AttrBool("bold", value);
		}


		public bool? Italic
		{
			get => AttrBool("italic");
			set => AttrBool("italic", value);
		}


		public bool? Underline
		{
			get => AttrBool("underline");
			set => AttrBool("underline", value);
		}


		public bool? Strikethrough
		{
			get => AttrBool("strikethrough");
			set => AttrBool("strikethrough", value);
		}


		public bool? Superscript
		{
			get => AttrBool("superscript");
			set => AttrBool("superscript", value);
		}


		public bool? Subscript
		{
			get => AttrBool("subscript");
			set => AttrBool("subscript", value);
		}


		/// <summary>Space above the paragraph in points.</summary>
		public double? SpaceBefore
		{
			get => AttrDouble("spaceBefore");
			set => AttrDouble("spaceBefore", value);
		}


		/// <summary>Space below the paragraph in points.</summary>
		public double? SpaceAfter
		{
			get => AttrDouble("spaceAfter");
			set => AttrDouble("spaceAfter", value);
		}


		/// <summary>
		/// Produces a StyleString representing the typographic properties of this def,
		/// for use with StyleString.Merge() in the three-tier cascade.
		/// </summary>
		public StyleString ToStyleString()
		{
			var s = StyleString.Empty;
			if (Font is not null) s.FontFamily = Font;
			if (FontSize is not null) s.FontSizePt = FontSize;
			if (FontColor is not null) s.Color = FontColor;
			if (HighlightColor is not null) s.Background = HighlightColor;
			if (Bold is not null) s.Bold = Bold;
			if (Italic is not null) s.Italic = Italic;
			if (Underline is not null) s.Underline = Underline;
			if (Strikethrough is not null) s.Strikethrough = Strikethrough;
			if (Superscript is not null) s.Superscript = Superscript;
			if (Subscript is not null) s.Subscript = Subscript;
			return s;
		}
	}
}
