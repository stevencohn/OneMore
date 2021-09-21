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
			await new TimerWindow().RunModeless();
		}
	}
}
