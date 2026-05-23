//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>Wraps the one:Title element on a page.</summary>
	internal sealed class TitleNode : OneNoteNode
	{
		internal TitleNode(XElement el) : base(el) { }


		/// <summary>
		/// Plain text of the title's first OE. Setting this replaces the first OE's text.
		/// </summary>
		public string Text
		{
			get
			{
				var oe = el.Elements(NS + "OE").FirstOrDefault();
				if (oe is null) return string.Empty;
				return new OENode(oe).PlainText;
			}
			set
			{
				var oe = el.Elements(NS + "OE").FirstOrDefault();
				if (oe is null)
				{
					el.Add(E("OE", E("T", new XCData(value ?? string.Empty))));
				}
				else
				{
					new OENode(oe).SetPlainText(value ?? string.Empty);
				}
			}
		}
	}
}
