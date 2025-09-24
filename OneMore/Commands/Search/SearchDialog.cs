//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
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
				Text = Resx.word_Search;

				Localize(new string[]
				{
					"searchTab=word_Search",
					"searchAndGoTab"
				});
			}

			DefaultControl = searchTab;
			ElevatedWithOneNote = true;
		}


		public bool CopySelections { get; private set; }


		public List<string> SelectedPages { get; private set; }


		protected override void OnShown(EventArgs e)
		{
			var textSheet = tabControl.TabPages["searchTab"].Controls[0] as SearchDialogTextControl;
			textSheet.Focus();
		}


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
