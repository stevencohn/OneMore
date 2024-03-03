//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreTray
{
	using System;


	internal class ScanRequest
	{

		/// <summary>
		/// The schedule time to build/rebuild the hashtag database.
		/// Could be past or future; if past then run immediately.
		/// </summary>
		public DateTime ScheduledRebuild { get; set; }


		/// <summary>
		/// Indicates whether to shutdown the tray after operation completes
		/// </summary>
		public bool Shutdown { get; set; }


		/// <summary>
		/// Indicates whether the operation is in progress; is not reset until after
		/// operation is confirmed to be completed, so can be used to recognize 
		/// interruptions and infer continuation
		/// </summary>
		public bool Running { get; set; }
	}
}
