//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant
#pragma warning disable IDE1006 // Words must begin with upper case

namespace River.OneMoreAddIn.Commands
{
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class BreakingDialog : UI.LocalizableForm
	{
		public BreakingDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.BreakingDialog_Text;

				Localize(new string[]
				{
					"groupBox",
					"oneButton",
					"twoButton",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public bool SingleSpace => oneButton.Checked;
	}
}
