//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Office
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Runtime.InteropServices;
	using Microsoft.Office.Interop.Outlook;


	/// <summary>
	/// Automated interaction with Outlook
	/// </summary>
	internal class Outlook : IDisposable
	{
		private Application outlook;
		private bool disposed;

		private Calendar calendar;
		private CalendarWeekRule weekRule;
		private DayOfWeek firstDay;


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
			if (disposed || outlook == null)
			{
				return;
			}

			// this automation class will create a process with the -Embedding cmdline
			// switch but a user-started Outlook will not so look for a user process
			// and skip disposing so we don't interrupt the user's interactive session
			// (note: an embedded instance can be taken over by a later UI launch, so the
			// check is best-effort; we always release our RCW regardless)
			//outlook.Quit();
			Marshal.ReleaseComObject(outlook);
			outlook = null;
			disposed = true;
		}


		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// Returns a list of all categories defined in the current Outlook session.
		/// </summary>
		/// <returns>An enumerable collection of OutlookCategory instances</returns>
		public IEnumerable<OutlookCategory> GetCategories()
		{
			foreach (Category cat in outlook.Session.Categories)
			{
				yield return new OutlookCategory
				{
					Name = cat.Name,

					// color is a string of the form olCategoryColor<name>
					// so strip the "olCategoryColor" prefix to get the actual color name
					ColorName = cat.Color.ToString().Substring(15)
				};
			}
		}


		/// <summary>
		/// Returns a list of all contact folders defined in the current Outlook session.
		/// </summary>
		/// <returns>An enumerable collection of OutlookFolder instances</returns>
		public IEnumerable<OutlookFolder> GetContactFolders()
		{
			var ns = outlook.GetNamespace("MAPI");
			//Logger.Current.WriteLine($"default store: {ns.DefaultStore.DisplayName}");

			// deleted Items folder path (language‑safe)
			var deletedFolder = ns.GetDefaultFolder(OlDefaultFolders.olFolderDeletedItems);
			var deletedFolderPath = deletedFolder.FolderPath;

			foreach (Store store in ns.Stores)
			{
				if (store.GetRootFolder() is Folder root)
				{
					foreach (var folder in EnumerateFolders(root))
					{
						yield return new OutlookFolder(folder);
					}
				}
			}

			IEnumerable<Folder> EnumerateFolders(Folder folder)
			{
				// skip Deleted Items subtree
				if (folder.FolderPath.StartsWith(deletedFolderPath, StringComparison.OrdinalIgnoreCase))
				{
					yield break;
				}

				// skip hidden folders
				if (IsHidden(folder))
				{
					yield break;
				}

				// only real contact folders
				if (folder.DefaultItemType == OlItemType.olContactItem &&
					folder.DefaultMessageClass == "IPM.Contact")
				{
					yield return folder;
				}

				// recurse
				foreach (Folder sub in folder.Folders)
				{
					foreach (var f in EnumerateFolders(sub))
					{
						yield return f;
					}
				}
			}

			bool IsHidden(Folder folder)
			{
				const string PR_ATTR_HIDDEN = "http://schemas.microsoft.com/mapi/proptag/0x10F4000B";

				try
				{
					var pa = folder.PropertyAccessor;
					var value = pa.GetProperty(PR_ATTR_HIDDEN);
					return value is bool b && b;
				}
				catch
				{
					return false; // property not present → treat as visible
				}
			}
		}


		/// <summary>
		/// Loads specific contacts by their Outlook EntryID, used to refresh a previously
		/// generated contacts report without re-enumerating every contact folder.
		/// </summary>
		/// <param name="contactIDs">The EntryIDs of the contacts to load</param>
		/// <returns>An enumerable collection of OutlookContact instances</returns>
		public IEnumerable<OutlookContact> LoadContactsByID(IEnumerable<string> contactIDs)
		{
			foreach (var id in contactIDs)
			{
				if (outlook.Session.GetItemFromID(id) is ContactItem item)
				{
					yield return new OutlookContact(item);
				}
			}
		}


		/// <summary>
		/// Builds a hierarchy of task folders populated with their tasks
		/// </summary>
		/// <returns></returns>
		public OutlookTaskFolders GetTaskHierarchy()
		{
			var culture = CultureInfo.CurrentUICulture;
			calendar = culture.Calendar;
			weekRule = culture.DateTimeFormat.CalendarWeekRule;
			firstDay = culture.DateTimeFormat.FirstDayOfWeek;

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
					Categories = item.Categories,
					Complete = item.Complete,
					CreationTime = item.CreationTime,
					DateCompleted = item.DateCompleted,
					DueDate = item.DueDate,
					Importance = (OutlookImportance)item.Importance,
					PercentComplete = item.PercentComplete,
					StartDate = item.StartDate,
					Status = (OutlookTaskStatus)item.Status,
					FolderPath = path,
					Year = item.DueDate.Year,
					WoYear = calendar.GetWeekOfYear(item.DueDate, weekRule, firstDay),
					OneNoteTaskID = item.UserProperties["OneNoteTaskID"]?.Value as string,
					OneNoteURL = item.UserProperties["OneNoteURL"]?.Value as string,
					OneNotePageID = item.UserProperties["OneNotePageID"]?.Value as string,
					OneNoteObjectID = item.UserProperties["OneNoteObjectID"]?.Value as string
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
		public OutlookTasks LoadTasksByID(IEnumerable<string> taskIDs)
		{
			var tasks = new OutlookTasks();

			var hierarchy = GetTaskHierarchy();
			if (hierarchy.Any())
			{
				tasks.AddRange(Flatten(hierarchy).Where(t => taskIDs.Contains(t.OneNoteTaskID)));
			}

			return tasks;
		}


		private IEnumerable<OutlookTask> Flatten(OutlookTaskFolders folders)
		{
			foreach (var folder in folders)
			{
				foreach (var task in folder.Tasks
					.Where(t => !string.IsNullOrEmpty(t.OneNoteTaskID)))
				{
					yield return task;
				}

				foreach (var task in Flatten(folder.Folders)
					.Where(t => !string.IsNullOrEmpty(t.OneNoteTaskID)))
				{
					yield return task;
				}
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public bool SaveTask(OutlookTask task)
		{
			if (outlook.Session.GetItemFromID(task.EntryID) is not TaskItem item)
			{
				return false;
			}

			try
			{
				SetUserProperty(item, "OneNoteTaskID", task.OneNoteTaskID);
				SetUserProperty(item, "OneNoteURL", task.OneNoteURL);
				SetUserProperty(item, "OneNotePageID", task.OneNotePageID);
				SetUserProperty(item, "OneNoteObjectID", task.OneNoteObjectID);

				item.Save();
			}
			finally
			{
				Marshal.ReleaseComObject(item);
			}

			return true;
		}


		private void SetUserProperty(TaskItem item, string name, string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				// UserProperties is a base-1 index!
				var index = 1;
				while (index <= item.UserProperties.Count && item.UserProperties[index].Name != name)
				{
					index++;
				}

				if (index < item.UserProperties.Count)
				{
					item.UserProperties.Remove(index);
				}
			}
			else
			{
				var prop = item.UserProperties.Find(name)
					?? item.UserProperties.Add(name, OlUserPropertyType.olText);

				prop.Value = value;
			}
		}
	}
}
