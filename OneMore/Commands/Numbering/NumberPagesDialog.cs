//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant
#pragma warning disable IDE1006 // Words must begin with upper case

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class NumberPagesDialog : UI.LocalizableForm
	{
		public NumberPagesDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.NumberPagesDialog_Text;

				Localize(new string[]
				{
					"numberingGroup",
					"introLabel",
					"alphaRadio",
					"numRadio",
					"cleanBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public bool AlphaNumbering => alphaRadio.Checked;

		public bool NumericNumbering => numRadio.Checked;

		public bool CleanupNumbering => cleanBox.Checked;


		private void okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}


		private void cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}
}
