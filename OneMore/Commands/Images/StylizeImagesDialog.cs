//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class StylizeImagesDialog : UI.MoreForm
	{

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

			foreBox.Text = string.Format(
				Resx.StylizeImagesDialog_foreBox_Text,
				foreCount, foreSelected);

			backBox.Text = string.Format(
				Resx.StylizeImagesDialog_backBox_Text,
				backCount, backSelected);

			foreBox.Enabled = foreCount > 0;
			backBox.Enabled = backCount > 0;

			foreBox.Checked = foreCount > 0 && backCount == 0;
			backBox.Checked = backCount > 0 && foreCount == 0;

			styleBox.SelectedIndex = 0;
		}


		public bool ApplyForeground => foreBox.Checked;


		public bool ApplyBackground => backBox.Checked;


		public ImageEditor.Stylization Style => styleBox.SelectedIndex switch
		{
			0 => ImageEditor.Stylization.GrayScale,
			1 => ImageEditor.Stylization.Sepia,
			2 => ImageEditor.Stylization.Polaroid,
			_ => ImageEditor.Stylization.Invert
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
