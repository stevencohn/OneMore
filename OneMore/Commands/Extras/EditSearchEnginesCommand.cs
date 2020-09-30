//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Dialogs;
	using System.Windows.Forms;


	internal class EditSearchEnginesCommand : Command
	{
		public EditSearchEnginesCommand()
		{
		}


		public void Execute()
		{
			using (var dialog = new SearchEngineDialog())
			{
				if (dialog.ShowDialog(owner) == DialogResult.OK)
				{

				}
			}
		}
	}
}
