//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Collections.Generic;


	/// <summary>
	/// Compares strings using Windows natural sort order, the same digit-run-aware
	/// comparison used by Windows Explorer, e.g. "2" sorts before "10" and
	/// "0" sorts before "00" sorts before "000".
	/// </summary>
	internal sealed class NaturalStringComparer : IComparer<string>
	{
		public static readonly NaturalStringComparer Instance = new();

		public int Compare(string x, string y)
		{
			return Native.StrCmpLogicalW(x ?? string.Empty, y ?? string.Empty);
		}
	}
}
