//************************************************************************************************
// Copyright © 2026 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	/// <summary>
	/// What <see cref="RepairUrlsCommand"/> did with a link.
	/// </summary>
	internal enum RepairUrlsOutcome
	{
		/// <summary>The link was out of date and now points to the page where it is now.</summary>
		Repaired,

		/// <summary>The link could not be repaired and its text was highlighted.</summary>
		Highlighted,

		/// <summary>The link was left as it was, because it is not safe to judge or to change.</summary>
		Unchanged
	}


	/// <summary>
	/// One link that <see cref="RepairUrlsCommand"/> repaired, highlighted or left alone because
	/// it could not tell. Links that are fine are not recorded.
	/// </summary>
	internal sealed class RepairUrlsResult
	{
		public RepairUrlsOutcome Outcome { get; set; }

		/// <summary>The visible text of the link</summary>
		public string LinkText { get; set; }

		/// <summary>The ID of the page that holds the link, used to go to it</summary>
		public string PageID { get; set; }

		/// <summary>The title of the page that holds the link</summary>
		public string PageTitle { get; set; }

		/// <summary>Says why the link was highlighted or left alone, or where it points now</summary>
		public string Detail { get; set; }
	}
}
