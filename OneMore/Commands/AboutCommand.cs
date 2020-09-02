//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	internal class AboutCommand : Command
	{
		public AboutCommand ()
		{
		}


		public void Execute ()
		{
			logger.WriteLine("AboutCommand.Execute()");

			using (var dialog = new Dialogs.AboutDialog())
			{
				dialog.ShowDialog(owner);
			}
		}
	}
}
