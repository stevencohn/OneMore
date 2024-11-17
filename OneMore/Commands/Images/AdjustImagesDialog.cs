//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class AdjustImagesDialog : UI.MoreForm
	{
		private readonly Image image;
		private readonly int viewWidth;
		private readonly int viewHeight;
		private readonly UI.Scaling scaling;
		private readonly int originalWidth;
		private readonly int originalHeight;
		private readonly bool singleImage;
		private SettingsProvider settings;
		private Image preview;
		private int storageSize;
		private bool suspended = true;
		private bool mruWidth = true;


		#region Lifecycle
		/// <summary>
		/// Initializes a new dialog to resize all images on the page to a standard width
		/// and height with respective ratio
		/// </summary>
		public AdjustImagesDialog()
		{
			Initialize();

			VerticalOffset = -(Height / 3);

			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;

			originalWidth = originalHeight = 1;

			singleImage = false;

			// hide controls that do not apply...

			viewSizeLabel.Text = Resx.AdjustImagesDialog_appliesTo;

			allLabel.Location = viewSizeLink.Location;
			allLabel.Visible = true;

			storageLabel.Text = Resx.word_Constraint;
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

			repositionBox.Visible = !ForegroundImages;

			scaling = null;
		}


		/// <summary>
		/// Initializes a new dialog to resize a selected image
		/// </summary>
		/// <param name="image"></param>
		public AdjustImagesDialog(Image image, int viewWidth, int viewHeight)
		{
			Initialize();

			MinimumSize = new Size(Width, Height);

			singleImage = true;

			this.image = image;

			this.viewWidth = viewWidth;
			this.viewHeight = viewHeight;

			viewSizeLink.Text = string.Format(
				Resx.AdjustImagesDialog_sizeLink_Text, this.viewWidth, this.viewHeight);

			originalWidth = image.Width;
			originalHeight = image.Height;

			imageSizeLink.Text = string.Format(
				Resx.AdjustImagesDialog_sizeLink_Text, originalWidth, originalHeight);

			widthBox.Value = viewWidth;
			heightBox.Value = viewHeight;

			scaling = new UI.Scaling(image.HorizontalResolution, image.VerticalResolution);

			previewGroup.Text = string.Format(
				Resx.AdjustImagesDialog_previewGroup_Text, image.GetSignature());
		}


		private void Initialize()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.AdjustImagesDialog_Text;

				Localize(new string[]
				{
					"viewSizeLabel",
					"imageSizeLabel",
					"storageLabel=word_Storage",
					"limitsBox",
					"autoSizeRadio",
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
					"repositionBox",
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


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			LoadSettings();

			if (allLabel.Visible)
			{
				allLabel.Text = string.Format(ForegroundImages
					? Resx.AdjustImagesDialog_allLabelForeground
					: Resx.AdjustImagesDialog_allLabelBackground, ImageCount);
			}

			suspended = false;
		}


		private void LoadSettings()
		{
			settings = new SettingsProvider();
			var collection = settings.GetCollection("images");

			int value = collection.Get("mruSizeBy", singleImage ? 0 : 2);
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
			else if (value == 2)
			{
				presetRadio.Checked = true;
				RadioClick(presetRadio, EventArgs.Empty);
			}
			else
			{
				autoSizeRadio.Checked = true;
				RadioClick(autoSizeRadio, EventArgs.Empty);
			}

			presetBox.Value = collection.Get("mruWidth",
				settings.GetCollection(nameof(ImagesSheet)).Get("presetWidth", 500));

			if (singleImage)
			{
				DrawPreview();
			}
			else
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
			repositionBox.Checked = collection.Get("stack", true);
		}
		#endregion Lifecycle


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		public bool ForegroundImages { private get; set; }

		public int ImageCount { private get; set; }

		public bool RepositionImages => repositionBox.Checked;


		public ImageEditor GetImageEditor(Image image)
		{
			var editor = new ImageEditor();
			if (qualBar.Value < 100) editor.Quality = qualBar.Value;
			if (brightnessBar.Value != 0) editor.Brightness = brightnessBar.Value / 100f;
			if (contrastBar.Value != 0) editor.Contrast = contrastBar.Value / 100f;
			if (saturationBar.Value != 0) editor.Saturation = saturationBar.Value / 100f;

			if (styleBox.SelectedIndex > 0)
				editor.Style = styleBox.SelectedIndex switch
				{
					1 => ImageEditor.Stylization.GrayScale,
					2 => ImageEditor.Stylization.Sepia,
					3 => ImageEditor.Stylization.Polaroid,
					_ => ImageEditor.Stylization.Invert
				};

			if (opacityBox.Value < 100) editor.Opacity = (float)opacityBox.Value / 100f;

			// size...

			if (autoSizeRadio.Checked)
			{
				editor.AutoSize = true;
			}
			else if (presetRadio.Checked && (int)presetBox.Value != image.Width)
			{
				editor.Size = new Size(
					(int)presetBox.Value,
					(int)(image.Height * (presetBox.Value / image.Width))
					);
			}
			else if (absRadio.Checked &&
				(widthBox.Value != image.Width || heightBox.Value != image.Height))
			{
				editor.Size = new Size((int)widthBox.Value, (int)heightBox.Value);
			}
			else // %
			{
				if (singleImage)
				{
					var w = (int)(viewWidth * percentBox.Value / 100);
					var h = (int)(viewHeight * percentBox.Value / 100);

					if (w != image.Width || h != image.Height)
					{
						editor.Size = new Size(w, h);
					}
				}
				else
				{
					editor.SizePercent = (float)percentBox.Value / 100;
				}
			}

			editor.PreserveStorageSize = preserveBox.Checked;

			// limitsBox is Visible=false by now, but check if selected index is not default=0
			// and, if so, update editor; note that the default editor.Constraint=All

			if (limitsBox.SelectedIndex == 1)
			{
				editor.Constraint = ImageEditor.SizeConstraint.OnlyEnlarge;
			}
			else if (limitsBox.SelectedIndex == 2)
			{
				editor.Constraint = ImageEditor.SizeConstraint.OnlyShrink;
			}

			return editor;
		}

		#region Internals
		private void DrawPreview()
		{
			//logger.StartClock();

			previewBox.Image = null;
			preview?.Dispose();

			// NOTE: OneNote's zoom factor skews viewable images, e.g. on a HDPI display at
			// 150% scaling, an image displayed at 80% zoom is equivalent to the raw painting
			// of an image at 100% of its size...

			//var ratio = scaling.GetRatio(image, previewBox.Width, previewBox.Height, 0);
			var width = Math.Round(widthBox.Value * (decimal)scaling.FactorX); //(decimal)ratio);
			var height = Math.Round(heightBox.Value * (decimal)scaling.FactorY); // (decimal)ratio);

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

			var editor = GetImageEditor(image);
			// customize size for preview box
			editor.Size = new Size(w, h);
			// always force create a new image for preview
			editor.PreserveStorageSize = false;

			preview = editor.Apply(image);
			previewBox.Image = preview;

			if (storageSize == 0 || !preserveBox.Checked)
			{
				var bytes = ((byte[])new ImageConverter().ConvertTo(preview, typeof(byte[])));
				storageSize = bytes.Length;
				var size = storageSize.ToBytes(1);
				storedSizeLabel.Text = size;

				//var s64 = Convert.ToBase64String(bytes).Length.ToBytes();
				//logger.WriteTime($"estimated {widthBox.Value} x {heightBox.Value} = {size} bytes (base64 = {s64})");
			}

			//logger.StopClock();
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
			lockButton.Image = lockButton.Checked ? Resx.Locked : Resx.Unlocked;

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
			SaveSettings();
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
				var by = absRadio.Checked ? "1" : presetRadio.Checked ? "2" : "3";
				collection.Add("mruSizeBy", by);
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
			collection.Add("stack", repositionBox.Checked);

			settings.SetCollection(collection);
			settings.Save();
		}


		private void Cancel(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
		#endregion Internals
	}
}
