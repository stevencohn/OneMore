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


	internal partial class SaveSnippetDialog : LocalizableForm
	{
		private List<string> names;
		private char[] invalidChars;


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
			set { nameBox.Text = value; }
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

			if (invalidChars == null)
			{
				invalidChars = Path.GetInvalidFileNameChars();
			}

			if (name.IndexOfAny(invalidChars) >= 0)
			{
				errorLabel.Text = Resx.SaveSnippetDialog_invalidName;
				errorLabel.Visible = true;
				return;
			}

			if (names == null)
			{
				names = new SnippetsProvider().GetNames().Select(n => n.ToLower()).ToList();
			}

			if (names.Contains(name))
			{
				errorLabel.Text = Resx.SaveSnippetDialog_duplicateName;
				errorLabel.Visible = true;
				okButton.Enabled = false;
				return;
			}

			errorLabel.Visible = false;
			okButton.Enabled = true;
		}


		private void OK(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
