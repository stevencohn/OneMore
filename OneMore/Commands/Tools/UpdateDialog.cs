//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Tools.Updater;
	using System;
	using System.Globalization;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class UpdateDialog : UI.LocalizableForm
	{
		private readonly string url;


		public UpdateDialog()
		{
			InitializeComponent();

			timer.Enabled = true;
			timer.Start();
		}


		public UpdateDialog(IUpdateReport info)
			: this()
		{
			if (info.IsUpToDate)
			{
				if (NeedsLocalizing())
				{
					Text = Resx.UpdateDialog_Text;
					Localize(new string[]
					{
						"currentLabel",
						"versionLabel",
						"lastpdatedLabel",
						"releaseNotesLink",
						"okButton=word_OK"
					});
				}

				updatePanel.Visible = false;
				Height = readyPanel.Height + okButton.Height + Padding.Top + Padding.Bottom * 2;
				AcceptButton = okButton;
				CancelButton = okButton;

				versionBox.Text = info.InstalledVersion;
				lastUpdatedBox.Text = FormatDate(info.InstalledDate);
				url = info.InstalledUrl;
			}
			else
			{
				if (NeedsLocalizing())
				{
					Text = Resx.UpdateDialog_Text;
					Localize(new string[]
					{
						"upIntroLabel",
						"upVersionLabel",
						"upDescriptionLabel",
						"upReleaseDateLabel",
						"upReleaseNotesLink",
						"upCurrentVersionLabel",
						"upLastUpdatedLabel",
						"upOKButton",
						"cancelButton=word_Cancel"
					});
				}

				readyPanel.Visible = false;
				updatePanel.Top = readyPanel.Top;
				Height = updatePanel.Height + upOKButton.Height + Padding.Top + Padding.Bottom * 2;
				AcceptButton = upOKButton;
				CancelButton = cancelButton;

				upVersionBox.Text = info.UpdateVersion;
				upDescriptionBox.Text = info.UpdateDescription;
				upReleaseDateBox.Text = FormatDate(info.UpdateDate);
				upCurrentVersionBox.Text = info.InstalledVersion;
				upLastUpdatedBox.Text = FormatDate(info.InstalledDate);
				url = info.UpdateUrl;
			}
		}


		private string FormatDate(string value)
		{
			if (value.Length == 8)
			{
				if (DateTime.TryParseExact(value, new string[] { "yyyyMMdd" },
					CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
				{
					return date.ToShortDateString();
				}
			}
			else
			{
				if (DateTime.TryParse(value, out var date))
				{
					return date.ToShortDateString();
				}
			}

			return value;
		}


		private void GotoReleaseNotes(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(url);
			timer.Enabled = false;
			TopMost = false;
		}

		private void TimerTick(object sender, EventArgs e)
		{
			TopMost = true;
		}
	}
}
