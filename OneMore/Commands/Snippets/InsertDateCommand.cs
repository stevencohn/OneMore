//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Threading.Tasks;
	using Resx = Properties.Resources;

	internal class InsertDateCommand : Command
	{

		public InsertDateCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var includeTime = (bool)args[0];
			var text = DateTime.Now.ToString(includeTime ? "yyy-MM-dd hh:mm tt" : "yyy-MM-dd");

			var clipboard = new ClipboardProvider();
			await clipboard.StashState();

			try
			{
				var success = await clipboard.SetText(text, unicode: true);
				if (!success)
				{
					MoreMessageBox.ShowWarning(owner, Resx.Clipboard_locked);
					return;
				}

				await using var one = new OneNote();
				Native.SetForegroundWindow(one.WindowHandle);
				await ClipboardProvider.Paste();
			}
			finally
			{
				await clipboard.RestoreState();
			}
		}
	}
}
