//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Office
{
	using System;
	using System.Collections.Generic;


	/// <summary>
	/// Typedef for a collection of Outlook tasks
	/// </summary>
	internal class OutlookTasks : List<OutlookTask> { }


	/// <summary>
	/// Represents a single Outlook task with properties needed by OneMore
	/// </summary>
	internal class OutlookTask
	{
		public const string PathDelimeter = "/";
		public const int UnspecifiedYear = 4000;

		public string Subject { get; set; }

		/// <summary>
		/// The unique ID of this task item. This is a mutable value; it will change if
		/// the task is moved to another Outlook folder
		/// </summary>
		public string EntryID { get; set; }

		public DateTime CreationTime { get; set; }
		public bool Complete { get; set; }
		public DateTime DateCompleted { get; set; }
		public DateTime DueDate { get; set; }
		public OutlookImportance Importance { get; set; }
		public int PercentComplete { get; set; }
		public DateTime StartDate { get; set; }
		public OutlookTaskStatus Status { get; set; }

		/// <summary>
		/// Gets or sets a transient property indicating the full folder path of the task
		/// </summary>
		public string FolderPath { get; set; }

		/// <summary>
		/// Gets or sets the ID assigned to this task by OneNote
		/// </summary>
		public string OneNoteTaskID { get; set; }

		/// <summary>
		/// Gets or sets the URL of where this task is embedded on a OneNote page
		/// </summary>
		public string OneNoteURL { get; set; }


		// Temporary properties used for report generation...

		public int Year { get; set; }
		public int WoYear { get; set; }
	}
}
