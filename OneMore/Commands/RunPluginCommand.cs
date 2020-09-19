//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Dialogs;
	using System.IO;
	using System.Windows.Forms;


	internal class RunPluginCommand : Command
	{
		public RunPluginCommand()
		{
		}


		public void Execute()
		{
			string path = null;
			using (var dialog = new RunPluginDialog())
			{
				if (dialog.ShowDialog(owner) == DialogResult.OK)
				{
					path = dialog.PluginPath;	
				}
			}

			if (!string.IsNullOrEmpty(path) && File.Exists(path))
			{

			}
		}
	}
}
