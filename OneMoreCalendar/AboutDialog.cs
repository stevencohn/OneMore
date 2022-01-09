﻿//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Windows.Forms;


	internal partial class AboutDialog : Form
	{

		public AboutDialog()
		{
			InitializeComponent();

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
	}
}
