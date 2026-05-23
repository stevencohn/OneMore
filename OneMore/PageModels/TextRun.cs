//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Linq;


	/// <summary>
	/// Wraps a one:T element (text run). Content is either a plain XText node or an XCData
	/// section that may contain HTML span elements for character-level formatting.
	/// </summary>
	internal sealed class TextRun : OneNoteNode
	{
		private static readonly Regex StripHtml =
			new Regex(@"<[^>]+>", RegexOptions.Compiled);


		internal TextRun(XElement el) : base(el) { }


		/// <summary>Creates a new plain-text run, optionally with a style.</summary>
		public static TextRun Create(string text, StyleString style = null)
		{
			var t = E("T");
			t.Add(new XCData(text ?? string.Empty));
			if (style is not null && !style.IsEmpty)
				t.Add(new XAttribute("style", style.ToString()));
			return new TextRun(t);
		}


		/// <summary>Creates a new styled run using a configure delegate.</summary>
		public static TextRun Create(string text, System.Action<StyleString> configure)
		{
			var style = StyleString.Empty;
			configure(style);
			return Create(text, style);
		}


		/// <summary>
		/// Plain text content with HTML span tags stripped. For CDATA content this strips
		/// span elements but preserves the text they wrap.
		/// </summary>
		public string PlainText
		{
			get
			{
				var cdata = el.DescendantNodes().OfType<XCData>().FirstOrDefault();
				if (cdata is not null)
					return StripHtml.Replace(cdata.Value, string.Empty);

				// plain text node
				return el.Nodes().OfType<XText>().FirstOrDefault()?.Value ?? el.Value;
			}
			set
			{
				el.ReplaceNodes(new XCData(value ?? string.Empty));
			}
		}


		/// <summary>Raw CDATA content including any span HTML, or null if not CDATA.</summary>
		public string RawContent
		{
			get => el.DescendantNodes().OfType<XCData>().FirstOrDefault()?.Value;
			set
			{
				var cdata = el.DescendantNodes().OfType<XCData>().FirstOrDefault();
				if (cdata is not null)
					cdata.ReplaceWith(new XCData(value ?? string.Empty));
				else
					el.ReplaceNodes(new XCData(value ?? string.Empty));
			}
		}


		/// <summary>Style attribute on the T element (character-level override).</summary>
		public StyleString Style
		{
			get => StyleString.Parse(Attr("style"));
			set => Attr("style", value?.IsEmpty == false ? value.ToString() : null);
		}


		/// <summary>
		/// Raw value of the selected attribute: "all", "partial", or null (not selected).
		/// "all" means this run is fully selected. "partial" appears on ancestor elements,
		/// not on T runs directly — treat it as not-selected at the run level.
		/// </summary>
		public string SelectedValue => Attr("selected");


		/// <summary>True when this run is fully selected (selected="all").</summary>
		public bool IsSelected => SelectedValue == "all";


		public bool PreserveWhiteSpace
		{
			get => AttrBool("preserveWhiteSpace") == true;
			set => AttrBool("preserveWhiteSpace", value ? (bool?)true : null);
		}
	}
}
