//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Prompts the user for the name of a new top-level section group.
	/// </summary>
	internal partial class AddTopSectionGroupDialog : UI.MoreForm
	{
		private readonly IEnumerable<string> siblingNames;


		public AddTopSectionGroupDialog(IEnumerable<string> siblingNames, string defaultName)
		{
			InitializeComponent();

			this.siblingNames = siblingNames;

			nameBox.Text = defaultName;

			if (NeedsLocalizing())
			{
				Text = Resx.AddTopSectionGroupDialog_Text;

				Localize(new string[]
				{
					"nameLabel=word_Name",
					"errorLabel=RenameDialog_errorLabel.Text",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		/// <summary>
		/// The name entered by the user for the new section group
		/// </summary>
		public string GroupName { get; private set; }


		private void DialogLoad(object sender, EventArgs e)
		{
			nameBox.Focus();
			nameBox.SelectAll();
		}


		private void NameBoxTextChanged(object sender, EventArgs e)
		{
			var text = nameBox.Text.Trim();
			if (string.IsNullOrEmpty(text))
			{
				errorLabel.Visible = false;
				okButton.Enabled = false;
				return;
			}

			var duplicate = siblingNames.Any(n =>
				n.Equals(text, StringComparison.InvariantCultureIgnoreCase));

			errorLabel.Visible = duplicate;
			okButton.Enabled = !duplicate;
		}


		private void Accept(object sender, EventArgs e)
		{
			GroupName = nameBox.Text.Trim();

			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
