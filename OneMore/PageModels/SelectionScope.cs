//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	/// <summary>
	/// Describes the selection state of an OE paragraph, mirroring the values of the
	/// "selected" attribute in OneNote XML.
	/// </summary>
	internal enum SelectionScope
	{
		/// <summary>No selection touches this element.</summary>
		None,

		/// <summary>
		/// A selection passes through this element but does not cover it entirely.
		/// Set on OE (and ancestor) elements when some but not all child T runs are selected.
		/// </summary>
		Partial,

		/// <summary>This element is fully selected (selected="all").</summary>
		All
	}
}
