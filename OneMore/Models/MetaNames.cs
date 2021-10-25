//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{

	/// <summary>
	/// Reserved Page level meta tags
	/// </summary>
	internal static class MetaNames
	{

		// data storage analysis report
		public static readonly string AnalysisReport = "omAnalysisReport";

		// keep track of rotating highlighter index
		public static readonly string HighlightIndex = "omHighlightIndex";

		// page is reference linked to another page, so don't include it in subsequent links
		public static readonly string LinkReference = "omLinkReference";

		// page is a reference map, so don't include it in subsequent maps
		public static readonly string PageMap = "omPageMap";

		// serialized reminder store for current page
		public static readonly string Reminder = "omReminder";

		// page is a reminder summary report, content is scope
		public static readonly string ReminderReport = "omReminderReport";

		// Outline meta to mark visible word bank
		public static readonly string TaggingBank = "omTaggingBank";

		// page tag list
		public static readonly string TaggingLabels = "omTaggingLabels";
	}
}
