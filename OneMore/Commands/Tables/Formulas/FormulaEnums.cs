//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{

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
