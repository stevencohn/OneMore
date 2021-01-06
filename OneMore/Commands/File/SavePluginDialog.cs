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


	internal partial class SavePluginDialog : LocalizableForm
	{
		private List<string> names;
		private char[] invalidChars;


		public SavePluginDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.SavePluginDialog_Text;

				Localize(new string[]
				{
					"nameLabel",
					"okButton",
					"cancelButton"
				});
			}

			errorLabel.Visible = false;
		}


		public string PluginName
		{
			get => nameBox.Text.Trim();
			set => nameBox.Text = value;
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
				errorLabel.Text = Resx.SavePluginDialog_InvalidName;
				errorLabel.Visible = true;
				return;
			}

			if (names == null)
			{
				names = new PluginsProvider().GetNames().Select(n => n.ToLower()).ToList();
			}

			if (names.Contains(name))
			{
				errorLabel.Text = Resx.SavePluginDialog_DuplicateName;
				errorLabel.Visible = true;
				okButton.Enabled = true;
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
