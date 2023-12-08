//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;


	internal class HashtagCommand : Command
	{

		public HashtagCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote();
			using var dialog = new HashtagDialog(one.CurrentNotebookId, one.CurrentSectionId);
			await dialog.RunModeless(null, 20); 
		}
	}
}
