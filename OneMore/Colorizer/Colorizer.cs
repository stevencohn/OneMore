//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************                

namespace River.OneMoreAddIn.Colorizer
{
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using System.Text;
	using System.Web;
	using System.Xml.Linq;


	/// <summary>
	/// This colorizer is suited specifically to generating OneNote content
	/// </summary>
	internal class Colorizer
	{
		private readonly Parser parser;
		private readonly ITheme theme;
		private readonly bool autoOverride;


		/// <summary>
		/// Initializes a new instance for the specified language.
		/// </summary>
		/// <param name="languageName">
		/// The language name; should match the name of the language definition file
		/// </param>
		/// <param name="themeName">Must be "light" or "dark"</param>
		/// <param name="autoOverride">
		/// True to use color overrides from theme file; this is need in dark mode
		/// when the page color is auto to change the plain text color
		/// </param>
		public Colorizer(string languageName, string themeName, bool autoOverride)
		{
			this.autoOverride = autoOverride;

			var root = GetColorizerDirectory();
			var path = Path.Combine(root, $@"Languages\{languageName}.json");

			if (!File.Exists(path))
			{
				throw new FileNotFoundException(path);
			}

			parser = new Parser(Compiler.Compile(Provider.LoadLanguage(path)));

			theme = Provider.LoadTheme(
				Path.Combine(root, $@"Themes\{themeName}-theme.json"), autoOverride);
		}


		/// <summary>
		/// Colorizes the given source code for the current language, producing OneNote
		/// content.
		/// </summary>
		/// <param name="source">The original source code text</param>
		/// <returns>An XElement describing an OEChildren hierarchy</returns>
		public XElement Colorize(string source, XNamespace ns)
		{
			var container = new XElement(ns + "OEChildren");
			var builder = new StringBuilder();

			parser.Parse(source, (code, scope) =>
			{
				if (string.IsNullOrEmpty(code))
				{
					// end-of-line
					container.Add(new XElement(ns + "OE",
						new XElement(ns + "T",
							new XCData(builder.ToString()))
						));

					builder.Clear();
				}
				else
				{
					if (scope == null)
					{
						// plain text prior to capture
						code = code.Replace("\t", " ");
						if (theme != null)
						{
							var style = theme.GetStyle("plaintext");
							builder.Append(style == null ? code : style.Apply(code));
						}
						else
						{
							builder.Append(code);
						}
					}
					else
					{
						if (theme != null)
						{
							var style = theme.GetStyle(scope);
							builder.Append(style == null ? code : style.Apply(code));
						}
						else
						{
							builder.Append(code);
						}
					}
				}
			});

			return container;
		}


		/// <summary>
		/// Colorizes the given source code for the current language, producting
		/// raw HTML-style text with span elements and style attributes.
		/// </summary>
		/// <param name="source">The original source code text</param>
		/// <returns></returns>
		public string ColorizeOne(string source)
		{
			var builder = new StringBuilder();

			parser.Parse(source, (code, scope) =>
			{
				if (string.IsNullOrEmpty(code) && parser.HasMoreCaptures)
				{
					// end-of-line
					builder.Append("<br/>");
				}
				else
				{
					code = HttpUtility.HtmlEncode(code);

					if (scope == null)
					{
						// plain text prior to capture
						// simple conversion of tabs to spaces (shouldn't be tabs in OneNote)
						code = code.Replace("\t", " ");

						if (theme != null)
						{
							var style = theme.GetStyle("plaintext");
							builder.Append(style == null ? code : style.Apply(code));
						}
						else
						{
							builder.Append(code);
						}
					}
					else
					{
						if (theme != null)
						{
							var style = theme.GetStyle(scope);
							builder.Append(style == null ? code : style.Apply(code));
						}
						else
						{
							builder.Append(code);
						}
					}
				}
			});

			return builder.ToString();
		}


		/// <summary>
		/// Returns the root path of the directory containing the Colorizer language and theme
		/// definitions
		/// </summary>
		/// <returns></returns>
		public static string GetColorizerDirectory()
		{
			return Path.Combine(
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
				"Colorizer");
		}

		/// <summary>
		/// Gets a list of available language names
		/// </summary>
		/// <param name="dirPath">The directory path containing the language definition files</param>
		/// <returns></returns>
		public static IDictionary<string, string> LoadLanguageNames()
		{
			return Provider.LoadLanguageNames(
				Path.Combine(GetColorizerDirectory(), "Languages"));
		}
	}
}
