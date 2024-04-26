//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class FitGridToTextDialog : UI.MoreForm
	{
		private readonly double spacing;


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


		public FitGridToTextDialog(string fontSize, double spacing)
			: this()
		{
			this.spacing = Math.Round(spacing, 2);
			sizeBox.Value = (decimal)this.spacing;

			recommendBox.Text = string.Format(
				Resx.FitGridToTextDialog_recommendation, fontSize, this.spacing);
		}


		public double Spacing => customButton.Enabled ? (double)sizeBox.Value : spacing;


		private void ChangeSelection(object sender, EventArgs e)
		{
			sizeBox.Enabled = customButton.Checked;
		}
	}
}
