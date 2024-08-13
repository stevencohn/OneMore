//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
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

			var writer = new MarkdownWriter(page, false);

			// discover selection scope
			var range = new SelectionRange(page);
			range.GetSelection();

			if (range.Scope == SelectionScope.None)
			{
				return;
			}

			if (range.Scope == SelectionScope.TextCursor ||
				range.Scope == SelectionScope.SpecialCursor)
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
