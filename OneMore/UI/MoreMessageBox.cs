//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
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
				Localize(new string[]
				{
					"hideBox"
				});

				Text = Resx.ProgramName;
			}

			messageBox.Clear();
		}


		public bool SuppressMessage { get; private set; }


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
			FitHeight();
		}


		public void EnableSuppression()
		{
			hideBox.Visible = true;
		}


		public void SetMessage(string message)
		{
			messageBox.Text = message;
			FitHeight();
		}


		/// <summary>
		/// Grows the dialog height, up to a maximum equal to its width, so the message
		/// fits without a scrollbar. Measures using the RichTextBox's own wrapped layout.
		/// </summary>
		private void FitHeight()
		{
			if (messageBox.TextLength == 0)
			{
				return;
			}

			// top of the last line in the control's actual layout, plus that line
			// and a little room
			var last = messageBox.GetPositionFromCharIndex(messageBox.TextLength - 1);
			var needed = last.Y + (messageBox.Font.Height * 2);

			var grow = needed - messageBox.ClientSize.Height;
			if (grow <= 0)
			{
				return;
			}

			var maxHeight = Width;
			if (Height >= maxHeight)
			{
				return;
			}

			Height = Math.Min(Height + grow, maxHeight);
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
			MessageBoxIcon icon,
			float widthScale = 1f,
			float heightScale = 1f)
		{
			using var box = new MoreMessageBox();

			if (widthScale != 1f || heightScale != 1f)
			{
				// scale relative to the current (already DPI-scaled) size rather than a
				// hardcoded pixel size, so this stays correct across DPI/font settings;
				// applied before SetMessage so its own auto-grow-to-fit (if the text is
				// long enough to still not fit) layers on top of this instead of being
				// multiplied by it
				box.Size = new Size(
					(int)(box.Width * widthScale),
					(int)(box.Height * heightScale));
			}

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
			return Show(owner, text, MessageBoxButtons.OK, MessageBoxIcon.Error);
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


		public static DialogResult ShowWarning(IWin32Window owner, string text)
		{
			return Show(owner, text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}


		private void ChangeSuppression(object sender, System.EventArgs e)
		{
			SuppressMessage = hideBox.Visible && hideBox.Checked;
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
