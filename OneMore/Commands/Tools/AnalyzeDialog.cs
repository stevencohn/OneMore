//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace River.OneMoreAddIn.Commands
{
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal enum AnalysisDetail
	{
		All,
		Current,
		None
	}


	internal partial class AnalyzeDialog : UI.LocalizableForm
	{
		public AnalyzeDialog()
		{
			InitializeComponent();

			sizeBox.SelectedIndex = 0;

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
					"okButton",
					"cancelButton"
				});
			}
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

		public int ThumbnailSize
		{
			get
			{
				if (sizeBox.SelectedIndex == 0) return 20;
				if (sizeBox.SelectedIndex == 1) return 40;
				return 80;
			}
		}


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
		}
	}
}
