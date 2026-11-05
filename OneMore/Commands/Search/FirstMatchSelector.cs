//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;


	/// <summary>
	/// Selects the first match in each paragraph. OneNote supports only one contiguous
	/// selection range per paragraph, so any further matches in a paragraph are ignored.
	/// </summary>
	internal static class FirstMatchSelector
	{
		/// <summary>
		/// Selects the first match of each paragraph by splitting its run.
		/// </summary>
		/// <param name="matches">Matches in document order, from FindAllMatches</param>
		/// <returns>The matches that were selected, in document order</returns>
		public static List<MatchInfo> Select(IEnumerable<MatchInfo> matches)
		{
			var selected = new List<MatchInfo>();
			var seen = new HashSet<string>();

			foreach (var match in matches)
			{
				if (string.IsNullOrEmpty(match.ParagraphObjectId) ||
					seen.Contains(match.ParagraphObjectId))
				{
					continue;
				}

				// splitting a run affects only its own paragraph, so the remaining
				// MatchInfos, which are all in other paragraphs, stay valid
				if (RunSplitter.Split(match) is not null)
				{
					seen.Add(match.ParagraphObjectId);
					selected.Add(match);
				}
			}

			return selected;
		}
	}
}
