//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Formula
{

	internal enum FormulaRangeType
	{
		Single,
		Columns,
		Rows,
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


	internal enum FunctionStatus
	{
		OK,
		UndefinedFunction,
		WrongParameterCount,
	}


	internal enum SymbolStatus
	{
		OK,
		None,
		UndefinedSymbol,
	}
}
