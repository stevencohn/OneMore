//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  Yada yada...
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class MoreMessageBox : LocalizableForm
	{
		public MoreMessageBox()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ProgramName;
			}
		}


		private void SetMessage(string message)
		{
			textBox.Text = message;
		}


		private void SetButtons(MessageBoxButtons buttons)
		{
			if (buttons == MessageBoxButtons.OK)
			{
				okButton.Text = Resx.word_OK;
				cancelButton.Visible = false;
			}
			else if (buttons == MessageBoxButtons.OKCancel)
			{
				(cancelButton.Left, okButton.Left) = (okButton.Left, cancelButton.Left);
				okButton.Text = Resx.word_OK;
				cancelButton.Text = Resx.word_Cancel;
			}
			else if (buttons == MessageBoxButtons.YesNo)
			{
				(cancelButton.Left, okButton.Left) = (okButton.Left, cancelButton.Left);
				okButton.Text = Resx.word_Yes;
				cancelButton.Text = Resx.word_No;
			}
		}


		private void SetIcon(MessageBoxIcon mbicon)
		{
			Icon icon;
			switch (mbicon)
			{
				case MessageBoxIcon.Error: icon = SystemIcons.Error; break;
				case MessageBoxIcon.Exclamation: icon = SystemIcons.Exclamation; break;
				case MessageBoxIcon.Question: icon = SystemIcons.Question; break;
				default: icon = SystemIcons.Information; break;
			}

			iconBox.Image = icon.ToBitmap();
		}


		public static DialogResult Show(IWin32Window owner, string text)
		{
			return Show(owner, text, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}


		public static DialogResult Show(IWin32Window owner, string text,
			MessageBoxButtons buttons,
			MessageBoxIcon icon)
		{
			using (var box = new MoreMessageBox())
			{
				box.SetMessage(text);
				box.SetIcon(icon);
				box.SetButtons(buttons);
				return box.ShowDialog(owner);
			}
		}

		private void DoKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				e.Handled = true;
				DialogResult = DialogResult.Cancel;
				Close();
			}
		}

		private void HideSelection(object sender, System.EventArgs e)
		{
			// Move the cursor to the end
			if (textBox.SelectionStart != textBox.TextLength)
			{
				textBox.SelectionStart = textBox.TextLength;
				okButton.Focus();
			}
		}
	}
}
