//************************************************************************************************
// Copyright © 2019 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class SortDialog : UI.LocalizableForm
	{

		public enum Directions
		{
			Ascending,
			Descending
		}


		public SortDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.SortDialog_Text;

				scopeBox.Items.Clear();
				scopeBox.Items.AddRange(Resx.SortDialog_scopeBox_Items.Split('\n'));

				Localize(new string[]
				{
					"scopeLabel",
					"sortLabel",
					"nameButton=word_Name",
					"createdButton",
					"modifiedButton",
					"directionLabel",
					"ascButton",
					"desButton",
					"pinNotesBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			scopeBox.SelectedIndex = 1;
		}


		public OneNote.Scope Scope
		{
			get
			{
				switch (scopeBox.SelectedIndex)
				{
					case 0: return OneNote.Scope.Children;
					case 1: return OneNote.Scope.Pages;
					case 2: return OneNote.Scope.Sections;
					default: return OneNote.Scope.Notebooks;
				}
			}
		}


		public Directions Direction =>
			ascButton.Checked ? Directions.Ascending : Directions.Descending;


		public bool PinNotes => pinNotesBox.Checked;


		public SortCommand.SortBy Sorting =>
			nameButton.Checked
				? SortCommand.SortBy.Name
				: (createdButton.Checked
					? SortCommand.SortBy.Created
					: SortCommand.SortBy.Modified);


		public void SetScope(OneNote.Scope scope)
		{
			switch (scope)
			{
				case OneNote.Scope.Children: scopeBox.SelectedIndex = 0; break;
				case OneNote.Scope.Pages: scopeBox.SelectedIndex = 1; break;
				case OneNote.Scope.Sections: scopeBox.SelectedIndex = 2; break;
				case OneNote.Scope.Notebooks: scopeBox.SelectedIndex = 3; break;
				default: return;
			}

			scopeBox.Enabled = false;
		}


		private void OK(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}


		private void Cancel(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void ChangeSelection(object sender, EventArgs e)
		{
			createdButton.Enabled = scopeBox.SelectedIndex <= 1;
			if (!createdButton.Enabled)
			{
				if (createdButton.Checked)
				{
					nameButton.Checked = true;
				}
			}

			pinNotesBox.Enabled = (scopeBox.SelectedIndex == 2);
		}
	}
}
