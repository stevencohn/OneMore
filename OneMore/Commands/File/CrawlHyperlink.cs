//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.ComponentModel;
	using System.Xml.Linq;


	internal sealed class CrawlHyperlink
	{

		[Browsable(false)]
		public XCData CData { get; set; }

		public bool Selected { get; set; }

		public string Address { get; set; }

		public string Text { get; set; }

		public int Order { get; set; }
	}

}
