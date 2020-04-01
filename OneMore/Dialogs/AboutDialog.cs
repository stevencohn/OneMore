//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.IO;
	using System.Windows.Forms;


	internal partial class AboutDialog : Form, IOneMoreWindow
	{
		public AboutDialog()
		{
			InitializeComponent();

			Logger.DesignMode = DesignMode;

			var logpath = ((Logger)Logger.Current).LogPath;

			versionLabel.Text = "Version " + AssemblyInfo.Version;
			logLabel.Text = logpath;

			clearLogLabel.Visible = File.Exists(logpath);
		}


		protected override void OnShown(EventArgs e)
		{
			UIHelper.SetForegroundWindow(this);
		}


		private void okButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void logLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(logLabel.Text); // e.Link.LinkData.ToString());
		}

		private void clearLogLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (File.Exists(((Logger)Logger.Current).LogPath))
			{
				var result = MessageBox.Show(
					"Clear the log file now?", "Confirm",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button1);

				if (result == DialogResult.Yes)
				{
					((Logger)Logger.Current).Clear();
				}
			}
			else
			{
				MessageBox.Show("No log file is available", "It's all good", MessageBoxButtons.OK);
			}
		}
	}
}
