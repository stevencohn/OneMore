//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
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
		/// Gets the unique rowID returned by the DB
		/// </summary>
		public int RowID { get; set; }


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
		/// The OneNote owning paragraph object ID
		/// </summary>
		public string ObjectID { get; set; }


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
