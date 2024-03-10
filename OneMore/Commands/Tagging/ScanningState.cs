//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{

	/// <summary>
	/// Indicates the current state of the hashtag catalog and a scheduled scan.
	/// </summary>
	internal enum ScanningState
	{
		/// <summary>
		/// Catalog is missing but no oustanding schedule.
		/// </summary>
		None,

		/// <summary>
		/// No catalog; scheduled to rebuild catalog and scan all notebooks
		/// </summary>
		PendingRebuild,


		/// <summary>
		/// User requested a scan to import a large amount of data, e.g. newly added notebooks
		/// </summary>
		PendingScan,


		/// <summary>
		/// In process of rebuilding the catalog db; this should be very quick!
		/// </summary>
		Rebuilding,


		/// <summary>
		/// Currently scanning notebooks for hashatags
		/// </summary>
		Scanning,

		/// <summary>
		/// Catalog is ready to query
		/// </summary>
		Ready
	}
}