//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.FillCellModels
{

	/// <summary>
	/// Possible data types that can be incremented in a repeatable fashion in the table
	/// fill down or fill across commands.
	/// </summary>
	internal enum FillType
	{

		/// <summary>
		/// No data or type of data was not recognized as a repeatable pattern
		/// </summary>
		None,


		/// <summary>
		/// Use for copy-down/across, hold generic cell text
		/// </summary>
		Generic,


		/// <summary>
		/// Data is a numeric value, decimal, or currency
		/// </summary>
		Number,


		/// <summary>
		/// Data is a string followed by an integer that can be incremented
		/// </summary>
		AlphaNumeric,


		/// <summary>
		/// Data is date in one of the prescribed known formats
		/// </summary>
		Date
	}
}
