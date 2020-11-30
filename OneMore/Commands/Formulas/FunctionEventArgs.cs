//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************
/*
 * Based on the C# Expression Evaluator by Jonathan Wood, 2010
 */

namespace River.OneMoreAddIn.Commands.Formulas
{
	using System;
	using System.Collections.Generic;


	/// <summary>
	/// Arguments sent to the ProcessFunction event handler
	/// </summary>
	internal class FunctionEventArgs : EventArgs
	{
		public string Name { get; set; }

		public List<double> Parameters { get; set; }

		public double Result { get; set; }

		public FunctionStatus Status { get; set; }
	}
}