//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	internal enum StyleType
	{
		/// <summary>
		/// Style applies to characters and words within a paragraph;
		/// does not apply paragraph styles to select text
		/// </summary>
		Character,

		/// <summary>
		/// Style applies to the entire paragraph regardless of selection
		/// </summary>
		Paragraph,

		/// <summary>
		/// Special type of Paragraph that is included when generated Table of Contents
		/// </summary>
		Heading
	}
}
