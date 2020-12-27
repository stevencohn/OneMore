//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Microsoft.Office.Core;
	using River.OneMoreAddIn.Settings;


	internal class SettingsCommand : Command
	{
		public SettingsCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var dialog = new SettingsDialog(args[0] as IRibbonUI))
			{
				dialog.ShowDialog(owner);
			}
		}
	}
}
