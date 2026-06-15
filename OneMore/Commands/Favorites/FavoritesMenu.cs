//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal static class FavoritesMenu
	{
		public static readonly string MenuID = "ribFavoritesMenu";

		public static readonly string AddAction = "AddFavoritePageCmd";
		public static readonly string GotoAction = "GotoFavoriteCmd";

		private static readonly XNamespace ns = "http://schemas.microsoft.com/office/2009/07/customui";
		private static readonly string AddButtonId = "omAddFavoriteButton";
		private static readonly string ManageButtonId = "omManageFavoritesButton";
		private static readonly string KbdShortcutsId = "omShowKeyboardShortcutsButton";


		/// <summary>
		/// Load the raw favorites document; use for Settings
		/// </summary>
		/// <returns></returns>
		public static XElement LoadMenu()
		{
			var root = MakeMenuRoot();

			using var provider = new FavoritesProvider();
			var collection = provider.ReadFavorites();

			foreach (var folder in collection.Folders)
			{
				var menu = new XElement(ns + "menu",
					new XAttribute("id", $"omFavoritesFolder{folder.FolderID}"),
					new XAttribute("label", Chop(folder.Name))
					);

				foreach (var favorite in folder.Items)
				{
					menu.Add(MakeButton(favorite));
				}

				root.Add(menu);
			}

			foreach (var favorite in collection.Items)
			{
				root.Add(MakeButton(favorite));
			}

			AddKeyboardShortcutsButton(root);

			return root;
		}


		private static XElement MakeMenuRoot()
		{
			var addButton = new XElement(ns + "button",
				new XAttribute("id", AddButtonId),
				new XAttribute("label", Resx.ribAddFavoritePageButton_Label),
				new XAttribute("screentip", Resx.ribAddFavoritePageButton_Screentip),
				new XAttribute("imageMso", "AddToFavorites"),
				new XAttribute("onAction", AddAction)
				);

			var manageButton = new XElement(ns + "button",
				new XAttribute("id", ManageButtonId),
				new XAttribute("label", Resx.ribManageFavoritesButton_Label),
				new XAttribute("imageMso", "NameManager"),
				new XAttribute("onAction", "ManageFavoritesCmd")
				);

			var root = new XElement(ns + "menu",
				addButton,
				manageButton,
				new XElement(ns + "menuSeparator",
					new XAttribute("id", "omFavoritesSeparator")
					)
				);

			return root;
		}


		private static XElement MakeButton(Favorite favorite)
		{
			var imageMso = string.IsNullOrWhiteSpace(favorite.PageID)
				? "FileLinksToFiles"
				: "GroupInsertLinks";

			return new XElement(ns + "button",
				new XAttribute("id", $"omFavorite{favorite.ID}"),
				new XAttribute("onAction", GotoAction),
				new XAttribute("imageMso", imageMso),
				new XAttribute("label", Chop(favorite.Alias)),
				new XAttribute("tag", favorite.Uri),
				new XAttribute("screentip", favorite.Location)
				);
		}


		private static void AddKeyboardShortcutsButton(XElement root)
		{
			var addButton = new Settings.SettingsProvider()
				.GetCollection(nameof(Settings.FavoritesSheet))?
				.Get<bool>("kbdshorts") == true;

			if (addButton)
			{
				root.Add(new XElement(ns + "button",
					new XAttribute("id", KbdShortcutsId),
					new XAttribute("label", Resx.ribShowKeyboardShortcutsButton_Label),
					new XAttribute("imageMso", "AdpPrimaryKey"),
					new XAttribute("onAction", "ShowKeyboardShortcutsCmd")
					));
			}
		}


		private static string Chop(string s)
		{
			return s.Length <= 45 ? s : s.Substring(0, 42) + "...";
		}
	}
}
