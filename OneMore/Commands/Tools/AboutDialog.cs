//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class AboutDialog : UI.MoreForm
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

			versionLabel.Text = string.Format(Resx.AboutDialog_versionLabel_Text,
				AssemblyInfo.Version, GetOneNoteVersion());

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
					"githubLink",
					"updateLink",
					"pleaseLabel",
					"clearLogLabel",
					"okButton=word_OK"
				});
			}

			// reposition sponsor button based on width of translated label
			using var g = pleaseLabel.CreateGraphics();
			var size = g.MeasureString(pleaseLabel.Text, pleaseLabel.Font);
			sponsorButton.Left = (int)(pleaseLabel.Left + pleaseLabel.Margin.Right + size.Width);
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// doing this in OnLoad to ensure we have ThemeManager initialized...

			sponsorButton.BackgroundImage = manager.DarkMode
				? Resx.sponsor_dark
				: Resx.sponsor_light;
		}


		public string GetOneNoteVersion()
		{
			var processes = Process.GetProcessesByName("ONENOTE");
			if (processes.Length > 0)
			{
				return processes[0].MainModule.FileVersionInfo.ProductVersion;
			}

			return "unknown";
		}


		private void OK(object sender, EventArgs e)
		{
			Close();
		}


		private void GoHome(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(Resx.OneMore_Home);
			okButton.Focus();
		}


		private void GoGitHub(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(Resx.OneMore_GitHub);
			okButton.Focus();
		}


		private void GotoSponsorship(object sender, EventArgs e)
		{
			Process.Start(Resx.OneMore_Sponsor);
			okButton.Focus();
		}


		// async event handlers should be be declared 'async void'
		private async void CheckForUpdates(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var command = await factory.Run<UpdateCommand>(true) as UpdateCommand;
			if (command.Updated)
			{
				Close();
			}
			else
			{
				okButton.Focus();
			}
		}


		private void OpenLog(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(logLabel.Text);
			okButton.Focus();
		}


		// async event handlers should be be declared 'async void'
		private async void ClearLog(object sender, LinkLabelLinkClickedEventArgs e)
		{
			await factory.Run<ClearLogCommand>();
			okButton.Focus();
		}
	}
}
