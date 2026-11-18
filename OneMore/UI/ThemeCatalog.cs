//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using Newtonsoft.Json.Linq;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using Resx = Properties.Resources;


	/// <summary>
	/// Describes one selectable UI color theme
	/// </summary>
	internal sealed class ThemeInfo
	{
		/// <summary>
		/// Gets the stable identifier persisted in settings; for user themes this is the
		/// file name without extension
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Gets the display name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets a value indicating whether this theme is embedded with OneMore as opposed to
		/// discovered in the user's Themes folder
		/// </summary>
		public bool IsBuiltIn { get; set; }

		/// <summary>
		/// Gets a function that returns the theme's JSON, or null for the System theme which
		/// is not a theme itself but resolves to either Light or Dark
		/// </summary>
		public Func<string> ReadJson { get; set; }

		public bool IsSystem => ReadJson is null;

		public override string ToString() => Name;
	}


	/// <summary>
	/// Registry of available UI themes: System, the Light and Dark base themes, other embedded
	/// themes, and any *.json themes found in %APPDATA%\OneMore\Themes.
	/// </summary>
	/// <remarks>
	/// Light and Dark are the base themes; System resolves to one of them. Every other theme
	/// is an enhancement that is chosen explicitly. To add a built-in theme, add a JSON file
	/// under UI\Themes, an embedded resource for it, and one entry to <see cref="BuiltIns"/>.
	/// </remarks>
	internal static class ThemeCatalog
	{
		public const string SystemId = "System";
		public const string LightId = "Light";
		public const string DarkId = "Dark";
		public const string DarkBlueId = "DarkBlue";

		// the number of leading built-ins whose names are localized by the settings dialog
		public const int LocalizedCount = 3;

		private const string UserThemeFilter = "*.json";


		private static readonly ThemeInfo[] BuiltIns = new[]
		{
			new ThemeInfo { Id = SystemId, Name = SystemId, IsBuiltIn = true },
			new ThemeInfo { Id = LightId, Name = LightId, IsBuiltIn = true, ReadJson = () => Resx.LightTheme },
			new ThemeInfo { Id = DarkId, Name = DarkId, IsBuiltIn = true, ReadJson = () => Resx.DarkTheme },
			new ThemeInfo { Id = DarkBlueId, Name = "Dark Blue", IsBuiltIn = true, ReadJson = () => Resx.DarkBlueTheme }
		};


		/// <summary>
		/// Gets all available themes; built-ins first in a fixed order, then user themes
		/// sorted by name. A user theme whose id matches a built-in is ignored.
		/// </summary>
		public static List<ThemeInfo> GetThemes()
		{
			var themes = new List<ThemeInfo>(BuiltIns);

			var users = new List<ThemeInfo>();
			foreach (var path in GetUserThemeFiles())
			{
				var info = ReadUserTheme(path);
				if (info is not null && !themes.Exists(t => IdEquals(t.Id, info.Id)))
				{
					users.Add(info);
				}
			}

			users.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase));
			themes.AddRange(users);
			return themes;
		}


		/// <summary>
		/// Finds the theme with the given id.
		/// </summary>
		/// <returns>The theme or null if not found</returns>
		public static ThemeInfo Find(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return null;
			}

			foreach (var info in BuiltIns)
			{
				if (IdEquals(info.Id, id))
				{
					return info;
				}
			}

			var path = Path.Combine(GetUserThemeDirectory(), $"{id}.json");
			return File.Exists(path) ? ReadUserTheme(path) : null;
		}


		/// <summary>
		/// Translates the legacy integer theme setting to a theme id. The values are
		/// 0=System, 1=Light, 2=Dark, 3=User which is now DarkBlue.
		/// </summary>
		public static string FromLegacyIndex(int index)
		{
			return index switch
			{
				1 => LightId,
				2 => DarkId,
				3 => DarkBlueId,
				_ => SystemId
			};
		}


		public static string GetUserThemeDirectory()
		{
			return ThemeProvider.GetThemeDirectory();
		}


		private static bool IdEquals(string a, string b)
		{
			return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
		}


		private static IEnumerable<string> GetUserThemeFiles()
		{
			var dir = GetUserThemeDirectory();
			if (!Directory.Exists(dir))
			{
				return Array.Empty<string>();
			}

			try
			{
				return Directory.GetFiles(dir, UserThemeFilter, SearchOption.TopDirectoryOnly);
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error enumerating user themes", exc);
				return Array.Empty<string>();
			}
		}


		private static ThemeInfo ReadUserTheme(string path)
		{
			try
			{
				var json = File.ReadAllText(path);
				var root = JObject.Parse(json);
				if (root["Colors"] is not JObject)
				{
					Logger.Current.WriteLine($"ignoring theme without colors: {path}");
					return null;
				}

				var id = Path.GetFileNameWithoutExtension(path);
				var name = root.Value<string>("Name");

				return new ThemeInfo
				{
					Id = id,
					Name = string.IsNullOrWhiteSpace(name) ? id : name,
					IsBuiltIn = false,
					ReadJson = () => File.ReadAllText(path)
				};
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"ignoring unreadable theme: {path}", exc);
				return null;
			}
		}
	}
}
