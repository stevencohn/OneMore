//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S1117 // Local variables should not shadow class fields

namespace River.OneMoreAddIn.Styles
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.IO;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class ThemeProvider : Loggable
	{
		private const string SettingsKey = "pageTheme";
		private const string SettingsKeyKey = "key";

		private readonly Theme theme;


		public ThemeProvider()
			: this(GetSavedKey())
		{
		}


		public ThemeProvider(string key)
		{
			// may be null if from default constructor and no setting
			if (!string.IsNullOrEmpty(key))
			{
				// key actually specifies the path to a theme file
				if (!string.IsNullOrEmpty(Path.GetDirectoryName(key)))
				{
					var root = LoadFromFile(key);
					if (root != null)
					{
						theme = new Theme(root, Path.GetFileNameWithoutExtension(key));
					}

					// explicit path not found, leave theme null
					return;
				}

				// load by key in expected appdata paths
				theme = Load(key);
			}

			if (theme == null)
			{
				// last ditch, load something!
				theme = new Theme(XElement.Parse(Resx.DefaultStyles), "Default");
			}
		}



		public Theme Theme => theme;


		private static string GetSavedKey()
		{
			var sp = new SettingsProvider();
			var settings = sp.GetCollection(SettingsKey);
			if (settings != null)
			{
				return settings.Get<string>(SettingsKeyKey);
			}

			return null;
		}


		private Theme Load(string key)
		{
			// appdata\Roaming\OneMore\Themes\key.xml
			var root = LoadFromFile(Path.Combine(
				PathFactory.GetAppDataPath(), Resx.ThemesFolder, $"{key}.xml"));

			if (root == null)
			{
				// appdata\Roaming\OneMore\CustomStyles.xml -- backwards compatibility
				root = LoadFromFile(Path.Combine(
					PathFactory.GetAppDataPath(), Resx.CustomStylesFilename));
			}

			if (root == null)
			{
				// key not found, custom not found, so load default theme
				root = XElement.Parse(Resx.DefaultStyles);
			}

			return new Theme(root, key);
		}


		private XElement LoadFromFile(string path)
		{
			if (!File.Exists(path))
			{
				//logger.WriteLine($"file not found {path}");
				return null;
			}

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
						root.Add(new XAttribute(
							"name", Path.GetFileNameWithoutExtension(path)));
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

			return null;
		}


		public static void RecordTheme(string key)
		{
			var setprovider = new SettingsProvider();
			var settings = setprovider.GetCollection(SettingsKey);
			if (settings.Add(SettingsKeyKey, key))
			{
				setprovider.SetCollection(settings);
				setprovider.Save();
			}
		}


		public static void Save(Style style)
		{
			var theme = new ThemeProvider().Theme;
			theme.ReplaceStyle(style);
			Save(theme);
		}


		public static void Save(Theme theme, string path = null)
		{
			string key;
			string name;
			if (string.IsNullOrEmpty(path))
			{
				path = Path.Combine(PathFactory.GetAppDataPath(), Resx.ThemesFolder, theme.Key);
				key = theme.Key;
				name = theme.Name;
			}
			else
			{
				key = name = Path.GetFileNameWithoutExtension(path);
			}

			if (string.IsNullOrEmpty(Path.GetExtension(path)))
			{
				path = $"{path}.xml";
			}

			PathFactory.EnsurePathExists(Path.GetDirectoryName(path));

			// create a new instance to handle the save-as workflow
			var root = new XElement("Theme",
				new XAttribute("key", key),
				new XAttribute("name", name),
				new XAttribute("dark", theme.Dark)
				);

			foreach (var record in theme.GetRecords())
			{
				root.Add(record.ToXElement());
			}

			root.Save(path, SaveOptions.None);

		}
	}
}
