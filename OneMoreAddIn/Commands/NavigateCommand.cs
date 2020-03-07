//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	internal class NavigateCommand : Command
	{
		public NavigateCommand() : base()
		{
		}


		public void Execute(string pageId)
		{
			using (var manager = new ApplicationManager())
			{
				manager.NavigateTo(pageId);
			}
		}
	}
}
