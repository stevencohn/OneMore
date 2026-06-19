//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Workspaces
{
	/// <summary>
	/// The ephemeral status of a page reference after verification by TargetChecker.
	/// </summary>
	internal enum TargetStatus
	{
		/// <summary>
		/// Found by notebookID + objectID, high confidence that this is the same target
		/// as originally referenced.
		/// </summary>
		Known,

		/// <summary>
		/// Not found by notebookID + objectID but found by path, screentip, or other
		/// heuristic, low/medium confidence
		/// </summary>
		Suspect,

		/// <summary>
		/// Not found by notebookID + objectID or by any heuristic, zero confidence but at least
		/// has notebookID + objectID
		/// </summary>
		Unknown
	}


	/// <summary>
	/// Common shape shared by Favorite and LayoutWindow, letting TargetChecker verify and
	/// auto-path-repair either one without depending on either concrete type.
	/// </summary>
	internal interface ITargetReference
	{
		/// <summary>
		/// The name of the target, either a section, section group, or page name.
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// The hierarchy path of the target including notebook, section groups and sections.
		/// </summary>
		string Location { get; set; }

		/// <summary>
		/// The onenote:// URL of the target, which may be used as a hyperlink or for
		/// verification. This is the raw URL as stored in the database and may not be valid
		/// if the target is no longer reachable at that URL.
		/// </summary>
		string Uri { get; set; }

		/// <summary>
		/// The ID of the notebook that contains the target.
		/// </summary>
		string NotebookID { get; set; }

		/// <summary>
		/// The ID of the section or section group that contains the target or the target itself.
		/// </summary>
		string SectionID { get; set; }

		/// <summary>
		/// The ID of the page that is the target, or null if the target is a section/section
		/// group rather than a page.
		/// </summary>
		string PageID { get; set; }

		/// <summary>
		/// The Verified status of the target.
		/// </summary>
		TargetStatus Status { get; set; }
	}
}
