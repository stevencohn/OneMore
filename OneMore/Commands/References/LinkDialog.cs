//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Resx = Properties.Resources;


	internal partial class LinkDialog : UI.MoreForm
	{
		public LinkDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.MapDialog_Text;

				Localize(new string[]
				{
					"introLabel",
					"groupBox",
					"notebooksRadio=phrase_AllNotebooks",
					"notebookRadio=phrase_AllSectionInTheCurrentNotebook",
					"sectionRadio=phrase_TheCurrentSection",
					"synopsisBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public LinkDialog(string title)
			: this()
		{
			titleLabel.Text = title;
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


		public bool Synopsis => synopsisBox.Checked;
	}
}
