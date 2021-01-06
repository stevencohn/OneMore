﻿//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class PluginsProvider : Loggable
	{
		private const string SavePluginButtonId = "ribSavePluginButton";
		private const string ManagePluginsButtonId = "ribManagePluginsButton";

		private const string DirectoryName = "Plugins";
		private const string Extension = ".plu";

		private readonly string store;


		public PluginsProvider() : base()
		{
			store = Path.Combine(PathFactory.GetAppDataPath(), DirectoryName);
		}


		/// <summary>
		/// Delete the plugins with the specified path
		/// </summary>
		/// <param name="path"></param>
		public void Delete(string path)
		{
			if (File.Exists(path))
			{
				try
				{
					File.Delete(path);
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error deleting {path}", exc);
				}
			}
		}


		/// <summary>
		/// Gets a list of plugin names for comparison when naming and saving a new plugin
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetNames()
		{
			if (!Directory.Exists(store))
			{
				yield break;
			}

			foreach (var file in Directory.GetFiles(store, $"*{Extension}"))
			{
				yield return Path.GetFileNameWithoutExtension(file);
			}
		}


		/// <summary>
		/// Gets a list of the full file paths of the custom plugins
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetPaths()
		{
			if (!Directory.Exists(store))
			{
				yield break;
			}

			foreach (var file in Directory.GetFiles(store, $"*{Extension}"))
			{
				yield return file;
			}
		}


		/// <summary>
		/// Loads the contents of a plugin
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public async Task<string> Load(string path)
		{
			if (!File.Exists(path))
			{
				return null;
			}

			try
			{
				using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
				using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
				{
					return await reader.ReadToEndAsync();
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error load plugin from {path}", exc);
				return null;
			}
		}


		/// <summary>
		/// Saves a new named splugin
		/// </summary>
		/// <param name="plugin"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public async Task Save(string plugin, string name)
		{
			var path = Path.Combine(store, $"{name}{Extension}");

			try
			{
				PathFactory.EnsurePathExists(store);

				using (var writer = new StreamWriter(path))
				{
					await writer.WriteAsync(plugin);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error saving plugin to {path}", exc);
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		public XElement MakePluginsMenu(XNamespace ns)
		{
			var menu = new XElement(ns + "menu",
				new XAttribute("id", "ribPluginsMenu"),
				new XAttribute("getLabel", "GetRibbonLabel"),
				new XAttribute("imageMso", "GroupInsertShapes"),
				new XElement(ns + "button",
					new XAttribute("id", SavePluginButtonId),
					new XAttribute("getLabel", "GetRibbonLabel"),
					new XAttribute("imageMso", "SaveSelectionToQuickPartGallery"),
					new XAttribute("onAction", "SavePluginCmd")
					),
				new XElement(ns + "button",
					new XAttribute("id", ManagePluginsButtonId),
					new XAttribute("getLabel", "GetRibbonLabel"),
					new XAttribute("imageMso", "BibliographyManageSources"),
					new XAttribute("onAction", "ManagePluginsCmd")
					),
				new XElement(ns + "menuSeparator",
					new XAttribute("id", "ribPluginsMenuSep")
					)
				);

			var plugins = GetPaths();
			if (plugins.Any())
			{
				var b = 0;
				foreach (var plugin in plugins)
				{
					menu.Add(new XElement(ns + "button",
						new XAttribute("id", $"ribMyPlugin{b++}"),
						new XAttribute("imageMso", "PasteAlternative"),
						new XAttribute("label", Path.GetFileNameWithoutExtension(plugin)),
						new XAttribute("tag", plugin),
						new XAttribute("onAction", "RunPluginCmd")
						));
				}
			}

			return menu;
		}
	}
}
