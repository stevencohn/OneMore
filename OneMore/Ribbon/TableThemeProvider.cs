//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Collections.Generic;
	using System.Drawing;


	internal class TableThemeProvider
	{
		private readonly List<TableTheme> themes;


		public TableThemeProvider()
		{
			themes = new List<TableTheme>
			{
				/*
				Purple	Blue	Green	Yellow	Orange	Red	    Gray
				#E5E0EC	#DEEBF6	#E2EFD9	#FFF2CC	#FBE5D5	#FADBD2	#F2F2F2
				#B2A1C7	#9CC3E5	#A8D08D	#FFD965	#F4B183	#F1937A	#BFBFBF
				#8064A2	#5B9BD5	#70AD47	#FFC000	#ED7D31	#E84C22	#A5A5A5
				 */

				// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
				// white-purple rows
				new TableTheme
				{
					FirstRowStripe = Color.Transparent,
					SecondRowStripe = ColorTranslator.FromHtml("#E5E0EC")
				},
				// white-blue rows
				new TableTheme
				{
					FirstRowStripe = Color.Transparent,
					SecondRowStripe = ColorTranslator.FromHtml("#DEEBF6")
				},
				// white-green rows
				new TableTheme
				{
					FirstRowStripe = Color.Transparent,
					SecondRowStripe = ColorTranslator.FromHtml("#E2EFD9")
				},
				// white-yellow rows
				new TableTheme
				{
					FirstRowStripe = Color.Transparent,
					SecondRowStripe = ColorTranslator.FromHtml("#FFF2CC")
				},
				// white-orange rows
				new TableTheme
				{
					FirstRowStripe = Color.Transparent,
					SecondRowStripe = ColorTranslator.FromHtml("#FBE5D5")
				},
				// white-red rows
				new TableTheme
				{
					FirstRowStripe = Color.Transparent,
					SecondRowStripe = ColorTranslator.FromHtml("#FADBD2")
				},
				// white-gray rows
				new TableTheme
				{
					FirstRowStripe = Color.Transparent,
					SecondRowStripe = ColorTranslator.FromHtml("#F2F2F2")
				},

				// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
				// purple-purple rows
				new TableTheme
				{
					FirstRowStripe = ColorTranslator.FromHtml("#E5E0EC"),
					SecondRowStripe = ColorTranslator.FromHtml("#B2A1C7")
				},
				// blue-blue rows
				new TableTheme
				{
					FirstRowStripe = ColorTranslator.FromHtml("#DEEBF6"),
					SecondRowStripe = ColorTranslator.FromHtml("#9CC3E5")
				},
				// green-green rows
				new TableTheme
				{
					FirstRowStripe = ColorTranslator.FromHtml("#E2EFD9"),
					SecondRowStripe = ColorTranslator.FromHtml("#A8D08D")
				},
				// yellow-yellow rows
				new TableTheme
				{
					FirstRowStripe = ColorTranslator.FromHtml("#FFF2CC"),
					SecondRowStripe = ColorTranslator.FromHtml("#FFD965")
				},
				// orange-orange rows
				new TableTheme
				{
					FirstRowStripe = ColorTranslator.FromHtml("#FBE5D5"),
					SecondRowStripe = ColorTranslator.FromHtml("#F4B183")
				},
				// red-red rows
				new TableTheme
				{
					FirstRowStripe = ColorTranslator.FromHtml("#FADBD2"),
					SecondRowStripe = ColorTranslator.FromHtml("#F1937A")
				},
				// gray-gray rows
				new TableTheme
				{
					FirstRowStripe = ColorTranslator.FromHtml("#F2F2F2"),
					SecondRowStripe = ColorTranslator.FromHtml("#BFBFBF")
				},

				// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
				// purple-purple-header rows
				new TableTheme
				{
					HeaderRow = ColorTranslator.FromHtml("#8064A2"),
					SecondRowStripe = ColorTranslator.FromHtml("#E5E0EC"),
					FirstRowStripe = ColorTranslator.FromHtml("#B2A1C7")
				},
				// blue-blue-header rows
				new TableTheme
				{
					HeaderRow = ColorTranslator.FromHtml("#5B9BD5"),
					SecondRowStripe = ColorTranslator.FromHtml("#DEEBF6"),
					FirstRowStripe = ColorTranslator.FromHtml("#9CC3E5")
				},
				// green-green-header rows
				new TableTheme
				{
					HeaderRow = ColorTranslator.FromHtml("#70AD47"),
					SecondRowStripe = ColorTranslator.FromHtml("#E2EFD9"),
					FirstRowStripe = ColorTranslator.FromHtml("#A8D08D")
				},
				// yellow-yellow-header rows
				new TableTheme
				{
					HeaderRow = ColorTranslator.FromHtml("#FFC000"),
					SecondRowStripe = ColorTranslator.FromHtml("#FFF2CC"),
					FirstRowStripe = ColorTranslator.FromHtml("#FFD965")
				},
				// orange-orange-header rows
				new TableTheme
				{
					HeaderRow = ColorTranslator.FromHtml("#ED7D31"),
					FirstColumn = ColorTranslator.FromHtml("#ED7D31"),
					SecondRowStripe = ColorTranslator.FromHtml("#FBE5D5"),
					FirstRowStripe = ColorTranslator.FromHtml("#F4B183")
				},
				// red-red-header rows
				new TableTheme
				{
					HeaderRow = ColorTranslator.FromHtml("#E84C22"),
					FirstColumn = ColorTranslator.FromHtml("#E84C22"),
					FirstRowStripe = ColorTranslator.FromHtml("#F1937A"),
					SecondRowStripe = ColorTranslator.FromHtml("#FADBD2")
				},
				// gray-gray-header rows
				new TableTheme
				{
					HeaderRow = ColorTranslator.FromHtml("#A5A5A5"),
					FirstColumn = ColorTranslator.FromHtml("#A5A5A5"),
					FirstRowStripe = ColorTranslator.FromHtml("#BFBFBF"),
					SecondRowStripe = ColorTranslator.FromHtml("#F2F2F2")
				},

				// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
				// purple-purple-header-col rows
				new TableTheme
				{
					HeaderRow = ColorTranslator.FromHtml("#8064A2"),
					FirstColumn = ColorTranslator.FromHtml("#8064A2"),
					FirstRowStripe = ColorTranslator.FromHtml("#B2A1C7"),
					SecondRowStripe = ColorTranslator.FromHtml("#E5E0EC")
				},
				// blue-blue-header-col rows
				new TableTheme
				{
					HeaderRow = ColorTranslator.FromHtml("#5B9BD5"),
					FirstColumn = ColorTranslator.FromHtml("#5B9BD5"),
					FirstRowStripe = ColorTranslator.FromHtml("#9CC3E5"),
					SecondRowStripe = ColorTranslator.FromHtml("#DEEBF6")
				},
				// green-green-header-col rows
				new TableTheme
				{
					HeaderRow = ColorTranslator.FromHtml("#70AD47"),
					FirstColumn = ColorTranslator.FromHtml("#70AD47"),
					FirstRowStripe = ColorTranslator.FromHtml("#A8D08D"),
					SecondRowStripe = ColorTranslator.FromHtml("#E2EFD9")
				},
				// yellow-yellow-header-col rows
				new TableTheme
				{
					HeaderRow = ColorTranslator.FromHtml("#FFC000"),
					FirstColumn = ColorTranslator.FromHtml("#FFC000"),
					FirstRowStripe = ColorTranslator.FromHtml("#FFD965"),
					SecondRowStripe = ColorTranslator.FromHtml("#FFF2CC")
				},
				// orange-orange-header-col rows
				new TableTheme
				{
					HeaderRow = ColorTranslator.FromHtml("#ED7D31"),
					FirstColumn = ColorTranslator.FromHtml("#ED7D31"),
					FirstRowStripe = ColorTranslator.FromHtml("#F4B183"),
					SecondRowStripe = ColorTranslator.FromHtml("#FBE5D5")
				},
				// red-red-header-col rows
				new TableTheme
				{
					HeaderRow = ColorTranslator.FromHtml("#E84C22"),
					FirstColumn = ColorTranslator.FromHtml("#E84C22"),
					FirstRowStripe = ColorTranslator.FromHtml("#F1937A"),
					SecondRowStripe = ColorTranslator.FromHtml("#FADBD2")
				},
				// gray-gray-header-col rows
				new TableTheme
				{
					HeaderRow = ColorTranslator.FromHtml("#A5A5A5"),
					FirstColumn = ColorTranslator.FromHtml("#A5A5A5"),
					SecondRowStripe = ColorTranslator.FromHtml("#F2F2F2"),
					FirstRowStripe = ColorTranslator.FromHtml("#BFBFBF")
				}
			};
		}


		public int Count => themes.Count;

		public TableTheme GetTheme(int index)
		{
			return themes[index];
		}

		public string GetName(int index)
		{
			return index.ToString();
		}
	}
}
