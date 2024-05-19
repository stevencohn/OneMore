//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Resx = Properties.Resources;


	internal partial class SortListDialog : UI.MoreForm
	{
		public SortListDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.SortListDialog_Text;

				Localize(new string[]
				{
					"introLabel",
					"thisListButton",
					"allListsButton",
					"deepBox",
					"typeBox",
					"duplicatesBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public bool IncludeAllLists => allListsButton.Checked;


		public bool IncludeChildLists => deepBox.Checked;


		public bool IncludeNumberedLists => typeBox.Checked;


		public bool RemoveDuplicates => duplicatesBox.Checked;


		private void LoadForm(object sender, System.EventArgs e)
		{
			ActiveControl = okButton;
		}
	}
}
