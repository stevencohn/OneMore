//************************************************************************************************
// Copyright © 2026 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System.Collections.Generic;
	using System.Globalization;


	/// <summary>
	/// Splits a string into alternating runs of plain text and emoji so each kind can be
	/// drawn with a different font; GDI+ has no glyphs for most emoji in the default UI font.
	/// </summary>
	internal static class EmojiRuns
	{
		// codepoints that never start or end a run on their own; they're always absorbed
		// into whatever run is currently open, keeping multi-codepoint sequences (skin tone
		// modifiers, flags, ZWJ family/profession emoji, keycaps) as a single atomic run
		private const int ZeroWidthJoiner = 0x200D;
		private const int VariationSelector15 = 0xFE0E;
		private const int VariationSelector16 = 0xFE0F;
		private const int CombiningEnclosingKeycap = 0x20E3;


		/// <summary>
		/// Splits the given title into runs of consecutive emoji or non-emoji codepoints.
		/// </summary>
		/// <param name="title">The string to split</param>
		/// <returns>An ordered list of runs; a title with no emoji yields exactly one entry</returns>
		internal static IReadOnlyList<(string Text, bool IsEmoji)> Split(string title)
		{
			var runs = new List<(string Text, bool IsEmoji)>();

			if (string.IsNullOrEmpty(title))
			{
				runs.Add((title ?? string.Empty, false));
				return runs;
			}

			var start = 0;
			var runIsEmoji = false;
			var runStarted = false;

			var i = 0;
			while (i < title.Length)
			{
				var width = char.IsSurrogatePair(title, i) ? 2 : 1;
				var codepoint = width == 2 ? char.ConvertToUtf32(title, i) : title[i];

				var isGlue = IsGlue(codepoint);
				var isEmoji = !isGlue && IsEmojiCodepoint(title, i, codepoint, width);

				if (!runStarted)
				{
					runIsEmoji = isEmoji;
					runStarted = true;
				}
				else if (!isGlue && isEmoji != runIsEmoji)
				{
					runs.Add((title.Substring(start, i - start), runIsEmoji));
					start = i;
					runIsEmoji = isEmoji;
				}

				i += width;
			}

			runs.Add((title.Substring(start), runIsEmoji));
			return runs;
		}


		private static bool IsGlue(int codepoint)
		{
			return codepoint is ZeroWidthJoiner or VariationSelector15 or VariationSelector16
				or CombiningEnclosingKeycap;
		}


		private static bool IsEmojiCodepoint(string title, int index, int codepoint, int width)
		{
			if (CharUnicodeInfo.GetUnicodeCategory(title, index) == UnicodeCategory.OtherSymbol)
			{
				return true;
			}

			// keycap base: a digit, '#', or '*' immediately followed by U+20E3
			if ((codepoint is >= '0' and <= '9' or '#' or '*') &&
				index + width < title.Length &&
				char.ConvertToUtf32(title, index + width) == CombiningEnclosingKeycap)
			{
				return true;
			}

			return false;
		}
	}
}
