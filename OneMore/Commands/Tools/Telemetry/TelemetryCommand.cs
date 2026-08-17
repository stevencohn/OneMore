//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;


	internal class TelemetryCommand : Command
	{
		public TelemetryCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			using var guard = EnterOnce();
			if (guard is null) { return; }

			using var dialog = new TelemetryDialog(factory);
			dialog.ShowDialog(owner);

			await Task.Yield();
		}
	}
}
