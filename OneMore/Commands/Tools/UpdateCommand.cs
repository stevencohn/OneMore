//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Updater;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


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
			var updater = new Updater();

			if (!await updater.FetchLatestRelease())
			{
				// todo: display error if 'report'?
				return;
			}

			if (updater.IsUpToDate &&
				(args.Length > 0) && (args[0] is bool report && report))
			{
				// up to date...
				using (var dialog = new UpdateDialog(updater))
				{
					dialog.TopMost = true;
					dialog.ShowDialog();
				}

				return;
			}

			DialogResult answer;
			using (var dialog = new UpdateDialog(updater))
			{
				dialog.TopMost = true;
				answer = dialog.ShowDialog();
			}

			if (answer == DialogResult.Yes)
			{
				Updated = await updater.Update();
			}
		}
	}
}
