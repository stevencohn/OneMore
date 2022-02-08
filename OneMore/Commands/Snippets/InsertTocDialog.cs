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
					"rightAlignBox",
					"sectionRadio",
					"previewBox",
					"notebookRadio",
					"pagesBox",
					"preview2Box=InsertTocDialog_previewBox.Text",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
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


		public bool PreviewPages =>
			(sectionRadio.Checked && previewBox.Checked) ||
			(notebookRadio.Checked && preview2Box.Checked);


		public bool RightAlignTopLinks => rightAlignBox.Enabled && rightAlignBox.Checked;


		public bool SectionPages => pagesBox.Enabled && pagesBox.Checked;


		public bool TopLinks => topBox.Enabled && topBox.Checked;


		private void ToggleRightAlignOption(object sender, EventArgs e)
		{
			rightAlignBox.Enabled = topBox.Checked;
		}


		private void PagesBoxCheckedChanged(object sender, EventArgs e)
		{
			preview2Box.Enabled = pagesBox.Checked;
		}


		private void ChangedRadio(object sender, EventArgs e)
		{
			if (sender == pageRadio)
			{
				topBox.Enabled = true;
				rightAlignBox.Enabled = true;
				previewBox.Enabled = false;
				pagesBox.Enabled = false;
				preview2Box.Enabled = false;
			}
			else if (sender == sectionRadio)
			{
				topBox.Enabled = pagesBox.Enabled = false;
				rightAlignBox.Enabled = false;
				previewBox.Enabled = true;
				preview2Box.Enabled = false;
			}
			else
			{
				topBox.Enabled = false;
				rightAlignBox.Enabled = false;
				previewBox.Enabled = false;
				pagesBox.Enabled = true;
				preview2Box.Enabled = pagesBox.Checked;
			}
		}
	}
}
