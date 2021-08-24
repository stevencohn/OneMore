//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	/// <summary>
	/// Loads, save, and manages a cache of styles
	/// </summary>
	internal class StyleProvider : Loggable
	{

		private readonly List<StyleRecord> cache;
		private string name;
		private bool dark;


		/// <summary>
		/// Initialize a new style cache.
		/// </summary>
		/// <param name="load">
		/// If False then does not initialize the provider cache and presumes the instance
		/// will only be used to Save new style themes.
		/// </param>
		public StyleProvider(bool load = true)
		{
			if (!load)
			{
				return;
			}

			// first try appdata\Roaming\OneMore\Themes\CustomStyles.xml
			var root = Load(Path.Combine(
				PathFactory.GetAppDataPath(), Resx.ThemesFolder, Resx.CustomStylesFilename));

			if (root == null)
			{
				// second try appdata\Roaming\OneMore\CustomStyles.xml (backwards compatibility)
				root = Load(Path.Combine(
					PathFactory.GetAppDataPath(), Resx.CustomStylesFilename));
			}

			if (root == null)
			{
				// file not found so load default theme
				root = XElement.Parse(Resx.CustomStyles);
			}

			root.GetAttributeValue("name", out name, "Default");
			root.GetAttributeValue("dark", out dark, false);

			//logger.WriteLine($"loaded theme '{name}' (dark:{dark})");
			//logger.WriteLine(root.ToString(SaveOptions.None));

			cache = root.Elements(root.GetDefaultNamespace() + "Style").ToList()
				.ConvertAll(e => new StyleRecord(e));
		}


		private XElement Load(string path)
		{
			if (File.Exists(path))
			{
				try
				{
					var root = XElement.Load(path);
					if (root.Name.LocalName == "CustomStyles" ||
						root.Name.LocalName == "Theme")
					{
						if (root.Attribute("name") == null)
						{
							// themes provided by OneMore will have an internal name
							// but user-defined themes will not so infer from filename
							root.Add(
								new XAttribute("name", Path.GetFileNameWithoutExtension(path))
								);
						}

						return root;
					}

					logger.WriteLine($"specified file is not a style theme {path}");
					logger.WriteLine(root);
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error reading {path}", exc);
				}
			}
			//else
			//{
			//	logger.WriteLine($"file not found {path}");
			//}

			return null;
		}


		/// <summary>
		/// Gets the number of styles cached and managed.
		/// </summary>
		public int Count => cache.Count;


		/// <summary>
		/// Gets a Boolean value indicating if this theme is intended for a dark background
		/// </summary>
		public bool Dark => dark;


		/// <summary>
		/// Gets the name of the loaded style theme
		/// </summary>
		public string Name => name;


		/// <summary>
		/// Get the name of the style at the specified index.
		/// </summary>
		/// <param name="index">The ordered index of the desired style</param>
		/// <returns>A string specifying the name of the style.</returns>
		public string GetName(int index)
		{
			if ((index >=0) && (index < cache.Count))
			{
				return cache.ElementAt(index).Name;
			}

			return null;
		}


		/// <summary>
		/// Gets the style at the specified index as a Style.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public Style GetStyle(int index)
		{
			if ((index >= 0) && (index < cache.Count))
			{
				return new Style(cache.ElementAt(index));
			}

			return null;
		}


		/// <summary>
		/// Gets a list of all styles in the cache.
		/// </summary>
		/// <returns>A List of Style instances</returns>
		public List<Style> GetStyles()
		{
			return cache.ConvertAll(e => new Style(e));
		}


		/// <summary>
		/// Load the specified theme file as a collection of styles
		/// </summary>
		/// <param name="path">Path of the theme file to load</param>
		/// <returns>A List of Style items</returns>
		public List<Style> LoadTheme(string path)
		{
			if (File.Exists(path))
			{
				try
				{
					var root = Load(path);
					if (root != null)
					{
						var ns = root.GetDefaultNamespace();

						root.GetAttributeValue("name", out name, Path.GetFileNameWithoutExtension(path));
						root.GetAttributeValue("dark", out dark, false);

						return root.Elements(ns + "Style").ToList()
							.ConvertAll(e => new Style(new StyleRecord(e)));
					}
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error loading theme {path}", exc);
				}
			}

			return new List<Style>();
		}


		/// <summary>
		/// Add a new style to or update an existing style in the style collection;
		/// used to create new styles
		/// </summary>
		/// <param name="style">A new style to save</param>
		public void Save(Style style)
		{
			var target = cache.FirstOrDefault(e => e.Name.Equals(style.Name));
			if (target != null)
			{
				// remove existing so it can be replaced
				cache.Remove(target);
			}
			else
			{
				// calculate new index
				style.Index = cache.Max(s => s.Index) + 1;
			}

			cache.Add(new StyleRecord(style));

			Save(cache.OrderBy(e => e.Index));
		}


		/// <summary>
		/// Saves a collection of styles to the app data store
		/// </summary>
		/// <param name="styles">The list of styles</param>
		/// <param name="path">The target path including filename</param>
		public void Save(List<Style> styles, string path = null)
		{
			Save(styles.ConvertAll(e => new StyleRecord(e)).OrderBy(e => e.Index), path);
		}


		private void Save(IEnumerable<StyleRecord> styles, string path = null)
		{
			var upgrade = path == null;

			if (path == null)
			{
				path = Path.Combine(
					PathFactory.GetAppDataPath(), Resx.ThemesFolder, Resx.CustomStylesFilename);
			}

			PathFactory.EnsurePathExists(Path.GetDirectoryName(path));

			var shortName = Path.GetFileNameWithoutExtension(path);
			if (File.Exists(path))
			{
				var theme = Load(path);
				if (theme != null)
				{
					theme.GetAttributeValue("name", out shortName, shortName);
				}
			}

			var root = new XElement("Theme",
				new XAttribute("name", shortName)
				);

			foreach (var style in styles)
			{
				root.Add(style.ToXElement());
			}

			root.Save(path, SaveOptions.None);

			if (upgrade)
			{
				var old = Path.Combine(PathFactory.GetAppDataPath(), Resx.CustomStylesFilename);
				if (File.Exists(old))
				{
					try
					{
						File.Delete(old);
					}
					catch (Exception exc)
					{
						logger.WriteLine($"failed to delete old file {path}", exc);
					}
				}
			}
		}
	}
}
