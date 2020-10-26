//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;


	internal class NavigateCommand : Command
	{
		public NavigateCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			var pageTag = (string)args[0];

			int retry = 0;
			while (retry < 4)
			{
				try
				{
					using (var one = new OneNote())
					{
						one.NavigateTo(pageTag);
					}

					retry = int.MaxValue;
				}
				catch (Exception exc)
				{
					retry++;
					var ms = 250 * retry;

					logger.WriteLine($"ERROR navigating to {pageTag}, retyring in {ms}ms", exc);
					System.Threading.Thread.Sleep(ms);
				}
			}
		}
	}
}
