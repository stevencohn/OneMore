//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;


	/// <summary>
	/// Duplicates the current line (paragraph), pasting it either above or below the
	/// current line.
	/// </summary>
	internal class DuplicateLineCommand : Command
	{

		public DuplicateLineCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			bool above = (bool)args[0];

			using var one = new OneNote(out var page, out var _);

			var cursor = page.GetTextCursor();
			if (cursor == null)
			{
				UIHelper.ShowInfo("Place the cursor on the paragraph to duplicate");
				return;
			}

			var copy = cursor.Parent.Clone()
				.RemoveID()
				.RemovePII()
				.RemoveSelections();

			// this logic appears to be backwards but we want to visually "move" the
			// cursor to the duplicate rather than keeping on the current paragraph...
			if (above)
			{
				cursor.Parent.AddAfterSelf(copy);
			}
			else
			{
				cursor.Parent.AddBeforeSelf(copy);
			}

			await one.Update(page);
		}
	}
}
