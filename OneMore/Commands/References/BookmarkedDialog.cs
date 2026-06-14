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


	internal partial class BookmarkedDialog : MoreForm
	{

		public BookmarkedDialog()
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


		public BookmarkedDialog(string text, string note = null)
			: this()
		{
			var message = string.Format(Resx.BookmarkedDialog_Message, text);

			if (note is not null)
			{
				message = $"{message}\n{note}";
			}

			var size = TextRenderer.MeasureText(
				message, messageBox.Font, messageBox.Size, TextFormatFlags.NoClipping);

			// leave a little room (is this the correct way?)
			var preferred = size.Height + messageBox.Font.Height;
			if (preferred > messageBox.Height)
			{
				Height += preferred - messageBox.Height;
			}

			messageBox.Text = message;
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
				var collection = settings.GetCollection(BookmarkCommand.CollectionName);
				collection.Add(BookmarkCommand.SettingName, true);
				settings.SetCollection(collection);
				settings.Save();
			}
		}
	}
}
