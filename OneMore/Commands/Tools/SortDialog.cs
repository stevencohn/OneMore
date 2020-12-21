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

		public enum Sortings
		{
			ByName,
			ByCreated,
			ByModified
		}


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
					"nameButton",
					"createdButton",
					"modifiedButton",
					"directionLabel",
					"ascButton",
					"desButton",
					"pinNotesBox",
					"okButton",
					"cancelButton"
				});
			}

			scopeBox.SelectedIndex = 0;
		}


		public OneNote.Scope Scope
		{
			get
			{
				switch (scopeBox.SelectedIndex)
				{
					case 0:
						return OneNote.Scope.Pages;

					case 1:
						return OneNote.Scope.Sections;

					default:
						return OneNote.Scope.Notebooks;
				}
			}
		}


		public Directions Direction =>
			ascButton.Checked ? Directions.Ascending : Directions.Descending;


		public bool PinNotes => pinNotesBox.Checked;


		public Sortings Soring =>
			nameButton.Checked ? Sortings.ByName
			: (createdButton.Checked ? Sortings.ByCreated : Sortings.ByModified);


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
			createdButton.Enabled = scopeBox.SelectedIndex == 0;
			if (!createdButton.Enabled)
			{
				if (createdButton.Checked)
				{
					nameButton.Checked = true;
				}
			}

			pinNotesBox.Enabled = (scopeBox.SelectedIndex == 1);
		}
	}
}
