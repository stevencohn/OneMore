//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Newtonsoft.Json;
	using System.Collections.Generic;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class TableThemeProvider
	{
		private readonly List<TableTheme> themes;


		public TableThemeProvider()
		{
			// Reminder that when adding a .json file into a .resx file, you need to change
			// the FileType property of the resource to Text instead of Binary...

			themes = JsonConvert.DeserializeObject<List<TableTheme>>(Resx.DefaultTableThemes);
		}


		public int Count => themes.Count;

		public TableTheme GetTheme(int index)
		{
			return themes[index];
		}

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
	}
}
