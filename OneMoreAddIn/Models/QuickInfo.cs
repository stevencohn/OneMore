//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Xml.Linq;


	internal class QuickInfo : StyleInfoBase
	{

		public QuickInfo (XElement element)
		{
			XAttribute a;

			if (Name == null)
			{
				a = element.Attribute("name");
				if (a != null)
				{
					Name = a.Value;
				}
			}

			if (Index == null)
			{
				a = element.Attribute("index");
				if (a != null)
				{
					Index = a.Value;
				}
			}

			if (FontFamily == null)
			{
				a = element.Attribute("font");
				if (a != null)
				{
					FontFamily = FormatFontFamily(a.Value);
				}
			}

			if (FontSize == null)
			{
				a = element.Attribute("fontSize");
				if (a != null)
				{
					FontSize = FormatSize(a.Value);
				}
			}

			if (IsBold == null)
			{
				a = element.Attribute("bold");
				if (a != null)
				{
					IsBold = a.Value.Equals("True", StringComparison.InvariantCultureIgnoreCase);
				}
			}

			if (IsItalic == null)
			{
				a = element.Attribute("italic");
				if (a != null)
				{
					IsItalic = a.Value.Equals("True", StringComparison.InvariantCultureIgnoreCase);
				}
			}

			if (IsUnderline == null)
			{
				a = element.Attribute("underline");
				if (a != null)
				{
					IsUnderline = a.Value.Equals("True", StringComparison.InvariantCultureIgnoreCase);
				}
			}

			if (IsStrikethrough == null)
			{
				a = element.Attribute("strikethrough");
				if (a != null)
				{
					IsStrikethrough = a.Value.Equals("True", StringComparison.InvariantCultureIgnoreCase);
				}
			}

			if (IsSubscript == null)
			{
				a = element.Attribute("subscript");
				if (a != null)
				{
					IsSubscript = a.Value.Equals("True", StringComparison.InvariantCultureIgnoreCase);
				}
			}

			if (IsSuperscript == null)
			{
				a = element.Attribute("superscript");
				if (a != null)
				{
					IsSuperscript = a.Value.Equals("True", StringComparison.InvariantCultureIgnoreCase);
				}
			}

			if (Color == null)
			{
				a = element.Attribute("fontColor");
				if (a != null)
				{
					Color = FormatColor(a.Value.ToLower());
				}
			}

			if (Highlight == null)
			{
				a = element.Attribute("highlightColor");
				if (a != null)
				{
					Highlight = FormatColor(a.Value.ToLower());
				}
			}

			if (SpaceAfter == null)
			{
				a = element.Attribute("spaceAfter");
				if (a != null)
				{
					SpaceAfter = FormatSpace(a.Value);
				}
			}

			if (SpaceBefore == null)
			{
				a = element.Attribute("spaceBefore");
				if (a != null)
				{
					SpaceBefore = FormatSpace(a.Value);
				}
			}
		}
	}
}
