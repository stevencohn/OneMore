//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.FillCellModels
{
	using River.OneMoreAddIn.Models;


	/// <summary>
	/// Common members of fillers used to fill down or fill across cells in a table
	/// </summary>
	internal interface IFiller
	{

		/// <summary>
		/// Get the table cell wrapped by this filler
		/// </summary>
		TableCell Cell { get; }


		/// <summary>
		/// Get the data type of this filler
		/// </summary>
		FillType Type { get; }


		/// <summary>
		/// Increments the value of the filler and returns a formatted string
		/// </summary>
		/// <param name="increment">The increment value, can be negative</param>
		/// <returns>A string of the value formatted according to the filler rules</returns>
		string Increment(int increment);


		/// <summary>
		/// Subtracts the value of the given filler from the value of the current filler
		/// to calculate an integer amount used to increment values.
		/// </summary>
		/// <param name="other">
		/// Another filler of the same type to subtract from the current filler
		/// </param>
		/// <returns>An integer value used to increment</returns>
		int Subtract(IFiller other);
	}
}
