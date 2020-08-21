//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Helpers.Updater;
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

			var year = DateTime.Now.Year;
			copyLabel.Text = $"Copyright \u00a9 2016-{year} Steven M Cohn";

			logLabel.Text = logpath;

			clearLogLabel.Visible = File.Exists(logpath);
		}


		protected override void OnShown(EventArgs e)
		{
			Location = new System.Drawing.Point(Location.X, Location.Y - Height);
			UIHelper.SetForegroundWindow(this);
		}


		private void OK(object sender, EventArgs e)
		{
			Close();
		}


		private void GoHome(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(homeLink.Text);
		}


		private async void CheckForUpdates(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var updater = new Updater();

			if (await updater.FetchLatestRelease())
			{
				if (updater.UpdateToDate(AssemblyInfo.Version))
				{
					MessageBox.Show("You have the latest version", "You're good to go!");
					return;
				}

				var answer = MessageBox.Show(
					"A new version is available:" +
					$"\n\n   {updater.Tag} - \"{updater.Name}\"" +
					$"\n\nDo you want to update now?\nOneNote will close automatically", "Update OneMore",
					MessageBoxButtons.YesNo);

				if (answer == DialogResult.Yes)
				{
					var updating = await updater.Update();
					if (updating)
					{
						Close();
					}
				}
			}
		}


		private void OpenLog(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(logLabel.Text);
		}


		private void ClearLog(object sender, LinkLabelLinkClickedEventArgs e)
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
