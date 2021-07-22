//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

using System.Threading.Tasks;

namespace River.OneMoreAddIn.Commands
{

	internal class AboutCommand : Command
	{
		public AboutCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			using (var dialog = new AboutDialog())
			{
				dialog.ShowDialog(owner);
			}

			await Task.Yield();
		}
	}
}
