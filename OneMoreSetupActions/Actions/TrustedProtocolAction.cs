//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using Microsoft.Win32;
	using System;


	/// <summary>
	/// Along with the ProtocolHandlerAction this tells Windows to trust the onemore://
	/// protocol as safe.
	/// </summary>
	internal class TrustedProtocolAction : CustomAction
	{

		public TrustedProtocolAction(Logger logger, Stepper stepper)
			: base(logger, stepper)
		{
		}


		public void GetPolicyPaths(out string policiesPath, out string policyPath)
		{
			var version = GetVersion("Excel", 16);
			policiesPath = @"Software\Policies";
			policyPath = $@"Microsoft\Office\{version}\Common\Security\Trusted Protocols\All Applications\onemore:";
		}

		private Version GetVersion(string name, int latest)
		{
			using (var key = Registry.ClassesRoot.OpenSubKey($@"\{name}.Application\CurVer", false))
			{
				if (key != null)
				{
					// get default value string
					var value = (string)key.GetValue(string.Empty);
					// extract version number
					var version = new Version(value.Substring(value.LastIndexOf('.') + 1) + ".0");
					logger.WriteLine($"found Office version {latest}");
					return version;
				}
			}

			// presume latest
			logger.WriteLine($"defaulting Office version to {latest}");
			return new Version(latest, 0);
		}


		//========================================================================================

		public override int Install()
		{
			logger.WriteLine();
			logger.WriteLine("TrustedProtocolAction.Install ---");

			// Declares the onemore: protocol as trusted so OneNote doesn't show a security dialog

			/*
			[HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\
			Office\16.0\Common\Security\Trusted Protocols\All Applications\onemore:]
			*/

			var sid = RegistryHelper.GetUserSid("registering trusted protocol");

			GetPolicyPaths(out var policiesPath, out var policyPath);
			var path = $@"{policiesPath}\{policyPath}";

			logger.WriteLine($@"step {stepper.Step()}: opening HKCU:\{path}");

			// running as a custom action from the installer, this will run under an elevated
			// context as the System account (S-1-5-18) so we need to impersonate the current
			// user by referencing their hive from HKEY_USERS\sid\...
			// HKEY_CURRENT_USER will point to the System account's hive and we don't want that!
			// HKEY_USERS will only contain the SID keys of logged in users; these are transient
			// keys that are dynamically loaded and unloaded as users log in and log out.

			using (var hive = Registry.Users.OpenSubKey(sid))
			{
				var key = hive.OpenSubKey(path, false);
				if (key != null)
				{
					key.Dispose();
					logger.WriteLine("key already exists");
					return SUCCESS;
				}

				logger.WriteLine($@"step {stepper.Step()}: creating HKCU:\{path}");
				try
				{
					using (var polKey = hive.OpenSubKey(policiesPath,
						RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryHelper.Rights))
					{
						key = polKey.CreateSubKey(policyPath, false);
						if (key == null)
						{
							logger.WriteLine("key not created, returned null");
							return FAILURE;
						}

						key.Dispose();
					}
				}
				catch (Exception exc)
				{
					logger.WriteLine("error registering trusted protocol");
					logger.WriteLine(exc);
					return FAILURE;
				}

				// confirm
				var verified = SUCCESS;
				using (key = hive.OpenSubKey(path, false))
				{
					if (key != null)
					{
						logger.WriteLine($"key created {key.Name}");
					}
					else
					{
						logger.WriteLine("key not created");
						verified = FAILURE;
					}
				}

				return verified;
			}
		}


		//========================================================================================

		public override int Uninstall()
		{
			logger.WriteLine();
			logger.WriteLine("TrustedProtocolAction.Uninstall ---");

			var sid = RegistryHelper.GetUserSid("unregistering trusted protocol");
			using (var hive = Registry.Users.OpenSubKey(sid))
			{
				var version = GetVersion("Excel", 16);
				var path = $@"Software\Policies\Microsoft\Office\{version}\Common\Security\" +
					@"Trusted Protocols\All Applications\onemore:";

				logger.WriteLine($@"step {stepper.Step()}: deleting HKCU:\{path}");

				try
				{
					hive.DeleteSubKey(path, false);
				}
				catch (Exception exc)
				{
					logger.WriteLine("warning deleting trusted protocol");
					logger.WriteLine(exc);
					return FAILURE;
				}

				// confirm
				var verified = SUCCESS;
				using (var key = hive.OpenSubKey(path, false))
				{
					if (key == null)
					{
						logger.WriteLine("key deleted");
					}
					else
					{
						logger.WriteLine("key not deleted");
						verified = FAILURE;
					}
				}

				return verified;
			}
		}
	}
}
