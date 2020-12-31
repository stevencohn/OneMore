//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

using System.Threading.Tasks;

namespace River.OneMoreAddIn
{

	internal class RemoveFootnoteCommand : Command
	{

		public RemoveFootnoteCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote())
			{
				await new FootnoteEditor(one).RemoveFootnote();
			}

			await Task.Yield();
		}
	}
}
