//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Office
{
	using Microsoft.Office.Interop.Outlook;
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Runtime.InteropServices;


	/// <summary>
	/// 
	/// </summary>
	internal class Outlook : IDisposable
	{
		private Application outlook;
		private bool disposed;


		/// <summary>
		/// Initializes a new instance of the Outlook interop application. If Outlook is not
		/// currently running, it will be started
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

			if (outlook != null)
			{
				Marshal.ReleaseComObject(outlook);
				outlook = null;
			}
		}


		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// Builds a hierarchy of task folders populated with their tasks
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
					Subject = item.Subject,
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


		/// <summary>
		/// 
		/// </summary>
		/// <param name="entryID"></param>
		/// <param name="oneNoteTaskID"></param>
		/// <param name="oneNoteURL"></param>
		/// <returns></returns>
		public bool BindTaskToOneNote(string entryID, string oneNoteTaskID, string oneNoteURL)
		{
			var task = outlook.Session.GetItemFromID(entryID) as TaskItem;
			if (task == null)
			{
				return false;
			}

			UserProperty prop;

			prop = task.UserProperties.Find("OneNoteTaskID")
				?? task.UserProperties.Add("OneNoteTaskID", OlUserPropertyType.olText);

			prop.Value = oneNoteTaskID;

			prop = task.UserProperties.Find("OneNoteURL")
				?? task.UserProperties.Add("OneNoteURL", OlUserPropertyType.olText);
			
			prop.Value = oneNoteURL;

			task.Save();

			return true;
		}
	}
}
