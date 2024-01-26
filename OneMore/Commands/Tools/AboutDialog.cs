//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Diagnostics;
	using System.Drawing;
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

			// resize background image if low-def monitor
			var img = manager.DarkMode
				? Resx.sponsor_dark
				: Resx.sponsor_light;

			if (img.Height > sponsorButton.Height || img.Width > sponsorButton.Width)
			{
				sponsorButton.BackgroundImage = new Bitmap(img,
					(int)(img.Width * 0.65),
					(int)(img.Height * 0.65));

				img.Dispose();
			}
			else
			{
				sponsorButton.BackgroundImage = img;
			}
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
		}


		private void GoGitHub(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(Resx.OneMore_GitHub);
		}


		private void GotoSponsorship(object sender, EventArgs e)
		{
			Process.Start(Resx.OneMore_Sponsor);
		}


		// async event handlers should be be declared 'async void'
		private async void CheckForUpdates(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var command = await factory.Run<UpdateCommand>(true) as UpdateCommand;
			if (command.Updated)
			{
				Close();
			}
		}


		private void OpenLog(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(logLabel.Text);
		}


		// async event handlers should be be declared 'async void'
		private async void ClearLog(object sender, LinkLabelLinkClickedEventArgs e)
		{
			await factory.Run<ClearLogCommand>();
		}
	}
}
