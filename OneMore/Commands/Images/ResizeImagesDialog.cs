//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant
#pragma warning disable IDE1006 // must begin with upper case

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class ResizeImagesDialog : UI.LocalizableForm
	{
		private readonly SettingsProvider settings;
		private readonly int currentWidth;
		private readonly int currentHeight;
		private int originalWidth;
		private int originalHeight;
		private bool suspended;


		public ResizeImagesDialog(int width, int height, bool presetOnly = false)
		{
			InitializeComponent();

			suspended = true;
			currentWidth = width;
			currentHeight = height;
			sizeLink.Text = string.Format(Resx.ResizeImagesDialog_sizeLink_Text, width, height);

			widthUpDown.Value = originalWidth = width;
			heightUpDown.Value = originalHeight = height;

			settings = new SettingsProvider();
			presetUpDown.Value = settings.GetImageWidth();

			if (presetOnly)
			{
				currentLabel.Text = Resx.ResizeImagesDialog_currentLabel_Text;
				allLabel.Left = sizeLink.Left;
				allLabel.Visible = true;
				origLabel.Visible = sizeLink.Visible = origSizeLink.Visible = false;

				presetRadio.Checked = true;
				Radio_Click(presetRadio, null);
				pctRadio.Enabled = false;
				absRadio.Enabled = false;
			}

			if (NeedsLocalizing())
			{
				Text = Resx.ResizeImagesDialog_Text;

				Localize(new string[]
				{
					"pctRadio",
					"absRadio",
					"presetRadio",
					"presetLabel",
					"pctLabel",
					"aspectBox",
					"widthLabel",
					"heightLabel",
					"origLabel",
					"allLabel",
					"okButton",
					"cancelButton"
				});
			}

			suspended = false;
		}


		public decimal HeightPixels => heightUpDown.Value;


		public decimal WidthPixels => widthUpDown.Value;


		public void SetOriginalSize(Size size)
		{
			origSizeLink.Text = string.Format(Resx.ResizeImagesDialog_origSizeLink_Text, size.Width, size.Height);
			originalWidth = size.Width;
			originalHeight = size.Height;
		}


		private void OK(object sender, EventArgs e)
		{
			if (presetRadio.Checked)
			{
				settings.SetImageWidth((int)(presetUpDown.Value));
				settings.Save();

				suspended = true;
				widthUpDown.Value = presetUpDown.Value;
				heightUpDown.Value = (int)(originalHeight * (widthUpDown.Value / originalWidth));
			}

			DialogResult = DialogResult.OK;
		}


		private void Cancel(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}


		private void Radio_Click(object sender, EventArgs e)
		{
			pctUpDown.Enabled = sender == pctRadio;

			var abs = sender == absRadio;
			aspectBox.Enabled = abs;
			widthUpDown.Enabled = abs;
			heightUpDown.Enabled = abs;

			presetUpDown.Enabled = sender == presetRadio;
		}


		private void ResetCurrentSize(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Radio_Click(absRadio, null);
			absRadio.Checked = true;
			widthUpDown.Value = currentWidth;
			heightUpDown.Value = currentHeight;
		}


		private void ResetOriginalSize(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Radio_Click(absRadio, null);
			absRadio.Checked = true;
			widthUpDown.Value = originalWidth;
			heightUpDown.Value = originalHeight;
		}


		private void pctUpDown_ValueChanged(object sender, EventArgs e)
		{
			suspended = true;
			widthUpDown.Value = (int)(originalWidth * (pctUpDown.Value / 100));
			heightUpDown.Value = (int)(originalHeight * (pctUpDown.Value / 100));
			suspended = false;
		}


		private void aspectBox_CheckedChanged(object sender, EventArgs e)
		{
			if (aspectBox.Checked)
			{
				var aspect = widthUpDown.Value < originalWidth 
					? widthUpDown.Value / originalWidth
					: originalWidth / widthUpDown.Value;

				heightUpDown.Value = (int)(originalHeight * aspect);
			}
		}


		private void widthUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (widthUpDown.Enabled && !suspended)
			{
				if (aspectBox.Checked)
				{
					suspended = true;
					heightUpDown.Value = (int)(originalHeight * (widthUpDown.Value / originalWidth));
					suspended = false;
				}
			}
		}


		private void heightUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (heightUpDown.Enabled && !suspended)
			{
				if (aspectBox.Checked)
				{
					suspended = true;
					widthUpDown.Value = (int)(originalWidth * (heightUpDown.Value / originalHeight));
					suspended = false;
				}
			}
		}
	}
}
