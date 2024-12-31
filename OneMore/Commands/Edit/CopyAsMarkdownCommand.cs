//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Threading.Tasks;
	using System.Xml.Linq;


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
			#region WriteMarkdown(PageEditor editor, MarkdownWriter writer, XElement start)
			async void WriteMarkdown(PageEditor editor, MarkdownWriter writer, XElement start)
			{
				var content = editor.ExtractSelectedContent(start);

				logger.Debug("markdown content");
				logger.Debug(content);
				logger.Debug("- - -");

				await writer.Copy(content);
			}
			#endregion WriteMarkdown

			await using var one = new OneNote(out var page, out var _);

			var writer = new MarkdownWriter(page, false);

			// discover selection scope
			var range = new SelectionRange(page);
			range.GetSelection();

			if (range.Scope == SelectionScope.None)
			{
				return;
			}

			try
			{
				var editor = new PageEditor(page);

				if (range.Scope == SelectionScope.TextCursor ||
					range.Scope == SelectionScope.SpecialCursor)
				{
					editor.AllContent = true;
					range.Deselect();

					logger.Debug(page.Root);

					foreach (var outline in page.BodyOutlines)
					{
						WriteMarkdown(editor, writer, outline);
					}
				}
				else
				{
					WriteMarkdown(editor, writer, null);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}

			//MoreBubbleWindow.Show(Resx.CopyAsMarkdownCommand_copied);
		}
	}
}
