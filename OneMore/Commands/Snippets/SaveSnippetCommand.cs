//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


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

				await new ClipboardProvider().Copy();
			}

			var html = await new ClipboardProvider().GetHtml();
			if (string.IsNullOrWhiteSpace(html))
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
