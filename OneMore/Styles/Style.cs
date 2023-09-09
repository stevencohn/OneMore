//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CA1822 // member can be made static

namespace River.OneMoreAddIn.Styles
{
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Drawing;
	using System.Globalization;
	using System.Text;
	using System.Text.RegularExpressions;


	/// <summary>
	/// Commonly used throughout OneMore to convert to/from OneNote CSS
	/// </summary>
	public class Style : StyleBase
	{
		public const string HintMeta = "omStyleHint";


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
			Merge(properties);

			if ((spaceBefore > 0) || (spaceAfter > 0) || (spacing > 0))
			{
				StyleType = StyleType.Paragraph;
			}
		}


		/// <summary>
		/// Initialize a new instance by parsing the given CSS
		/// </summary>
		/// <param name="css">The value of an element's style attribute.</param>
		/// <param name="setDefaults">
		/// A Boolean indicating whether to set default values even for missing properties.
		/// Set this to false to not include defaulted missing properties in ToCss()
		/// </param>
		public Style(string css, bool setDefaults = true) : base(setDefaults)
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
					found.Add(key);
				}
				else if (key.Equals("font-size") && !found.Contains(key))
				{
					fontSize = ParseFontSize(val);
					found.Add(key);
				}
				else if (key.Equals("color") && !found.Contains(key))
				{
					Color = FormatColor(val);
					found.Add(key);
				}
				else if (key.Equals("background") && !found.Contains(key))
				{
					Highlight = FormatColor(val);
					found.Add(key);
				}
				else if (key.Equals("spaceBefore") && !found.Contains(key))
				{
					spaceBefore = ParseSpace(val);
					found.Add(key);
				}
				else if (key.Equals("spaceAfter") && !found.Contains(key))
				{
					spaceAfter = ParseSpace(val);
					found.Add(key);
				}
				else if (key.Equals("spacing") && !found.Contains(key))
				{
					spacing = ParseSpace(val);
					found.Add(key);
				}
				else if (key.Equals("font-style") && !found.Contains(key))
				{
					IsItalic = true; // presume style is 'italic'
					found.Add(key);
				}
				else if (key.Equals("font-weight") && !found.Contains(key))
				{
					IsBold = true; // presume style is 'bold'
					found.Add(key);
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


		/// <summary>
		/// Gets or sets the font size for this style, handling set values like "12.0pt"
		/// </summary>
		public override string FontSize
		{
			get => base.FontSize;
			set => base.fontSize = ParseFontSize(value);
		}


		/// <summary>
		/// Gets a Boolean value indicating if this style has specific colors,
		/// meaning either font color or background color
		/// </summary>
		public bool HasColors =>
			!string.IsNullOrEmpty(Color) && !Color.Equals(Automatic) &&
			!string.IsNullOrEmpty(Highlight) && !Highlight.Equals(Automatic);


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
		/// Gets or sets the spacing for this style, handling set values like "12.0"
		/// </summary>
		public override string Spacing
		{
			get => base.Spacing;
			set => base.spacing = ParseSpace(value);
		}


		// Methods...


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

			// normalize color as 6-byte hex HTML color string
			return ColorTranslator.FromHtml(color).ToRGBHtml();
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

			var match = Regex.Match(size, @"^([0-9]+(?:\.[0-9]+)?)(?:pt){0,1}");
			if (match.Success)
			{
				size = match.Groups[match.Groups.Count - 1].Value;
				if (!string.IsNullOrEmpty(size))
				{
					return double.Parse(size, CultureInfo.InvariantCulture);
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

			if (double.TryParse(space, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
			{
				return value;
			}

			return 0;
		}
		#endregion



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
			Spacing = other.Spacing;

			if (ApplyColors)
			{
				Color = other.Color;
				if (other.Highlight != null) Highlight = other.Highlight;
			}
		}


		/// <summary>
		/// Merge or overlay the given style propeties onto the current style
		/// </summary>
		/// <param name="properties">A collection of properties to overlay</param>
		public void Merge(Dictionary<string, string> properties)
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

			if (properties.ContainsKey("spacing"))
				Spacing = properties["spacing"];
		}


		/// <summary>
		/// Copies only color properties from the given style into this style
		/// </summary>
		/// <param name="other"></param>
		public void MergeColors(Style other)
		{
			Color = other.color;
			Highlight = other.highlight;
		}


		/// <summary>
		/// Builds a CSS string for use within a one:Page
		/// </summary>
		/// <param name="all">
		/// Include full HTML styling,
		/// otherwise include only attributes applicable to a CDATA span
		/// </param>
		/// <returns></returns>
		public string ToCss(bool all = true)
		{
			var builder = new StringBuilder();

			if (!string.IsNullOrEmpty(FontFamily) && all)
			{
				builder.Append("font-family:'" + FontFamily + "';");
			}

			if (fontSize > 0 && all)
			{
				builder.Append("font-size:" + FontSize + "pt;");
			}

			if (ApplyColors)
			{
				if (!string.IsNullOrEmpty(Color) && !Color.Equals(Automatic) && all)
					builder.Append("color:" + FormatColor(Color) + ";");

				if (!string.IsNullOrEmpty(Highlight) && !Highlight.Equals(Automatic))
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