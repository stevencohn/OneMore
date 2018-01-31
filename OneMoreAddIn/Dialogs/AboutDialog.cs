//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Windows.Forms;


	internal partial class AboutDialog : Form, IOneMoreWindow
	{
		public AboutDialog ()
		{
			InitializeComponent();

			Logger.DesignMode = DesignMode;

			versionLabel.Text = "Version " + AssemblyInfo.Version;
			logLabel.Text = ((Logger)Logger.Current).Path;
		}


		protected override void OnShown (EventArgs e)
		{
			UIHelper.SetForegroundWindow(this);
		}


		private void okButton_Click (object sender, EventArgs e)
		{
			Close();
		}

		private void logLabel_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(logLabel.Text); // e.Link.LinkData.ToString());
		}
	}
}
