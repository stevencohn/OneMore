//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{

	internal class AboutCommand : Command
	{
		public AboutCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var dialog = new AboutDialog())
			{
				dialog.ShowDialog(owner);
			}
		}
	}
}
