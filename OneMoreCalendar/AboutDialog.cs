//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Windows.Forms;


	internal partial class AboutDialog : ThemedForm
	{
		private const float DesignDpi = 144f;

		private Font ambientFont;


		public AboutDialog()
		{
			InitializeComponent();

			// must precede the string.Format calls below
			Translator.Localize(this, new[]
			{
				"this",
				"titleLabel",
				"versionLabel",
				"copyLabel",
				"okButton",
				"pleaseLabel",
				"homeLink"
			});

			sponsorButton.SetHandCursor();

			versionLabel.Text = string.Format(versionLabel.Text, AssemblyInfo.Version + AssemblyInfo.BuildTag);
			copyLabel.Text = string.Format(copyLabel.Text, DateTime.Now.Year);
		}


		protected override void OnLoad(EventArgs e)
		{
			// the ambient font comes from the system, which isn't rescaled per monitor with
			// autoscaling off; pin it to the point size the layout was designed with
			ambientFont = new Font("Segoe UI", 9F);
			Font = ambientFont;

			// the designer layout was authored at 150% (144 DPI) with autoscaling off; scale it
			// to the actual DPI, including the sponsor button images which are drawn 1:1
			this.ScaleLayout(DesignDpi);
			ScaleSponsorImages();
			CenterToParent();

			base.OnLoad(e);
			BackColor = Theme.BackColor;
		}


		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			base.OnFormClosed(e);
			ambientFont?.Dispose();
			ambientFont = null;
		}


		private void ScaleSponsorImages()
		{
			var factor = DeviceDpi / DesignDpi;
			if (Math.Abs(factor - 1f) <= 0.01f)
			{
				return;
			}

			static Image Resize(Image image, float factor)
			{
				var bitmap = new Bitmap(
					(int)Math.Round(image.Width * factor),
					(int)Math.Round(image.Height * factor));

				using var g = Graphics.FromImage(bitmap);
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;
				g.DrawImage(image, 0, 0, bitmap.Width, bitmap.Height);
				return bitmap;
			}

			// MoreButton disposes its own Image and ImageOver
			if (sponsorButton.Image is not null)
			{
				var image = sponsorButton.Image;
				sponsorButton.Image = Resize(image, factor);
				image.Dispose();
			}

			if (sponsorButton.ImageOver is not null)
			{
				var image = sponsorButton.ImageOver;
				sponsorButton.ImageOver = Resize(image, factor);
				image.Dispose();
			}
		}


		private void OK(object sender, EventArgs e)
		{
			Close();
		}


		private void GoHome(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(homeLink.Text);
		}

		private void GotoSponsorship(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start((string)sponsorButton.Tag);
		}
	}
}
