//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// Loads, save, and manages user settings
	/// </summary>
	internal class SettingsProvider
	{
		private readonly string path;
		private readonly XElement root;


		/// <summary>
		/// Initialize a new snipProvider.
		/// </summary>
		public SettingsProvider()
		{
			path = Path.Combine(
				PathHelper.GetAppDataPath(), Properties.Resources.SettingsFilename);

			if (File.Exists(path))
			{
				try
				{
					root = XElement.Load(path);
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"error reading {path}", exc);
				}
			}

			if (root == null)
			{
				// file not found so initialize with defaults
				root = new XElement("settings",
					new XElement("images",
						new XAttribute("width", "500")
					)
				);
			}
		}


		/// <summary>
		/// Get the named collection. If the named element contains child elements then it is
		/// presumed to be an XElement, otherwise it is presumed to be a simple key/value entry
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public SettingsCollection GetCollection(string name)
		{
			var settings = new SettingsCollection(name);

			var elements = root.Element(name)?.Elements();
			if (elements != null)
			{
				foreach (var element in elements)
				{
					if (element.HasElements)
					{
						// has child elements so this must be an XElement
						settings.Add(element.Name.LocalName, element);
					}
					else
					{
						// has no children so this must be a simple key/value entry
						settings.Add(element.Name.LocalName, element.Value);
					}
				}
			}

			return settings;
		}


		public bool RemoveCollection(string name)
		{
			var element = root.Element(name);
			if (element != null)
			{
				element.Remove();
				return true;
			}

			return false;
		}


		public void SetCollection(SettingsCollection settings)
		{
			var element = new XElement(settings.Name);
			var keys = settings.Keys.ToList();
			foreach (var key in keys)
			{
				if (settings.IsElement(key))
				{
					element.Add(settings.Get<XElement>(key));
				}
				else
				{
					element.Add(new XElement(key, settings[key]));
				}
			}

			var e = root.Element(settings.Name);
			if (e != null)
			{
				e.ReplaceWith(element);
			}
			else
			{
				root.Add(element);
			}
		}


		public void Save()
		{
			PathHelper.EnsurePathExists(Path.GetDirectoryName(path));
			root.Save(path, SaveOptions.None);
		}
	}
}
