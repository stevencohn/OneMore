//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************
/*
 * Based on the C# Expression Evaluator by Jonathan Wood, 2010
 */

#pragma warning disable CA1032 // Implement standard exception constructors
#pragma warning disable CA2237 // Mark ISerializable types with serializable
#pragma warning disable S3871 // Exception types should be "public"

namespace River.OneMoreAddIn.Commands.Formulas
{
	using System;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	/// <summary>
	/// Custom exception for formula evaluation errors
	/// </summary>
	internal class FormulaException : Exception
	{

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message">Message that describes this exception</param>
		/// <param name="position">Position within expression where exception occurred</param>
		public FormulaException(string message, int position) : base(message)
		{
			Column = position;
		}


		/// <summary>
		/// Zero-based position in expression where exception occurred
		/// </summary>
		public int Column { get; set; }


		/// <summary>
		/// Gets the message associated with this exception
		/// </summary>
		public override string Message =>
			string.Format(Resx.FormulaCommand_Exception, base.Message, Column + 1);
	}
}