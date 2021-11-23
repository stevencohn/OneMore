//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using WindowsInput;
	using WindowsInput.Native;
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

				//SendKeys.SendWait("^(c)");
				new InputSimulator().Keyboard
					.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);

				// SendWait can be very unpredictable so wait a little
				await Task.Delay(200);
			}

			var html = await SingleThreaded.Invoke(() =>
			{
				if (Win.Clipboard.ContainsText(Win.TextDataFormat.Html))
					return Win.Clipboard.GetText(Win.TextDataFormat.Html);
				else
					return null;
			});

			if (html == null || html.Length == 0)
			{
				return;
			}

			using (var dialog = new SaveSnippetDialog())
			{
				if (dialog.ShowDialog(Owner) != DialogResult.OK)
				{
					return;
				}

				await new SnippetsProvider().Save(html, dialog.SnippetName);

				ribbon.InvalidateControl("ribFavoritesMenu");
			}
		}
	}
}
