//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;


	internal class StartTimerCommand : Command
	{

		public StartTimerCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var win = new TimerWindow();

			await win.RunModeless();
		}
	}
}
