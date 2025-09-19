//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Search
{
	using River.OneMoreAddIn.UI;
	using System.Collections.Generic;
	using Resx = Properties.Resources;


	internal partial class SearchDialog : MoreForm
	{
		private readonly OneNote one;


		public SearchDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.SearchDialog_Title;
			}

			SelectedPages = new List<string>();
		}


		public bool CopySelections { get; private set; }


		public List<string> SelectedPages { get; private set; }
	}
}
