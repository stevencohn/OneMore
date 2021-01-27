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
	using System.IO;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class ResizeImagesDialog : UI.LocalizableForm
	{
		private readonly SettingsProvider settings;
		private readonly string tempfile;
		private readonly int currentWidth;
		private readonly int currentHeight;
		private int originalWidth;
		private int originalHeight;
		private Image image;
		private bool suspended;


		/// <summary>
		/// Initializes a new dialog to resize all images on the page to a standard width
		/// and height with respective ratio
		/// </summary>
		public ResizeImagesDialog()
		{
			Initialize();

			originalWidth = originalHeight = 1;

			// disable controls that do not apply...

			currentLabel.Text = Resx.ResizeImagesDialog_currentLabel_Text;
			allLabel.Left = sizeLink.Left;
			allLabel.Visible = true;
			origLabel.Visible = sizeLink.Visible = origSizeLink.Visible = false;

			presetRadio.Checked = true;
			Radio_Click(presetRadio, null);
			pctRadio.Enabled = false;
			absRadio.Enabled = false;

			settings = new SettingsProvider();
			presetUpDown.Value = settings.GetImageWidth();
		}


		/// <summary>
		/// Initializes a new dialog to resize a selected image
		/// </summary>
		/// <param name="image"></param>
		public ResizeImagesDialog(Image image, int viewWidth, int viewHeight)
		{
			Initialize();

			this.image = image;
			tempfile = Path.GetTempFileName();

			suspended = true;
			currentWidth = viewWidth;
			currentHeight = viewHeight;

			sizeLink.Text = string.Format(
				Resx.ResizeImagesDialog_sizeLink_Text, currentWidth, currentHeight);

			widthUpDown.Value = originalWidth = image.Width;
			heightUpDown.Value = originalHeight = image.Height;

			origSizeLink.Text = string.Format(
				Resx.ResizeImagesDialog_sizeLink_Text, originalWidth, originalHeight);

			settings = new SettingsProvider();
			presetUpDown.Value = settings.GetImageWidth();

			suspended = false;

			EstimateStorage();
		}


		private void Initialize()
		{
			InitializeComponent();

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
					"qualBox.Text",
					"okButton",
					"cancelButton"
				});
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		public decimal HeightPixels => heightUpDown.Value;


		public decimal WidthPixels => widthUpDown.Value;


		public bool PreserveSize => preserveBox.Checked;


		public int Quality => qualBar.Value;


		public Image GetImage()
		{
			if (!string.IsNullOrEmpty(tempfile) && File.Exists(tempfile))
			{
				return Image.FromFile(tempfile);
			}

			return null;
		}


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
			EstimateStorage();
		}


		private void ResetOriginalSize(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Radio_Click(absRadio, null);
			absRadio.Checked = true;
			widthUpDown.Value = originalWidth;
			heightUpDown.Value = originalHeight;
			EstimateStorage();
		}


		private void pctUpDown_ValueChanged(object sender, EventArgs e)
		{
			suspended = true;
			widthUpDown.Value = (int)(originalWidth * (pctUpDown.Value / 100));
			heightUpDown.Value = (int)(originalHeight * (pctUpDown.Value / 100));
			EstimateStorage();
			
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
			if (suspended)
				return;

			if (widthUpDown.Enabled)
			{
				if (aspectBox.Checked)
				{
					suspended = true;
					heightUpDown.Value = (int)(originalHeight * (widthUpDown.Value / originalWidth));
					EstimateStorage();
					
					suspended = false;
				}
			}
		}


		private void heightUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (suspended)
				return;

			if (heightUpDown.Enabled)
			{
				if (aspectBox.Checked)
				{
					suspended = true;
					widthUpDown.Value = (int)(originalWidth * (heightUpDown.Value / originalHeight));
					EstimateStorage();
					
					suspended = false;
				}
			}
		}

		private void EstimateStorage(object sender, EventArgs e)
		{
			qualLabel.Text = string.Format(Resx.ResizeImageDialog_qualLabel_Text, qualBar.Value);
			EstimateStorage();
		}


		private void EstimateStorage()
		{
			if (image != null)
			{
				logger.StartClock();

				if (preserveBox.Checked)
				{
					image.Resize(originalWidth, originalHeight, Quality).Save(tempfile);
				}
				else
				{
					image.Resize((int)WidthPixels, (int)HeightPixels, Quality).Save(tempfile);
				}

				logger.WriteTime("resized image");

				var size = new FileInfo(tempfile).Length;
				qualBox.Text = string.Format(Resx.ResizeImageDialog_qualBox_Size, size.ToBytes(1));
			}
		}
	}
}
