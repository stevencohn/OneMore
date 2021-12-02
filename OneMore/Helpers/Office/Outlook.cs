//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Office
{
	using Microsoft.Office.Interop.Outlook;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Runtime.InteropServices;

	internal enum OutlookImportance
	{
		Low = OlImportance.olImportanceLow,
		Normal = OlImportance.olImportanceNormal,
		High = OlImportance.olImportanceHigh
	}
	internal enum OutlookTaskStatus
	{
		Complete = OlTaskStatus.olTaskComplete,
		Deferred = OlTaskStatus.olTaskDeferred,
		InProgress = OlTaskStatus.olTaskInProgress,
		NotStarted = OlTaskStatus.olTaskNotStarted,
		Waiting = OlTaskStatus.olTaskWaiting
	}

	internal class OutlookTasks : List<OutlookTask> { }

	internal class OutlookTaskFolders : List<OutlookTaskFolder> { }

	internal class OutlookTask
	{
		public string Name { get; set; }
		public string EntryID { get; set; }
		public bool Complete { get; set; }
		public DateTime DateCompleted { get; set; }
		public DateTime DueDate { get; set; }
		public OutlookImportance Importance { get; set; }
		public int PercentComplete { get; set; }
		public OutlookTaskStatus Status { get; set; }
		public string OneNoteTaskID { get; set; }
		public string OneNoteURL { get; set; }
	}

	internal class OutlookTaskFolder
	{
		public string Name { get; set; }
		public string EntryID { get; set; }
		public OutlookTaskFolders Folders { get; private set; }
		public OutlookTasks Tasks { get; private set; }
		public OutlookTaskFolder()
		{
			Folders = new OutlookTaskFolders();
			Tasks = new OutlookTasks();
		}
	}


	/// <summary>
	/// 
	/// </summary>
	internal class Outlook : IDisposable
	{
		private Application outlook;
		private bool disposed;


		/// <summary>
		/// 
		/// </summary>
		public Outlook()
		{
			outlook = new Application();

			// if Outlook is already open then don't close it in Dispose
			disposed = Process.GetProcessesByName("OUTLOOK").Any();
		}


		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				outlook.Quit();
				disposed = true;
			}

			outlook = null;
		}


		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public OutlookTaskFolders GetTaskHierarchy()
		{
			return GetTaskHierarchy(
				outlook.Session.GetDefaultFolder(OlDefaultFolders.olFolderTasks) as Folder,
				new OutlookTaskFolders());
		}

		private OutlookTaskFolders GetTaskHierarchy(
			Folder parent, OutlookTaskFolders container)
		{
			var folder = new OutlookTaskFolder
			{
				Name = parent.Name,
				EntryID = parent.EntryID
			};

			var items = parent.Items;
			items.IncludeRecurrences = true;
			foreach (TaskItem item in items)
			{
				folder.Tasks.Add(new OutlookTask
				{
					Name = item.Subject,
					EntryID = item.EntryID,
					Complete = item.Complete,
					DateCompleted = item.DateCompleted,
					DueDate = item.DueDate,
					Importance = (OutlookImportance)item.Importance,
					PercentComplete = item.PercentComplete,
					Status = (OutlookTaskStatus)item.Status,
					OneNoteTaskID = item.UserProperties["OneNoteTaskID"]?.Value as string,
					OneNoteURL = item.UserProperties["OneNoteTaskURL"]?.Value as string
				});

				Marshal.ReleaseComObject(item);
			}

			container.Add(folder);

			foreach (Folder child in parent.Folders)
			{
				GetTaskHierarchy(child, folder.Folders);
				Marshal.ReleaseComObject(child);
			}

			Marshal.ReleaseComObject(parent);
			return container;
		}
	}
}
