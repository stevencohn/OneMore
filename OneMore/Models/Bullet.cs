//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Xml.Linq;


	internal class Bullet : XElement
	{
		public Bullet(XNamespace ns, string text)
			: this(ns, new XElement(ns + "T", new XCData(text)))
		{
		}


		public Bullet(XNamespace ns, XElement content)
			: base(ns + "OE",
				new XElement(ns + "List",
					new XElement(ns + "Bullet", new XAttribute("bullet", "2"))),
				content)
		{
		}


		public Bullet(XNamespace ns, params XNode[] nodes)
			: base(ns + "OE",
				new XElement(ns + "List",
					new XElement(ns + "Bullet", new XAttribute("bullet", "2"))),
				nodes)
		{
		}
	}
}
