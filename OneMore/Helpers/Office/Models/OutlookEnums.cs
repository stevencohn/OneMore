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
		Complete = OlTaskStatus.olTaskComplete,
		Deferred = OlTaskStatus.olTaskDeferred,
		InProgress = OlTaskStatus.olTaskInProgress,
		NotStarted = OlTaskStatus.olTaskNotStarted,
		Waiting = OlTaskStatus.olTaskWaiting
	}
}