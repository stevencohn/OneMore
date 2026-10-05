//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	/// <summary>
	/// Where a page with hashtags currently is, as recorded in the hashtag catalog. The page
	/// key identifies the page; the rest says how to reach it and is refreshed when it moves
	/// or when OneNote regenerates its IDs.
	/// </summary>
	internal sealed class HashtagPageInfo
	{
		/// <summary>The page key from the identity catalog, as text.</summary>
		public string MoreID { get; set; }

		public string PageID { get; set; }

		public string TitleID { get; set; }

		public string NotebookID { get; set; }

		public string SectionID { get; set; }

		/// <summary>The path of the section, such as /Notebook/Group/Section.</summary>
		public string Path { get; set; }

		/// <summary>The title of the page.</summary>
		public string Name { get; set; }


		/// <summary>
		/// Gets whether this describes the same location as the other, ignoring the title
		/// paragraph ID, which only a scan of the page can supply.
		/// </summary>
		public bool SameLocation(HashtagPageInfo other)
		{
			return string.Equals(PageID, other.PageID, System.StringComparison.Ordinal)
				&& string.Equals(NotebookID, other.NotebookID, System.StringComparison.Ordinal)
				&& string.Equals(SectionID, other.SectionID, System.StringComparison.Ordinal)
				&& string.Equals(Path, other.Path, System.StringComparison.Ordinal)
				&& string.Equals(Name, other.Name, System.StringComparison.Ordinal);
		}
	}
}
