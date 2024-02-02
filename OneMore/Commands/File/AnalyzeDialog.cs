//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace River.OneMoreAddIn.Commands
{
	using Resx = Properties.Resources;


	internal enum AnalysisDetail
	{
		All,
		Current,
		None
	}


	internal partial class AnalyzeDialog : UI.MoreForm
	{
		public AnalyzeDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.AnalyzeDialog_Text;

				Localize(new string[]
				{
					"notebookBox",
					"sectionBox",
					"sectionDetailsBox",
					"allDetailsBox",
					"noDetailsBox",
					"warningLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			sizeBox.Items.Insert(0, Resx.word_None);
			sizeBox.SelectedIndex = 0;
		}


		public bool IncludeNotebookSummary => notebookBox.Checked;


		public bool IncludeSectionSummary => sectionBox.Checked;


		public AnalysisDetail Detail
		{
			get
			{
				if (allDetailsBox.Checked) return AnalysisDetail.All;
				if (sectionDetailBox.Checked) return AnalysisDetail.Current;
				return AnalysisDetail.None;
			}
		}

		public int ThumbnailSize => sizeBox.SelectedIndex switch
		{
			1 => 20,
			2 => 40,
			3 => 80,
			_ => 0
		};


		private void Validate(object sender, System.EventArgs e)
		{
			okButton.Enabled =
				notebookBox.Checked ||
				sectionBox.Checked ||
				allDetailsBox.Checked ||
				sectionDetailBox.Checked;

			sizeBox.Enabled =
				allDetailsBox.Checked ||
				sectionDetailBox.Checked;

			warningLabel.Visible =
				(sectionDetailBox.Checked || allDetailsBox.Checked) &&
				sizeBox.SelectedIndex > 0;
		}
	}
}
