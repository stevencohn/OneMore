//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Threading.Tasks;


	/// <summary>
	/// Inserts a new footnote from the current cursor to a reference at the bottom of the page
	/// </summary>
	internal class AddFootnoteCommand : Command
	{

		public AddFootnoteCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote();
			var editor = new FootnoteEditor(one);
			if (editor.ValidContext())
			{
				await editor.AddFootnote();
			}
		}
	}
}
