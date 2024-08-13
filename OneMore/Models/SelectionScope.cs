//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	/// <summary>
	/// Describes the selection state on the page or within an outline element
	/// </summary>
	internal enum SelectionScope
	{
		/// <summary>
		/// Can't resolve scope; focus likely on UI widgets rather than page
		/// </summary>
		None = 0,

		/// <summary>
		/// One or more top elements that are not T text runs, like OE or OEChildren.
		/// The value 3 is equivalent to Range+Run
		/// </summary>
		Block = 3,

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
		TextCursor = 4,
	}
}
