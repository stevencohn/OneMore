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

			var sagSheet = tabControl.TabPages["searchAndGoTab"].Controls[0] as SearchAndGoControl;
			sagSheet.SearchClosing += ClosingSearchAndGo;
		}


		public bool CopySelections { get; private set; }


		public List<string> SelectedPages { get; private set; }


		private void ClosingSearchAndGo(object sender, SearchCloseEventArgs e)
		{
			if (e.DialogResult == DialogResult.OK &&
				sender is SearchAndGoControl sheet)
			{
				CopySelections = sheet.CopySelections;
				SelectedPages = sheet.SelectedPages;
			}

			DialogResult = e.DialogResult;
			Close();
		}
	}
}
