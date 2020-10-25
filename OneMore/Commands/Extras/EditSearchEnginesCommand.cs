//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Dialogs;


	internal class EditSearchEnginesCommand : Command
	{
		public EditSearchEnginesCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var dialog = new SearchEngineDialog())
			{
				dialog.ShowDialog(owner);
			}

			ribbon.Invalidate();
		}
	}
}
