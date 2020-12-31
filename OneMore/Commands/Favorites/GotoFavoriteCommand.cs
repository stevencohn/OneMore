//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Threading.Tasks;

	internal class GotoFavoriteCommand : Command
	{
		public GotoFavoriteCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var pageTag = (string)args[0];

			try
			{
				using (var one = new OneNote())
				{
					await one.NavigateTo(pageTag);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error navigating to {pageTag}", exc);
			}
		}
	}
}
