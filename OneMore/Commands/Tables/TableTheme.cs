//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Drawing;


	public class TableTheme
	{
		public static Color Rainbow => ColorTranslator.FromHtml("#12345678");


		// styles are applied in this order of the properties below
		// where lower styles override upper styles

		// *** DO NOT RE-ORDER THESE PROPERTIES ***

		public Color WholeTable { get; set; }

		public Color FirstColumnStripe { get; set; }

		public Color SecondColumnStripe { get; set; }

		public Color FirstRowStripe { get; set; }

		public Color SecondRowStripe { get; set; }

		public Color FirstColumn { get; set; }

		public Color LastColumn { get; set; }

		public Color HeaderRow { get; set; }

		public Color TotalRow { get; set; }

		public Color HeaderFirstCell { get; set; }

		public Color HeaderLastCell { get; set; }

		public Color TotalFirstCell { get; set; }

		public Color TotalLastCell { get; set; }
	}
}
