//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	/// <summary>
	/// Finds text and applies a highlight color to each match, either a chosen color or the next
	/// color in the rotating highlighter sequence. Shares its dialog and search engine with
	/// <see cref="SearchAndReplaceCommand"/>.
	/// </summary>
	internal class SearchAndHighlightCommand : SearchAndReplaceCommand
	{
		protected override bool Highlighting => true;
	}
}
