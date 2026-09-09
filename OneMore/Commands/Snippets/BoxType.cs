//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	/// <summary>
	/// Describes the shading, symbol, and colors of an info/note/warn-style box inserted
	/// by InsertInfoBoxCommand, either one of the three built-in types or a user-defined
	/// custom type.
	/// </summary>
	internal class BoxType
	{
		public string Id { get; set; }

		public string Title { get; set; }

		public string Shading { get; set; }

		/// <summary>
		/// Unicode codepoint of the symbol, expressed as a hex string, e.g. "1F6C8"
		/// </summary>
		public string Symbol { get; set; }

		public string SymbolColor { get; set; }

		public int SymbolSize { get; set; } = 22;

		public string TitleColor { get; set; }

		public string TextColor { get; set; }

		/// <summary>
		/// True for the three built-in types (info, note, warn); false for user-defined types
		/// </summary>
		public bool IsBuiltin { get; set; }


		public BoxType Clone()
		{
			return new BoxType
			{
				Id = Id,
				Title = Title,
				Shading = Shading,
				Symbol = Symbol,
				SymbolColor = SymbolColor,
				SymbolSize = SymbolSize,
				TitleColor = TitleColor,
				TextColor = TextColor,
				IsBuiltin = IsBuiltin
			};
		}
	}
}
