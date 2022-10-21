//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Newtonsoft.Json;
	using System.Collections.Generic;
	using System.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class TableThemeProvider
	{
		private readonly List<TableTheme> themes;
		private readonly int syscount;


		public TableThemeProvider()
		{
			// Reminder that when adding a .json file into a .resx file, you need to change
			// the FileType property of the resource to Text instead of Binary...

			themes = JsonConvert.DeserializeObject<List<TableTheme>>(Resx.DefaultTableThemes);

			// first 'syscount' entires are system-defined default themes
			syscount = themes.Count;
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
		/// 
		/// </summary>
		/// <returns></returns>
		public List<TableTheme> GetUserThemes()
		{
			return themes.Skip(syscount).ToList();
		}
	}
}
