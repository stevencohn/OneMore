//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.IO;
	using System.Threading.Tasks;


	internal class OpenLogCommand : Command
	{
		public OpenLogCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


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
