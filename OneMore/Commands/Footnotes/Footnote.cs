//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Xml.Linq;


	internal class Footnote
	{
		public int Number { get; set; }

		public string Text { get; set; }

		public XElement Footer { get; set; }

		public XElement Paragraph { get; set; }
	}
}
