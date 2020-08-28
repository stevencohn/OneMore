//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Windows.Forms;
	using River.OneMoreAddIn.Dialogs;


	internal class OutlineCommand : Command
	{
		public OutlineCommand() : base()
		{
		}


		public void Execute()
		{
			using (var dialog = new OutlineDialog())
			{
				var result = dialog.ShowDialog(owner);				
				if (result == DialogResult.OK)
				{
				}
			}

			//using (var manager = new ApplicationManager())
			//{
			//	var page = manager.CurrentPage();
			//	var ns = page.GetNamespaceOfPrefix("one");
			//}
		}
	}
}
