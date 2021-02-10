//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Threading.Tasks;


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

		}
	}
}
