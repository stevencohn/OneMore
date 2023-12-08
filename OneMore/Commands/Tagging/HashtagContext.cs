//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;


	/// <summary>
	/// A container model used to collate all tags on a page into one instance so it
	/// can be displayed as a search result
	/// </summary>
	internal class HashtagContext : Hashtag
	{
		/// <summary>
		/// Initialize a new instance, inferring page details from the given tag
		/// and initializing the Snippets collection with the tag's snippet
		/// </summary>
		/// <param name="tag">The hashtag to clone</param>
		public HashtagContext(Hashtag tag)
		{
			MoreID = tag.MoreID;
			PageID = tag.PageID;
			TitleID = tag.TitleID;
			NotebookID = tag.NotebookID;
			SectionID = tag.SectionID;
			HierarchyPath = tag.HierarchyPath;
			PageTitle = tag.PageTitle;
			ScanTime = tag.ScanTime;

			Snippets = new HashtagSnippets
			{
				new(tag.ObjectID, tag.Snippet, tag.ScanTime)
			};
		}


		public HashtagSnippets Snippets { get; set; }
	}


	internal class HashtagContexts : List<HashtagContext> { }


	/// <summary>
	/// A paragraph on the page containing at least one tag
	/// </summary>
	internal class HashtagSnippet
	{
		public HashtagSnippet(string objectID, string snippet, string scanTime)
		{
			ObjectID = objectID;
			Snippet = snippet;
			ScanTime = scanTime;
		}

		public string ObjectID { get; set; }
		public string Snippet { get; set; }
		public string ScanTime { get; set; }
	}


	internal class HashtagSnippets : List<HashtagSnippet> { }
}
