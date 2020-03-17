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

		/// <summary>
		/// Used by InsertTocCommand to keep track of TOC indent level...
		/// Move to QuickInfo?
		/// </summary>
		int Level { get; set; }

		/// <summary>
		/// Used by InsertTocCommand to match styles against internal QuickInfos...
		/// Move to QuickInfo?
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		bool Matches (object obj);
	}
}