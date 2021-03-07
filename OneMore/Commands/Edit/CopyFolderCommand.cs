//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Search;
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class CopyFolderCommand : Command
	{

		public CopyFolderCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote())
			{
				one.SelectLocation(
					Resx.SearchQF_Title, Resx.SearchQF_DescriptionCopy, 
					OneNote.Scope.Sections, Callback);
			}

			await Task.Yield();
		}


		private async Task Callback(string sectionId)
		{
			if (string.IsNullOrEmpty(sectionId))
			{
				// cancelled
				return;
			}

			logger.Start($"..copying folder ..");

			try
			{
				using (var one = new OneNote())
				{
					var service = new SearchServices(owner, one, sectionId);

					//await service.CopyPages(pageIds);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}
			finally
			{
				logger.End();
			}
		}
	}
}
