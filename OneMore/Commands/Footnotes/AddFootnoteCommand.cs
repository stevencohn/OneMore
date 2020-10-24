//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{

	internal class AddFootnoteCommand : Command
	{

		public AddFootnoteCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var one = new OneNote())
			{
				new FootnoteEditor(one).AddFootnote();
			}
		}
	}
}
