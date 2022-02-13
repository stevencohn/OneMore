﻿//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using Microsoft.Win32;


	/// <summary>
	/// Patches the Active Setup Version property, replacing dots with commas in the version
	/// string; this is required due to an historical oddity!
	/// </summary>
	internal class ActiveSetupAction : CustomAction
	{

		public ActiveSetupAction(Logger logger, Stepper stepper)
			: base(logger, stepper)
		{
		}


		//========================================================================================

		public override int Install()
		{
			logger.WriteLine();
			logger.WriteLine("ActiveSetupAction.Install ---");

			using (var key = Registry.LocalMachine.OpenSubKey(
				$@"Software\Microsoft\Active Setup\Installed Components\{RegistryHelper.OneNoteID}",
				RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryHelper.Rights))
			{
				var version = (string)key.GetValue("Version");
				var commas = version.Replace('.', ',');

				logger.WriteLine($"replacing '{version}' with '{commas}'");
				key.SetValue("Version", commas);
			}

			return SUCCESS;
		}


		//========================================================================================

		public override int Uninstall()
		{
			return SUCCESS;
		}
	}
}
