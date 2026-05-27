//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using System.IO;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal class ClearLogCommand : Command, ICliCommand
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
				var result = runningFromCli
					? DialogResult.Yes
					: UI.MoreMessageBox.Show(owner,
						Resx.ClearLog_Message, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
				{
					((Logger)logger).Clear();
				}
			}
			else if (!runningFromCli)
			{
				UI.MoreMessageBox.Show(owner,
					Resx.ClearLog_NoneMessage, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

			await Task.Yield();
		}


		#region CLI Implementation

		public string CommandName => "ClearLog";


		public string Description => "Clears the log file";



		public CliParameterDefinition DefineParameters() => new();

		#endregion CLI Implementation
	}
}
