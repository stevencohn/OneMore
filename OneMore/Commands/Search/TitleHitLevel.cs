//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Drawing;


	/// <summary>
	/// The hierarchy level at which a Search Titles hit was found.
	/// </summary>
	internal enum TitleHitLevel
	{
		Notebook,
		SectionGroup,
		Section,
		Page
	}


	/// <summary>
	/// Fixed, theme-aware colors used to badge a Search Titles hit by its TitleHitLevel,
	/// both for the small type-chip glyph and the card's left accent bar.
	/// </summary>
	internal static class TitleHitPalette
	{
		/// <summary>
		/// Short, non-localized glyph shown in the type-chip badge.
		/// </summary>
		public static string GetCode(TitleHitLevel level) => level switch
		{
			TitleHitLevel.Notebook => "N",
			TitleHitLevel.SectionGroup => "SG",
			TitleHitLevel.Section => "S",
			TitleHitLevel.Page => "P",
			_ => "?"
		};


		/// <summary>
		/// Text color for the glyph drawn inside the chip badge. Dark mode uses lighter, more
		/// pastel badge fills (to read against a dark card), which need dark glyph text; light
		/// mode uses more saturated fills, which need light glyph text.
		/// </summary>
		public static Color GetGlyphColor(bool darkMode) =>
			darkMode ? Color.FromArgb(32, 32, 32) : Color.White;


		/// <summary>
		/// Fill color for the chip badge and the card's left accent bar.
		/// </summary>
		public static Color GetColor(TitleHitLevel level, bool darkMode) => level switch
		{
			TitleHitLevel.Notebook => darkMode
				? Color.FromArgb(214, 138, 60)
				: Color.FromArgb(181, 106, 32),

			TitleHitLevel.SectionGroup => darkMode
				? Color.FromArgb(178, 130, 224)
				: Color.FromArgb(124, 77, 168),

			TitleHitLevel.Section => darkMode
				? Color.FromArgb(96, 165, 224)
				: Color.FromArgb(37, 110, 168),

			TitleHitLevel.Page => darkMode
				? Color.FromArgb(110, 190, 120)
				: Color.FromArgb(58, 130, 68),

			_ => Color.Gray
		};
	}
}
