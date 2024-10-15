//************************************************************************************************
// Copyright © 2024 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;


	/// <summary>
	/// Attributes of a notebook scanned by the HashtagScanner
	/// </summary>
	internal class HashtagNotebook
	{
		/// <summary>
		/// Gets or sets the OneNote ID of the notebook
		/// </summary>
		public string NotebookID { get; set; }


		/// <summary>
		/// Gets or sets the name of the notebook
		/// </summary>
		public string Name { get; set; }


		/// <summary>
		/// Gets or sets a value indicating the last known date when a hashtag
		/// was discovered and recorded or delete in this notebook
		/// </summary>
		public string LastModified { get; set; }
	}


	/// <summary>
	/// A collection of HashtagNotebooks
	/// </summary>
	internal class HashtagNotebooks : List<HashtagNotebook>
	{
		public bool Contains(string notebookID)
		{
			return Find(e => e.NotebookID == notebookID) is not null;
		}
	}
}
