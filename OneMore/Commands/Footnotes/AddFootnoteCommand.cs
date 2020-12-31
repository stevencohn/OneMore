//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

using System.Threading.Tasks;

namespace River.OneMoreAddIn
{

	internal class AddFootnoteCommand : Command
	{

		public AddFootnoteCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote())
			{
				await new FootnoteEditor(one).AddFootnote();
			}

			await Task.Yield();
		}
	}
}
