//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;


	internal partial class InsertTocDialog : UI.LocalizableForm
	{
		public InsertTocDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				//Text = Resx.TocDialog_Text;

				Localize(new string[]
				{
					"pageRadio",
					"topBox",
					"sectionRadio",
					"notebookRadio",
					"pagesBox",
					"okButton",
					"cancelButton"
				});
			}
		}


		/// <summary>
		/// Gets the scope of contents where Self mean add a table of headings to the 
		/// current page, Pages means add a page with links to all pages in this section,
		/// and Sections means add a page with links to sections in this notebook.
		/// </summary>
		public OneNote.Scope Scope
		{
			get
			{
				if (pageRadio.Checked) return OneNote.Scope.Self;
				if (sectionRadio.Checked) return OneNote.Scope.Pages;
				return OneNote.Scope.Sections;
			}
		}


		public bool TopLinks => topBox.Enabled && topBox.Checked;


		public bool SectionPages => pagesBox.Enabled && pagesBox.Checked;


		private void ChangedRadio(object sender, EventArgs e)
		{
			if (sender == pageRadio)
			{
				topBox.Enabled = true;
				pagesBox.Enabled = false;
			}
			else if (sender == sectionRadio)
			{
				topBox.Enabled = pagesBox.Enabled = false;
			}
			else
			{
				topBox.Enabled = false;
				pagesBox.Enabled = true;
			}
		}
	}
}
