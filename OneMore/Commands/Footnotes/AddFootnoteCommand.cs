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
					new FootnoteEditor(manager).AddFootnote();
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"Error executing {nameof(AddFootnoteCommand)}", exc);
			}
		}
	}
}
