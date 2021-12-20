//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Office
{
	using System.Collections.Generic;


	/// <summary>
	/// Typedef for a collection of Outlook task folders
	/// </summary>
	internal class OutlookTaskFolders : List<OutlookTaskFolder> { }


	/// <summary>
	/// Represents a single Outlook task folder with properties needed by OneMore
	/// </summary>
	internal class OutlookTaskFolder
	{

		/// <summary>
		/// Gets or sets the short name of the folder
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The unique ID of this folder item. This is a mutable value; it will change if
		/// the folder is moved to another parent Outlook folder
		/// </summary>
		public string EntryID { get; set; }

		/// <summary>
		/// Gets the list of subfolders beneath this folder
		/// </summary>
		public OutlookTaskFolders Folders { get; private set; }

		/// <summary>
		/// Gets the list of tasks assigned to this folder
		/// </summary>
		public OutlookTasks Tasks { get; private set; }


		/// <summary>
		/// Initialize a new instance of this folder
		/// </summary>
		public OutlookTaskFolder()
		{
			Folders = new OutlookTaskFolders();
			Tasks = new OutlookTasks();
		}
	}
}
