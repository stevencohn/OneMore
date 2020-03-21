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
					Logger.Current.WriteLine($"Loading styles from {path}");
					root = XElement.Load(path);
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"Error reading {path}", exc);
				}
			}

			if (root == null)
			{
				// file not found so load default theme
				Logger.Current.WriteLine("Loading default styles");
				root = XElement.Parse(Properties.Resources.CustomStyles);
			}

			Logger.Current.WriteLine(root.ToString(SaveOptions.None));

			foreach (var record in root.Elements("Style"))
			{
				cache.Add(new StyleRecord(record));
			}
		}


		public void Add(StyleRecord record)
		{
			record.Index = cache.Max(e => e.Index);
			cache.Add(record);
		}


		/// <summary>
		/// Gets the number of styles cached and managed.
		/// </summary>
		public int Count => cache.Count;


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


		public XElement GetXml()
		{
			var root = new XElement("CustomStyles",
				new XAttribute(XNamespace.Xmlns + "om", Properties.Resources.OneMoreNamespace) 
				);

			foreach (var record in cache)
			{
				root.Add(record.ToXElement());
			}

			return root;
		}



		public List<Style> LoadTheme(string path)
		{
			if (File.Exists(path))
			{
				try
				{
					var root = XElement.Load(path);
					var ns = root.GetDefaultNamespace();

					return root.Elements(ns + "Style").ToList()
						.ConvertAll(e => new Style(new StyleRecord(e)));
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"Error loading theme {path}", exc);
				}
			}

			return null;
		}


		public void Save(Style style)
		{
			var target = cache.FirstOrDefault(e => e.Name.Equals(style.Name));
			if (target != null)
			{
				cache.Remove(target);
			}

			cache.Add(new StyleRecord(style));

			Save(cache.OrderBy(e => e.Index));
		}


		public void Save(List<Style> styles)
		{
			Save(styles.ConvertAll(e => new StyleRecord(e)).OrderBy(e => e.Index));
		}


		private void Save(IEnumerable<StyleRecord> styles)
		{
			var path = PathFactory.GetAppDataPath();
			PathFactory.EnsurePathExists(path);
			path = Path.Combine(path, Properties.Resources.CustomStylesFilename);

			var root = new XElement("CustomStyles");

			foreach (var style in styles)
			{
				root.Add(style.ToXElement());
			}

			root.Save(path, SaveOptions.None);
		}
	}
}
