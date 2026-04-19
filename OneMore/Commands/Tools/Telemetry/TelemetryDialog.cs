//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Diagnostics;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class TelemetryDialog : UI.MoreForm
	{

		public TelemetryDialog()
		{
			InitializeComponent();

			Logger.SetDesignMode(DesignMode);
		}


		public TelemetryDialog(CommandFactory factory)
			: this()
		{
			if (NeedsLocalizing())
			{
				Text = Resx.AboutDialog_Text;

				Localize(new string[]
				{
					"subtitleLabel",
					"readLabel",
					"yesButton",
					"noButton"
				});
			}

			titleLabel.Text = string.Format(Resx.TelemetryDialog_titleLabel_Text, Resx.ProgramName);
			whyBox.Text = string.Format(Resx.TelemetryDialog_whyBox_Text, Resx.ProgramName);
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}


		private void GotoDesign(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(Resx.TelemetryDialog_designLink);
			yesButton.Focus();
		}


		private void GotoGithub(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(Resx.OneMore_GitHub);
			yesButton.Focus();
		}


		private void EnableTelemetry(object sender, EventArgs e)
		{
			SetTelemetry(true);
			Close();
		}


		private void DisableTelemetry(object sender, EventArgs e)
		{
			SetTelemetry(false);
			Close();
		}


		private static void SetTelemetry(bool enabled)
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection(nameof(GeneralSheet));
			settings.Add("telemetry", enabled);
			provider.SetCollection(settings);
			provider.Save();
		}
	}
}
