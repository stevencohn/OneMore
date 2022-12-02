//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Threading.Tasks;


	/// <summary>
	/// Deletes the current footnote (by reference label or footnote context) and resequences
	/// remaining footnote on the page.
	/// </summary>
	internal class RemoveFootnoteCommand : Command
	{

		public RemoveFootnoteCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote();
			var editor = new FootnoteEditor(one);
			if (editor.ValidContext())
			{
				await editor.RemoveFootnote();
			}
		}
	}
}
