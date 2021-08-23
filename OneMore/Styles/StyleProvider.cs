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


	/// <summary>
	/// Loads, save, and manages a cache of styles
	/// </summary>
	internal class StyleProvider
	{

		private readonly List<StyleRecord> cache;
		private string name;
		private bool dark;


		/// <summary>
		/// Initialize a new style cache.
		/// </summary>
		public StyleProvider()
		{
			cache = new List<StyleRecord>();

			var path = Path.Combine(
				PathFactory.GetAppDataPath(), Properties.Resources.CustomStylesFilename);

			XElement root = null;
			if (File.Exists(path))
			{
				try
				{
					root = XElement.Load(path);
					root.GetAttributeValue("name", out name, Path.GetFileNameWithoutExtension(path));
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"error reading {path}", exc);
				}
			}

			if (root == null)
			{
				// file not found so load default theme
				root = XElement.Parse(Properties.Resources.CustomStyles);
				root.GetAttributeValue("name", out name, "Default");
			}

			root.GetAttributeValue("dark", out dark, false);

			//Logger.Current.WriteLine($"loaded theme '{name}' (dark:{dark})");
			//Logger.Current.WriteLine(root.ToString(SaveOptions.None));

			foreach (var record in root.Elements("Style"))
			{
				cache.Add(new StyleRecord(record));
			}
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


		public List<Style> LoadTheme(string path)
		{
			if (File.Exists(path))
			{
				try
				{
					var root = XElement.Load(path);
					var ns = root.GetDefaultNamespace();

					root.GetAttributeValue("name", out name, Path.GetFileNameWithoutExtension(path));
					root.GetAttributeValue("dark", out dark, false);

					return root.Elements(ns + "Style").ToList()
						.ConvertAll(e => new Style(new StyleRecord(e)));
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"error loading theme {path}", exc);
				}
			}

			return new List<Style>();
		}


		public void Save(Style style)
		{
			var target = cache.FirstOrDefault(e => e.Name.Equals(style.Name));
			if (target != null)
			{
				cache.Remove(target);
			}
			else
			{
				style.Index = cache.Max(s => s.Index) + 1;
			}

			cache.Add(new StyleRecord(style));

			Save(cache.OrderBy(e => e.Index));
		}


		public static void Save(List<Style> styles, string path = null)
		{
			Save(styles.ConvertAll(e => new StyleRecord(e)).OrderBy(e => e.Index), path);
		}


		private static void Save(IEnumerable<StyleRecord> styles, string path = null)
		{
			if (path == null)
			{
				path = Path.Combine(PathFactory.GetAppDataPath(), Properties.Resources.CustomStylesFilename);
			}

			PathFactory.EnsurePathExists(Path.GetDirectoryName(path));

			var root = new XElement("Theme",
				new XAttribute("name", "Custom")
				);

			foreach (var style in styles)
			{
				root.Add(style.ToXElement());
			}

			root.Save(path, SaveOptions.None);
		}
	}
}
