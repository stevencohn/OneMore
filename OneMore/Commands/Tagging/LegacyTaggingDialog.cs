//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class LegacyTaggingDialog : UI.MoreForm
	{

		public LegacyTaggingDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ProgramName;
			}

			iconBox.Image = SystemIcons.Warning.ToBitmap();

			VerticalOffset = -Height / 2;
		}


		public bool HideQuestion => hideBox.Checked;


		private void DoKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Modifiers != Keys.None)
			{
				return;
			}

			if (e.KeyCode == Keys.Escape ||
				e.KeyCode == Keys.N ||
				e.KeyCode == Keys.C)
			{
				e.Handled = true;
				DialogResult = cancelButton.DialogResult;
				Close();
			}
		}


		private void HideSelection(object sender, System.EventArgs e)
		{
			// Move the cursor to the end
			if (messageBox.SelectionStart != messageBox.TextLength)
			{
				messageBox.SelectionStart = messageBox.TextLength;
				okButton.Focus();
			}
		}
	}
}
