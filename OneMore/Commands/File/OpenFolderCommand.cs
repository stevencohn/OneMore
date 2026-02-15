//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.IO;
	using System.Threading.Tasks;


	internal class OpenFolderCommand : Command
	{
		private static bool commandIsActive = false;

		public OpenFolderCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (commandIsActive) { return; }
			commandIsActive = true;

			try
			{
				using var dialog = new OpenFolderDialog();
				var result = dialog.ShowDialog(owner);

				if (result != System.Windows.Forms.DialogResult.OK)
				{
					return;
				}

				var path = dialog.FolderPath;
				if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
				{
					// no need to check if the folder is already open as a notebook; OneNote will
					// handle that by navigating directly to the notebook for the user. This is
					// also true for subfolders within that notebook.

					// TODO: do we need to check if opening the parent folder of an open notebook?

					using var one = new OneNote();
					await one.OpenHierarchy(path);
				}
			}
			finally
			{
				commandIsActive = false;
			}
		}
	}
}
