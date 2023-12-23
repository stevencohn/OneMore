﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************                

namespace River.OneMoreAddIn.Colorizer
{
	using River.OneMoreAddIn.Settings;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
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
		private string fontStyle;


		/// <summary>
		/// Initializes a new instance for the specified language.
		/// </summary>
		/// <param name="languageName">
		/// The language name; should match the name of the language definition file
		/// </param>
		/// <param name="themeName">Must be "light" or "dark"</param>
		/// <param name="autoOverride">
		/// True to use color overrides from theme file; this is needed in native dark mode
		/// when the page color is auto to change the plain text color
		/// </param>
		public Colorizer(string languageName, string themeName, bool autoOverride)
		{
			var root = GetColorizerDirectory();
			var path = Path.Combine(root, $@"Languages\{languageName}.json");

			if (!File.Exists(path))
			{
				throw new FileNotFoundException(path);
			}

			parser = new Parser(Compiler.Compile(Provider.LoadLanguage(path)));

			theme = Provider.LoadTheme(
				Path.Combine(root, $@"Themes\{themeName}-theme.json"), autoOverride);

			var settings = new SettingsProvider().GetCollection(nameof(ColorizerSheet));
			if (settings.Get("apply", false))
			{
				// both must exist to be applied
				var size = settings.Get<string>("size");
				if (!string.IsNullOrWhiteSpace(size))
				{
					var family = settings.Get<string>("family");
					if (!string.IsNullOrWhiteSpace(family))
					{
						if (size.IndexOf('.') < 0)
						{
							size = $"{size}.0";
						}

						fontStyle = $"font-family:{family};font-size:{size}pt";
					}
				}
			}
		}


		/// <summary>
		/// Use the secondary font from user settings
		/// </summary>
		public void EnableSecondaryFont()
		{
			var settings = new SettingsProvider().GetCollection(nameof(ColorizerSheet));
			if (settings.Get("apply", false))
			{
				// both must exist to be applied
				var size = settings.Get<string>("size2");
				if (!string.IsNullOrWhiteSpace(size))
				{
					var family = settings.Get<string>("family2");
					if (!string.IsNullOrWhiteSpace(family))
					{
						if (size.IndexOf('.') < 0)
						{
							size = $"{size}.0";
						}

						fontStyle = $"font-family:{family};font-size:{size}pt";
					}
				}
			}
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

			if (fontStyle != null)
			{
				builder.Insert(0, $"<span style='{fontStyle}'>");
				builder.Append("</span>");
			}

			return builder.ToString();
		}


		/// <summary>
		/// Apply diff styling to OE
		/// </summary>
		/// <param name="element">An OE element</param>
		/// <param name="addition">True if addition, otherwise removal</param>
		public void ColorizeDiffs(XElement element, bool addition)
		{
			var style = theme.GetStyle(addition ? "diffadd" : "diffremove");
			if (style == null)
			{
				return;
			}

			var ns = element.GetNamespaceOfPrefix(OneNote.Prefix);
			foreach (var run in element.Elements(ns + "T"))
			{
				var attribute = run.Attribute("style");
				if (attribute == null)
				{
					run.SetAttributeValue("style", $"background:{style.Background}");
				}
				else
				{
					var parts = attribute.Value.Split(
						new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries)
						.ToList();

					var index = parts.FindIndex(p => p.StartsWith("background:"));
					if (index < 0)
					{
						parts.Add($"background:{style.Background}");
					}
					else
					{
						parts[index] = $"background:{style.Background}";
					}

					run.SetAttributeValue("style", string.Join(";", parts));
				}
			}
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
