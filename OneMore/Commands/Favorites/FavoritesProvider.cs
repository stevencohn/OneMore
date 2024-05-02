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
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal sealed class FavoritesProvider : Loggable, IAsyncDisposable
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

		private OneNote one;
		private XElement books;
		private Dictionary<string, XElement> notebooks;
		private bool disposed;


		public FavoritesProvider(IRibbonUI ribbon)
		{
			path = Path.Combine(PathHelper.GetAppDataPath(), Resx.FavoritesFilename);
			this.ribbon = ribbon;
		}


		public async ValueTask DisposeAsync()
		{
			await DisposeAsyncCore().ConfigureAwait(false);
			// DO NOT call this otherwise OneNote will not shutdown properly
			//GC.SuppressFinalize(this);
		}


		async ValueTask DisposeAsyncCore()
		{
			if (!disposed && one is not null)
			{
				await one.DisposeAsync();
				disposed = true;
			}
		}


		public async Task AddFavorite(bool addSection = false)
		{
			XElement root = null;

			if (File.Exists(path))
			{
				try
				{
					root = UpgradeFavoritesMenu(XElement.Load(path, LoadOptions.None), false);
				}
				catch (Exception exc)
				{
					logger.WriteLine("could not load favorites.xml; trying to dump its contents...", exc);
					root = null;

					try
					{
						logger.WriteLine(File.ReadAllText(path));
					}
					catch (Exception e2)
					{
						logger.WriteLine("could not dump favorites.xml; possibly locked", e2);

					}
				}
			}

			root ??= MakeMenuRoot();

			one ??= new OneNote();

			var info = addSection ? await one.GetSectionInfo() : await one.GetPageInfo();
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


		public List<Favorite> LoadFavorites()
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
					Status = FavoriteStatus.Known,
					Root = element
				});
			}

			return list;
		}


		public List<Favorite> SortFavorites()
		{
			var list = new List<Favorite>();
			if (!File.Exists(path))
			{
				return list;
			}

			var root = XElement.Load(path, LoadOptions.None);

			var favorites = root.Elements()
				.Where(e => e.Attribute("notebookID") is not null)
				.ToList();

			favorites.Remove();
			root.Add(favorites.OrderBy(e => e.Attribute("label").Value));

			SaveFavorites(root);

			return LoadFavorites();
		}


		/// <summary>
		/// Confirm that each favorite exists and is reachable with the by ID. For those that
		/// are not reachable, confirm if the Location/Screentip/path is reachable and, if so,
		/// patch the favorite with an updated ID.
		/// </summary>
		/// <param name="favorites">List of favorites to validate</param>
		/// <returns></returns>
		public async Task ValidateFavorites(List<Favorite> favorites)
		{
			one ??= new OneNote();

			notebooks = new Dictionary<string, XElement>();
			var updated = false;

			foreach (var f in favorites)
			{
				if (!string.IsNullOrWhiteSpace(f.NotebookID) &&
					!string.IsNullOrWhiteSpace(f.ObjectID))
				{
					await ConfirmByID(f);
				}

				// ConfirmByID would only return either Known or Unknown
				if (f.Status == FavoriteStatus.Unknown)
				{
					// infer from location and update ID
					updated = await ConfirmByLocation(f) || updated;
				}
			}

			if (updated)
			{
				logger.WriteLine("saving updated favorites!");
				var root = FavoritesProvider.MakeMenuRoot();
				foreach (var favorite in favorites)
				{
					root.Add(favorite.Root);
				}

				await using var provider = new FavoritesProvider(ribbon);
				provider.SaveFavorites(root);
			}
			else
			{
				logger.WriteLine("favorites check OK");
			}
		}


		private async Task ConfirmByID(Favorite favorite)
		{
			XElement notebook = null;
			if (notebooks.ContainsKey(favorite.NotebookID))
			{
				notebook = notebooks[favorite.NotebookID];
			}
			else
			{
				try
				{
					notebook = await one.GetNotebook(favorite.NotebookID, OneNote.Scope.Pages);
					if (notebook is not null)
					{
						notebooks.Add(favorite.NotebookID, notebook);
					}
				}
				catch (COMException exc) when ((uint)exc.ErrorCode == ErrorCodes.hrObjectMissing)
				{
					logger.WriteLine($"broken link to favorite notebook {favorite.Location}");
				}
				catch (Exception exc)
				{
					logger.WriteLine($"could not fetch favorite notebook {favorite.Location}", exc);
				}
			}

			if (notebook is not null &&
				notebook.Descendants().Any(e => e.Attribute("ID")?.Value == favorite.ObjectID))
			{
				favorite.Status = FavoriteStatus.Known;
			}
			else
			{
				logger.WriteLine($"broken link to favorite notebook {favorite.Location}");
				favorite.Status = FavoriteStatus.Unknown;
			}
		}


		private async Task<bool> ConfirmByLocation(Favorite favorite)
		{
			books ??= await one.GetNotebooks();

			var parts = favorite.Location.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length < 3)
			{
				// corrupt screentip attribute in XML? (populates Location)
				// must be at least: /notebook/section/title
				logger.WriteLine($"invalid favorite location {favorite.Location} [{favorite.Name}]");
				favorite.Status = FavoriteStatus.Suspect;
				return false;
			}

			var notebook = notebooks.Values.FirstOrDefault(n => n.Attribute("name")?.Value == parts[0]);
			if (notebook is null)
			{
				// first part should be name of notebook
				var nx = books.GetNamespaceOfPrefix(OneNote.Prefix);
				var book = books.Elements(nx + "Notebook")
					.FirstOrDefault(n => n.Attribute("name")?.Value == parts[0]);

				if (book is null)
				{
					logger.WriteLine($"broken link to favorite notebook {favorite.Location}");
					favorite.Status = FavoriteStatus.Suspect;
					return false;
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
				node = node.Elements().FirstOrDefault(n => n.Attribute("name")?.Value == parts[i]);
				if (node is null)
				{
					logger.WriteLine($"broken link to favorite parts[{i}] {favorite.Location}");
					favorite.Status = FavoriteStatus.Suspect;
					return false;
				}
			}

			favorite.Status = FavoriteStatus.Known;

			var notebookID = notebook.Attribute("ID").Value;
			var nodeID = node.Attribute("ID").Value;

			if (favorite.NotebookID != notebookID ||
				favorite.ObjectID != nodeID)
			{
				logger.WriteLine($"patching favorite IDs {favorite.Location}");

				favorite.NotebookID = notebookID;
				favorite.ObjectID = nodeID;

				favorite.Root.SetAttributeValue("notebookID", notebookID);
				favorite.Root.SetAttributeValue("objectID", nodeID);
				favorite.Root.SetAttributeValue("tag", one.GetHyperlink(nodeID, string.Empty));

				logger.WriteLine(favorite.Root);

				return true;
			}

			return false;
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
