//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;


	internal class CompleteReminderCommand : Command
	{
		public CompleteReminderCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			logger.WriteLine("CompleteReminder!");
			await Task.Yield();
		}
	}
}
