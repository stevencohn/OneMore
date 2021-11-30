//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using Microsoft.Win32;
	using System;
	using System.Security.Principal;


	internal class TrustedProtocolDeployment : Deployment
	{

		public TrustedProtocolDeployment(Logger logger, Stepper stepper)
			: base(logger, stepper)
		{
		}


		//========================================================================================

		public override bool Install()
		{
			logger.WriteLine();
			logger.WriteLine("TrustedProtocolDeployment.Install ---");

			// Declares the onemore: protocol as trusted so OneNote doesn't show a security dialog

			/*
			[HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\
			Office\16.0\Common\Security\Trusted Protocols\All Applications\onemore:]
			*/

			var sid = GetUserSid("registering trusted protocol");

			var version = GetVersion("Excel", 16);
			var policiesPath = @"Software\Policies";
			var policyPath = $@"Microsoft\Office\{version}\Common\Security\Trusted Protocols\All Applications\onemore:";
			var path = $@"{policiesPath}\{policyPath}";

			logger.WriteLine($@"step {stepper.Step()}: opening HKCU:\{path}");

			// running as a custom action from the installer, this will run under an elevated
			// context as the System account (S-1-5-18) so we need to impersonate the current
			// user by referencing their hive from HKEY_USERS\sid\...
			// HKEY_CURRENT_USER will point to the System account's hive and we don't want that!

			using (var hive = Registry.Users.OpenSubKey(sid))
			{
				var key = hive.OpenSubKey(path, false);
				if (key != null)
				{
					key.Dispose();
					logger.WriteLine("key already exists");
					return true;
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
							return false;
						}

						key.Dispose();
					}
				}
				catch (Exception exc)
				{
					logger.WriteLine("error registering trusted protocol");
					logger.WriteLine(exc);
					return false;
				}

				// confirm
				var verified = true;
				using (key = hive.OpenSubKey(path, false))
				{
					if (key != null)
					{
						logger.WriteLine($"key created {key.Name}");
					}
					else
					{
						logger.WriteLine("key not created");
						verified = false;
					}
				}

				return verified;
			}
		}


		private string GetUserSid(string note)
		{
			var domain = Environment.GetEnvironmentVariable("USERDOMAIN");
			var username = Environment.GetEnvironmentVariable("USERNAME");

			var userdom = domain != null
				? $@"{domain.ToUpper()}\{username.ToLower()}"
				: username.ToLower();

			logger.WriteLine($"translating user {userdom} to SID");

			var tries = 0;
			while (tries <= 2)
			{
				try
				{
					var account = new NTAccount(userdom);
					var sid = ((SecurityIdentifier)account.Translate(typeof(SecurityIdentifier))).ToString();
					logger.WriteLine($"{note} for user {userdom} ({sid})");
					return sid;
				}
				catch (Exception exc)
				{
					tries++;
					logger.WriteLine(exc);
					logger.WriteLine($"error translating, retrying {tries} of 2");
					System.Threading.Thread.Sleep(200 * tries);
				}
			}

			logger.WriteLine("fallback to search username in HKEY_USERS");

			foreach (var sid in Registry.Users.GetSubKeyNames())
			{
				var key = Registry.Users.OpenSubKey($@"{sid}\Volatile Environment");
				if (key != null)
				{
					var vname = key.GetValue("USERNAME") as string;
					if (!string.IsNullOrEmpty(vname))
					{
						var vdomain = key.GetValue("USERDOMAIN") as string;

						var candidate = !string.IsNullOrEmpty(vdomain)
							? $@"{vdomain.ToUpper()}\{vname.ToLower()}"
							: vname.ToLower();

						if (candidate == userdom)
						{
							return sid;
						}
					}
				}
			}

			return null;
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

		public override bool Uninstall()
		{
			logger.WriteLine();
			logger.WriteLine("TrustedProtocolDeployment.Uninstall ---");

			var sid = GetUserSid("unregistering trusted protocol");
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
					return false;
				}

				// confirm
				var verified = true;
				using (var key = hive.OpenSubKey(path, false))
				{
					if (key == null)
					{
						logger.WriteLine("key deleted");
					}
					else
					{
						logger.WriteLine("key not deleted");
						verified = false;
					}
				}

				return verified;
			}
		}
	}
}
