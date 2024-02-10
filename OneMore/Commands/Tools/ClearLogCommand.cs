//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.IO;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal class ClearLogCommand : Command
	{
		public ClearLogCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			if (File.Exists(logger.LogPath))
			{
				var result = UI.MoreMessageBox.Show(owner,
					Resx.ClearLog_Message, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
				{
					((Logger)logger).Clear();
				}
			}
			else
			{
				UI.MoreMessageBox.Show(owner,
					Resx.ClearLog_NoneMessage, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

			await Task.Yield();
		}
	}
}
