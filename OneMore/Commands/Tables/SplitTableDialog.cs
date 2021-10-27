//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class SplitTableDialog : UI.LocalizableForm
	{

		public SplitTableDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.TextToTableDialog_Text;

				Localize(new string[]
				{
					"copyHeaderBox",
					"fixedColsBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public bool CopyHeader => copyHeaderBox.Checked;


		public bool FixedColumns => fixedColsBox.Checked;
	}
}