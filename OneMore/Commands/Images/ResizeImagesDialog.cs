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
		private readonly Image image;
		private readonly string temp;
		private readonly int viewWidth;
		private readonly int viewHeight;
		private int originalWidth;
		private int originalHeight;
		private bool suspended = true;


		/// <summary>
		/// Initializes a new dialog to resize all images on the page to a standard width
		/// and height with respective ratio
		/// </summary>
		public ResizeImagesDialog()
		{
			Initialize();

			originalWidth = originalHeight = 1;

			// hide controls that do not apply...

			currentLabel.Text = Resx.ResizeImagesDialog_applyToLabel;
			allLabel.Left = sizeLink.Left;
			allLabel.Visible = true;
			origLabel.Visible = sizeLink.Visible = origSizeLink.Visible = false;

			presetRadio.Checked = true;
			RadioClick(presetRadio, null);

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
			temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

			this.viewWidth = viewWidth;
			this.viewHeight = viewHeight;

			sizeLink.Text = string.Format(
				Resx.ResizeImagesDialog_sizeLink_Text, this.viewWidth, this.viewHeight);

			originalWidth = image.Width;
			originalHeight = image.Height;

			origSizeLink.Text = string.Format(
				Resx.ResizeImagesDialog_sizeLink_Text, originalWidth, originalHeight);

			widthUpDown.Value = viewWidth;
			heightUpDown.Value = viewHeight;

			settings = new SettingsProvider();
			presetUpDown.Value = settings.GetImageWidth();

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
					"currentLabel",
					"origLabel",
					"allLabel",
					"pctRadio",
					"pctLabel",
					"absRadio",
					"aspectBox",
					"widthLabel",
					"heightLabel",
					"presetRadio",
					"presetLabel",
					"preserveBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});

				qualLabel.Text = string.Format(Resx.ResizeImageDialog_qualLabel_Text, qualBar.Value);
			}
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			suspended = false;
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		public bool MaintainAspect => aspectBox.Checked;


		public decimal HeightPixels => heightUpDown.Value;


		public decimal WidthPixels => widthUpDown.Value;


		public decimal Percent => pctRadio.Checked ? pctUpDown.Value : 0;


		public bool PreserveSize => preserveBox.Checked;


		public int Quality => qualBar.Value;


		/// <summary>
		/// Get the resized image used for size estimation.
		/// </summary>
		/// <returns></returns>
		public Image GetImage()
		{
			if (!string.IsNullOrEmpty(temp) && File.Exists(temp))
			{
				return Image.FromFile(temp);
			}

			return null;
		}


		public void SetOriginalSize(Size size)
		{
			origSizeLink.Text = string.Format(Resx.ResizeImagesDialog_origSizeLink_Text, size.Width, size.Height);
			originalWidth = size.Width;
			originalHeight = size.Height;
		}


		private void ResetToCurrentSize(object sender, LinkLabelLinkClickedEventArgs e)
		{
			RadioClick(absRadio, null);
			absRadio.Checked = true;
			widthUpDown.Value = viewWidth;
			heightUpDown.Value = viewHeight;
			EstimateStorage();
		}


		private void ResetToOriginalSize(object sender, LinkLabelLinkClickedEventArgs e)
		{
			RadioClick(absRadio, null);
			absRadio.Checked = true;
			widthUpDown.Value = originalWidth;
			heightUpDown.Value = originalHeight;
			EstimateStorage();
		}


		private void RadioClick(object sender, EventArgs e)
		{
			pctUpDown.Enabled = sender == pctRadio;

			var abs = sender == absRadio;
			aspectBox.Enabled = abs;
			widthUpDown.Enabled = abs;
			heightUpDown.Enabled = abs;

			presetUpDown.Enabled = sender == presetRadio;
		}


		private void RadioKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.Handled = true;
				OK(this, e);
			}
		}


		private void PercentValueChanged(object sender, EventArgs e)
		{
			suspended = true;
			widthUpDown.Value = (int)(viewWidth * (pctUpDown.Value / 100));
			heightUpDown.Value = (int)(viewHeight * (pctUpDown.Value / 100));
			EstimateStorage();
			
			suspended = false;
		}


		private void MaintainAspectCheckedChanged(object sender, EventArgs e)
		{
			if (aspectBox.Checked)
			{
				var aspect = widthUpDown.Value < originalWidth 
					? widthUpDown.Value / originalWidth
					: originalWidth / widthUpDown.Value;

				heightUpDown.Value = (int)(originalHeight * aspect);
			}
		}


		private void WidthValueChanged(object sender, EventArgs e)
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


		private void HeightValueChanged(object sender, EventArgs e)
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


		private void OpacityValueChanged(object sender, EventArgs e)
		{
			EstimateStorage();
		}


		private void PresetValueChanged(object sender, EventArgs e)
		{
			if (suspended)
				return;

			suspended = true;
			widthUpDown.Value = (int)presetUpDown.Value;
			heightUpDown.Value = (int)(originalHeight * (widthUpDown.Value / originalWidth));
			EstimateStorage();

			suspended = false;
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

				int rewidth;
				int reheight;

				if (preserveBox.Checked)
				{
					rewidth = originalWidth;
					reheight = originalHeight;
				}
				else
				{
					rewidth = (int)WidthPixels;
					reheight = (int)HeightPixels;
				}

				// resize image without disposing it, use a temp variable 'resized' to dispose
				using (var resized = image.Resize(rewidth, reheight, Quality))
				{
					if (opacityBox.Value < 100)
					{
						using (var t = ((Bitmap)image).SetOpacity((int)opacityBox.Value / 100f))
						{
							t.Save(temp);
						}
					}
					else
					{
						resized.Save(temp);
					}
				}

				logger.WriteTime("resized image");

				var size = new FileInfo(temp).Length;
				qualBox.Text = string.Format(Resx.ResizeImageDialog_qualBox_Size, size.ToBytes(1));
			}
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
	}
}
