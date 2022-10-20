//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Drawing;


	public class TableTheme
	{
		public static Color Rainbow => ColorTranslator.FromHtml("#12345678");

		/*
		Purple	Blue	Green	Yellow	Orange	Red	    Gray
		#E5E0EC	#DEEBF6	#E2EFD9	#FFF2CC	#FBE5D5	#FADBD2	#F2F2F2
		#B2A1C7	#9CC3E5	#A8D08D	#FFD965	#F4B183	#F1937A	#BFBFBF
		#8064A2	#5B9BD5	#70AD47	#FFC000	#ED7D31	#E84C22	#A5A5A5
		*/

		public static readonly string[] LightColorNames = new string[] 
		{
			"#E5E0EC", "#DEEBF6", "#E2EFD9", "#FFF2CC", "#FBE5D5", "#FADBD2"
		};

		public static readonly string[] MediumColorNames = new string[]
		{
			"#B2A1C7", "#9CC3E5", "#A8D08D", "#FFD965", "#F4B183", "#F1937A"
		};


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
