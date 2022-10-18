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

			messageBox.Clear();
		}


		public void ShowLogLink()
		{
			logLink.Visible = true;
		}


		public void AppendMessage(string message)
		{
			AppendMessage(message, ForeColor);
		}


		public void AppendMessage(string message, Color color)
		{
			messageBox.AppendFormattedText(message, color);
		}


		public void SetMessage(string message)
		{
			messageBox.Text = message;
		}


		public void SetButtons(MessageBoxButtons buttons)
		{
			if (buttons == MessageBoxButtons.OK)
			{
				okButton.Text = Resx.word_OK;
				okButton.DialogResult = DialogResult.OK;
				cancelButton.Visible = false;
			}
			else if (buttons == MessageBoxButtons.OKCancel)
			{
				(cancelButton.Left, okButton.Left) = (okButton.Left, cancelButton.Left);
				okButton.Text = Resx.word_OK;
				okButton.DialogResult = DialogResult.OK;
				cancelButton.Text = Resx.word_Cancel;
				cancelButton.DialogResult = DialogResult.Cancel;
			}
			else if (buttons == MessageBoxButtons.YesNo)
			{
				(cancelButton.Left, okButton.Left) = (okButton.Left, cancelButton.Left);
				okButton.Text = Resx.word_Yes;
				okButton.DialogResult = DialogResult.Yes;
				cancelButton.Text = Resx.word_No;
				cancelButton.DialogResult = DialogResult.No;
			}
		}


		public void SetIcon(MessageBoxIcon mbicon)
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


		public static DialogResult ShowErrorWithLogLink(IWin32Window owner, string text)
		{
			using (var box = new MoreMessageBox())
			{
				box.SetMessage(text);
				box.ShowLogLink();
				box.SetIcon(MessageBoxIcon.Error);
				box.SetButtons(MessageBoxButtons.OK);
				return box.ShowDialog(owner);
			}
		}


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

		private void OpenLog(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(logger.LogPath);
			Close();
		}
	}
}
