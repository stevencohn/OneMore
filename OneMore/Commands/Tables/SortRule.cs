//************************************************************************************************
// Copyright © 2026 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	internal enum SortDataType
	{
		Auto,
		Text,
		DateTime,
		Number
	}


	/// <summary>
	/// Describes one level of a multi-column table sort.
	/// </summary>
	internal class SortRule
	{
		public int ColumnIndex { get; set; }
		public bool Descending { get; set; }
		public SortDataType DataType { get; set; }
	}
}
