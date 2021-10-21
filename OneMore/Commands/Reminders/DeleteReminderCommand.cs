//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;


	internal class DeleteReminderCommand : Command
	{
		public DeleteReminderCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			logger.WriteLine("DeleteReminder!");
			await Task.Yield();
		}
	}
}
