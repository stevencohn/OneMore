//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{

	internal class Coordinates
	{
		public Coordinates(int col, int row)
		{
			ColNumber = col;
			RowNumber = row;
		}


		public int ColNumber { get; private set; }


		public int RowNumber { get; private set; }
	}
}
