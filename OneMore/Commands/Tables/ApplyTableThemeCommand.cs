//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class ApplyTableThemeCommand : Command
	{
		public ApplyTableThemeCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var selectedIndex = (int)args[0];
			logger.WriteLine($"apply table theme ({selectedIndex})...");

			await Task.Yield();
		}
	}
}
