//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using System;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class FavoritesProvider
	{
		private static readonly XNamespace ns = "http://schemas.microsoft.com/office/2009/07/customui";
		private static readonly string AddButtonId = "omAddFavoriteButton";
		private static readonly string ManageButtonId = "omManageFavoritesButton";
		public static readonly string KbdShortcutsId = "omKeyboardShortcutsButton";
		public static readonly string GotoFavoriteCmd = "GotoFavoriteCmd";

		private readonly string path;
		private readonly IRibbonUI ribbon;
		private readonly ILogger logger;


		public FavoritesProvider(IRibbonUI ribbon)
		{
			logger = Logger.Current;
			path = Path.Combine(PathHelper.GetAppDataPath(), Resx.FavoritesFilename);
			this.ribbon = ribbon;
		}


		public void AddFavorite(bool addSection = false)
		{
			XElement root;

			if (File.Exists(path))
			{
				root = UpgradeFavoritesMenu(XElement.Load(path, LoadOptions.None));
			}
			else
			{
				root = MakeMenuRoot();
			}

			using (var one = new OneNote())
			{
				var info = addSection ? one.GetSectionInfo() : one.GetPageInfo();

				var name = info.Name;
				if (name.Length > 50)
				{
					name = name.Substring(0, 50) + "...";
				}

				// similar to mongo ObjectId, a random-enough identifier for our needs
				var id = ((DateTimeOffset.Now.ToUnixTimeSeconds() << 32)
					+ new Random().Next()).ToString("x");

				var imageMso = addSection ? "GroupInsertLinks" : "FileLinksToFiles";

				root.Add(new XElement(ns + "button",
					new XAttribute("id", $"omFavoriteLink{id}"),
					new XAttribute("onAction", GotoFavoriteCmd),
					new XAttribute("imageMso", imageMso),
					new XAttribute("label", name),
					new XAttribute("tag", info.Link),
					new XAttribute("screentip", info.Path)
					));

				logger.WriteLine($"Saving favorite '{info.Path}' ({info.Link})");
			}

			SaveFavorites(root);
		}


		/// <summary>
		/// Load the raw favorites document; use for Settings
		/// </summary>
		/// <returns></returns>
		public XElement LoadFavoritesMenu()
		{
			XElement root = null;

			if (File.Exists(path))
			{
				root = UpgradeFavoritesMenu(XElement.Load(path, LoadOptions.None));
			}
			else
			{
				root = MakeMenuRoot();
			}

			var kbdshorts = new Settings.SettingsProvider()
				.GetCollection("FavoritesSheet")?
				.Get<bool>("kbdshorts") == true;

			var button = root.Elements(ns + "button")
				.FirstOrDefault(e => e.Attribute("id").Value == KbdShortcutsId);

			if (kbdshorts && button == null)
			{
				root.Add(MakeKeyboardShortcutsButton());
			}
			else if (!kbdshorts && button != null)
			{
				button.Remove();
			}

			return root;
		}


		public static XElement MakeMenuRoot()
		{
			var root = new XElement(ns + "menu",
				MakeAddButton(),
				MakeManageButton(),
				new XElement(ns + "menuSeparator",
					new XAttribute("id", "omFavoritesSeparator")
					)
				);

			return root;
		}


		private static XElement MakeAddButton()
		{
			return new XElement(ns + "button",
				new XAttribute("id", AddButtonId),
				new XAttribute("label", Resx.Favorites_addButton_Label),
				new XAttribute("imageMso", "AddToFavorites"),
				new XAttribute("onAction", "AddFavoritePageCmd")
				);
		}


		private static XElement MakeManageButton()
		{
			return new XElement(ns + "button",
				new XAttribute("id", ManageButtonId),
				new XAttribute("label", Resx.Favorites_manageButton_Label),
				new XAttribute("imageMso", "NameManager"),
				new XAttribute("onAction", "ManageFavoritesCmd")
				);
		}


		private static XElement MakeKeyboardShortcutsButton()
		{
			return new XElement(ns + "button",
				new XAttribute("id", KbdShortcutsId),
				new XAttribute("label", Resx.Favorite_OneNoteKeyboardShortcuts),
				new XAttribute("imageMso", "AdpPrimaryKey"),
				new XAttribute("onAction", "ShowKeyboardShortcutsCmd")
				);
		}


		// temporary upgrade routine...
		private static XElement UpgradeFavoritesMenu(XElement root)
		{
			root = RewriteNamespace(root, ns);

			// re-id old add favorite button
			var addButton = root.Elements(ns + "button")
				.FirstOrDefault(e => e.Attribute("id").Value == "omFavoriteAddButton"); // old id

			if (addButton != null)
			{
				// use new values
				addButton.Attribute("id").Value = AddButtonId;
				addButton.Attribute("onAction").Value = "AddFavoritePageCmd";
			}
			else
			{
				addButton = root.Elements(ns + "button")
					.FirstOrDefault(e => e.Attribute("id").Value == AddButtonId);
			}

			if (addButton == null)
			{
				addButton = MakeAddButton();
				root.AddFirst(addButton);
			}

			// manage buttons...

			var manButton = root.Elements(ns + "button")
				.FirstOrDefault(e => e.Attribute("id").Value == ManageButtonId);

			if (manButton == null)
			{
				addButton.AddAfterSelf(MakeManageButton());
			}

			// convert splitButton to button, removing the delete sub-menu
			var elements = root.Elements(ns + "splitButton").ToList();
			foreach (var element in elements)
			{
				var button = element.Elements(ns + "button").First();
				button.Attribute("onAction").Value = GotoFavoriteCmd;
				element.ReplaceWith(button);
			}

			return root;
		}


		private static XElement RewriteNamespace(XElement element, XNamespace ns)
		{
			RewriteChildNamespace(element, ns);

			// cannot change ns of root element directly so must rebuild it
			return new XElement(ns + element.Name.LocalName,
				element.Attributes().Where(a => a.Name != "xmlns"),
				element.Elements()
				);
		}

		private static void RewriteChildNamespace(XElement element, XNamespace ns)
		{
			foreach (var child in element.Elements())
			{
				RewriteChildNamespace(child, ns);
				var a = child.Attribute("xmlns");
				if (a == null)
				{
					// change XName of element when xmlns is implicit
					child.Name = ns + child.Name.LocalName;
				}
				else
				{
					// remove explicit xmlns attribute
					a.Remove();
				}
			}
		}



		public void SaveFavorites(XElement root)
		{
			try
			{
				PathHelper.EnsurePathExists(PathHelper.GetAppDataPath());
				root.Save(path, SaveOptions.None);

				if (ribbon != null)
				{
					ribbon.InvalidateControl("ribFavoritesMenu");
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"cannot save {path}");
				logger.WriteLine(exc);
			}
		}
	}
}
/*
<menu xmlns="http://schemas.microsoft.com/office/2006/01/customui">
  <button id="omAddFavoriteButton" label="Add current page" 
	imageMso="AddToFavorites" onAction="AddFavoritePageCmd" />
  <button id="omManageFavoriteButton" label="Manage Favorites" 
	imageMso="NameManager" onAction="ManageFavoritesCmd" />

  <!-- separator present only when there are favorites available -->
  <menuSeparator id="favotiteSeparator" />

  <!-- one or more favorites as split buttons -->
  <button id="favoriteLink1" imageMso="FileLinksToFiles" 
	label="Some fancy page" screentip="Notebook/Section/Some fancy page long name..." />
  <button id="favoriteLink2" imageMso="FileLinksToFiles" 
	label="Some fancy page2" screentip="Notebook/Section/Some fancy page long name2..." />
  ...

</menu>
*/
