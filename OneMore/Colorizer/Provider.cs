//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************                

namespace River.OneMoreAddIn.Colorizer
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Web.Script.Serialization;


	internal static class Provider
	{

		/// <summary>
		/// Loads a language from the given file path
		/// </summary>
		/// <param name="path">The path to the language json definition file</param>
		/// <returns>An ILanguage describing the langauge</returns>
		public static ILanguage LoadLanguage(string path)
		{
			var json = File.ReadAllText(path);
			var serializer = new JavaScriptSerializer();
			Language language = null;

			try
			{
				language = serializer.Deserialize<Language>(json);
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"error loading language {path}", exc);
			}

			return language;
		}


		/// <summary>
		/// Gets a list of available language names
		/// </summary>
		/// <param name="dirPath">The directory path containing the language definition files</param>
		/// <returns></returns>
		public static IDictionary<string, string> LoadLanguageNames(string dirPath)
		{
			if (!Directory.Exists(dirPath))
			{
				throw new DirectoryNotFoundException(dirPath);
			}

			var names = new SortedDictionary<string, string>();

			foreach (var file in Directory.GetFiles(dirPath, "*.json"))
			{
				var language = LoadLanguage(file);
				if (language != null)
				{
					names.Add(language.Name, Path.GetFileNameWithoutExtension(file));
				}
			}

			return names;
		}


		/// <summary>
		/// Loads a syntax coloring theme from the given file path
		/// </summary>
		/// <param name="path"></param>
		/// <param name="autoOverride"></param>
		/// <returns></returns>
		public static ITheme LoadTheme(string path, bool autoOverride)
		{
			var json = File.ReadAllText(path);
			var serializer = new JavaScriptSerializer();
			Theme theme = null;

			try
			{
				theme = serializer.Deserialize<Theme>(json);
				theme.TranslateColorNames(autoOverride);
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"error loading theme {path}", exc);
			}

			return theme;
		}
	}
}
