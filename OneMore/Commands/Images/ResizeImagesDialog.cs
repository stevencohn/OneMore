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
	using Resx = Properties.Resources;


	internal partial class ResizeImagesDialog : UI.LocalizableForm
	{
		private readonly Image image;
		private readonly int viewWidth;
		private readonly int viewHeight;
		private readonly MagicScaling scaling;
		private readonly int originalWidth;
		private readonly int originalHeight;
		private SettingsProvider settings;
		private Image preview;
		private int storageSize;
		private bool suspended = true;
		private bool mruWidth = true;


		/// <summary>
		/// Initializes a new dialog to resize all images on the page to a standard width
		/// and height with respective ratio
		/// </summary>
		public ResizeImagesDialog(bool hasBackgroundImages)
		{
			Initialize();

			VerticalOffset = -(Height / 3);

			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;

			originalWidth = originalHeight = 1;

			// hide controls that do not apply...

			viewSizeLabel.Text = Resx.ResizeImagesDialog_appliesTo;
			allLabel.Location = viewSizeLink.Location;
			allLabel.Visible = true;

			storageLabel.Text = Resx.word_Limit;
			storageLabel.Top -= 5;
			limitsBox.Location = new Point(storedSizeLabel.Location.X, storedSizeLabel.Location.Y - 5);
			limitsBox.SelectedIndex = 0;
			limitsBox.Visible = true;

			viewSizeLink.Visible
				= imageSizeLabel.Visible
				= imageSizeLink.Visible
				= storedSizeLabel.Visible = false;

			lockButton.Checked = true;
			lockButton.Enabled = false;
			heightBox.Enabled = false;

			previewGroup.Visible = false;
			Width -= (previewGroup.Width + Padding.Right);

			presetRadio.Checked = true;
			RadioClick(presetRadio, null);

			repositionBox.Visible = hasBackgroundImages;

			scaling = null;

			LoadSettings(false);
		}


		/// <summary>
		/// Initializes a new dialog to resize a selected image
		/// </summary>
		/// <param name="image"></param>
		public ResizeImagesDialog(Image image, int viewWidth, int viewHeight)
		{
			Initialize();

			MinimumSize = new Size(Width, Height);

			this.image = image;

			this.viewWidth = viewWidth;
			this.viewHeight = viewHeight;

			viewSizeLink.Text = string.Format(
				Resx.ResizeImagesDialog_sizeLink_Text, this.viewWidth, this.viewHeight);

			originalWidth = image.Width;
			originalHeight = image.Height;

			imageSizeLink.Text = string.Format(
				Resx.ResizeImagesDialog_sizeLink_Text, originalWidth, originalHeight);

			widthBox.Value = viewWidth;
			heightBox.Value = viewHeight;

			scaling = new MagicScaling(image.HorizontalResolution, image.VerticalResolution);

			LoadSettings(true);
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
					"viewSizeLabel",
					"imageSizeLabel",
					"storageLabel=word_Storage",
					"allLabel",
					"limitsBox",
					"pctRadio",
					"pctLabel=word_PercentSymbol",
					"absRadio",
					"widthLabel=word_Width",
					"heightLabel",
					"presetRadio",
					"presetLabel=word_Width",
					"opacityLabel=word_Opacity",
					"brightnessLabel=word_Brightness",
					"contrastLabel=word_Contrast",
					"saturationLabel=word_Saturation",
					"styleLabel=word_Stylize",
					"styleBox",
					"qualityLabel=word_Quality",
					"preserveBox",
					"previewGroup=word_Preview",
					"resetLinkLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			//lockButton.AutoSize = false;
			//lockButton.Size = new Size(28, 28);

			//var (fx, fy) = UIHelper.GetScalingFactors();
			//logger.WriteLine($"fx {fx} fy {fy}");

			styleBox.SelectedIndex = 0;
		}


		private void LoadSettings(bool oneImage)
		{
			settings = new SettingsProvider();
			var collection = settings.GetCollection("images");

			int value = collection.Get("mruSizeBy", oneImage ? 0 : 2);
			if (value == 0)
			{
				pctRadio.Checked = true;
				percentBox.Value = collection.Get("mruPercent", 100);
				RadioClick(pctRadio, EventArgs.Empty);
			}
			else if (value == 1)
			{
				absRadio.Checked = true;
				RadioClick(absRadio, EventArgs.Empty);
			}
			else
			{
				presetRadio.Checked = true;
				RadioClick(presetRadio, EventArgs.Empty);
			}

			presetBox.Value = collection.Get("mruWidth",
				settings.GetCollection(nameof(ImagesSheet)).Get("presetWidth", 500));

			if (!oneImage)
			{
				limitsBox.SelectedIndex = collection.Get("limits", 0);
				if (limitsBox.SelectedIndex < 0)
				{
					limitsBox.SelectedIndex = 0;
				}
			}

			opacityBox.Value = collection.Get("opacity", 100);
			brightnessBox.Value = collection.Get("brightness", 0);
			contrastBox.Value = collection.Get("contrast", 0);
			saturationBox.Value = collection.Get("saturation", 0);
			qualBox.Value = collection.Get("quality", 100);
			preserveBox.Checked = collection.Get("preserve", true);
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			suspended = false;
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		public decimal ImageHeight => heightBox.Value;


		public decimal ImageWidth => widthBox.Value;


		public ResizeOption ResizeOption =>
			// all
			limitsBox.SelectedIndex == 0 ? ResizeOption.All
			// do not shrink
			: limitsBox.SelectedIndex == 1 ? ResizeOption.OnlyEnlarge
			// do not enlarge
			: ResizeOption.OnlyShrink;


		public bool LockAspect => lockButton.Checked;


		public bool NeedsRewrite =>
			!preserveBox.Checked ||
			opacityBox.Value < 100 ||
			brightnessBox.Value != 0 ||
			contrastBox.Value != 0 ||
			saturationBox.Value != 0 ||
			styleBox.SelectedIndex != 0 ||
			qualBar.Value < 100;


		public decimal Percent => pctRadio.Checked ? percentBox.Value : 0;


		public bool RepositionImages => repositionBox.Checked;



		/// <summary>
		/// Gets a new image after applying the desired modifications to the source image.
		/// </summary>
		/// <returns>A new image</returns>
		public Image GetImage()
		{
			previewBox.Image = null;
			preview.Dispose();

			preview = ImageWidth == image.Width && ImageHeight == image.Height
				? (Image)image.Clone()
				: image.Resize((int)ImageWidth, (int)ImageHeight);

			return Adjust(preview);
		}


		public Image Adjust(Image image)
		{
			var adjusted = image;

			if (qualBar.Value < 100)
			{
				using var p = adjusted;
				adjusted = p.SetQuality(qualBar.Value);
			}

			if (brightnessBox.Value != 0 || contrastBox.Value != 0)
			{
				using var p = adjusted;
				adjusted = p.SetBrightnessContrast(
					(float)brightnessBox.Value / 100f,
					(float)contrastBox.Value / 100f);
			}

			if (saturationBox.Value != 0)
			{
				using var p = adjusted;
				adjusted = p.SetSaturation((float)saturationBox.Value / 100f);
			}

			if (styleBox.SelectedIndex == 1)
			{
				using var p = adjusted;
				adjusted = p.ToGrayscale();
			}
			else if (styleBox.SelectedIndex == 2)
			{
				using var p = adjusted;
				adjusted = p.ToSepia();
			}
			else if (styleBox.SelectedIndex == 3)
			{
				using var p = adjusted;
				adjusted = p.ToPolaroid();
			}

			// opacity must be set last
			if (opacityBox.Value < 100)
			{
				using var p = adjusted;
				adjusted = p.SetOpacity((float)(opacityBox.Value / 100));
			}

			return adjusted;
		}


		private void DrawPreview()
		{
			previewBox.Image = null;
			preview?.Dispose();

			// NOTE: OneNote's zoom factor skews viewable images, e.g. on a HDPI display at
			// 150% scaling, an image displayed at 80% zoom is equivalent to the raw painting
			// of an image at 100% of its size...

			//var ratio = scaling.GetRatio(image, previewBox.Width, previewBox.Height, 0);
			var width = Math.Round(ImageWidth * (decimal)scaling.FactorX); //(decimal)ratio);
			var height = Math.Round(ImageHeight * (decimal)scaling.FactorY); // (decimal)ratio);

			int w;
			int h;

			if (width <= previewBox.Width && height <= previewBox.Height)
			{
				w = (int)width;
				h = (int)height;
			}
			else
			{
				w = previewBox.Width;
				h = previewBox.Height;
				if (width > w && height > h)
				{
					if (width > height)
					{
						h = (int)(height * (w / width));
					}
					else
					{
						w = (int)(width * (h / height));
					}
				}
				else if (width > w)
				{
					h = (int)(height * (w / width));
				}
				else
				{
					w = (int)(width * (h / height));
				}
			}

			preview = Adjust(image.Resize(w, h));
			previewBox.Image = preview;

			if (storageSize == 0 || !preserveBox.Checked)
			{
				storageSize = ((byte[])new ImageConverter().ConvertTo(preview, typeof(byte[]))).Length;
				var size = storageSize.ToBytes(1);
				storedSizeLabel.Text = size;

				//logger.WriteTime($"estimated {ImageWidth} x {ImageHeight} = {size}");
			}
		}


		private void DrawOnSizeChanged(object sender, EventArgs e)
		{
			base.OnSizeChanged(e);
			if (image != null)
			{
				DrawPreview();
			}
		}


		private void ViewSizeClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			RadioClick(absRadio, null);
			absRadio.Checked = true;
			percentBox.Value = 100;
			widthBox.Value = viewWidth;
			heightBox.Value = viewHeight;
			mruWidth = true;

			if (image != null)
				DrawPreview();
		}


		private void OriginalSizeClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			RadioClick(absRadio, null);
			absRadio.Checked = true;
			percentBox.Value = (decimal)originalWidth / viewWidth * 100;
			widthBox.Value = originalWidth;
			heightBox.Value = originalHeight;
			mruWidth = true;

			if (image != null)
				DrawPreview();
		}


		private void RadioClick(object sender, EventArgs e)
		{
			percentBox.Enabled = sender == pctRadio;

			var abs = sender == absRadio;
			lockButton.Enabled = abs;
			widthBox.Enabled = abs;
			heightBox.Enabled = abs && image != null;
			presetBox.Enabled = sender == presetRadio;
			mruWidth = true;
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
			if (suspended)
				return;

			var w = (int)(viewWidth * (percentBox.Value / 100));
			var h = (int)(viewHeight * (percentBox.Value / 100));

			if (w > 0 && h > 0)
			{
				suspended = true;
				widthBox.Value = w;
				heightBox.Value = h;
				mruWidth = true;

				if (image != null)
					DrawPreview();

				suspended = false;
			}
		}


		private void LockAspectCheckedChanged(object sender, EventArgs e)
		{
			lockButton.BackgroundImage = lockButton.Checked ? Resx.Locked : Resx.Unlocked;

			if (image == null)
			{
				heightBox.Enabled = !lockButton.Checked;
				if (!heightBox.Enabled)
				{
					heightBox.Value = 0;
				}
				return;
			}

			if (!lockButton.Checked)
			{
				return;
			}

			suspended = true;
			decimal aspect;
			if (mruWidth)
			{
				aspect = widthBox.Value < viewWidth
					? widthBox.Value / viewWidth
					: viewWidth / widthBox.Value;

				heightBox.Value = (int)(viewHeight * aspect);
			}
			else
			{
				aspect = heightBox.Value < viewHeight
					? heightBox.Value / viewHeight
					: viewHeight / heightBox.Value;

				widthBox.Value = (int)(viewWidth * aspect);
			}

			percentBox.Value = widthBox.Value / viewWidth * 100;

			DrawPreview();

			suspended = false;
		}


		private void WidthValueChanged(object sender, EventArgs e)
		{
			if (suspended || !widthBox.Enabled)
			{
				return;
			}

			suspended = true;

			if (image != null)
			{
				if (lockButton.Checked)
				{
					heightBox.Value = (int)(viewHeight * (widthBox.Value / viewWidth));
				}

				percentBox.Value = heightBox.Value / viewHeight * 100;
			}

			suspended = false;
			mruWidth = true;

			if (image != null)
				DrawPreview();
		}


		private void HeightValueChanged(object sender, EventArgs e)
		{
			if (suspended || !heightBox.Enabled)
			{
				return;
			}

			suspended = true;

			if (image != null)
			{
				if (lockButton.Checked)
				{
					widthBox.Value = (int)(viewWidth * (heightBox.Value / viewHeight));
				}

				percentBox.Value = widthBox.Value / viewWidth * 100;
			}

			suspended = false;
			mruWidth = false;

			if (image != null)
				DrawPreview();
		}


		private void PresetValueChanged(object sender, EventArgs e)
		{
			if (suspended)
				return;

			suspended = true;
			widthBox.Value = (int)presetBox.Value;

			if (image != null)
			{
				heightBox.Value = (int)(viewHeight * (widthBox.Value / viewWidth));
				percentBox.Value = widthBox.Value / viewWidth * 100;
			}

			mruWidth = true;

			if (image != null)
				DrawPreview();

			suspended = false;
		}


		private void SlideValueChanged(object sender, EventArgs e)
		{
			if (sender == opacityBox) opacityBar.Value = (int)opacityBox.Value;
			else if (sender == opacityBar) opacityBox.Value = opacityBar.Value;
			else if (sender == brightnessBox) brightnessBar.Value = (int)brightnessBox.Value;
			else if (sender == brightnessBar) brightnessBox.Value = brightnessBar.Value;
			else if (sender == contrastBox) contrastBar.Value = (int)contrastBox.Value;
			else if (sender == contrastBar) contrastBox.Value = contrastBar.Value;
			else if (sender == saturationBox) saturationBar.Value = (int)saturationBox.Value;
			else if (sender == saturationBar) saturationBox.Value = saturationBar.Value;

			if (image != null)
			{
				DrawPreview();
			}
		}


		private void ResetDoubleClick(object sender, EventArgs e)
		{
			if (sender == opacityLabel) opacityBox.Value = 100;
			else if (sender == brightnessLabel) brightnessBox.Value = 0;
			else if (sender == contrastLabel) contrastBox.Value = 0;
			else if (sender == saturationLabel) saturationBox.Value = 0;
			else if (sender == qualityLabel) qualBox.Value = 100;

			if (image != null)
			{
				DrawPreview();
			}
		}


		private void ResetDefaultValues(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (limitsBox.Items.Count > 0)
			{
				limitsBox.SelectedIndex = 0;
			}

			if (repositionBox.Visible)
			{
				repositionBox.Checked = true;
			}

			percentBox.Value = 100;

			presetBox.Value = new SettingsProvider()
				.GetCollection(nameof(ImagesSheet)).Get("presetWidth", 500);

			opacityBox.Value = 100;
			brightnessBox.Value = 0;
			contrastBox.Value = 0;
			saturationBox.Value = 0;
			qualBox.Value = 100;
			preserveBox.Checked = true;

			SaveSettings();
		}



		private void StylizeSelectedIndexChanged(object sender, EventArgs e)
		{
			if (image != null)
			{
				DrawPreview();
			}
		}


		private void EstimateStorage(object sender, EventArgs e)
		{
			if (sender == qualBar)
				qualBox.Value = qualBar.Value;
			else
				qualBar.Value = (int)qualBox.Value;

			storageSize = 0;

			if (image != null)
				DrawPreview();
		}


		private void OK(object sender, EventArgs e)
		{
			if (presetRadio.Checked)
			{
				SaveSettings();

				suspended = true;
				widthBox.Value = presetBox.Value;

				heightBox.Value = image == null
					? 0
					: viewHeight * (widthBox.Value / viewWidth);
			}

			DialogResult = DialogResult.OK;
		}


		private void SaveSettings()
		{
			// images/viewer setting is deprecated but preserve integrity until that is
			// gone from the wild and OpenImageWithCmd no longer uses it...

			var collection = settings.GetCollection("images");

			if (pctRadio.Checked)
			{
				collection.Add("mruSizeBy", "0");
				collection.Add("mruPercent", (int)percentBox.Value);
			}
			else
			{
				collection.Add("mruSizeBy", presetRadio.Checked ? "2" : "1");
			}

			collection.Add("mruWidth", (int)presetBox.Value);

			if (limitsBox.Items.Count > 0 && limitsBox.SelectedIndex >= 0)
			{
				collection.Add("limits", limitsBox.SelectedIndex);
			}

			collection.Add("opacity", (int)opacityBox.Value);
			collection.Add("brightness", (int)brightnessBox.Value);
			collection.Add("contrast", (int)contrastBox.Value);
			collection.Add("saturation", (int)saturationBox.Value);
			collection.Add("quality", (int)qualBox.Value);
			collection.Add("preserve", preserveBox.Checked);

			settings.SetCollection(collection);
			settings.Save();
		}


		private void Cancel(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
