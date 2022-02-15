//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;


	internal class CrawlWebPageCommand : Command
	{
		public CrawlWebPageCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await Task.Yield();
		}
	}
}

