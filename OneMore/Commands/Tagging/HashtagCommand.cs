//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Linq;
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
			PopulateTags(dialog, EventArgs.Empty);

			await dialog.RunModeless(null, 20); 
			
			await Task.Yield();
		}


		private void PopulateTags(object sender, EventArgs e)
		{
			var dialog = sender as HashtagDialog;

			var provider = new HashtagProvider();
			var names = provider.ReadTagNames();
			var recent = provider.ReadLatestTagNames();
			logger.WriteLine($"discovered {names.Count()} tags, {recent.Count()} mru");

			dialog.PopulateTags(names, recent);
		}
	}
}
