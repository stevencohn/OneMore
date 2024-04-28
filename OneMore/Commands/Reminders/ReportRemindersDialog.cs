//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Resx = Properties.Resources;


	internal partial class ReportRemindersDialog : UI.MoreForm
	{
		public ReportRemindersDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ReportRemindersDialog_Text;

				Localize(new string[]
				{
					"introLabel",
					"notebooksRadio=phrase_AllNotebooks",
					"notebookRadio=phrase_AllSectionInTheCurrentNotebook",
					"sectionRadio=phrase_TheCurrentSection",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public bool IncludeCompleted => showCompletedBox.Checked;


		public OneNote.Scope Scope
		{
			get
			{
				if (notebooksRadio.Checked) return OneNote.Scope.Notebooks;
				if (notebookRadio.Checked) return OneNote.Scope.Sections;
				return OneNote.Scope.Pages;
			}
		}
	}
}
