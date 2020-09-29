//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Settings
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Web.Script.Serialization;


	/// <summary>
	/// Loads, save, and manages user settings
	/// </summary>
	internal class SearchProvider
	{
		private readonly string path;


		/// <summary>
		/// Initialize a new provider.
		/// </summary>
		public SearchProvider()
		{
			path = Path.Combine(
				PathFactory.GetAppDataPath(), "SearchEngines.json");
		}


		public List<SearchEngine> LoadEngines()
		{
			var engines = new List<SearchEngine>();

			if (File.Exists(path))
			{
				try
				{
					var json = File.ReadAllText(path);
					var serializer = new JavaScriptSerializer();
					engines.AddRange(serializer.Deserialize<SearchEngine[]>(json));
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"Error reading {path}", exc);
				}
			}

			return engines;
		}
	}
}
