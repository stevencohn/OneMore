//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;
	using River.OneMoreAddIn.UI;
	using Resx = Properties.Resources;


	/// <summary>
	/// Pastes the contents of the clipboard as plain text.
	/// </summary>
	internal class PasteTextCommand : Command
	{

		public PasteTextCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var text = await ClipboardProvider.GetText();
			if (string.IsNullOrEmpty(text))
			{
				return;
			}

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
