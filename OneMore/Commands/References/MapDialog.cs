//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class MapDialog : UI.LocalizableForm
	{
		public MapDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.MapDialog_Text;

				Localize(new string[]
				{
					"groupBox",
					"notebooksRadio",
					"notebookRadio",
					"sectionRadio",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public bool FullCatalog => catalogBox.Enabled && catalogBox.Checked;


		public OneNote.Scope Scope
		{
			get
			{
				if (notebooksRadio.Checked) return OneNote.Scope.Notebooks;
				if (notebookRadio.Checked) return OneNote.Scope.Sections;
				return OneNote.Scope.Pages;
			}
		}


		private void ChangeScope(object sender, System.EventArgs e)
		{
			catalogBox.Enabled = !notebooksRadio.Checked;
		}
	}
}
