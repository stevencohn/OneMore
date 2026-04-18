//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;


	internal class TelemetryCommand : Command
	{
		private static bool commandIsActive = false;


		public TelemetryCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			if (commandIsActive) { return; }
			commandIsActive = true;

			try
			{
				using var dialog = new TelemetryDialog(factory);
				dialog.ShowDialog(owner);

				await Task.Yield();
			}
			finally
			{
				commandIsActive = false;
			}
		}
	}
}
