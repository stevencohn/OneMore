//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class RefreshPageLinksDialog : UI.LocalizableForm
	{
		public RefreshPageLinksDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.RefreshPageLinksDialog_Text;

				Localize(new string[]
				{
					"groupBox",
					"notebooksRadio",
					"notebookRadio",
					"sectionRadio",
					"okButton",
					"cancelButton"
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
