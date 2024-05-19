﻿//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S1117 // Local variables should not shadow class fields

namespace River.OneMoreAddIn.Styles
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class ThemeProvider : Loggable
	{
		private const string SettingsKey = "pageTheme";
		private const string SettingsKeyKey = "key";
		private const string EditedLabel = "-edited";

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
				if (key.IndexOf(Path.DirectorySeparatorChar) >= 0 ||
					key.IndexOf(Path.AltDirectorySeparatorChar) >= 0)
				{
					var root = LoadFromFile(key);
					if (root is not null)
					{
						theme = new Theme(root, Path.GetFileNameWithoutExtension(key));
					}

					// explicit path not found, try just key...
				}

				// load by key in expected appdata paths
				theme = Load(key);
				return;
			}
			
			// last ditch, load something!
			var defroot = XElement.Parse(Resx.DefaultStyles);
			theme = new Theme(defroot, defroot.Attribute("name").Value);
		}



		public Theme Theme => theme;


		private static string GetSavedKey()
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection(SettingsKey);
			if (settings is not null)
			{
				return settings.Get<string>(SettingsKeyKey);
			}

			return null;
		}


		private Theme Load(string key)
		{
			// appdata\Roaming\OneMore\Themes\key-edited.xml
			var root = LoadFromFile(Path.Combine(
				PathHelper.GetAppDataPath(), Resx.ThemesFolder, $"{key}{EditedLabel}.xml"));

			// appdata\Roaming\OneMore\Themes\key.xml
			root ??= LoadFromFile(Path.Combine(
				PathHelper.GetAppDataPath(), Resx.ThemesFolder, $"{key}.xml"));

			if (root is null)
			{
				// return default style set
				root = XElement.Parse(Resx.DefaultStyles);
				key = root.Attribute("name").Value;
			}

			// backwards-compatible upgrade; eliminate Default.xml
			if (key == "Default")
			{
				key = root.Attribute("name").Value;
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
				var regex = new Regex(@"^\\s*//");
				var lines = File.ReadAllLines(path)
					.Where(line => !regex.IsMatch(line));

				var root = XElement.Parse(string.Join(Environment.NewLine, lines));
				if (root.Name.LocalName == "CustomStyles" ||
					root.Name.LocalName == "Theme")
				{
					if (root.Attribute("name") is null)
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


		public Theme ResetPredefinedTheme(string key)
		{
			var prefix = Path.Combine(PathHelper.GetAppDataPath(), Resx.ThemesFolder, key);

			var source = $"{prefix}.xml";
			if (File.Exists(source))
			{
				try
				{
					var target = $"{prefix}{EditedLabel}.xml";
					if (File.Exists(target))
					{
						File.Delete(target);
					}
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error resetting theme {key}", exc);
				}
			}

			return Load(key);
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
				key = theme.Key;
				name = theme.Name;

				path = Path.Combine(
					PathHelper.GetAppDataPath(), Resx.ThemesFolder,
					theme.IsPredefined ? $"{theme.Key}{EditedLabel}" : theme.Key) + ".xml";
			}
			else
			{
				var dir = Path.GetDirectoryName(path);
				var ext = Path.GetExtension(path);

				key = Path.GetFileNameWithoutExtension(path);
				if (key.EndsWith(EditedLabel))
				{
					key = key.Substring(0, key.Length - 7);
				}

				name = key;

				path = Path.Combine(dir, theme.IsPredefined ? $"{key}{EditedLabel}" : key) + ext;
			}

			PathHelper.EnsurePathExists(Path.GetDirectoryName(path));

			// create a new instance to handle the save-as workflow
			var root = new XElement("Theme",
				new XAttribute("key", key),
				new XAttribute("name", name),
				new XAttribute("color", theme.Color)
				);

			if (theme.SetColor)
			{
				root.Add(new XAttribute("setColor", "True"));
			}

			if (theme.Dark)
			{
				root.Add(new XAttribute("dark", "True"));
			}

			if (theme.IsPredefined)
			{
				root.Add(new XAttribute("isPredefined", "True"));
			}

			foreach (var record in theme.GetRecords())
			{
				root.Add(record.ToXElement());
			}

			root.Save(path, SaveOptions.None);
		}
	}
}
