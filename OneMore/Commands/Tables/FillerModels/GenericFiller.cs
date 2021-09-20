//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.FillCellModels
{
	using River.OneMoreAddIn.Models;


	internal class GenericFiller : Filler
	{
		private readonly string value;


		public GenericFiller(TableCell cell)
			: base(cell)
		{
			value = cell.GetText(true);
		}


		public override FillType Type => FillType.Generic;


		public string Value => value;


		public override string Increment(int increment)
		{
			return value;
		}


		public override int Subtract(IFiller other)
		{
			return 0;
		}
	}
}
