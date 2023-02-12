//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal sealed class FavoritesProvider : IDisposable
	{
		public enum FavoriteStatus
		{
			Known,      // found by notebookID+objectID, high confidence
			Suspect,    // not found by path, renamed? low confidence
			Unknown     // has notebookID+objectID but not found, zero confidence
		}


		public sealed class Favorite
		{
			// sequence number
			public int Index { get; set; }
			// short name (page or section)
			public string Name { get; set; }
			// named hierarchy path (notebook/section/page)
			public string Location { get; set; }
			// onenote:// URL of favorite
			public string Uri { get; set; }
			// hierarchy notebook ID
			public string NotebookID { get; set; }
			// hierarchy object ID (page or section)
			public string ObjectID { get; set; }
			// known status
			public FavoriteStatus Status { get; set; }
			// button XML root, used by settings sheet
			public XElement Root { get; set; }
		}


		private static readonly XNamespace ns = "http://schemas.microsoft.com/office/2009/07/customui";
		private static readonly string AddButtonId = "omAddFavoriteButton";
		private static readonly string ManageButtonId = "omManageFavoritesButton";
		public static readonly string KbdShortcutsId = "omShowKeyboardShortcutsButton";
		public static readonly string AddFavoritePageCmd = "AddFavoritePageCmd";
		public static readonly string GotoFavoriteCmd = "GotoFavoriteCmd";

		private readonly string path;
		private readonly IRibbonUI ribbon;
		private readonly ILogger logger;

		private OneNote one;
		private XElement books;
		private Dictionary<string, XElement> notebooks;
		private bool disposed;


		public FavoritesProvider(IRibbonUI ribbon)
		{
			logger = Logger.Current;
			path = Path.Combine(PathHelper.GetAppDataPath(), Resx.FavoritesFilename);
			this.ribbon = ribbon;
		}


		public void Dispose()
		{
			if (!disposed)
			{
				one?.Dispose();
				disposed = true;
			}
		}



		public void AddFavorite(bool addSection = false)
		{
			XElement root;

			if (File.Exists(path))
			{
				root = UpgradeFavoritesMenu(XElement.Load(path, LoadOptions.None), true);
			}
			else
			{
				root = MakeMenuRoot();
			}

			one ??= new OneNote();

			var info = addSection ? one.GetSectionInfo() : one.GetPageInfo();
			var notebookID = one.CurrentNotebookId;

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
				new XAttribute("notebookID", notebookID),
				new XAttribute("objectID", addSection ? info.SectionId : info.PageId),
				new XAttribute("onAction", GotoFavoriteCmd),
				new XAttribute("imageMso", imageMso),
				new XAttribute("label", name),
				new XAttribute("tag", info.Link),
				new XAttribute("screentip", info.Path)
				));

			logger.WriteLine($"Saving favorite '{info.Path}' ({info.Link})");

			SaveFavorites(root);
		}


		public async Task<List<Favorite>> LoadFavorites()
		{
			var list = new List<Favorite>();
			var root = LoadFavoritesMenu(false);
			var nx = root.Name.Namespace;

			// filter out the add/manage/shortcuts buttons
			var elements = root.Elements(nx + "button")
				.Where(e => e.Attribute("onAction")?.Value == GotoFavoriteCmd);

			int index = 0;
			foreach (var element in elements)
			{
				list.Add(new Favorite
				{
					Index = index++,
					Name = element.Attribute("label").Value,
					Location = element.Attribute("screentip").Value,
					Uri = element.Attribute("tag").Value,
					NotebookID = element.Attribute("notebookID")?.Value,
					ObjectID = element.Attribute("objectID")?.Value,
					Root = element
				});
			}

			await ValidateFavorites(list);

			return list;
		}


		private async Task ValidateFavorites(List<Favorite> favorites)
		{
			one ??= new OneNote();

			notebooks = new Dictionary<string, XElement>();

			foreach (var f in favorites)
			{
				f.Status = string.IsNullOrWhiteSpace(f.NotebookID) || string.IsNullOrWhiteSpace(f.ObjectID)
					? await ConfirmByName(f.Location)
					: await ConfirmByID(f.NotebookID, f.ObjectID);
			}
		}


		private async Task<FavoriteStatus> ConfirmByName(string path)
		{
			books ??= await one.GetNotebooks();

			var parts = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			var notebook = notebooks.Values.FirstOrDefault(n => n.Name == parts[0]);
			if (notebook == null)
			{
				var nx = books.GetNamespaceOfPrefix(OneNote.Prefix);
				var book = books.Elements(nx + "Notebook")
					.FirstOrDefault(n => n.Attribute("name")?.Value == parts[0]);

				if (book == null)
				{
					return FavoriteStatus.Suspect;
				}

				var id = book.Attribute("ID").Value;

				if (notebooks.ContainsKey(id))
				{
					notebook = notebooks[id];
				}
				else
				{
					notebook = await one.GetNotebook(id, OneNote.Scope.Pages);
					notebooks.Add(id, notebook);
				}
			}

			var node = notebook;
			for (int i = 1; i < parts.Length; i++)
			{
				node = node.Elements().FirstOrDefault(n => n.Attribute("name").Value == parts[i]);
				if (node == null)
				{
					return FavoriteStatus.Suspect;
				}
			}

			return FavoriteStatus.Known;
		}


		private async Task<FavoriteStatus> ConfirmByID(string notebookID, string objectID)
		{
			XElement notebook;
			if (notebooks.ContainsKey(notebookID))
			{
				notebook = notebooks[notebookID];
			}
			else
			{
				notebook = await one.GetNotebook(notebookID, OneNote.Scope.Pages);
				notebooks.Add(notebookID, notebook);
			}

			return notebook.Descendants().Any(e => e.Attribute("ID")?.Value == objectID)
				? FavoriteStatus.Known
				: FavoriteStatus.Unknown;
		}


		/// <summary>
		/// Load the raw favorites document; use for Settings
		/// </summary>
		/// <returns></returns>
		public XElement LoadFavoritesMenu(bool cleanup = true)
		{
			XElement root = null;

			if (File.Exists(path))
			{
				root = UpgradeFavoritesMenu(XElement.Load(path, LoadOptions.None), cleanup);
			}
			else
			{
				root = MakeMenuRoot();
			}

			var kbdshorts = new Settings.SettingsProvider()
				.GetCollection(nameof(Settings.FavoritesSheet))?
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
				new XAttribute("label", Resx.ribAddFavoritePageButton_Label),
				new XAttribute("screentip", Resx.ribAddFavoritePageButton_Screentip),
				new XAttribute("imageMso", "AddToFavorites"),
				new XAttribute("onAction", "AddFavoritePageCmd")
				);
		}


		private static XElement MakeManageButton()
		{
			return new XElement(ns + "button",
				new XAttribute("id", ManageButtonId),
				new XAttribute("label", Resx.ribManageFavoritesButton_Label),
				new XAttribute("imageMso", "NameManager"),
				new XAttribute("onAction", "ManageFavoritesCmd")
				);
		}


		private static XElement MakeKeyboardShortcutsButton()
		{
			return new XElement(ns + "button",
				new XAttribute("id", KbdShortcutsId),
				new XAttribute("label", Resx.ribShowKeyboardShortcutsButton_Label),
				new XAttribute("imageMso", "AdpPrimaryKey"),
				new XAttribute("onAction", "ShowKeyboardShortcutsCmd")
				);
		}


		// temporary upgrade routine...
		private static XElement UpgradeFavoritesMenu(XElement root, bool cleanup)
		{
			root = RewriteNamespace(root, ns);

			// re-id old add favorite button
			var addButton = root.Elements(ns + "button")
				.FirstOrDefault(e => e.Attribute("id").Value == "omFavoriteAddButton"); // old id

			if (addButton != null)
			{
				// use new values
				addButton.Attribute("id").Value = AddButtonId;
				addButton.Attribute("onAction").Value = AddFavoritePageCmd;
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

			if (cleanup)
			{
				// THIS IS NEW! remove the notebookID and objectID attributes because they are not
				// valid attributes of the button element; we use them internally but not for the menu
				root.Elements(ns + "button").Attributes()
					.Where(a => a.Name == "notebookID" || a.Name == "objectID")
					.Remove();
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

				ribbon?.InvalidateControl("ribFavoritesMenu");
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
