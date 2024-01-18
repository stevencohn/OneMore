//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class StylizeImagesDialog : UI.LocalizableForm
	{
		public enum ImageStyle
		{
			GrayScale,
			Sepia,
			Polaroid,
			Invert
		}


		public StylizeImagesDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.StylizeImagesDialog_Text;

				Localize(new string[]
				{
					"foreBox",
					"backBox",
					"styleLabel=word_Style",
					"styleBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		public StylizeImagesDialog(
			int foreCount, int foreSelected, int backCount, int backSelected)
			: this()
		{
			foreBox.Enabled = foreCount > 0 || foreSelected > 0;
			backBox.Enabled = backCount > 0 || backSelected > 0;

			foreImagesLabel.Text = string.Format(
				Resx.StylizeImagesDialog_foreImagesLabel_Text,
				foreCount, foreSelected);

			backImagesLabel.Text = string.Format(
				Resx.StylizeImagesDialog_backImagesLabel_Text,
				backCount, backSelected);

			foreBox.Enabled = foreImagesLabel.Enabled = foreCount > 0;
			backBox.Enabled = backImagesLabel.Enabled = backCount > 0;

			foreBox.Checked = foreCount > 0 && backCount == 0;
			backBox.Checked = backCount > 0 && foreCount == 0;

			styleBox.SelectedIndex = 0;
		}


		public bool ApplyForeground => foreBox.Checked;


		public bool ApplyBackground => backBox.Checked;


		public ImageStyle Style => styleBox.SelectedIndex switch
		{
			0 => ImageStyle.GrayScale,
			1 => ImageStyle.Sepia,
			2 => ImageStyle.Polaroid,
			_ => ImageStyle.Invert
		};


		private void ChangeSelections(object sender, System.EventArgs e)
		{
			okButton.Enabled = foreBox.Checked || backBox.Checked;
		}


		private void OK(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
