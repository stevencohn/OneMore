//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Commands.Search;
	using System.Windows.Forms;


	internal class SearchCommand : Command
	{
		private bool copySelections;


		public SearchCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			copySelections = false;
			using (var dialog = new SearchDialog())
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				copySelections = dialog.CopySelections;
			}

			using (var one = new OneNote())
			{
				one.SelectLocation("title", "description", OneNote.Scope.Sections, Callback);
			}
		}


		private void Callback(string nodeId)
		{
			if (string.IsNullOrEmpty(nodeId))
			{
				logger.WriteLine("search cancelled");
				return;

			}
			logger.WriteLine($"nodeId={nodeId} (copy:{copySelections})");
		}
	}
}
