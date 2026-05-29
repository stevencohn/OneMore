//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Xml.Linq;


	internal class MatchInfo
	{
		public XElement TElement { get; set; }
		public int StartIndex { get; set; }
		public int Length { get; set; }
		public string MatchedText { get; set; }
		public string ParagraphObjectId { get; set; }
	}
}
