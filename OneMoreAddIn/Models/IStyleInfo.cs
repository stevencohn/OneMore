//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	internal interface IStyleInfo
	{
		// Name tracks CustomStyle/Style[@name]
		string Name { get; set; }

		// Index tracks QuickStyleDef[@index]
		string Index { get; set; }

		string FontFamily { get; set; }
		string FontSize { get; set; }
		bool? IsBold { get; set; }
		bool? IsItalic { get; set; }
		bool? IsUnderline { get; set; }
		bool? IsStrikethrough { get; set; }
		bool? IsSuperscript { get; set; }
		bool? IsSubscript { get; set; }
		string Color { get; set; }
		string Highlight { get; set; }
		string SpaceBefore { get; set; }
		string SpaceAfter { get; set; }
		bool? IsHeading { get; set; }

		int Level { get; set; }

		bool Matches (object obj);
	}
}