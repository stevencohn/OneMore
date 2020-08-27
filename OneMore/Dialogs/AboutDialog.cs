//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Dialogs
{
	using River.OneMoreAddIn.Helpers.Updater;
	using System;
	using System.IO;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class AboutDialog : LocalizableForm
	{

		public AboutDialog()
		{
			InitializeComponent();

			Logger.DesignMode = DesignMode;

			versionLabel.Text = string.Format(Resx.AboutDialog_versionLabel_Text, AssemblyInfo.Version);
			copyLabel.Text = string.Format(Resx.AboutDialog_copyLabel_Text, DateTime.Now.Year);

			var logpath = ((Logger)Logger.Current).LogPath;
			logLabel.Text = logpath;

			clearLogLabel.Visible = File.Exists(logpath);

			if (NeedsLocalizing())
			{
				Text = Resx.AboutDialoge_Text;

				Localize(new string[]
				{
					"titleLabel",
					"okButton",
					"clearLogLabel",
					"updateLink"
				});
			}
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
				if (updater.IsUpToDate(AssemblyInfo.Version))
				{
					MessageBox.Show(
						Resx.AboutDialog_LatestMessage,
						Resx.AboutDialog_LatestTitle);

					return;
				}

				var answer = MessageBox.Show(
					string.Format(Resx.AboutDialog_NewVersionMessage, updater.Tag, updater.Name),
					Resx.AboutDialog_NewVersionTitle,
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
					Resx.AboutDialog_ClearLogMessage,
					Resx.AboutDialog_ClearLogTitle,
					MessageBoxButtons.YesNo, MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button1);

				if (result == DialogResult.Yes)
				{
					((Logger)Logger.Current).Clear();
				}
			}
			else
			{
				MessageBox.Show(
					Resx.AboutDialog_ClearLogNoneMessage,
					Resx.AboutDialog_ClearLogNoneTitle,
					MessageBoxButtons.OK);
			}
		}
	}
}
