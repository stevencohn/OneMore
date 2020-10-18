//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Formula
{

	internal enum FormulaDirection
	{
		Single,
		Horizontal,
		Vertical,
		Rectangular
	}


	internal enum FormulaFormat
	{
		Number,
		Currency,
		Percentage
	}


	internal enum FormulaFunction
	{
		Sum,
		Average,
		Min,
		Max,
		Range,
		Median,
		Mode,
		Variance,
		StandardDeviation
	}
}
