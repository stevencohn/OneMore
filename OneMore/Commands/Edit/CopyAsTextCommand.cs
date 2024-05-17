//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System.Threading.Tasks;
	using Resx = Properties.Resources;


	/// <summary>
	/// Copy the page or selected content as plain text onto the system clipboard
	/// </summary>
	internal class CopyAsTextCommand : Command
	{

		public CopyAsTextCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			logger.StartClock();

			await using var one = new OneNote(out var page, out _);

			var reader = new PageReader(page);
			var text = reader.GetSelectedText();

			var success = await new ClipboardProvider().SetText(text, true);

			if (!success)
			{
				MoreMessageBox.ShowWarning(owner, Resx.Clipboard_locked);
				return;
			}

			logger.WriteTime("copied text");
		}
	}
}
