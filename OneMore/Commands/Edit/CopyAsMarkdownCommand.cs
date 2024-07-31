//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System.Threading.Tasks;
	using Resx = Properties.Resources;


	/// <summary>
	/// Copy the page or selected content as markdown into the system clipboard
	/// </summary>
	internal class CopyAsMarkdownCommand : Command
	{
		public CopyAsMarkdownCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out var _);
			var cursor = page.GetTextCursor();

			var writer = new MarkdownWriter(page, false);

			if (// cursor is not null if selection range is empty
				cursor is not null &&
				// selection range is a single line containing a hyperlink
				!(page.SelectionScope == SelectionScope.TextCursor ||
				 page.SelectionScope == SelectionScope.SpecialCursor))
			{
				await writer.Copy(page.Root);
			}
			else
			{
				// selection range found so move it into snippet
				var editor = new PageEditor(page);
				var content = await editor.ExtractSelectedContent();

				await writer.Copy(content);
			}

			MoreBubbleWindow.Show(Resx.CopyAsMarkdownCommand_copied);
		}
	}
}
