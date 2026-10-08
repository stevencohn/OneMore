//************************************************************************************************
// Copyright © 2026 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	/// <summary>
	/// The pages whose links are checked by <see cref="RepairUrlsCommand"/>.
	/// </summary>
	internal enum RepairUrlsScope
	{
		/// <summary>The current page, or only the selection if there is one.</summary>
		Page,

		/// <summary>
		/// The current page with its indented subpages, or the parent of the current page
		/// with all of its subpages if the current page is itself a subpage.
		/// </summary>
		PageGroup,

		/// <summary>All pages in the current section.</summary>
		Section,

		/// <summary>All pages in all sections of the section group that holds the current section.</summary>
		SectionGroup,

		/// <summary>All pages in the current notebook.</summary>
		Notebook,

		/// <summary>All pages in all open notebooks.</summary>
		Notebooks
	}
}
