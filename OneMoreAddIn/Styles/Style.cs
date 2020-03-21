//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Text;
	using System.Text.RegularExpressions;


	/// <summary>
	/// Commonly used throughout OneMore to convert to/from OneNote CSS
	/// </summary>
	public class Style : StyleBase
	{

		/// <summary>
		/// Initialize a new instance; for creating new custom styles
		/// </summary>
		public Style() : base()
		{

		}


		/// <summary>
		/// Copy properties from a given Style to initialize a new instance.
		/// </summary>
		/// <param name="record">
		/// A Style from which property values are to be copied.
		/// </param>
		public Style(StyleBase record) : base(record)
		{
		}


		/// <summary>
		/// Initialize a new instance by parsing the style attribute of the given element.
		/// </summary>
		/// <param name="element"></param>
		public Style(Dictionary<string, string> properties) : base()
		{
			if (properties.ContainsKey("font-family"))
				FontFamily = properties["font-family"];

			if (properties.ContainsKey("font-size"))
				FontSize = properties["font-size"];

			if (properties.ContainsKey("font-weight") && properties["font-weight"].Equals("bold"))
				IsBold = true;

			if (properties.ContainsKey("font-style") && properties["font-style"].Equals("italic"))
				IsItalic = true;

			if (properties.ContainsKey("text-decoration") && properties["text-decoration"].Contains("underline"))
				IsUnderline = true;

			if (properties.ContainsKey("text-decoration") && properties["text-decoration"].Contains("line-through"))
				IsStrikethrough = true;

			if (properties.ContainsKey("vertical-align") && properties["vertical-align"].Equals("super"))
				IsSuperscript = true;

			if (properties.ContainsKey("vertical-align") && properties["vertical-align"].Equals("sub"))
				IsSubscript = true;

			if (properties.ContainsKey("color"))
				Color = properties["color"];

			if (properties.ContainsKey("background"))
				Highlight = properties["background"];

			if (properties.ContainsKey("spaceBefore"))
				SpaceBefore = properties["spaceBefore"];

			if (properties.ContainsKey("spaceAfter"))
				SpaceAfter = properties["spaceAfter"];
		}


		/// <summary>
		/// Initialize a new instance by parsing the given CSS
		/// </summary>
		/// <param name="css">The value of an element's style attribute.</param>
		public Style(string css) : base()
		{
			ReadCss(css);
		}


		private void ReadCss(string css)
		{
			var parts = css.Split(';');
			var found = new StringCollection();

			for (int i = 0; i < parts.Length; i++)
			{
				var part = parts[i];

				var pair = part.Split(':');
				if (pair.Length < 2) continue;

				var key = pair[0].Trim();
				var val = pair[1].Trim();

				if (key.Equals("font-family") && !found.Contains(key))
				{
					FontFamily = FormatFontFamily(val);
				}
				else if (key.Equals("font-size") && !found.Contains(key))
				{
					fontSize = ParseFontSize(val);
				}
				else if (key.Equals("color") && !found.Contains(key))
				{
					Color = FormatColor(val);
				}
				else if (key.Equals("background") && !found.Contains(key))
				{
					Highlight = FormatColor(val);
				}
				else if (key.Equals("spaceBefore") && !found.Contains(key))
				{
					spaceBefore = ParseSpace(val);
				}
				else if (key.Equals("spaceAfter") && !found.Contains(key))
				{
					spaceAfter = ParseSpace(val);
				}
				else if (key.Equals("font-style") && !found.Contains(key))
				{
					IsItalic = true; // presume style is 'italic'
				}
				else if (key.Equals("font-weight") && !found.Contains(key))
				{
					IsBold = true; // presume style is 'bold'
				}
				else if (key.Equals("text-decoration"))
				{
					if (!found.Contains("underline"))
					{
						IsUnderline = val.Contains("underline");
						found.Add("underline");
					}

					if (!found.Contains("line-through"))
					{
						IsStrikethrough = val.Contains("line-through");
						found.Add("line-through");
					}
				}
				else if (key.Equals("vertical-align"))
				{
					if (!found.Contains("super"))
					{
						IsSuperscript = val.Contains("super");
						found.Add("super");
					}

					if (!found.Contains("sub"))
					{
						IsSubscript = val.Contains("sub");
						found.Add("sub");
					}
				}
			}
		}

		#region Formatters
		protected string FormatColor(string color)
		{
			if (string.IsNullOrEmpty(color))
			{
				return null;
			}

			// look for "automatic" or "yellow"...
			if (char.IsLetter(color[0]))
			{
				return color;
			}

			// else must be an #rgb value
			var match = Regex.Match(color, "^#([0-9a-fA-F]+)");
			if (match.Success)
			{
				color = match.Groups[match.Groups.Count - 1].Captures[0].Value;

				if (color.Length > 6)
				{
					color = color.Substring(color.Length - 6);
				}
				else while (color.Length < 6)
				{
					color = "0" + color;
				}

				return "#" + color.ToLower();
			}

			return color;
		}


		protected string FormatFontFamily(string family)
		{
			if (string.IsNullOrEmpty(family))
			{
				return null;
			}

			return family.Replace("'", string.Empty);
		}


		protected double ParseFontSize(string size)
		{
			if (string.IsNullOrEmpty(size))
			{
				return DefaultFontSize;
			}

			var match = Regex.Match(size, @"^[0-9]+(\.[0-9]+)?$(?:pt){0,1}");
			if (match.Success)
			{
				size = match.Captures[0].Value;
				if (!string.IsNullOrEmpty(size))
				{
					return double.Parse(size);
				}
			}

			return DefaultFontSize;
		}


		protected double ParseSpace(string space)
		{
			if (string.IsNullOrEmpty(space))
			{
				return 0;
			}

			if (double.TryParse(space, out var value))
			{
				return value;
			}

			return 0;
		}
		#endregion


		/// <summary>
		/// Gets or sets the font size for this style, handling set values like "12.0pt"
		/// </summary>
		public override string FontSize
		{
			get => base.FontSize;
			set => base.fontSize = ParseFontSize(value);
		}


		/// <summary>
		/// Gets or sets the spacebefore for this style, handling set values like "12.0px"
		/// </summary>
		public override string SpaceAfter
		{
			get => base.SpaceAfter;
			set => base.spaceAfter = ParseSpace(value);
		}


		/// <summary>
		/// Gets or sets the spaceafter for this style, handling set values like "12.0px"
		/// </summary>
		public override string SpaceBefore
		{
			get => base.SpaceBefore;
			set => base.spaceBefore = ParseSpace(value);
		}


		/// <summary>
		/// Merge this style with another given style.
		/// </summary>
		/// <param name="other"></param>
		public void Merge(Style other)
		{
			FontFamily = other.FontFamily;
			FontSize = other.FontSize;
			IsBold = other.IsBold;
			IsItalic = other.IsItalic;
			IsUnderline = other.IsUnderline;
			IsSubscript = other.IsSubscript;
			IsSuperscript = other.IsSuperscript;
			SpaceAfter = other.SpaceAfter;
			SpaceBefore = other.SpaceBefore;

			if (ApplyColors)
			{
				Color = other.Color;
				if (other.Highlight != null) Highlight = other.Highlight;
			}
		}


		/// <summary>
		/// Builds a CSS string for use within a one:Page
		/// </summary>
		/// <returns></returns>
		public string ToCss()
		{
			var builder = new StringBuilder();

			if (!string.IsNullOrEmpty(FontFamily))
			{
				builder.Append("font-family:'" + FontFamily + "';");
			}

			if (fontSize > 0)
			{
				builder.Append("font-size:" + FontSize + "pt;");
			}

			if (ApplyColors)
			{
				if (!string.IsNullOrEmpty(Color) && !Color.Equals("automatic"))
					builder.Append("color:" + FormatColor(Color) + ";");

				if (!string.IsNullOrEmpty(Highlight) && !Highlight.Equals("automatic"))
					builder.Append("background:" + FormatColor(Highlight) + ";");
			}

			if (IsBold)
				builder.Append("font-weight:bold;");

			if (IsItalic)
				builder.Append("font-style:italic;");

			if (IsUnderline || IsStrikethrough)
			{
				builder.Append("text-decoration:");

				if (IsUnderline)
					builder.Append("underline");

				if (IsStrikethrough)
					builder.Append(" line-through");

				builder.Append(";");
			}

			if (IsSuperscript)
			{
				builder.Append("vertical-align:super;");
			}
			else if (IsSubscript)
			{
				builder.Append("vertical-align:sub;");
			}

			return builder.ToString();
		}
	}
}