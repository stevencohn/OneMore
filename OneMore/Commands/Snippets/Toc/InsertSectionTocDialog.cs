//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using Snippets.Toc;
	using System;
	using Resx = Properties.Resources;


	internal partial class InsertSectionTocDialog : MoreForm
	{
		private readonly TocParameters parameters;


		public InsertSectionTocDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.InsertSectionTocDialog_Text;

				Localize(new string[]
				{
					"previewBox=InsertSectionTocDialog_previewBox",
					"timeBox=InsertSectionTocDialog_timeBox",
					"headingsBox",
					"levelsLabel=InsertPageTocDialog_levelsLabel.Text",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public InsertSectionTocDialog(TocParameters parameters)
			: this()
		{
			this.parameters = parameters;

			previewBox.Checked = parameters.Contains("preview");
			timeBox.Checked = parameters.Contains("time");

			levelsBox.Value = 6;
			if (parameters.Find(p => p.StartsWith("level")) is string level &&
				int.TryParse(level.Substring(5), out var value))
			{
				levelsBox.Enabled = true;
				levelsBox.Value = value;
				headingsBox.Checked = true;
			}
		}


		private void ToggleHeadings(object sender, EventArgs e)
		{
			levelsBox.Enabled = headingsBox.Checked;
		}


		private void CollectParametersOnOK(object sender, EventArgs e)
		{
			parameters.Clear();

			parameters.Add("section");
			if (previewBox.Checked) parameters.Add("preview");
			if (timeBox.Checked) parameters.Add("time");
			if (headingsBox.Checked)
			{
				parameters.Add($"level{levelsBox.Value}");
			}
		}
	}
}
