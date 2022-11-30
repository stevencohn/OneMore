//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
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
			using var one = new OneNote(out var page, out var _);
			var cursor = page.GetTextCursor();

			var writer = new MarkdownWriter(page, false);

			if (// cursor is not null if selection range is empty
				cursor != null &&
				// selection range is a single line containing a hyperlink
				!(page.SelectionSpecial && page.SelectionScope == SelectionScope.Empty))
			{
				await writer.Copy(page.Root);
			}
			else
			{
				// selection range found so move it into snippet
				var content = page.ExtractSelectedContent(out var firstParent);
				await writer.Copy(content);
			}

			MoreBubbleWindow.Show(Resx.CopyAsMarkdownCommand_copied);
		}
	}
}
