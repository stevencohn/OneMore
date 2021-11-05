//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class FitGridToTextDialog : UI.LocalizableForm
	{
		private double spacing;


		public FitGridToTextDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.FitGridToTextDialog_Text;

				Localize(new string[]
				{
					"autoButton",
					"customButton",
					"sizeLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public double Spacing => customButton.Enabled ? (double)sizeBox.Value : spacing;


		public FitGridToTextDialog(string fontSize, double spacing)
			: this()
		{
			sizeBox.Value = (decimal)spacing;
			this.spacing = spacing;

			recommendBox.Text = string.Format(
				Resx.FitGridToTextDialog_recommendation, fontSize, spacing);
		}


		private void ChangeSelection(object sender, EventArgs e)
		{
			sizeBox.Enabled = customButton.Checked;
		}
	}
}
