//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.FillCellModels
{

	internal interface IFiller
	{
		FillType Type { get; }


		bool CanParse(string text);


		string Increment(int increment);
	}
}
