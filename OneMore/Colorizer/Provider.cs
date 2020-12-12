//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************                

namespace River.OneMoreAddIn.Colorizer
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text.Json;
	using System.Text.Json.Serialization;


	internal static class Provider
	{

		/// <summary>
		/// Loads a language from the given file path
		/// </summary>
		/// <param name="path">The path to the language json definition file</param>
		/// <returns>An ILanguage describing the langauge</returns>
		public static ILanguage LoadLanguage(string path)
		{
			var serializeOptions = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				ReadCommentHandling = JsonCommentHandling.Skip,
				AllowTrailingCommas = true,

				Converters =
				{
					// handles interface->class conversion
					new RuleConverter()
				}
			};

			var json = File.ReadAllText(path);
			var language = JsonSerializer.Deserialize<Language>(json, serializeOptions);

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
				names.Add(language.Name, Path.GetFileNameWithoutExtension(file));
			}

			return names;
		}


		/// <summary>
		/// Loads a syntax coloring theme from the given file path
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static ITheme LoadTheme(string path)
		{
			var serializeOptions = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				ReadCommentHandling = JsonCommentHandling.Skip,
				AllowTrailingCommas = true,

				Converters =
				{
					// handles interface->class conversion
					new StyleConverter()
				}
			};

			var json = File.ReadAllText(path);
			var theme = JsonSerializer.Deserialize<Theme>(json, serializeOptions);

			theme.TranslateColorNames();

			return theme;
		}
	}


	internal class RuleConverter : JsonConverter<IRule>
	{
		public override IRule Read(
			ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			// convert from ILanguageRule to LanguageRule
			return JsonSerializer.Deserialize<Rule>(ref reader, options);
		}

		public override void Write(
			Utf8JsonWriter writer, IRule value, JsonSerializerOptions options)
		{
			// we're not serializing so this isn't used
			throw new NotImplementedException();
		}
	}


	internal class StyleConverter : JsonConverter<IStyle>
	{
		public override IStyle Read(
			ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			// convert from IStyle to Style
			return JsonSerializer.Deserialize<Style>(ref reader, options);
		}

		public override void Write(
			Utf8JsonWriter writer, IStyle value, JsonSerializerOptions options)
		{
			// we're not serializing so this isn't used
			throw new NotImplementedException();
		}
	}
}
