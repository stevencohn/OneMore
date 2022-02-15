//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.ComponentModel;
	using System.Xml.Linq;


	internal sealed class CrawlHyperlink
	{

		// prevent this property from appearing in DataGridView
		[Browsable(false)]
		public XCData CData { get; set; }

		public bool Selected { get; set; }

		public string Address { get; set; }

		public string Text { get; set; }

		public int Order { get; set; }
	}

}
