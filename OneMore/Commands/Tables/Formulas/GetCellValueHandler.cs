//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using System;


	/// <summary>
	/// Declares the delegate definition used by the Calculator to ask for the
	/// value of a table cell specified by its event args.
	/// </summary>
	/// <param name="sender">The Calculator instance</param>
	/// <param name="e">The event args used to indicate the cell address and contain its value</param>
	internal delegate void GetCellValueHandler(object sender, GetCellValueEventArgs e);


	/// <summary>
	/// The argument passed to the GetCellValueHandler delegate. The callback should populate
	/// the Value property with the value stored in the specified cell
	/// </summary>
	internal class GetCellValueEventArgs : EventArgs
	{

		/// <summary>
		/// Initializes a new instance for the specified cell name.
		/// </summary>
		/// <param name="name"></param>
		public GetCellValueEventArgs(string name)
		{
			Name = name;
		}


		/// <summary>
		/// Gets the cell name, equivalent to its spreadsheet address, e.g. "B2"
		/// </summary>
		public string Name { get; private set; }


		/// <summary>
		/// Gets or sets the value stored in the named table cell.
		/// </summary>
		public string Value { get; set; }
	}
}
