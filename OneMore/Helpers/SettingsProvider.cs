//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.IO;
	using System.Xml.Linq;


	/// <summary>
	/// Loads, save, and manages user settings
	/// </summary>
	internal class SettingsProvider
	{
		private string path;
		private XElement root;


		/// <summary>
		/// Initialize a new provider.
		/// </summary>
		public SettingsProvider()
		{
			path = Path.Combine(
				PathFactory.GetAppDataPath(), Properties.Resources.SettingsFilename);

			if (File.Exists(path))
			{
				try
				{
					root = XElement.Load(path);
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"Error reading {path}", exc);
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
		/// Gets the preset image width for resizing images
		/// </summary>
		/// <returns></returns>
		public int GetImageWidth()
		{
			var width = root.Element("images")?.Attribute("width").Value;
			return width == null ? 500 : int.Parse(width);
		}


		public void SetImageWidth(int width)
		{
			root.Element("images").Attribute("width").Value = width.ToString();
		}


		public void Save()
		{
			PathFactory.EnsurePathExists(Path.GetDirectoryName(path));
			root.Save(path, SaveOptions.None);
		}
	}
}
