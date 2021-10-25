//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class ArrangeContainersDialog : UI.LocalizableForm
	{
		public ArrangeContainersDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ArrangeContainersDialog_Text;

				Localize(new string[]
				{
					"verticalButton",
					"flowButton",
					"columnsLabel",
					"widthLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public bool Vertical => verticalButton.Checked;


		public int Columns => (int)columnsBox.Value;


		public int PageWidth => (int)widthBox.Value;


		private void ChangeSelection(object sender, System.EventArgs e)
		{
			columnsBox.Enabled = widthBox.Enabled = flowButton.Checked;
		}
	}
}
