//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Microsoft.Office.Core;
	using River.OneMoreAddIn.Settings;
	using System.Threading.Tasks;


	/// <summary>
	/// Opens Settings on the Form Controls sheet.
	/// </summary>
	internal class ManageFormControlsCommand : Command
	{
		public ManageFormControlsCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var dialog = new SettingsDialog(args[0] as IRibbonUI);
			dialog.ActivateSheet(SettingsDialog.Sheets.FormControls);
			dialog.ShowDialog(owner);

			await Task.Yield();
		}
	}
}
