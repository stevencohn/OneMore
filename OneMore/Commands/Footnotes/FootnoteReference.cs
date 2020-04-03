//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Xml.Linq;


	/// <summary>
	/// Represents the location of a footnote reference including its containing XCData,
	/// the reference label, and the index and length of that label within the XCData.
	/// This may be either a contennt body superscript reference or a footer section
	/// footnote text line.
	/// </summary>
	internal class FootnoteReference
	{
		/// <summary>
		/// The containing XCData of the reference
		/// </summary>
		public XCData CData { get; set; }

		/// <summary>
		/// The reference label number extracted from the XCData
		/// </summary>
		public int Label { get; set; }

		/// <summary>
		/// The index/offset of the label within the XCData text
		/// </summary>
		public int Index { get; set; }

		/// <summary>
		/// The length of the label string within the XCData text
		/// </summary>
		public int Length { get; set; }
	}
}
