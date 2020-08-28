//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
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
				dialog.ShowDialog(owner);
			}

			//using (var manager = new ApplicationManager())
			//{
			//	var page = manager.CurrentPage();
			//	var ns = page.GetNamespaceOfPrefix("one");
			//}
		}
	}
}
