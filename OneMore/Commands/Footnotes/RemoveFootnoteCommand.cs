//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;


	internal class RemoveFootnoteCommand : Command
	{

		public RemoveFootnoteCommand() : base()
		{
		}


		public void Execute()
		{
			try
			{
				using (var manager = new ApplicationManager())
				{
					new FootnoteEditor(manager).RemoveFootnote();
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"Error executing {nameof(RemoveFootnoteCommand)}", exc);
			}
		}
	}
}
