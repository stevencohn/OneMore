//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Favorites
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands.Favorites;
	using System.Linq;
	using System.Xml.Linq;


	[TestClass]
	public class FavoritesMenuTests
	{
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/2009/07/customui";


		[TestMethod]
		public void BuildMenu_EmptyCollection_OnlyHasAddAndManageControls()
		{
			var root = FavoritesMenu.BuildMenu(new FavoritesCollection(), showShortcuts: false);

			// add button, manage button, separator - nothing else
			Assert.AreEqual(3, root.Elements().Count());
		}


		[TestMethod]
		public void BuildMenu_RootFavoriteWithPage_UsesPageIcon()
		{
			var collection = new FavoritesCollection();
			collection.Items.Add(new Favorite
			{
				ID = 1, Name = "Page1", Alias = "Page1",
				Uri = "onenote:#p1", Location = "loc1", PageID = "page-1"
			});

			var root = FavoritesMenu.BuildMenu(collection, showShortcuts: false);

			var button = root.Elements(Ns + "button")
				.First(b => (string)b.Attribute("id") == "omFavorite1");

			Assert.AreEqual("GroupInsertLinks", (string)button.Attribute("imageMso"));
		}


		[TestMethod]
		public void BuildMenu_RootFavoriteWithoutPage_UsesSectionIcon()
		{
			var collection = new FavoritesCollection();
			collection.Items.Add(new Favorite
			{
				ID = 2, Name = "Section1", Alias = "Section1",
				Uri = "onenote:#s1", Location = "loc2", PageID = null
			});

			var root = FavoritesMenu.BuildMenu(collection, showShortcuts: false);

			var button = root.Elements(Ns + "button")
				.First(b => (string)b.Attribute("id") == "omFavorite2");

			Assert.AreEqual("FileLinksToFiles", (string)button.Attribute("imageMso"));
		}


		[TestMethod]
		public void BuildMenu_Folder_NestsItsFavoritesInASubmenu()
		{
			var folder = new FavoritesFolder { FolderID = 7, Name = "MyFolder" };
			folder.Items.Add(new Favorite
			{
				ID = 10, Name = "X", Alias = "X", Uri = "onenote:#x", Location = "loc", PageID = "p"
			});

			var collection = new FavoritesCollection();
			collection.Folders.Add(folder);

			var root = FavoritesMenu.BuildMenu(collection, showShortcuts: false);

			var menu = root.Elements(Ns + "menu")
				.FirstOrDefault(m => (string)m.Attribute("id") == "omFavoritesFolder7");

			Assert.IsNotNull(menu, "expected a submenu for the folder");
			Assert.AreEqual("Folder", (string)menu.Attribute("imageMso"));
			Assert.AreEqual("MyFolder", (string)menu.Attribute("label"));
			Assert.AreEqual(1, menu.Elements(Ns + "button").Count());
		}


		[TestMethod]
		public void BuildMenu_AliasAtBoundary_IsNotTruncated()
		{
			var alias = new string('B', 45);
			var collection = new FavoritesCollection();
			collection.Items.Add(new Favorite
			{
				ID = 1, Name = "X", Alias = alias, Uri = "onenote:#x", Location = "loc"
			});

			var root = FavoritesMenu.BuildMenu(collection, showShortcuts: false);

			var button = root.Elements(Ns + "button")
				.First(b => (string)b.Attribute("id") == "omFavorite1");

			Assert.AreEqual(alias, (string)button.Attribute("label"));
		}


		[TestMethod]
		public void BuildMenu_AliasOverBoundary_IsTruncatedWithEllipsis()
		{
			var alias = new string('A', 50);
			var collection = new FavoritesCollection();
			collection.Items.Add(new Favorite
			{
				ID = 1, Name = "X", Alias = alias, Uri = "onenote:#x", Location = "loc"
			});

			var root = FavoritesMenu.BuildMenu(collection, showShortcuts: false);

			var button = root.Elements(Ns + "button")
				.First(b => (string)b.Attribute("id") == "omFavorite1");

			Assert.AreEqual(alias.Substring(0, 42) + "...", (string)button.Attribute("label"));
		}


		[TestMethod]
		public void BuildMenu_ShowShortcutsTrue_AddsKeyboardShortcutsButton()
		{
			var root = FavoritesMenu.BuildMenu(new FavoritesCollection(), showShortcuts: true);

			var button = root.Elements(Ns + "button")
				.FirstOrDefault(b => (string)b.Attribute("onAction") == "ShowKeyMapsPageCmd");

			Assert.IsNotNull(button);
		}


		[TestMethod]
		public void BuildMenu_ShowShortcutsFalse_OmitsKeyboardShortcutsButton()
		{
			var root = FavoritesMenu.BuildMenu(new FavoritesCollection(), showShortcuts: false);

			var button = root.Elements(Ns + "button")
				.FirstOrDefault(b => (string)b.Attribute("onAction") == "ShowKeyboardShortcutsCmd");

			Assert.IsNull(button);
		}
	}
}
