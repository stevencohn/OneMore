//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Styles
{

	/// <summary>
	/// Defines the type of a style and the scope to which the style should be applied.
	/// </summary>
	public enum StyleType
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
