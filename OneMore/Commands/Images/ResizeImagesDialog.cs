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
		private readonly Image image;
		private readonly int viewWidth;
		private readonly int viewHeight;
		private int originalWidth;
		private int originalHeight;
		private Image preview;
		private int storageSize;
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

			previewGroup.Visible = false;
			Width -= (previewGroup.Width + Padding.Right);

			presetRadio.Checked = true;
			RadioClick(presetRadio, null);

			settings = new SettingsProvider();
			presetUpDown.Value = settings.GetCollection("images").Get("mruWidth", 500);
		}


		/// <summary>
		/// Initializes a new dialog to resize a selected image
		/// </summary>
		/// <param name="image"></param>
		public ResizeImagesDialog(Image image, int viewWidth, int viewHeight)
		{
			Initialize();

			this.image = image;

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
			presetUpDown.Value = settings.GetCollection("images").Get("mruWidth", 500);

			DrawPreview();
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
					"previewBox",
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

		public decimal HeightPixels => heightUpDown.Value;


		public decimal ImageOpacity => opacityBox.Value;


		public int ImageQuality => qualBar.Value;


		public bool MaintainAspect => aspectBox.Checked;


		public decimal Percent => pctRadio.Checked ? pctUpDown.Value : 0;


		public bool PreserveSize => preserveBox.Checked;


		public decimal WidthPixels => widthUpDown.Value;



		/// <summary>
		/// Gets a new image after applying the desired modifications to the source image.
		/// </summary>
		/// <returns>A new image</returns>
		public Image GetImage()
		{
			previewBox.Image = null;
			preview.Dispose();

			if (WidthPixels == image.Width && HeightPixels == image.Height)
			{
				return image;
			}

			preview = image.Resize((int)WidthPixels, (int)HeightPixels);

			if (qualBar.Value < 100)
			{
				using (var p = preview)
					preview = p.SetQuality(qualBar.Value);
			}

			// opacity must be set last
			if (opacityBox.Value < 100)
			{
				using (var p = preview)
					preview = p.SetOpacity((float)(opacityBox.Value / 100));
			}

			return preview;
		}


		private void DrawPreview()
		{
			logger.StartClock();

			previewBox.Image = null;
			preview?.Dispose();

			if (WidthPixels <= previewBox.Width && HeightPixels <= previewBox.Height)
			{
				preview = image.Resize((int)WidthPixels, (int)HeightPixels);
			}
			else
			{
				var x = previewBox.Width - WidthPixels;
				var y = previewBox.Height - HeightPixels;
				if (x < y)
				{
					var height = (int)(HeightPixels * (image.Width / previewBox.Width));
					preview = image.Resize(previewBox.Width, height);
				}
				else
				{
					var width = (int)(WidthPixels * (image.Height / preserveBox.Height));
					preview = image.Resize(width, previewBox.Height);
				}
			}

			if (qualBar.Value < 100)
			{
				using (var p = preview)
					preview = p.SetQuality(qualBar.Value);
			}

			// opacity must be set last
			if (opacityBox.Value < 100)
			{
				using (var p = preview)
					preview = p.SetOpacity((float)(opacityBox.Value / 100));
			}

			previewBox.Image = preview;

			if (storageSize == 0 || !preserveBox.Checked)
			{
				storageSize = ((byte[])new ImageConverter().ConvertTo(preview, typeof(byte[]))).Length;
				var size = storageSize.ToBytes(1);
				qualBox.Text = string.Format(Resx.ResizeImageDialog_qualBox_Size, size);

				logger.WriteTime($"estimated {WidthPixels} x {HeightPixels} = {size}");
			}
		}


		private void ResetToCurrentSize(object sender, LinkLabelLinkClickedEventArgs e)
		{
			RadioClick(absRadio, null);
			absRadio.Checked = true;
			widthUpDown.Value = viewWidth;
			heightUpDown.Value = viewHeight;

			if (image != null)
				DrawPreview();
		}


		private void ResetToOriginalSize(object sender, LinkLabelLinkClickedEventArgs e)
		{
			RadioClick(absRadio, null);
			absRadio.Checked = true;
			widthUpDown.Value = originalWidth;
			heightUpDown.Value = originalHeight;

			if (image != null)
				DrawPreview();
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
			var w = (int)(viewWidth * (pctUpDown.Value / 100));
			var h = (int)(viewHeight * (pctUpDown.Value / 100));

			if (w > 0 && h > 0)
			{
				suspended = true;
				widthUpDown.Value = w;
				heightUpDown.Value = h;

				if (image != null)
					DrawPreview();

				suspended = false;
			}
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

					if (image != null)
						DrawPreview();

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

					if (image != null)
						DrawPreview();

					suspended = false;
				}
			}
		}


		private void OpacityValueChanged(object sender, EventArgs e)
		{
			if (image != null)
				DrawPreview();
		}


		private void PresetValueChanged(object sender, EventArgs e)
		{
			if (suspended)
				return;

			suspended = true;
			widthUpDown.Value = (int)presetUpDown.Value;
			heightUpDown.Value = (int)(originalHeight * (widthUpDown.Value / originalWidth));

			if (image != null)
				DrawPreview();

			suspended = false;
		}


		private void EstimateStorage(object sender, EventArgs e)
		{
			qualLabel.Text = string.Format(Resx.ResizeImageDialog_qualLabel_Text, qualBar.Value);

			storageSize = 0;

			if (image != null)
				DrawPreview();
		}


		private void OK(object sender, EventArgs e)
		{
			if (presetRadio.Checked)
			{
				var collection = settings.GetCollection("images");
				collection.Add("mruWidth", (int)presetUpDown.Value);
				settings.SetCollection(collection);
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
