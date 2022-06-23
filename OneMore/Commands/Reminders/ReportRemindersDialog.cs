//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class ReportRemindersDialog : UI.LocalizableForm
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
					"groupBox",
					"notebooksRadio",
					"notebookRadio",
					"sectionRadio",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


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
