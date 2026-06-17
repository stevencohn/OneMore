//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Prompts the user to rename (alias) a single favorite. Unlike the generic RenameDialog,
	/// this shows the favorite's original underlying name and offers a Revert button to clear
	/// any existing alias, falling back to that original name.
	/// </summary>
	internal partial class RenameFavoriteDialog : MoreForm
	{
		private readonly IEnumerable<string> siblingNames;
		private readonly string originalName;


		public RenameFavoriteDialog(IEnumerable<string> siblingNames, string originalName, string currentValue)
		{
			InitializeComponent();

			this.siblingNames = siblingNames;
			this.originalName = originalName;

			Text = Resx.FavoritesManagerControl_renameFavorite;
			originalNameLabel.Text = string.Format(Resx.RenameFavoriteDialog_originalNameLabel, originalName);
			nameBox.Text = currentValue;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"nameLabel=word_Name",
					"errorLabel=RenameDialog_errorLabel.Text",
					"okButton=word_OK",
					"cancelButton=word_Cancel",
					"revertButton=word_Revert"
				});
			}
		}


		/// <summary>
		/// The new alias entered by the user, or null if the user clicked Revert or entered
		/// the favorite's original name, either of which means the consumer should clear
		/// favorite.Alias.
		/// </summary>
		public string Value { get; private set; }


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
			var text = nameBox.Text.Trim();
			Value = text.Equals(originalName, StringComparison.CurrentCulture) ? null : text;

			DialogResult = DialogResult.OK;
			Close();
		}


		private void Revert(object sender, EventArgs e)
		{
			Value = null;
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
