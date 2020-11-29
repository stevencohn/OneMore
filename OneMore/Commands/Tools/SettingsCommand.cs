//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;


	internal class SettingsCommand : Command
	{
		public SettingsCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var dialog = new SettingsDialog())
			{
				dialog.ShowDialog(owner);
			}
		}
	}
}
