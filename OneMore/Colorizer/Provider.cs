//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************                

namespace River.OneMoreAddIn.Colorizer
{
	using Newtonsoft.Json;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;


	internal static class Provider
	{

		/// <summary>
		/// Loads a language from the given file path. Throws on IO or JSON parse failure
		/// so callers can distinguish a malformed file from a missing one.
		/// </summary>
		/// <param name="path">The path to the language json definition file</param>
		/// <returns>An ILanguage describing the langauge</returns>
		public static ILanguage LoadLanguage(string path)
		{
			var lines = File.ReadAllLines(path)
				.Where(line => !Regex.IsMatch(line, @"^\\s*//"));

			return JsonConvert.DeserializeObject<Language>(
				string.Join(Environment.NewLine, lines));
		}


		/// <summary>
		/// Gets a list of available language names. Per-file failures are logged and
		/// skipped so a single malformed definition does not break the picker.
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
				try
				{
					var language = LoadLanguage(file);
					if (language != null)
					{
						names.Add(language.Name, Path.GetFileNameWithoutExtension(file));
					}
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"error loading language {file}", exc);
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
			Theme theme = null;

			try
			{
				var lines = File.ReadAllLines(path)
					.Where(line => !Regex.IsMatch(line, @"^\\s*//"));

				theme = JsonConvert.DeserializeObject<Theme>(
					string.Join(Environment.NewLine, lines));

				theme.TranslateColorNames(autoOverride);
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine(exc.Message);
				Logger.Current.WriteLine($"error loading theme {path}", exc);
			}

			return theme;
		}
	}
}
