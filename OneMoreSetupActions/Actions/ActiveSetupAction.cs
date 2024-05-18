//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using Microsoft.Win32;
	using System;


	/// <summary>
	/// Patches the Active Setup Version property, replacing dots with commas in the version
	/// string; this is required due to an historical oddity!
	/// </summary>
	/// <remarks>
	/// Active Setup will run msiexec once per user upon logon. The complete command is
	/// 
	///	    msiexec.exe /fu [ProductCode] /qn
	///
	/// where /fu is a repair opton (/f) to require all user-specific registry entries
	///   and /qn is flag to disable all UI for the installer
	/// </remarks>
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

			try
			{
				var p = $@"Software\Microsoft\Active Setup\Installed Components\{RegistryHelper.OneNoteID}";
				logger.WriteLine($"opening key HKLM:\\{p}");

				using var key = Registry.LocalMachine.OpenSubKey(p,
					RegistryKeyPermissionCheck.ReadWriteSubTree,
					RegistryHelper.WriteRights);

				if (key is not null)
				{
					var version = (string)key.GetValue("Version");
					if (!string.IsNullOrWhiteSpace(version))
					{
						var commas = version.Replace('.', ',');

						logger.WriteLine($"replacing '{version}' with '{commas}'");
						key.SetValue("Version", commas);
					}
					else
					{
						logger.WriteLine("active setup version not found");
					}
				}
				else
				{
					logger.WriteLine("active setup key not found, skipping version tweak");
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error tweaking active setup version");
				logger.WriteLine(exc);
				return FAILURE;
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
