//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System.Xml.Linq;


	/// <summary>
	/// Wraps a one:Meta element — a name/value pair that can appear as a child of
	/// Page, Outline, or OE. OneMore uses these to store per-page and per-paragraph
	/// command state (e.g. breadcrumb tokens, anchor IDs, plugin flags).
	/// </summary>
	internal sealed class MetaNode : OneNoteNode
	{
		internal MetaNode(XElement el) : base(el) { }


		/// <summary>Creates a new Meta element with the given name and content.</summary>
		public static MetaNode Create(string name, string content = "")
			=> new MetaNode(E("Meta",
				new XAttribute("name", name),
				new XAttribute("content", content ?? string.Empty)));


		/// <summary>The meta key. Immutable after creation; use a new element to rename.</summary>
		public string Name => Attr("name");


		/// <summary>The meta value.</summary>
		public string Content
		{
			get => Attr("content") ?? string.Empty;
			set => Attr("content", value ?? string.Empty);
		}
	}
}
