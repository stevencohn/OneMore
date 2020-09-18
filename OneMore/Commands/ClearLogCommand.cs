//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.IO;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class ClearLogCommand : Command
	{
		public ClearLogCommand()
		{
		}


		public void Execute()
		{
			if (File.Exists(logger.LogPath))
			{
				var result = MessageBox.Show(
					Resx.ClearLog_Message,
					Resx.ClearLog_Title,
					MessageBoxButtons.YesNo, MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button1,
					MessageBoxOptions.DefaultDesktopOnly);

				if (result == DialogResult.Yes)
				{
					((Logger)logger).Clear();
				}
			}
			else
			{
				MessageBox.Show(
					Resx.ClearLog_NoneMessage,
					Resx.ClearLog_NoneTitle,
					MessageBoxButtons.OK,
					MessageBoxIcon.Information,
					MessageBoxDefaultButton.Button1,
					MessageBoxOptions.DefaultDesktopOnly);
			}
		}
	}
}
