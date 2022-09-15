//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Web.Script.Serialization;
	using System.Xml.Linq;


	internal class PluginsProvider : Loggable
	{
		private const string RunPluginButtonId = "ribPluginButton";
		private const string ManagePluginsButtonId = "ribManagePluginsButton";

		private const string DirectoryName = "Plugins";
		private const string Extension = ".js";

		private readonly string store;


		public PluginsProvider() : base()
		{
			store = Path.Combine(PathHelper.GetAppDataPath(), DirectoryName);
		}


		/// <summary>
		/// Delete the plugins with the specified path
		/// </summary>
		/// <param name="path"></param>
		public bool Delete(string path)
		{
			if (File.Exists(path))
			{
				try
				{
					File.Delete(path);
					return true;
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error deleting {path}", exc);
				}
			}

			return false;
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
		public async Task<Plugin> Load(string path)
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
					var json = await reader.ReadToEndAsync();

					var serializer = new JavaScriptSerializer();
					var plugin = serializer.Deserialize<Plugin>(json);

					plugin.OriginalName = plugin.Name;
					plugin.Path = path;

					return plugin;
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error load plugin from {path}", exc);
				return null;
			}
		}

		public async Task<Plugin> LoadByName(string name)
		{
			var path = Path.Combine(store, $"{name}{Extension}");
			return await Load(path);
		}


		public async Task<bool> Rename(Plugin plugin, string name)
		{
			if (File.Exists(plugin.Path))
			{
				var path = Path.Combine(Path.GetDirectoryName(plugin.Path), $"{name}{Extension}");
				name = name.Trim();

				try
				{
					logger.WriteLine($"renaming {plugin.Name} to {name}");
					File.Move(plugin.Path, path);
					plugin.Name = name;
					plugin.Path = path;

					await Save(plugin);
					return true;
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error renaming plugin {plugin.Path}", exc);
				}
			}

			return false;
		}


		/// <summary>
		/// Save or resave the given plugin using its name as the file name
		/// </summary>
		/// <param name="plugin"></param>
		/// <returns></returns>
		public async Task Save(Plugin plugin)
		{
			var name = plugin.Name.Trim();

			var path = Path.Combine(store, $"{name}{Extension}");
			logger.WriteLine($"saving {path}");

			try
			{
				PathHelper.EnsurePathExists(store);

				using (var writer = new StreamWriter(path, false))
				{
					var serializer = new JavaScriptSerializer();
					var json = serializer.Serialize(plugin);

					await writer.WriteAsync(json);
				}

				// overwrite original name
				plugin.OriginalName = name;
				plugin.Path = path;
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error saving plugin to {path}", exc);
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		public XElement MakePluginsMenu(XNamespace ns)
		{
			var plugins = GetPaths();
			if (!plugins.Any())
			{
				return null;
			}

			var menu = new XElement(ns + "menu",
				new XElement(ns + "button",
					new XAttribute("id", RunPluginButtonId),
					new XAttribute("getLabel", "GetRibbonLabel"),
					new XAttribute("imageMso", "ComAddInsDialog"),
					new XAttribute("onAction", "RunPluginCmd")
					),
				new XElement(ns + "button",
					new XAttribute("id", ManagePluginsButtonId),
					new XAttribute("getLabel", "GetRibbonLabel"),
					new XAttribute("imageMso", "GroupAddInsCustomToolbars"),
					new XAttribute("onAction", "ManagePluginsCmd")
					),
				new XElement(ns + "menuSeparator",
					new XAttribute("id", "ribPluginsMenuSep")
					)
				);

			var b = 0;
			foreach (var plugin in plugins)
			{
				menu.Add(new XElement(ns + "button",
					new XAttribute("id", $"ribMyPlugin{b++}"),
					new XAttribute("imageMso", "GroupAddInsToolbarCommands"),
					new XAttribute("label", Path.GetFileNameWithoutExtension(plugin)),
					new XAttribute("tag", plugin),
					new XAttribute("onAction", "RunPluginCmd")
					));
			}

			return menu;
		}
	}
}
