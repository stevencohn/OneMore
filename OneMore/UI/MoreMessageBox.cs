﻿//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class MoreMessageBox : MoreForm
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
			using var g = messageBox.CreateGraphics();
			var size = g.MeasureString(message, Font).ToSize();
			if (size.Height + Font.Height > messageBox.Height)
			{
				Height += Font.Height + (size.Height - messageBox.Height);
			}

			messageBox.Text = message;
		}


		public void SetButtons(MessageBoxButtons buttons)
		{
			// OK | No | Cancel ...

			if (buttons == MessageBoxButtons.OK)
			{
				cancelButton.Visible = false;
				noButton.Visible = false;
				okButton.Left = cancelButton.Left;
				okButton.Text = Resx.word_OK;
				okButton.DialogResult = DialogResult.OK;
			}
			else if (buttons == MessageBoxButtons.OKCancel)
			{
				noButton.Visible = false;
				okButton.Left = noButton.Left;
				okButton.Text = Resx.word_OK;
				okButton.DialogResult = DialogResult.OK;
				cancelButton.Text = Resx.word_Cancel;
				cancelButton.DialogResult = DialogResult.Cancel;
			}
			else if (buttons == MessageBoxButtons.YesNo)
			{
				cancelButton.Visible = false;
				okButton.Left = noButton.Left;
				okButton.Text = Resx.word_Yes;
				okButton.DialogResult = DialogResult.Yes;
				noButton.Left = cancelButton.Left;
				noButton.Text = Resx.word_No;
				noButton.DialogResult = DialogResult.No;
			}
			else if (buttons == MessageBoxButtons.YesNoCancel)
			{
				okButton.Text = Resx.word_Yes;
				okButton.DialogResult = DialogResult.Yes;
				noButton.Text = Resx.word_No;
				noButton.DialogResult = DialogResult.No;
				cancelButton.Text = Resx.word_Cancel;
				cancelButton.DialogResult = DialogResult.Cancel;
			}
		}


		public void SetIcon(MessageBoxIcon mbicon)
		{
			Icon icon = mbicon switch
			{
				MessageBoxIcon.Error => SystemIcons.Error,
				MessageBoxIcon.Question => SystemIcons.Question,
				MessageBoxIcon.Warning => SystemIcons.Warning, // same as Exclamation
				_ => SystemIcons.Information,
			};

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
			using var box = new MoreMessageBox();
			box.SetMessage(text);
			box.SetIcon(icon);
			box.SetButtons(buttons);
			if (owner is null)
			{
				box.StartPosition = FormStartPosition.CenterScreen;
				return box.ShowDialog();
			}

			return box.ShowDialog(owner);
		}


		public static DialogResult ShowError(IWin32Window owner, string text)
		{
			using var box = new MoreMessageBox();
			box.SetMessage(text);
			box.SetIcon(MessageBoxIcon.Error);
			box.SetButtons(MessageBoxButtons.OK);

			if (owner is null)
			{
				box.StartPosition = FormStartPosition.CenterScreen;
				return box.ShowDialog();
			}

			return box.ShowDialog(owner);
		}


		public static DialogResult ShowErrorWithLogLink(IWin32Window owner, string text)
		{
			using var box = new MoreMessageBox();
			box.SetMessage(text);
			box.ShowLogLink();
			box.SetIcon(MessageBoxIcon.Error);
			box.SetButtons(MessageBoxButtons.OK);

			if (owner is null)
			{
				box.StartPosition = FormStartPosition.CenterScreen;
				return box.ShowDialog();
			}

			return box.ShowDialog(owner);
		}


		public static DialogResult ShowQuestion(
			IWin32Window owner, string text, bool cancel = false)
		{
			var buttons = cancel
				? MessageBoxButtons.YesNoCancel
				: MessageBoxButtons.YesNo;

			return Show(owner, text, buttons, MessageBoxIcon.Question);
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
