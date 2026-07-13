//************************************************************************************************
// Copyright © 2019 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Windows.Forms;
	using Settings;
	using Resx = Properties.Resources;


	internal partial class SortDialog : UI.MoreForm
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
					"nameButton",
					"naturalButton",
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

			var collection = new SettingsProvider().GetCollection("SortDialog");
			var sortby = collection.Get("sortby", SortCommand.SortBy.Name);
			switch (sortby)
			{
				case SortCommand.SortBy.Name: nameButton.Checked = true; break;
				case SortCommand.SortBy.Natural: naturalButton.Checked = true; break;
				case SortCommand.SortBy.Created: createdButton.Checked = true; break;
				default: modifiedButton.Checked = true; break;
			}

			if (collection.Get("direction", Directions.Ascending) == Directions.Ascending)
			{
				ascButton.Checked = true;
			}
			else
			{
				desButton.Checked = true;
			}
		}


		public OneNote.Scope Scope =>
			scopeBox.SelectedIndex switch
			{
				0 => OneNote.Scope.Children,
				1 => OneNote.Scope.Pages,
				2 => OneNote.Scope.Sections,
				3 => OneNote.Scope.SectionGroups,
				_ => OneNote.Scope.Notebooks,
			};


		public Directions Direction =>
			ascButton.Checked ? Directions.Ascending : Directions.Descending;


		public bool PinNotes => pinNotesBox.Checked;


		public SortCommand.SortBy Sorting =>
			true switch
			{
				_ when nameButton.Checked => SortCommand.SortBy.Name,
				_ when naturalButton.Checked => SortCommand.SortBy.Natural,
				_ when createdButton.Checked => SortCommand.SortBy.Created,
				_ => SortCommand.SortBy.Modified
			};


		public void SetScope(OneNote.Scope scope)
		{
			switch (scope)
			{
				case OneNote.Scope.Children: scopeBox.SelectedIndex = 0; break;
				case OneNote.Scope.Pages: scopeBox.SelectedIndex = 1; break;
				case OneNote.Scope.Sections: scopeBox.SelectedIndex = 2; break;
				case OneNote.Scope.SectionGroups: scopeBox.SelectedIndex = 3; break;
				case OneNote.Scope.Notebooks: scopeBox.SelectedIndex = 4; break;
				default: return;
			}

			scopeBox.Enabled = false;
		}


		private void OK(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;

			var provider = new SettingsProvider();
			var collection = provider.GetCollection("SortDialog");
			collection.Add("sortby", Sorting.ToString());
			collection.Add("direction", Direction.ToString());
			provider.SetCollection(collection);
			provider.Save();
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
