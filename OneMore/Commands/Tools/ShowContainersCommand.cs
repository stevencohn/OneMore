//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Microsoft.Win32;
	using River.OneMoreAddIn.Helpers.Office;
    using System.Threading.Tasks;


	internal class ShowContainersCommand : Command
	{
		public ShowContainersCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var version = Office.GetOfficeVersion();
			var path = $@"SOFTWARE\Microsoft\Office\{version}\OneNote\Options\Other";

			using var key = Registry.CurrentUser.OpenSubKey(path, true);
			if (key is not null)
			{
				var value = key.GetValue("ShowNoteContainers") as int?;
				var setting = value.HasValue ? (value.Value == 1 ? 0 : 1) : 0;

				key.SetValue("ShowNoteContainers", setting, RegistryValueKind.DWord);

				logger.WriteLine($"ShowNoteContainers {(setting == 0 ? "off" : "on")}");
			}


			await Task.Yield();
		}
	}
}
