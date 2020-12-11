//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************                

namespace River.OneMoreAddIn.Colorizer
{
	using System;
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


	internal class RuleConverter: JsonConverter<IRule>
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
