//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Windows.Forms;


	internal partial class AboutDialog : ThemedForm
	{

		public AboutDialog()
		{
			InitializeComponent();

			sponsorButton.SetHandCursor();

			// TODO: beta
			versionLabel.Text = string.Format(versionLabel.Text, AssemblyInfo.Version);
			copyLabel.Text = string.Format(copyLabel.Text, DateTime.Now.Year);
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			BackColor = Theme.BackColor;
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
