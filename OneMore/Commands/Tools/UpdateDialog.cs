//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Updater;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Globalization;
	using System.Windows.Forms;


	internal partial class UpdateDialog : LocalizableForm
	{
		private string url;


		public UpdateDialog()
		{
			InitializeComponent();
		}


		public UpdateDialog(IUpdateInfo info)
			: this()
		{
			if (!info.IsUpToDate)
			{
				updatePanel.Visible = false;
				Height = readyPanel.Height + okButton.Height + Padding.Top + Padding.Bottom;
				AcceptButton = okButton;
				CancelButton = okButton;

				versionBox.Text = info.InstalledVersion;
				lastUpdatedBox.Text = FormatDate(info.InstalledDate);
				url = info.InstalledUrl;
			}
			else
			{
				readyPanel.Visible = false;
				updatePanel.Top = readyPanel.Top;
				Height = updatePanel.Height + upOKButton.Height + Padding.Top + Padding.Bottom;
				AcceptButton = upOKButton;
				CancelButton = cancelButton;

				upVersionBox.Text = info.UpdateVersion;
				upDescriptionBox.Text = info.UpdateDescription;
				upReleaseDateBox.Text = FormatDate(info.UpdateDate);
				upCurrentVersionBox.Text = info.InstalledVersion;
				upLastUpdatedBox.Text = info.InstalledDate;
				url = info.UpdateUrl;
			}
		}


		private string FormatDate(string value)
		{
			if (value.Length == 8)
			{
				string[] format = { "yyyyMMdd" };

				if (DateTime.TryParseExact(value, format,
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
		}
	}
}
