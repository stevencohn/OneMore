//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using Snippets.Toc.Generators;
	using System;
	using Resx = Properties.Resources;


	internal partial class InsertNotebookTocDialog : MoreForm
	{
		private readonly TocParameters parameters;


		public InsertNotebookTocDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.InsertNotebookTocDialog_Text;

				Localize(new string[]
				{
					"pagesBox",
					"previewBox=InsertNotebookTocDialog_previewBox",
					"timeBox=InsertNotebookTocDialog_timeBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public InsertNotebookTocDialog(TocParameters parameters)
			: this()
		{
			this.parameters = parameters;

			pagesBox.Checked = parameters.Contains("pages");
			previewBox.Checked = parameters.Contains("preview");
			timeBox.Checked = parameters.Contains("time");
		}


		private void TogglePagesBox(object sender, EventArgs e)
		{
			if (pagesBox.Checked)
			{
				previewBox.Enabled = true;
			}
			else
			{
				previewBox.Enabled = previewBox.Checked = false;
			}
		}


		private void CollectParametersOnOK(object sender, EventArgs e)
		{
			parameters.Clear();

			parameters.Add("notebook");
			if (pagesBox.Checked) parameters.Add("pages");
			if (previewBox.Checked) parameters.Add("preview");
			if (timeBox.Checked) parameters.Add("time");
		}
	}
}
