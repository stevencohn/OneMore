//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Xml.Linq;


	internal class Paragraph : XElement
	{
		public Paragraph(string text)
			: this(PageNamespace.Value, text)
		{
		}


		public Paragraph(XNamespace ns, string text)
			: this(ns, new XElement(ns + "T", new XCData(text)))
		{
		}


		public Paragraph(XElement content)
			: this(PageNamespace.Value, content)
		{
		}


		public Paragraph(XNamespace ns, XElement content)
			: base(ns + "OE")
		{
			Add(content);
		}


		public Paragraph(params XObject[] nodes)
			: this(PageNamespace.Value, nodes)
		{
		}


		public Paragraph(XNamespace ns, params XObject[] nodes)
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


		public Paragraph SetRTL(bool enable)
		{
			if (enable)
			{
				SetAttributeValue("alignment", "right");
				SetAttributeValue("RTL", "true");
			}
			else
			{
				SetAttributeValue("alignment", "left");
				SetAttributeValue("RTL", "false");
			}
			return this;
		}


		public Paragraph SetStyle(string style)
		{
			SetAttributeValue("style", style);
			return this;
		}
	}
}
