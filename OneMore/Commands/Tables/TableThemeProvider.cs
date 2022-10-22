//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Newtonsoft.Json;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Resx = Properties.Resources;


	internal class TableThemeProvider
	{
		private const string UserFileName = "TableStyles.json";

		private readonly List<TableTheme> themes;
		private readonly int syscount;


		public TableThemeProvider()
		{
			// Reminder that when adding a .json file into a .resx file, you need to change
			// the FileType property of the resource to Text instead of Binary...

			themes = JsonConvert.DeserializeObject<List<TableTheme>>(Resx.DefaultTableThemes);

			// first 'syscount' entires are system-defined default themes
			syscount = themes.Count;

			themes.AddRange(LoadUserThemes());
		}


		private IEnumerable<TableTheme> LoadUserThemes()
		{
			var path = Path.Combine(PathHelper.GetAppDataPath(), UserFileName);

			try
			{
				if (File.Exists(path))
				{
					return JsonConvert
						.DeserializeObject<List<TableTheme>>(File.ReadAllText(path));
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"error loading {path}", exc);
			}

			return new List<TableTheme>();
		}


		/// <summary>
		/// Gets the total number of system-defined and user-defined themes.
		/// </summary>
		public int Count => themes.Count;


		/// <summary>
		/// Gets the indexed theme.
		/// </summary>
		/// <param name="index">Index of the them to retrieve</param>
		/// <returns>The indexed TableTheme</returns>
		public TableTheme GetTheme(int index)
		{
			return themes[index];
		}


		/// <summary>
		/// Gets the name of the indexed theme.
		/// </summary>
		/// <param name="index">Index of the them to retrieve</param>
		/// <returns>The name of the indexed TableTheme</returns>
		public string GetName(int index)
		{
			if (index < 0 || index >= themes.Count)
			{
				return string.Format(Resx.TableTheme_Screentip, index);
			}

			var theme = themes[index];
			if (string.IsNullOrEmpty(theme.Name))
			{
				return string.Format(Resx.TableTheme_Screentip, index);
			}

			return theme.Name;
		}


		/// <summary>
		/// Returns only the user-defined themes.
		/// </summary>
		/// <returns>A List of themes</returns>
		public List<TableTheme> GetUserThemes()
		{
			return themes.Skip(syscount)
				.OrderBy(t => t.Name)
				.ToList();
		}


		/// <summary>
		/// Saves user defines themes to the user's appdata folder
		/// </summary>
		/// <param name="themes">A list of themes</param>
		public void SaveUserThemes(IEnumerable<TableTheme> themes)
		{
			var json = JsonConvert.SerializeObject(themes,
				Formatting.Indented,
				new JsonSerializerSettings
				{
					DefaultValueHandling = DefaultValueHandling.Ignore
				});

			var path = Path.Combine(PathHelper.GetAppDataPath(), UserFileName);

			try
			{
				File.WriteAllText(path, json);
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"error saving {path}", exc);
			}
		}
	}
}
