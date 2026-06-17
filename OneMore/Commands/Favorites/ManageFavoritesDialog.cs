//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using System;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Thin dialog wrapper around FavoritesManagerControl. Owns OK/Cancel; the control owns
	/// all favorites/folder editing logic and the in-memory staging of changes.
	/// </summary>
	internal partial class ManageFavoritesDialog : UI.MoreForm
	{
		public ManageFavoritesDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ManageFavoritesDialog_Text;

				Localize(new string[]
				{
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			DefaultControl = managerControl;
		}


		private void BindOnLoad(object sender, EventArgs e)
		{
			managerControl.LoadFavorites();
		}


		private void OK(object sender, EventArgs e)
		{
			if (!managerControl.Save())
			{
				UI.MoreMessageBox.ShowError(this, "Could not save favorites");
				return;
			}

			DialogResult = DialogResult.OK;
			Close();
		}


		private void ConfirmClosing(object sender, FormClosingEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
			{
				return;
			}

			if (managerControl.IsDirty())
			{
				if (UI.MoreMessageBox.Show(this, Resx.ManageFavoritesDialog_discard,
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
				{
					e.Cancel = true;
				}
			}
		}
	}
}
