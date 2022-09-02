//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.IO;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class AboutDialog : UI.LocalizableForm
	{
		private readonly CommandFactory factory;


		public AboutDialog()
		{
			InitializeComponent();
			sponsorButton.SetHandCursor();

			Logger.SetDesignMode(DesignMode);
		}


		public AboutDialog(CommandFactory factory)
			: this()
		{
			this.factory = factory;

			versionLabel.Text = string.Format(Resx.AboutDialog_versionLabel_Text, AssemblyInfo.Version);
			copyLabel.Text = string.Format(Resx.AboutDialog_copyLabel_Text, DateTime.Now.Year);

			var logpath = Logger.Current.LogPath;
			logLabel.Text = logpath;

			clearLogLabel.Visible = File.Exists(logpath);

			if (NeedsLocalizing())
			{
				Text = Resx.AboutDialog_Text;

				Localize(new string[]
				{
					"titleLabel",
					"okButton=word_OK",
					"clearLogLabel",
					"updateLink"
				});
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


		// async event handlers should be be declared 'async void'
		private async void CheckForUpdates(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var command = await factory.Run<UpdateCommand>(true, this) as UpdateCommand;
			if (command.Updated)
			{
				Close();
			}
		}


		private void OpenLog(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(logLabel.Text);
		}


		// async event handlers should be be declared 'async void'
		private async void ClearLog(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var cmd = new ClearLogCommand();
			cmd.SetLogger(Logger.Current);
			await cmd.Execute();
		}
	}
}
