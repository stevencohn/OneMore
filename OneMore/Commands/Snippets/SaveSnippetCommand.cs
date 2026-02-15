//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal class SaveSnippetCommand : Command
	{
		private static bool commandIsActive = false;


		public SaveSnippetCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (commandIsActive) { return; }
			commandIsActive = true;

			try
			{
				await using var one = new OneNote(out var page, out _);

				var range = new Models.SelectionRange(page);
				range.GetSelection(allowNonEmpty: true);

				if (range.Scope != SelectionScope.Range &&
					range.Scope != SelectionScope.Run)
				{
					ShowError(Resx.SaveSnippet_NeedSelection);
					return;
				}

				// since the Hotkey message loop is watching all input, explicitly setting
				// focus on the OneNote main window provides a direct path for SendKeys
				Native.SetForegroundWindow(one.WindowHandle);

				await ClipboardProvider.Copy();

				var html = await ClipboardProvider.GetHtml();
				if (string.IsNullOrWhiteSpace(html))
				{
					logger.WriteLine($"{nameof(SaveSnippetCommand)} empty HTML");
					ShowError(Resx.SaveSnippet_NoContext);
					return;
				}

				using var dialog = new SaveSnippetDialog();
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				await new SnippetsProvider().Save(html, dialog.SnippetName);

				ribbon.InvalidateControl("ribCustomSnippetsMenu");
			}
			finally
			{
				commandIsActive = false; 
			}
		}
	}
}
