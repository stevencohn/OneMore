//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Resx = Properties.Resources;


	internal partial class CreatePagesDialog : UI.MoreForm
	{
		public CreatePagesDialog(int count)
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.CreatePagesDialog_Text;

				Localize(new string[]
				{
					"linksBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			messageLabel.Text = string.Format(Resx.CreatePagesCommand_CreatePages, count);
		}


		public bool CreateLinks => linksBox.Checked;
	}
}
