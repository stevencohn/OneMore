//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System.Collections.Generic;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class SearchDialog : MoreForm
	{

		public SearchDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.SearchDialog_Title;
			}

			var actSheet = tabControl.TabPages["searchAndGoTab"].Controls[0] as SearchDialogActionControl;
			actSheet.SearchClosing += ClosingSearch;

			var textSheet = tabControl.TabPages["searchTab"].Controls[0] as SearchDialogTextControl;
			textSheet.SearchClosing += ClosingSearch;
		}


		public bool CopySelections { get; private set; }


		public List<string> SelectedPages { get; private set; }


		private void ClosingSearch(object sender, SearchCloseEventArgs e)
		{
			if (e.DialogResult == DialogResult.OK &&
				sender is SearchDialogActionControl sheet)
			{
				CopySelections = sheet.CopySelections;
				SelectedPages = sheet.SelectedPages;
			}

			DialogResult = e.DialogResult;
			Close();
		}
	}
}
