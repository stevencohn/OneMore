//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.IO;
	using System.Threading.Tasks;
	using River.OneMoreAddIn.Cli;

	internal class OpenLogCommand : Command, ICliCommand
	{
		public OpenLogCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		#region CLI Implementation

		public string CommandName => "OpenLog";


		public string Description => "Opens the log file in the default text editor";


		public CliParameterDefinition DefineParameters() => new();

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			if (File.Exists(logger.LogPath))
			{
				System.Diagnostics.Process.Start(logger.LogPath);
			}

			await Task.Yield();
		}
	}
}
