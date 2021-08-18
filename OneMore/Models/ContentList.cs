//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Xml.Linq;


	internal class ContentList : XElement
	{
		public ContentList(XNamespace ns)
			: base(ns + "OEChildren")
		{
		}


		public ContentList(XNamespace ns, params Bullet[] bullets)
			: this(ns)
		{
			Add(bullets);
		}
	}
}
