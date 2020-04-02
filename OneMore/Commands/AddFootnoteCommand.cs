//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;


	internal class AddFootnoteCommand : Command
	{

		public AddFootnoteCommand() : base()
		{
		}


		public void Execute()
		{
			try
			{
				using (var manager = new ApplicationManager())
				{
					var editor = new FootnoteEditor(manager.CurrentPage(), manager);
					editor.AddFootnote();
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"Error executing {nameof(AddFootnoteCommand)}", exc);
			}
		}
	}
}
