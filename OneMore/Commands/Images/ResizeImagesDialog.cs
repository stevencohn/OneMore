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
		private bool mruWidth = true;


		/// <summary>
		/// Initializes a new dialog to resize all images on the page to a standard width
		/// and height with respective ratio
		/// </summary>
		public ResizeImagesDialog()
		{
			Initialize();

			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;

			originalWidth = originalHeight = 1;

			// hide controls that do not apply...

			currentLabel.Text = Resx.ResizeImagesDialog_applyToLabel;
			allLabel.Left = sizeLink.Left;
			allLabel.Visible = true;
			origLabel.Visible = sizeLink.Visible = origSizeLink.Visible = false;

			heightUpDown.Enabled = false;

			previewGroup.Visible = false;
			Width -= (previewGroup.Width + Padding.Right);

			presetRadio.Checked = true;
			RadioClick(presetRadio, null);

			settings = new SettingsProvider();
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
					"opacityLabel=word_Opacity",
					"opacityPctLabel=word_PercentSymbol",
					"preserveBox",
					"previewGroup=word_Preview",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});

				qualBox.Text = string.Format(Resx.ResizeImageDialog_qualBox_Size, 0);
				qualLabel.Text = string.Format(Resx.ResizeImageDialog_qualLabel_Text, qualBar.Value);
			}
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			suspended = false;
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		public decimal ImageHeight => heightUpDown.Value;


		public decimal ImageOpacity => opacityBox.Value;


		public int ImageQuality => qualBar.Value;


		public decimal ImageWidth => widthUpDown.Value;


		public bool MaintainAspect => aspectBox.Checked;


		public bool NeedsRewrite => 
			qualBar.Value < 100 ||
			opacityBox.Value < 100 ||
			!preserveBox.Checked;


		public decimal Percent => pctRadio.Checked ? pctUpDown.Value : 0;


		public bool PreserveSize => preserveBox.Checked;



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

			if (ImageWidth <= previewBox.Width && ImageHeight <= previewBox.Height)
			{
				preview = image.Resize((int)ImageWidth, (int)ImageHeight);
			}
			else
			{
				var w = previewBox.Width;
				var h = previewBox.Height;
				if (ImageWidth > w && ImageHeight > h)
				{
					if (ImageWidth > ImageHeight)
					{
						h = (int)(ImageHeight * (w / ImageWidth));
					}
					else
					{
						w = (int)(ImageWidth * (h / ImageHeight));
					}
				}
				else if (ImageWidth > w)
				{
					h = (int)(ImageHeight * (w / ImageWidth));
				}
				else
				{
					w = (int)(ImageWidth * (h / ImageHeight));
				}

				preview = image.Resize(w, h);
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

				//logger.WriteTime($"estimated {ImageWidth} x {ImageHeight} = {size}");
			}
		}


		private void DrawOnResize(object sender, EventArgs e)
		{
			base.OnResize(e);
			if (image != null)
			{
				DrawPreview();
			}
		}


		private void ResetToCurrentSize(object sender, LinkLabelLinkClickedEventArgs e)
		{
			RadioClick(absRadio, null);
			absRadio.Checked = true;
			pctUpDown.Value = 100;
			widthUpDown.Value = viewWidth;
			heightUpDown.Value = viewHeight;
			mruWidth = true;

			if (image != null)
				DrawPreview();
		}


		private void ResetToOriginalSize(object sender, LinkLabelLinkClickedEventArgs e)
		{
			RadioClick(absRadio, null);
			absRadio.Checked = true;
			pctUpDown.Value = (decimal)originalWidth / viewWidth * 100;
			widthUpDown.Value = originalWidth;
			heightUpDown.Value = originalHeight;
			mruWidth = true;

			if (image != null)
				DrawPreview();
		}


		private void RadioClick(object sender, EventArgs e)
		{
			pctUpDown.Enabled = sender == pctRadio;

			var abs = sender == absRadio;
			aspectBox.Enabled = abs;
			widthUpDown.Enabled = abs;
			heightUpDown.Enabled = abs && image != null;
			presetUpDown.Enabled = sender == presetRadio;
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

			var w = (int)(viewWidth * (pctUpDown.Value / 100));
			var h = (int)(viewHeight * (pctUpDown.Value / 100));

			if (w > 0 && h > 0)
			{
				suspended = true;
				widthUpDown.Value = w;
				heightUpDown.Value = h;
				mruWidth = true;

				if (image != null)
					DrawPreview();

				suspended = false;
			}
		}


		private void MaintainAspectCheckedChanged(object sender, EventArgs e)
		{
			if (image == null)
			{
				heightUpDown.Enabled = !aspectBox.Checked;
				return;
			}

			if (!aspectBox.Checked)
			{
				return;
			}

			suspended = true;
			decimal aspect;
			if (mruWidth)
			{
				aspect = widthUpDown.Value < viewWidth
					? widthUpDown.Value / viewWidth
					: viewWidth / widthUpDown.Value;

				heightUpDown.Value = (int)(viewHeight * aspect);
			}
			else
			{
				aspect = heightUpDown.Value < viewHeight
					? heightUpDown.Value / viewHeight
					: viewHeight / heightUpDown.Value;

				widthUpDown.Value = (int)(viewWidth * aspect);
			}

			pctUpDown.Value = widthUpDown.Value / viewWidth * 100;

			DrawPreview();

			suspended = false;
		}


		private void WidthValueChanged(object sender, EventArgs e)
		{
			if (suspended || !widthUpDown.Enabled)
			{
				return;
			}

			suspended = true;

			if (image != null)
			{
				if (aspectBox.Checked)
				{
					heightUpDown.Value = (int)(viewHeight * (widthUpDown.Value / viewWidth));
				}

				pctUpDown.Value = heightUpDown.Value / viewHeight * 100;
			}

			suspended = false;
			mruWidth = true;

			if (image != null)
				DrawPreview();
		}


		private void HeightValueChanged(object sender, EventArgs e)
		{
			if (suspended || !heightUpDown.Enabled)
			{
				return;
			}

			suspended = true;

			if (image != null)
			{
				if (aspectBox.Checked)
				{
					widthUpDown.Value = (int)(viewWidth * (heightUpDown.Value / viewHeight));
				}

				pctUpDown.Value = widthUpDown.Value / viewWidth * 100;
			}

			suspended = false;
			mruWidth = false;

			if (image != null)
				DrawPreview();
		}


		private void OpacityValueChanged(object sender, EventArgs e)
		{
			if (image != null)
			{
				DrawPreview();
			}
		}


		private void PresetValueChanged(object sender, EventArgs e)
		{
			if (suspended)
				return;

			suspended = true;
			widthUpDown.Value = (int)presetUpDown.Value;

			if (image != null)
			{
				heightUpDown.Value = (int)(viewHeight * (widthUpDown.Value / viewWidth));
				pctUpDown.Value = widthUpDown.Value / viewWidth * 100;
			}

			mruWidth = true;

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

				heightUpDown.Value = image == null
					? 0
					: viewHeight * (widthUpDown.Value / viewWidth);
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
