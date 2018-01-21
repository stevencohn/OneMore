//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

using System.Text;

namespace River.OneMoreAddIn
{
	internal class CssInfo : StyleInfoBase
	{
		public CssInfo ()
		{
		}


		public CssInfo (string css)
		{
			Collect(css);
		}


		public override string FontSize
		{
			get => base.FontSize;
			set => base.FontSize = FormatSize(value);
		}


		public void Collect (string css)
		{
			var parts = css.Split(';');

			for (int i = 0; i < parts.Length; i++)
			{
				var part = parts[i];

				var pair = part.Split(':');
				if (pair.Length < 2) continue;

				var key = pair[0].Trim();
				var val = pair[1].Trim();

				if (key.Equals("font-family") && (FontFamily == null))
				{
					FontFamily = FormatFontFamily(val);
				}
				else if (key.Equals("font-size") && (FontSize == null))
				{
					FontSize = FormatSize(val);
				}
				else if (key.Equals("color") && (Color == null))
				{
					Color = FormatColor(val);
				}
				else if (key.Equals("background") && (Highlight == null))
				{
					Highlight = FormatColor(val);
				}
				else if (key.Equals("spaceBefore") && (SpaceBefore == null))
				{
					SpaceBefore = FormatSpace(val);
				}
				else if (key.Equals("spaceAfter") && (SpaceAfter == null))
				{
					SpaceAfter = FormatSpace(val);
				}
				else if (key.Equals("font-style") && (IsBold == null))
				{
					IsItalic = true; // presume style is 'italic'
				}
				else if (key.Equals("font-weight") && (IsBold == null))
				{
					IsBold = true; // presume style is 'bold'
				}
				else if (key.Equals("text-decoration"))
				{
					if (!IsUnderline == null)
					{
						IsUnderline = val.Contains("underline");
					}

					if (IsStrikethrough == null)
					{
						IsStrikethrough = val.Contains("line-through");
					}
				}
				else if (key.Equals("vertical-align"))
				{
					if ((IsSuperscript == null) && val.Equals("super"))
					{
						IsSuperscript = true;
					}
					else if ((IsSubscript == null) && val.Equals("sub"))
					{
						IsSubscript = true;
					}
				}
			}
		}

		public string ToCss ()
		{
			var builder = new StringBuilder();

			if (FontFamily != null) builder.Append("font-family:" + FontFamily + ";");
			if (FontSize != null) builder.Append("font-size:" + FontSize + ";");
			if (IsBold == true) builder.Append("font-weight:bold;");
			if (IsItalic == true) builder.Append("font-style:italic;");

			if (Color != null) builder.Append("color:" + Color + ";");
			if (Highlight != null) builder.Append("background:" + Highlight + ";");

			if ((IsUnderline == true) || (IsStrikethrough == true))
			{
				builder.Append("text-decoration:");
				if (IsUnderline == true) builder.Append("underline");
				if (IsStrikethrough == true) builder.Append(" line-through");
				builder.Append(";");
			}

			if (IsSuperscript == true)
			{
				builder.Append("vertical-align:super;");
			}
			else if (IsSubscript == true)
			{
				builder.Append("vertical-align:sub;");
			}

			return builder.ToString();
		}
	}
}