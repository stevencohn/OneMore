//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;


	internal class TableCell : CommonAttributes
	{
		public string objectID;
		public Selected selected;
		public DateTimeOffset lastModifiedTime;
		// COLOR :: <xsd:pattern value="#[a-fA-F0-9]{6}|none|automatic"/>
		public string shadingColor;
		public uint meetingContentType;
		//public List<Paragraph> children;
	}
}
