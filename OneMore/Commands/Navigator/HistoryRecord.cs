//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{

	/// <summary>
	/// A record of a single visited page including its ID and last time seen.
	/// </summary>
	internal class HistoryRecord
	{

		/// <summary>
		/// Gets or sets the ID of the page
		/// </summary>
		public string PageID {  get; set; }


		/// <summary>
		/// Gets or sets the last visited timestamp for this page
		/// </summary>
		public long Visited { get; set; }
	}
}
