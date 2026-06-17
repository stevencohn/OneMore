//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using System.Collections.Generic;


	/// <summary>
	/// The ephemeral status of a Favorite after verification
	/// </summary>
	internal enum FavoriteStatus
	{
		/// <summary>
		/// Found by notebookID + objectID, high confidence that this is the same favorite
		/// as originally bookmarked
		/// </summary>
		Known,

		/// <summary>
		/// Not found by notebookID + objectID but found by path, screentip, or other 
		/// heuristic, low/medium confidence
		/// </summary>
		Suspect,

		/// <summary>
		/// Not found by notebookID + objectID or by any heuristic, zero confidence but at least
		/// has notebookID + objectID
		/// </summary>
		Unknown
	}


	/// <summary>
	/// A single Favorite
	/// </summary>
	internal sealed class Favorite
	{
		/// <summary>
		/// Database ID of this favorite, used for updates and deletes.
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// Database ID of the folder that contains this favorite, or 0 if this favorite is not 
		/// in a folder - it's in the root
		/// </summary>
		public int FolderID { get; set; }

		/// <summary>
		/// The name of the target, either a section, section group, or page name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// User supplied friendly name or alias of the favorite, or null if the user has
		/// not overridden the name. Callers that display this value should fall back to
		/// Name, e.g. "favorite.Alias ?? favorite.Name".
		/// </summary>
		public string Alias { get; set; }

		/// <summary>
		/// The hierarchy path of the favorite including notebook, section groups and sections.
		/// </summary>
		public string Location { get; set; }

		/// <summary>
		/// The onenote:// URL of the favorite, which may be used as a hyperlink or for 
		/// verification. This is the raw URL as stored in the database and may not be valid 
		/// if the favorite is no longer reachable at that URL.
		/// </summary>
		public string Uri { get; set; }

		/// <summary>
		/// The ID of the notebook that contains the target.
		/// </summary>
		public string NotebookID { get; set; }

		/// <summary>
		/// The ID of the section or section group that contains the target or the target itself.
		/// </summary>
		public string SectionID { get; set; }

		/// <summary>
		/// The ID of the page that is the target of the favorite.
		/// </summary>
		public string PageID { get; set; }

		/// <summary>
		/// The custom sort order of the favorite. Default sort order is alphabetic by alias.
		/// </summary>
		public int SortOrder { get; set; }

		// runtime only properties

		/// <summary>
		/// The Verified status of the favorite.
		/// </summary>
		public FavoriteStatus Status { get; set; }
	}


	internal interface IFavoritesFolder
	{
		List<Favorite> Items { get; }
	}


	/// <summary>
	/// A single folder of favorites representing a second level of organization for favorites.
	/// </summary>
	internal sealed class FavoritesFolder : IFavoritesFolder
	{
		public FavoritesFolder()
		{
			Items = new();
		}

		/// <summary>
		/// The folder ID, auto-assigned by the DB
		/// </summary>
		public int FolderID { get; set; }

		/// <summary>
		/// The user-supplied name of the folder.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The list of favorites within this folder.
		/// </summary>
		public List<Favorite> Items { get; set; }
	}


	/// <summary>
	/// The Favorites ribbon drop-down menu with second level folders and top-level favorites.
	/// </summary>
	internal sealed class FavoritesCollection : IFavoritesFolder
	{
		public FavoritesCollection()
		{
			Folders = new();
			Items = new();
		}

		/// <summary>
		/// The folder on the Favorites menu.
		/// </summary>
		public List<FavoritesFolder> Folders { get; set; }

		/// <summary>
		/// Uncategorized favorites (in the implied "root" folder) on the Favorites menu.
		/// </summary>
		public List<Favorite> Items { get; set; }
	}
}
