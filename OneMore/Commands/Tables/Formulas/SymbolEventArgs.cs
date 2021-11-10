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
	/// Arguments sent to the ProcessSymbol event handler
	/// </summary>
	internal class SymbolEventArgs : EventArgs
	{
		public string Name { get; set; }

		public double Result { get; set; }

		public SymbolStatus Status { get; set; }
	}
}