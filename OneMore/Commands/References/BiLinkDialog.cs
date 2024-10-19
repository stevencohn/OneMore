//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class BiLinkDialog : MoreForm
	{

		public BiLinkDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ProgramName;

				Localize(new string[]
				{
					"okButton=word_OK"
				});
			}

			iconBox.Image = SystemIcons.Information.ToBitmap();
			messageBox.Clear();
		}


		public void SetAnchorText(string text)
		{
			text = string.Format(Resx.BiLinkCommand_Marked, text);

			var size = TextRenderer.MeasureText(
				text, messageBox.Font, messageBox.Size, TextFormatFlags.NoClipping);

			// leave a little room (is this good?)
			var preferred = size.Height + messageBox.Font.Height;
			if (preferred > messageBox.Height)
			{
				Height += preferred - messageBox.Height;
			}

			messageBox.Text = text;
		}


		private void HideSelection(object sender, System.EventArgs e)
		{
			// move the cursor to the end
			if (messageBox.SelectionStart != messageBox.TextLength)
			{
				messageBox.SelectionStart = messageBox.TextLength;
				okButton.Focus();
			}
		}


		private void DoKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Modifiers != Keys.None)
			{
				return;
			}

			if (e.KeyCode == Keys.Escape)
			{
				e.Handled = true;
				DialogResult = DialogResult.Cancel;
				Close();
			}
		}


		private void DoClick(object sender, System.EventArgs e)
		{
			// check if user wants to hide this in the future
			if (hideBox.Checked)
			{
				var settings = new SettingsProvider();
				var collection = settings.GetCollection(BiLinkCommand.SettingsName);
				collection.Add("hideStartMessage", true);
				settings.SetCollection(collection);
				settings.Save();
			}
		}
	}
}
