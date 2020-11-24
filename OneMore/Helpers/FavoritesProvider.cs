//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using River.OneMoreAddIn.Dialogs;
	using System;
	using System.IO;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class FavoritesProvider
	{
		private static readonly XNamespace ns = "http://schemas.microsoft.com/office/2006/01/customui";

		private readonly string path;
		private readonly IRibbonUI ribbon;
		private readonly ILogger logger;


		/*
		<menu xmlns="http://schemas.microsoft.com/office/2006/01/customui">
		  <button id="favoriteAddButton" label="Add current page" 
			imageMso="AddToFavorites" onAction="AddFavoritePage" />

		  <!-- separator present only when there are favorites available -->
		  <menuSeparator id="favotiteSeparator" />

		  <!-- one or more favorites as split buttons -->
		  <splitButton id="favorite1">
			<button id="favoriteLink1" imageMso="FileLinksToFiles" 
			  label="Some fancy page" screentip="Notebook/Section/Some fancy page long name..." />
			<menu id="favoriteMenu1" label="Some fancy menu" >
			  <button id="favoriteRemove1" label="Remove this link" imageMso="HyperlinkRemove" />
			</menu>
		  </splitButton>

		  ...

		</menu>
		*/


		public FavoritesProvider(IRibbonUI ribbon)
		{
			logger = Logger.Current;
			path = Path.Combine(PathFactory.GetAppDataPath(), Resx.FavoritesFilename);
			this.ribbon = ribbon;
		}


		public string GetMenuContent()
		{
			if (File.Exists(path))
			{
				var content = XElement.Load(path, LoadOptions.None);
				return content.ToString(SaveOptions.DisableFormatting);
			}

			return MakeMenuRoot().ToString(SaveOptions.DisableFormatting);
		}


		public void AddFavorite()
		{
			XElement root;

			if (File.Exists(path))
			{
				root = XElement.Load(path, LoadOptions.None);
			}
			else
			{
				root = MakeMenuRoot();
			}

			using (var one = new OneNote())
			{
				var info = one.GetPageInfo();

				var name = EmojiDialog.RemoveEmojis(info.Name);
				if (name.Length > 50)
				{
					name = name.Substring(0, 50) + "...";
				}

				// similar to mongo ObjectId, a random-enough identifier for our needs
				var id = ((DateTimeOffset.Now.ToUnixTimeSeconds() << 32)
					+ new Random().Next()).ToString("x");

				root.Add(new XElement(ns + "splitButton",
					new XAttribute("id", $"omFavorite{id}"),
					new XElement(ns + "button",
						new XAttribute("id", $"omFavoriteLink{id}"),
						new XAttribute("onAction", "NavigateToFavorite"),
						new XAttribute("imageMso", "FileLinksToFiles"),
						new XAttribute("label", name),
						new XAttribute("tag", info.Link),
						new XAttribute("screentip", info.Path)
						),
					new XElement(ns + "menu",
						new XAttribute("id", $"omFavoriteMenu{id}"),
						new XElement(ns + "button",
							new XAttribute("id", $"omFavoriteRemoveButton{id}"),
							new XAttribute("onAction", "RemoveFavorite"),
							new XAttribute("label", "Remove this item"),
							new XAttribute("imageMso", "HyperlinkRemove"),
							new XAttribute("tag", $"omFavorite{id}")
							)
						)
					));

				// sort by name/label
				var items =
					from e in root.Elements(ns + "splitButton")
					let key = e.Element(ns + "button").Attribute("label").Value
					orderby key
					select e;

				root = MakeMenuRoot();
				foreach (var item in items)
				{
					root.Add(item);
				}

				logger.WriteLine($"Saving favorite '{info.Path}' ({info.Link})");
			}

			try
			{
				PathFactory.EnsurePathExists(PathFactory.GetAppDataPath());
				root.Save(path, SaveOptions.None);

				ribbon.InvalidateControl("ribFavoritesMenu");
			}
			catch (Exception exc)
			{
				logger.WriteLine($"cannot save {path}");
				logger.WriteLine(exc);
			}
		}


		private static XElement MakeMenuRoot()
		{
			var root = new XElement(ns + "menu",
				new XElement(ns + "button",
					new XAttribute("id", "omFavoriteAddButton"),
					new XAttribute("label", "Add current page"),
					new XAttribute("imageMso", "AddToFavorites"),
					new XAttribute("onAction", "AddFavoritePage")
					),
				new XElement(ns + "menuSeparator",
					new XAttribute("id", "omFavoritesSeparator")
					)
				);

			return root;
		}


		public void RemoveFavorite(string favoriteId)
		{
			if (File.Exists(path))
			{
				var root = XElement.Load(path, LoadOptions.None);

				var element =
					(from e in root.Elements(ns + "splitButton")
					 where e.Attribute("id").Value == favoriteId
					 select e).FirstOrDefault();

				if (element != null)
				{
					var label = element.Element(ns + "button")?.Attribute("label").Value;

					var result = MessageBox.Show(
						$"Remove {label}?",
						"Confirm",
						MessageBoxButtons.YesNo, MessageBoxIcon.Question,
						MessageBoxDefaultButton.Button2,
						MessageBoxOptions.DefaultDesktopOnly
						);

					if (result == DialogResult.Yes)
					{
						element.Remove();

						try
						{
							PathFactory.EnsurePathExists(PathFactory.GetAppDataPath());
							root.Save(path, SaveOptions.None);

							ribbon.InvalidateControl("ribFavoritesMenu");
						}
						catch (Exception exc)
						{
							logger.WriteLine($"cannot save {path}");
							logger.WriteLine(exc);
						}
					}
				}
			}
		}
	}
}
