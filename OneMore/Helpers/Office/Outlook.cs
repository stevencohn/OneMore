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


	/// <summary>
	/// Automated interaction with Outlook
	/// </summary>
	internal class Outlook : IDisposable
	{
		private Application outlook;
		private bool disposed;

		#region class FolderPath
		private sealed class FolderPath
		{
			public string EntryID { get; set; }
			public string Path { get; set; }
		}
		#endregion class FolderPath



		/// <summary>
		/// Initializes a new instance of the Outlook interop application. If Outlook is not
		/// currently running, it will be started
		/// </summary>
		public Outlook()
		{
			outlook = new Application();
		}


		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				// this automation class will create a process with the -Embedding cmdline
				// switch but a user-started Outlook will not so look for a user process
				// and skip disposing so we don't interrupt the user's interactive session
				if (!Process.GetProcessesByName("OUTLOOK")
					.Any(p =>
					{
						var cmd = p.GetCommandLine();
						return cmd != null && !cmd.Contains("-Embedding");
					}))
				{
					// unfortunately, the above assumptions are not true; if an embedded instance
					// is running and Outlook UI is then started, the embedded instance will
					// take over so we still have an -Embedding processing serving UI!
					//outlook.Quit();
					Marshal.ReleaseComObject(outlook);
					disposed = true;
				}
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
			Folder parent, OutlookTaskFolders container, string path = null)
		{
			var folder = new OutlookTaskFolder
			{
				Name = parent.Name,
				EntryID = parent.EntryID
			};

			path = path == null ? parent.Name : $"{path}{OutlookTask.PathDelimeter}{parent.Name}";

			var items = parent.Items;
			items.IncludeRecurrences = true;
			foreach (TaskItem item in items)
			{
				folder.Tasks.Add(new OutlookTask
				{
					Subject = item.Subject,
					EntryID = item.EntryID,
					Complete = item.Complete,
					CreationTime = item.CreationTime,
					DateCompleted = item.DateCompleted,
					DueDate = item.DueDate,
					Importance = (OutlookImportance)item.Importance,
					PercentComplete = item.PercentComplete,
					StartDate = item.StartDate,
					Status = (OutlookTaskStatus)item.Status,
					FolderPath = path,
					OneNoteTaskID = item.UserProperties["OneNoteTaskID"]?.Value as string,
					OneNoteURL = item.UserProperties["OneNoteTaskURL"]?.Value as string
				});

				Marshal.ReleaseComObject(item);
			}

			container.Add(folder);

			foreach (Folder child in parent.Folders)
			{
				GetTaskHierarchy(child, folder.Folders, path);
				Marshal.ReleaseComObject(child);
			}

			Marshal.ReleaseComObject(parent);
			return container;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="taskIDs"></param>
		/// <returns></returns>
		public OutlookTasks LoadTasks(IEnumerable<string> taskIDs)
		{
			var paths = GetFolders();
			var tasks = new OutlookTasks();
			foreach (var id in taskIDs)
			{
				if (outlook.Session.GetItemFromID(id) is TaskItem item)
				{
					var parent = item.Parent as Folder;

					tasks.Add(new OutlookTask
					{
						Subject = item.Subject,
						EntryID = item.EntryID,
						Complete = item.Complete,
						CreationTime = item.CreationTime,
						DateCompleted = item.DateCompleted,
						DueDate = item.DueDate,
						Importance = (OutlookImportance)item.Importance,
						PercentComplete = item.PercentComplete,
						StartDate = item.StartDate,
						Status = (OutlookTaskStatus)item.Status,
						FolderPath = paths[parent.EntryID],
						OneNoteTaskID = item.UserProperties["OneNoteTaskID"]?.Value as string,
						OneNoteURL = item.UserProperties["OneNoteTaskURL"]?.Value as string
					});

					Marshal.ReleaseComObject(item);
					Marshal.ReleaseComObject(parent);
				}
			}

			return tasks;
		}


		/// <summary>
		/// Builds a lookup cache of folders to optimize LoadTask so it doesn't need to
		/// discover the hierarchy path of each TaskItem.
		/// </summary>
		/// <returns></returns>
		private Dictionary<string, string> GetFolders()
		{
			var root = outlook.Session.GetDefaultFolder(OlDefaultFolders.olFolderTasks) as Folder;
			return GetFolders(root, null).ToDictionary(e => e.EntryID, e => e.Path);
		}

		private IEnumerable<FolderPath> GetFolders(Folder folder, string path)
		{
			var currpath = path == null ? folder.Name : $"{path}/{folder.Name}";
			yield return new FolderPath { EntryID = folder.EntryID, Path = currpath };
			foreach (Folder child in folder.Folders)
			{
				var xx = GetFolders(child, currpath);
				foreach (var x in xx)
				{
					yield return x;
				}

				Marshal.ReleaseComObject(child);
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public bool SaveTask(OutlookTask task)
		{
			if (!(outlook.Session.GetItemFromID(task.EntryID) is TaskItem item))
			{
				return false;
			}

			try
			{
				UserProperty prop;
				prop = item.UserProperties.Find("OneNoteTaskID")
					?? item.UserProperties.Add("OneNoteTaskID", OlUserPropertyType.olText);

				prop.Value = task.OneNoteTaskID;

				prop = item.UserProperties.Find("OneNoteURL")
					?? item.UserProperties.Add("OneNoteURL", OlUserPropertyType.olText);

				prop.Value = task.OneNoteURL;

				item.Save();
			}
			finally
			{
				Marshal.ReleaseComObject(item);
			}

			return true;
		}
	}
}
