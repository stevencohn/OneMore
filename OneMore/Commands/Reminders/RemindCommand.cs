//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;


	internal class RemindCommand : Command
	{
		public RemindCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{

			}

			await Task.Yield();
		}
	}
}
