//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S3249 // Classes directly extending "object" should not call "base"

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;


	/// <summary>
	/// A ##hashtag model
	/// </summary>
	internal class Hashtag
	{
		/// <summary>
		/// The hashtag string including its ## prefix
		/// </summary>
		public string Tag { get; set; }


		/// <summary>
		/// The unique assigned OneMore page ID
		/// </summary>
		public string MoreID { get; set; }


		/// <summary>
		/// The OneNote page ID
		/// </summary>
		public string PageID { get; set; }


		/// <summary>
		/// The OneNote ID of the page title paragraph.
		/// Let's us scroll the page to the top.
		/// </summary>
		public string TitleID { get; set; }


		/// <summary>
		/// The OneNote owning paragraph object ID
		/// </summary>
		public string ObjectID { get; set; }


		/// <summary>
		/// Gets the ID of the notebook containing the tag
		/// </summary>
		public string NotebookID { get; set; }


		/// <summary>
		/// Gets the ID of the section containing the tag
		/// </summary>
		public string SectionID { get; set; }


		/// <summary>
		/// Gets the hierarchy path of the page
		/// </summary>
		public string HierarchyPath { get; set; }


		/// <summary>
		/// Gets the title of the page
		/// </summary>
		public string PageTitle { get; set; }


		/// <summary>
		/// The contextual snippet of text surrounding the hashtag
		/// </summary>
		public string Context { get; set; }


		/// <summary>
		/// The time this tag was first captured
		/// </summary>
		public string LastScan { get; set; }


		public override bool Equals(object obj)
		{
			if (obj is Hashtag other)
			{
				return other.Tag == Tag && other.ObjectID == ObjectID;
			}

			return false;
		}

		public override int GetHashCode() => base.GetHashCode();
	}


	/// <summary>
	/// A collection of Hashtags
	/// </summary>
	internal class Hashtags : List<Hashtag> { }
}
