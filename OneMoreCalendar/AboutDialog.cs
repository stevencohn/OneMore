//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;


	internal partial class AboutDialog : Form
	{
		private readonly Image image;
		private readonly Image imageOver;


		public AboutDialog()
		{
			InitializeComponent();

			var size = new Size(
				sponsorButton.ClientSize.Width - sponsorButton.Padding.Left - sponsorButton.Padding.Right - 5,
				sponsorButton.ClientSize.Height - sponsorButton.Padding.Top - sponsorButton.Padding.Bottom - 5);

			image = new Bitmap(Properties.Resources.Sponsor, size);
			imageOver = new Bitmap(Properties.Resources.SponsorOver, size);

			sponsorButton.SetHandCursor();
			sponsorButton.Image = image;

			// TODO: beta
			versionLabel.Text = string.Format(versionLabel.Text, AssemblyInfo.Version) + " (BETA)";
			copyLabel.Text = string.Format(copyLabel.Text, DateTime.Now.Year);
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

		private void EnterSponsor(object sender, EventArgs e)
		{
			sponsorButton.Image = imageOver;
		}

		private void LeaveSponsor(object sender, EventArgs e)
		{
			sponsorButton.Image = image;
		}
	}
}
