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
			LastModified = tag.LastModified;

			Snippets = new HashtagSnippets
			{
				new(tag.ObjectID, tag.Snippet, tag.DirectHit, tag.LastModified)
			};
		}


		/// <summary>
		/// Gets or sets a value indicating that this tag is from a currently loaded and
		/// available notebook. Cataloged tags from unloaded notebooks will be marked unavailable
		/// </summary>
		public bool Available { get; set; }


		/// <summary>
		/// Gets or sets the body-level snippets of text on the page that contain contextual tags
		/// </summary>
		public HashtagSnippets Snippets { get; set; }


		public bool HasSnippet(string objectID)
		{
			return Snippets.Exists(s => s.ObjectID == objectID);
		}


		public void AddSnippet(Hashtag tag)
		{
			Snippets.Add(new HashtagSnippet(tag.ObjectID, tag.Snippet, tag.DirectHit, tag.LastModified));
			if (tag.LastModified.CompareTo(LastModified) > 0)
			{
				LastModified = tag.LastModified;
			}
		}
	}


	internal class HashtagContexts : List<HashtagContext> { }


	/// <summary>
	/// A paragraph on the page containing at least one tag
	/// </summary>
	internal class HashtagSnippet
	{
		public HashtagSnippet(string objectID, string snippet, bool directHit, string lastModified)
		{
			ObjectID = objectID;
			Snippet = snippet;
			DirectHit = directHit;
			LastModified = lastModified;
		}

		public bool DirectHit { get; set; }
		public string ObjectID { get; set; }
		public string Snippet { get; set; }
		public string LastModified { get; set; }
	}


	internal class HashtagSnippets : List<HashtagSnippet> { }
}
