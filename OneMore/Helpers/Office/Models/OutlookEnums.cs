//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Office
{
	using Microsoft.Office.Interop.Outlook;


	/// <summary>
	/// Wrap the OlImportance enumerations
	/// </summary>
	internal enum OutlookImportance
	{
		Low = OlImportance.olImportanceLow,
		Normal = OlImportance.olImportanceNormal,
		High = OlImportance.olImportanceHigh
	}


	/// <summary>
	/// Wrap the OlTaskStatus enumerations
	/// </summary>
	internal enum OutlookTaskStatus
	{
		NotStarted = OlTaskStatus.olTaskNotStarted,
		InProgress = OlTaskStatus.olTaskInProgress,
		Complete = OlTaskStatus.olTaskComplete,
		Waiting = OlTaskStatus.olTaskWaiting,
		Deferred = OlTaskStatus.olTaskDeferred
	}
}