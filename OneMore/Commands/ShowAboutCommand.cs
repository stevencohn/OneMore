//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{

	internal class ShowAboutCommand : Command
	{
		public ShowAboutCommand ()
		{
		}


		public void Execute ()
		{
			logger.WriteLine("ShowAboutCommand.Execute()");

			using (var dialog = new Dialogs.AboutDialog())
			{
				dialog.ShowDialog(owner);
			}
		}
	}
}
