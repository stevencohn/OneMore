//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Workspaces
{
	using System;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	/// <summary>
	/// Lists every currently open OneNote window, grouped by full page path so duplicate
	/// pages are easy to spot, and activates the one the user picks.
	/// </summary>
	internal class ShowWindowsCommand : Command
	{
		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote();
			var windows = await one.GetWindows();

			IntPtr handle;
			using (var dialog = new ShowWindowsDialog())
			{
				dialog.Populate(windows);
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				handle = dialog.SelectedHandle;
			}

			// only safe once the dialog (and its modal hold on OneNote's window) is gone
			if (handle != IntPtr.Zero)
			{
				Native.SetForegroundWindow(handle);
				Native.ShowWindow(handle, Native.SW_RESTORE);
			}
		}
	}
}
