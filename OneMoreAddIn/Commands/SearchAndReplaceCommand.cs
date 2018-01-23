//************************************************************************************************
// Copyright © 2018 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml;
	using System.Xml.Linq;


	internal class SearchAndReplaceCommand : Command
	{
		public SearchAndReplaceCommand () : base()
		{
		}


		public void Execute ()
		{
			try
			{
				_Execute();
			}
			catch (Exception exc)
			{
				logger.WriteLine("Error executing SearchAndReplaace", exc);
			}
		}

		private void _Execute()
		{
			DialogResult result = DialogResult.None;

			using (var dialog = new SearchAndReplaceDialog())
			{
				result = dialog.ShowDialog();
				// what =
				// with =
			}

			if (result == DialogResult.OK)
			{
				using (var manager = new ApplicationManager())
				{
					var page = manager.CurrentPage();
					var ns = page.GetNamespaceOfPrefix("one");


					//manager.UpdatePageContent(page);
				}
			}
		}
	}
}
