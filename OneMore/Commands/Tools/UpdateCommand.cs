//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
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
			// intentional user request from About box; otherwise, silent check on startup
			var requested = args.Length > 0 && args[0] is bool req && req;

			if (!HttpClientFactory.IsNetworkAvailable())
			{
				if (requested)
				{
					ShowInfo(Properties.Resources.NetwordConnectionUnavailable);
				}

				return;
			}

			var updater = new Updater();

			if (!await updater.FetchLatestRelease())
			{
				if (requested)
				{
					MoreMessageBox.ShowErrorWithLogLink(owner,
						"Error fetching latest release; please see logs");
				}

				return;
			}

			// user previously asked to skip this release and has not intentionally
			// requested for a new update check from the About box
			if (updater.IsSkippedRelease && !requested)
			{
				return;
			}

			if (updater.IsUpToDate)
			{
				if (requested)
				{
					// up to date...
					using var dialog = new UpdateDialog(updater);
					dialog.ShowDialog(owner);
				}

				return;
			}

			using var question = new UpdateDialog(updater);
			var result = question.ShowDialog(owner);
			if (result == DialogResult.OK)
			{
				Updated = await updater.Update();
			}
			else if (result == DialogResult.Ignore)
			{
				updater.SkipRelease();
			}
		}
	}
}
