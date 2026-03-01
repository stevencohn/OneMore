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
				var p = $@"Software\Microsoft\Active Setup\Installed Components\{RegistryHelper.OneMoreID}";
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
					logger.WriteLine("active setup key not found, creating it");
					ConfigureActiveSetup(p);
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


		private void ConfigureActiveSetup(string p)
		{
			var productCode = FindProductCode();
			if (productCode is null)
			{
				logger.WriteLine($"could not find ProductCode for {AssemblyInfo.ProductName}");
				return;
			}

			using var key = Registry.LocalMachine.CreateSubKey(p, writable: true)
				?? throw new InvalidOperationException($"unable to create or open registry key: {p}");

			// (Default)
			key.SetValue(null, $"River.{AssemblyInfo.ProductName}", RegistryValueKind.String);

			// productCode includes curly braces
			key.SetValue("StubPath", $@"msiexec.exe /fu {productCode} /qn", RegistryValueKind.String);

			var commas = AssemblyInfo.Version.Replace('.', ',');
			logger.WriteLine($"replacing '{AssemblyInfo.Version}' with '{commas}'");
			key.SetValue("Version", commas, RegistryValueKind.String);
		}


		private static string FindProductCode()
		{
			static string Search(RegistryView view)
			{
				const string uninstallRoot =
					@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

				using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
				using var root = baseKey.OpenSubKey(uninstallRoot);
				if (root is null)
				{
					return null;
				}

				foreach (var subKeyName in root.GetSubKeyNames())
				{
					using var sub = root.OpenSubKey(subKeyName);
					if (sub is null)
					{
						continue;
					}

					var displayName = sub.GetValue("DisplayName") as string;
					if (string.Equals(displayName, AssemblyInfo.ProductName,
						StringComparison.OrdinalIgnoreCase))
					{
						return subKeyName;
					}
				}

				return null;
			}

			return Search(RegistryView.Registry64)
				?? Search(RegistryView.Registry32);
		}


		//========================================================================================

		public override int Uninstall()
		{
			return SUCCESS;
		}
	}
}
