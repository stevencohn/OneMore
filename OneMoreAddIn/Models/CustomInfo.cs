//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Xml.Linq;

	internal class CustomInfo : StyleInfoBase
	{
		public CustomInfo (XElement template)
		{
			Name = template.Attribute("name")?.Value;
			FontFamily = FormatFontFamily(template.Attribute("fontFamily")?.Value);
			FontSize = FormatSize(template.Attribute("fontSize")?.Value);

			var style = template.Attribute("fontStyle")?.Value;
			if (style != null)
			{
				if (style.Contains("Bold")) IsBold = true;
				if (style.Contains("Italic")) IsItalic = true;
				if (style.Contains("Underline")) IsItalic = true;
				if (style.Contains("Strikeout")) IsStrikethrough = true;
			}

			IsSuperscript = template.Attribute("isSuperscript")?.Value.Equals("true");
			IsSubscript = template.Attribute("isSubscript")?.Value.Equals("true");

			Color = FormatColor(template.Attribute("color")?.Value.ToLower());
			Highlight = FormatColor(template.Attribute("background")?.Value.ToLower());

			SpaceAfter = FormatSpace(template.Attribute("spaceAfter")?.Value);
			SpaceBefore = FormatSpace(template.Attribute("spaceBefore")?.Value);

			IsHeading = template.Attribute("isHeading")?.Value.Equals("true");
		}
	}
}
