//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

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


		public Paragraph SetAlignment(string alignment)
		{
			SetAttributeValue("alignment", alignment);
			return this;
		}


		public Paragraph SetQuickStyle(int index)
		{
			SetAttributeValue("quickStyleIndex", index);
			return this;
		}


		public Paragraph SetStyle(string style)
		{
			SetAttributeValue("style", style);
			return this;
		}
	}
}
