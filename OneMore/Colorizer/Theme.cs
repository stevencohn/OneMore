//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************                

namespace River.OneMoreAddIn.Colorizer
{
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Text;


	// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
	// Style...

	/// <summary>
	/// Defines minimum surface of Style needed beyond the Colorizer namespace
	/// </summary>
	internal interface IStyle
	{
		/// <summary>
		/// Gets the name of this style, must be unique within the theme
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Applies this style to the given string, returning a SPAN with a style attribute
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		string Apply(string code);
	}


	/// <summary>
	/// Used only for deserialization; the interface is used thereafter
	/// </summary>
	internal class Style : IStyle
	{
		public string Name { get; set; }
		public string Foreground { get; set; }
		public string Background { get; set; }
		public bool Bold { get; set; }
		public bool Italic { get; set; }

		public string Apply(string code)
		{
			var builder = new StringBuilder();
			if (!string.IsNullOrEmpty(Foreground))
				builder.Append($"color:{Foreground};");

			if (!string.IsNullOrEmpty(Background))
				builder.Append($"background:{Background};");

			if (Bold)
				builder.Append("font-weight:bold;");

			if (Italic)
				builder.Append("font-style:italic;");

			if (builder.Length > 0)
			{
				return $"<span style=\"{builder}\">{code}</span>";
			}

			return code;
		}
	}


	// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
	// Theme...

	/// <summary>
	/// Defines minimum surface of Theme needed beyond the Colorizer namespace
	/// </summary>
	internal interface ITheme
	{
		/// <summary>
		/// Gets the named style.
		/// </summary>
		/// <param name="name">The name of a style defined by this theme</param>
		/// <returns>An IStyle of the style</returns>
		Style GetStyle(string name);
	}


	/// <summary>
	/// Used only for deserialization; the interface is used thereafter
	/// </summary>
	internal class Theme : ITheme
	{
		public Dictionary<string, string> Colors { get; set; }

		public List<Style> Styles { get; set; }

		public Style GetStyle(string name)
		{
			return Styles.FirstOrDefault(s => s.Name == name);
		}


		public void TranslateColorNames(bool autoOverride)
		{
			if (autoOverride)
			{
				// normally, PlainText=Auto. But in native dark mode (Switch Background) then we
				// want PlainText to be AutoNative to compensate for OneNote's color witchcraft

				if (Colors.ContainsKey("AutoNative"))
				{
					var plain = GetStyle("PlainText");
					if (plain != null)
					{
						plain.Foreground = Colors["AutoNative"];
					}
				}
			}

			foreach (var style in Styles)
			{
				// standardize on lowercase names because users are stupid
				style.Name = style.Name.ToLower();

				style.Background = TranslateColorName(style.Background);
				style.Foreground = TranslateColorName(style.Foreground);
			}
		}

		private string TranslateColorName(string color)
		{
			if (string.IsNullOrEmpty(color))
			{
				return null;
			}

			if (Colors.ContainsKey(color))
			{
				color = Colors[color];

				// normalize color as 6-byte hex HTML color string
				return ColorTranslator.FromHtml(color).ToRGBHtml();
			}

			if (color.StartsWith("#"))
			{
				return ColorTranslator.FromHtml(color).ToRGBHtml();
			}

			return color;
		}
	}
}
