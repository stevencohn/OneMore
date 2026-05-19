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
			}

			DefaultControl = textSheet;
			ElevatedWithOneNote = true;
		}


		public bool CopySelections { get; private set; }


		public List<string> SelectedPages { get; private set; }


		protected override void OnShown(EventArgs e)
		{
			// base method must be called to complete the EvelatedWithOneNote procedure
			base.OnShown(e);

			textSheet.Focus();
		}


		private void ClosingSearch(object sender, SearchCloseEventArgs e)
		{
			if (e.DialogResult == DialogResult.OK)
			{
				CopySelections = textSheet.CopySelections;
				SelectedPages  = textSheet.SelectedPages;
			}

			DialogResult = e.DialogResult;
			Close();
		}
	}
}
