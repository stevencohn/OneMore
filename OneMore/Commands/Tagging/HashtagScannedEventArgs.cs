//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;


	/// <summary>
	/// Args sent to OnHashtagScanned listeners
	/// </summary>
	internal class HashtagScannedEventArgs : EventArgs
	{
		public HashtagScannedEventArgs(int total, int dirty, long time)
		{
			TotalPages = total;
			DirtyPages = dirty;
			Time = time;
		}


		public HashtagScannedEventArgs(string message)
		{
			ErrorMessage = message;
			Failed = true;
		}


		/// <summary>
		/// Gets the number of pages found with updated hashtags
		/// </summary>
		public int DirtyPages { private set; get; }


		/// <summary>
		/// Gets the time (ms) it took to complete the last scan
		/// </summary>
		public long Time { private set; get; }


		/// <summary>
		/// Gets the total number of pages analyzed in the last scan
		/// </summary>
		public int TotalPages { private set; get; }


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Failure state...

		/// <summary>
		/// Gets the error message from a failed scanner
		/// </summary>
		public string ErrorMessage { private set; get; }


		/// <summary>
		/// Gets a Boolean indicating whether the scanner has failed; check ErrorMessage.
		/// </summary>
		public bool Failed { private set; get; }
	}
}
