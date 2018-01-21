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

			versionLabel.Text = "Version " + AssemblyInfo.Version;
		}


		protected override void OnShown (EventArgs e)
		{
			UIHelper.SetForegroundWindow(this);
		}


		private void okButton_Click (object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
