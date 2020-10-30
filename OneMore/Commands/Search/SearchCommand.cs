//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Commands.Search;
	using System.Collections.Generic;
	using System.Windows.Forms;


	internal class SearchCommand : Command
	{
		private bool copySelections;
		private List<string> selections;


		public SearchCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			// search for keywords and find page

			copySelections = false;

			using (var dialog = new SearchDialog())
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				copySelections = dialog.CopySelections;
				selections = dialog.SelectedPages;
			}

			logger.WriteLine($"selected {selections.Count} pages");

			// choose where to copy/move the selected pages
			// This needs to be done here, on this thread; I've tried to play threading tricks
			// to do this from the SearchDialog but could find a way to prevent hanging

			using (var one = new OneNote())
			{
				one.SelectLocation("title", "description", OneNote.Scope.Sections, Callback);
			}
		}


		private void Callback(string nodeId)
		{
			if (string.IsNullOrEmpty(nodeId))
			{
				// cancelled
				return;
			}

			try
			{
				var action = copySelections ? "copying" : "moving";
				logger.Start($"..{action} {selections.Count} pages");
			}
			finally
			{
				logger.End();
			}
		}
	}
}
