//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{

	/// <summary>
	/// Describes the selection state on the page or within an outline element
	/// </summary>
	internal enum SelectionScope
	{
		/// <summary>
		/// Can't decipher scope
		/// </summary>
		Unknown,

		/// <summary>
		/// Insertion cursor, zero-width selection
		/// </summary>
		Empty,

		/// <summary>
		/// A region with more than one run is selected
		/// </summary>
		Region,

		/// <summary>
		/// Exactly one non-empty run is selected
		/// </summary>
		Run,

		/// <summary>
		/// CDATA contains an anchor link or an XML comment
		/// </summary>
		Special
	}
}
