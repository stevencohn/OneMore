//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Tools.Updater;
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
			var updater = new Updater();

			if (!await updater.FetchLatestRelease())
			{
				// todo: display error if 'report'?
				return;
			}

			if (updater.IsUpToDate)
			{
				if (args.Length > 0 && args[0] is bool report && report)
				{
					// up to date...
					using (var dialog = new UpdateDialog(updater))
					{
						dialog.ShowDialog(args.Length > 1 && args[0] is AboutDialog about
							? about : new OneNote().Window);
					}
				}

				return;
			}

			DialogResult answer;
			using (var dialog = new UpdateDialog(updater))
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
