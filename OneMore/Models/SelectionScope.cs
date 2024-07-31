//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;

	/// <summary>
	/// Describes the selection state on the page or within an outline element
	/// </summary>
	[Flags]
	internal enum SelectionScope
	{
		/// <summary>
		/// Can't decipher scope; focus likely on UI widgets rather than page
		/// </summary>
		None = 0,

		/// <summary>
		/// A region with more than one run is selected
		/// </summary>
		Range = 2,

		/// <summary>
		/// Exactly one non-empty run is selected
		/// </summary>
		Run = 1,

		/// <summary>
		/// Exactly one non-empty run containing an anchor link, mathML, or XML comment
		/// </summary>
		SpecialCursor = 8,

		/// <summary>
		/// Insertion caret "text cursor", zero-width selection
		/// </summary>
		TextCursor = 4
	}
}
