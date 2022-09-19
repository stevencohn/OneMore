﻿//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Tools.Updater;
	using River.OneMoreAddIn.UI;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	internal class UpdateCommand : Command
	{
		public UpdateCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public bool Updated { get; private set; }


		public override async Task Execute(params object[] args)
		{
			if (!HttpClientFactory.IsNetworkAvailable())
			{
				if (args.Length > 0 && args[0] is bool report && report)
				{
					UIHelper.ShowInfo(Properties.Resources.NetwordConnectionUnavailable);
				}

				return;
			}

			var updater = new Updater();

			if (!await updater.FetchLatestRelease())
			{
				if (args.Length > 0 && args[0] is bool report && report)
				{
					MoreMessageBox.ShowErrorWithLogLink(owner,
						"Error fetching latest release; please see logs");
				}

				return;
			}

			if (updater.IsUpToDate)
			{
				if (args.Length > 0 && args[0] is bool report && report)
				{
					// up to date...
					using (var dialog = new UpdateDialog(updater) { VerticalOffset = -2 })
					{
						dialog.ShowDialog(args.Length > 1 && args[0] is AboutDialog about
							? about : new OneNote().Window);
					}
				}

				return;
			}

			DialogResult answer;
			using (var dialog = new UpdateDialog(updater) { VerticalOffset = -2 })
			{
				answer = dialog.ShowDialog(args.Length > 1 && args[0] is AboutDialog about
					? about : new OneNote().Window);
			}

			if (answer == DialogResult.OK)
			{
				Updated = await updater.Update();
			}
		}
	}
}
