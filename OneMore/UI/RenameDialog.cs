//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Resx = Properties.Resources;


	internal partial class RenameDialog : UI.MoreForm
	{
		private readonly IEnumerable<string> names;


		public RenameDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.phrase_NewStyle;

				Localize(new string[]
				{
					"nameLabel=word_Name",
					"errorLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public RenameDialog(IEnumerable<string> names, string name)
			: this()
		{
			this.names = names;
			nameBox.Text = name;
		}


		public bool Rename
		{
			private get => true;

			set
			{
				Text = value
					? Resx.NameStyleDialog_RenameStyle
					: Resx.phrase_NewStyle;
			}
		}


		public string StyleName => nameBox.Text;


		private void DialogLoad(object sender, EventArgs e)
		{
			nameBox.Focus();
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

			var duplicate = names.Any(n =>
				n.Equals(text, StringComparison.InvariantCultureIgnoreCase));

			errorLabel.Visible = duplicate;
			okButton.Enabled = !duplicate;
		}
	}
}
