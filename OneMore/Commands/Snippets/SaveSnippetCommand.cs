//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;
	using Win = System.Windows;


	internal class SaveSnippetCommand : Command
	{

		public SaveSnippetCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out _))
			{
				if (page.GetTextCursor() != null)
				{
					UIHelper.ShowMessage(Resx.SaveSnippet_NeedSelection);
					return;
				}

				// since the Hotkey message loop is watching all input, explicitly setting
				// focus on the OneNote main window provides a direct path for SendKeys
				Native.SetForegroundWindow(one.WindowHandle);
				SendKeys.SendWait("^(c)");
			}

			var html = await SingleThreaded.Invoke(() =>
			{
				// sending the Ctrl+C through COM interop seems to be slower than immediate
				// so added a retry loop to account for time warp through the wormhole...
				int tries = 0;
				string text = null;
				while (tries < 3 && string.IsNullOrEmpty(text))
				{
					if (Win.Clipboard.ContainsText(Win.TextDataFormat.Html))
						text = Win.Clipboard.GetText(Win.TextDataFormat.Html);

					if (string.IsNullOrEmpty(text))
					{
						tries++;
						Thread.Sleep(25 * tries);
					}
				}

				return text;
			});

			if (html == null || html.Length == 0)
			{
				return;
			}

			using (var dialog = new SaveSnippetDialog())
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				await new SnippetsProvider().Save(html, dialog.SnippetName);

				ribbon.InvalidateControl("ribFavoritesMenu");
			}
		}
	}
}
