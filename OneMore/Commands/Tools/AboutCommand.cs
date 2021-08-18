//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;


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
