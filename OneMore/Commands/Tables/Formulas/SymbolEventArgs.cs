//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************
/*
 * Based on the C# Expression Evaluator by Jonathan Wood, 2010
 */

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using System;


	/// <summary>
	/// Arguments sent to the ProcessSymbol event handler.
	/// </summary>
	internal class SymbolEventArgs : EventArgs
	{
		public SymbolEventArgs(string name)
		{
			Name = name;
			Status = SymbolStatus.OK;
		}

	
		/// <summary>
		/// Specifies the name of the symbol to be resolved by the ProcessSymbol event handler.
		/// </summary>
		public string Name { get; set; }


		/// <summary>
		/// The Result cast to a Double value.
		/// </summary>
		public double DoubleResult { get => (double)Result; }


		/// <summary>
		/// The value resolved from the name, set by one of the setters.
		/// </summary>
		public object Result { get; private set; }


		/// <summary>
		/// Indicates the data type of the Result after it is set by one of the setters.
		/// </summary>
		public FormulaValueType Type { get; private set; }


		/// <summary>
		/// Gets or sets the status of the resolve.
		/// </summary>
		public SymbolStatus Status { get; set; }


		/// <summary>
		/// Stores a boolean value in Result
		/// </summary>
		/// <param name="value"></param>
		public void SetResult(bool value)
		{
			Result = value;
			Type = FormulaValueType.Boolean;
		}


		/// <summary>
		/// Stores a double value in Result
		/// </summary>
		/// <param name="value"></param>
		public void SetResult(double value)
		{
			Result = value;
			Type = FormulaValueType.Double;
		}


		/// <summary>
		/// Stores an int as a double value in Result
		/// </summary>
		/// <param name="value"></param>
		public void SetResult(int value)
		{
			Result = (double)value;
			Type = FormulaValueType.Double;
		}


		/// <summary>
		/// Stores a string value in Result
		/// </summary>
		/// <param name="value"></param>
		public void SetResult(string value)
		{
			Result = value;
			Type = FormulaValueType.String;
		}
	}
}