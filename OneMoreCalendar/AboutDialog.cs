//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class AboutDialog : Form
	{

		public AboutDialog()
		{
			InitializeComponent();

			versionLabel.Text = string.Format(versionLabel.Text, AssemblyInfo.Version);
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
	}
}
