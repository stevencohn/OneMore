//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant

namespace River.OneMoreAddIn.Dialogs
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;


	public partial class ResizeImagesDialog : Form
	{
		private readonly int originalWidth;
		private readonly int originalHeight;
		private SettingsProvider settings;
		private bool suspended;


		public ResizeImagesDialog(int width, int height, bool presetOnly = false)
		{
			InitializeComponent();

			suspended = true;
			sizeLabel.Text = $"{width} x {height}";
			widthUpDown.Value = originalWidth = width;
			heightUpDown.Value = originalHeight = height;

			settings = new SettingsProvider();
			presetUpDown.Value = settings.GetImageWidth();

			if (presetOnly)
			{
				currentLabel.Text = "Apply to";
				sizeLabel.Text = "all images on page";
				origLabel.Visible = origSizeLabel.Visible = false;

				presetRadio.Checked = true;
				Radio_Click(presetRadio, null);
				pctRadio.Enabled = false;
				absRadio.Enabled = false;
			}

			suspended = false;
		}


		public decimal HeightPixels => heightUpDown.Value;


		public decimal WidthPixels => widthUpDown.Value;


		public void SetOriginalSize(Size size)
		{
			origSizeLabel.Text = $"{size.Width} x {size.Height}";
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


		protected override void OnShown(EventArgs e)
		{
			Location = new Point(Location.X, Location.Y - (Height / 2));
			base.OnShown(e);
		}
	}
}
