//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class SaveSnippetDialog : MoreForm
	{
		private List<string> names;
		private char[] invalidChars;
		private string originalName;


		public SaveSnippetDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.SaveSnippetDialog_Text;

				Localize(new string[]
				{
					"nameLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			errorLabel.Visible = false;
		}


		public string SnippetName
		{
			get { return nameBox.Text.Trim(); }
			set
			{
				nameBox.Text = value;
				originalName = value;
			}
		}


		protected override void OnShown(System.EventArgs e)
		{
			base.OnShown(e);

			// when renaming, the existing name is preselected so typing replaces it
			nameBox.Focus();
			nameBox.SelectAll();
		}


		private void ValidateName(object sender, System.EventArgs e)
		{
			var name = nameBox.Text.Trim();
			if (name.Length == 0)
			{
				okButton.Enabled = false;
				return;
			}

			name = name.ToLower();

			invalidChars ??= Path.GetInvalidFileNameChars();

			if (name.IndexOfAny(invalidChars) >= 0)
			{
				errorLabel.Text = Resx.SaveSnippetDialog_invalidName;
				errorLabel.Visible = true;
				return;
			}

			errorLabel.Visible = false;
			okButton.Enabled = true;
		}


		private void OK(object sender, System.EventArgs e)
		{
			names ??= new SnippetsProvider().GetNames().Select(n => n.ToLower()).ToList();

			// no confirmation needed when renaming and the name is unchanged
			if (names.Contains(SnippetName.ToLower()) &&
				!string.Equals(SnippetName, originalName, System.StringComparison.OrdinalIgnoreCase))
			{
				var result = UI.MoreMessageBox.ShowQuestion(this,
					string.Format(Resx.SaveSnippetDialog_confirmOverwrite, SnippetName));

				if (result != System.Windows.Forms.DialogResult.Yes)
				{
					// keep the dialog open so the user can choose another name
					DialogResult = System.Windows.Forms.DialogResult.None;
					return;
				}
			}

			Close();
		}
	}
}
