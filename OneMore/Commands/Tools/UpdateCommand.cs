//************************************************************************************************
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
					ShowInfo(Properties.Resources.NetwordConnectionUnavailable);
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
					using var dialog = new UpdateDialog(updater);
					dialog.ShowDialog(owner);
				}

				return;
			}

			using var question = new UpdateDialog(updater);
			if (question.ShowDialog(owner) == DialogResult.OK)
			{
				Updated = await updater.Update();
			}
		}
	}
}
