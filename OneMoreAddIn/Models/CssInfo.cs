//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

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
	}
}