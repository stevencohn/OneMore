//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;


	internal class HashtagContext : Hashtag
	{
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
