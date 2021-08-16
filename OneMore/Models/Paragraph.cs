//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S2376 // Write-only properties should not be used

namespace River.OneMoreAddIn.Models
{
	using System.Xml.Linq;


	internal class Paragraph : XElement
	{
		public Paragraph(XNamespace ns, string text)
			: this(ns, new XElement(ns + "T", new XCData(text)))
		{
		}


		public Paragraph(XNamespace ns, XElement content)
			: base(ns + "OE")
		{
			Add(content);
		}


		public Paragraph(XNamespace ns, params XNode[] nodes)
			: base(ns + "OE")
		{
			Add(nodes);
		}


		public XElement SetAlignment(string alignment)
		{
			SetAttributeValue("alignment", alignment);
			return this;
		}


		public XElement SetQuickStyle(int index)
		{
			SetAttributeValue("quickStyleIndex", index);
			return this;
		}


		public XElement SetStyle(string style)
		{
			SetAttributeValue("style", style);
			return this;
		}
	}
}
