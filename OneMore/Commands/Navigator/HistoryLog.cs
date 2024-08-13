//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Newtonsoft.Json;
	using System.Collections.Generic;
	using HistoryRecord = OneNote.HierarchyInfo;


	/// <summary>
	/// POCO of navigation Json file
	/// </summary>
	internal class HistoryLog
	{

		public HistoryLog()
		{
			History = new List<HistoryRecord>();
			Pinned = new List<HistoryRecord>();
		}


		/// <summary>
		/// Gets or sets the saved history items.
		/// </summary>
		[JsonProperty("history")]
		public List<HistoryRecord> History { get; set; }


		/// <summary>
		/// Gets or sets the saved pinned items.
		/// </summary>
		[JsonProperty("pinned")]
		public List<HistoryRecord> Pinned { get; set; }
	}
}
