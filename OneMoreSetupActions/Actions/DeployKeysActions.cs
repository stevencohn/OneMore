//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using Microsoft.Win32;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;


	/// <summary>
	/// Deploys Registry settings to all known users on the current system. It does this by
	/// first testing if the user is logged in by looking in the active Registry profiles and
	/// updating the hive through there, or if the user is not logged in then updating their
	/// ntuser.dat file by loading it temporarily, modifying, and unloading it.
	/// </summary>
	internal class DeployKeysAction : CustomAction
	{
		private sealed class Profile
		{
			public string Sid;
			public string ProfilePath;
		}

		private readonly string[] templates;

		/*
		[HKEY_CURRENT_USER\SOFTWARE\Classes\AppID\{88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61}]
		"DllSurrogate"=""

		[HKEY_CURRENT_USER\SOFTWARE\Microsoft\Office\OneNote\AddIns\River.OneMoreAddIn]
		"LoadBehavior"=dword:00000003
		"Description"="Extension for OneNote"
		"FriendlyName"="OneMoreAddIn"

		[HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\River.OneMoreAddIn.dll]
		"Path"="C:\\Program Files (x86)\\River\\OneMoreAddIn\\River.OneMoreAddIn.dll"
		*/

		public DeployKeysAction(Logger logger, Stepper stepper)
			: base(logger, stepper)
		{
			new TrustedProtocolAction(logger, stepper)
				.GetPolicyPaths(out var policiesPath, out var policyPath);

			templates = new string[]
			{
				// first three are created by Setup.vdproj
				$@"SOFTWARE\Classes\AppID\{RegistryHelper.OneNoteID}",
				@"SOFTWARE\Microsoft\Office\OneNote\AddIns\River.OneMoreAddIn",
				@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\River.OneMoreAddIn.dll",
				// created by TrustedProcolAction
				$"{policiesPath}{policyPath}"
			};
		}


		//========================================================================================

		public override int Install()
		{
			logger.WriteLine();
			logger.WriteLine("DeployKeysAction.Install ---");

			//var sid = RegistryHelper.GetUserSid($"step {stepper.Step()}: deploying Registry keys");
			var profiles = GetProfiles(null);

			foreach (var profile in profiles)
			{
				bool transient;
				using (var hive = LoadUserProfileKey(profile.Sid, profile.ProfilePath, out transient))
				{
					if (hive != null)
					{
						foreach (var template in templates)
						{
							Copy(template, hive);
						}
					}
					else
					{
						logger.WriteLine($"user hive not loaded, skipping {profile.Sid}");
					}
				}

				if (transient)
				{
					if (!RegistryHelper.UnloadUserHive(profile.Sid))
					{
						logger.WriteLine($"continuing after unsuccessful user hive unload {profile.Sid}");
					}
				}
			}

			return SUCCESS;
		}


		private List<Profile> GetProfiles(string excludeSid)
		{
			var profiles = new List<Profile>();
			var key = Registry.LocalMachine
				.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList");

			if (key == null)
			{
				return profiles;
			}

			var names = key.GetSubKeyNames()
				.Where(n => n.StartsWith("S-1-5-21-") &&
				((excludeSid == null) || n != excludeSid));

			foreach (var name in names)
			{
				using (var prokey = key.OpenSubKey(name))
				{
					if (prokey.GetValue("ProfileImagePath") is string path)
					{
						profiles.Add(new Profile
						{
							Sid = name,
							ProfilePath = path
						});
					}
				}
			}

			logger.WriteLine($"found {profiles.Count} user profiles");
			return profiles;
		}


		private RegistryKey LoadUserProfileKey(string sid, string path, out bool transient)
		{
			transient = false;
			RegistryKey hive = null;

			try
			{
				hive = Registry.Users.OpenSubKey(sid);
				if (hive == null)
				{
					logger.WriteLine($"loading user hive from {path} ({sid})");
					if (RegistryHelper.LoadUserHive(sid, path))
					{
						hive = Registry.Users.OpenSubKey(sid);
						transient = hive != null;
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error loading user hive from {path} ({sid})");
				logger.WriteLine(exc);
			}

			return hive;
		}


		private void Copy(string keypath, RegistryKey hive)
		{
			using (var key = Registry.CurrentUser.OpenSubKey(keypath))
			{
				if (key != null)
				{
					logger.WriteLine($"copying template user key {keypath}");
					////key.CopyTo(hive);
				}
				else
				{
					logger.WriteLine($"template key not found {keypath}");
				}
			}
		}



		//========================================================================================

		public override int Uninstall()
		{
			logger.WriteLine();
			logger.WriteLine("DeployKeysAction.Uninstall ---");

			var sid = RegistryHelper.GetUserSid($"step {stepper.Step()}: withdrawing Registry keys");
			var profiles = GetProfiles(sid);

			foreach (var profile in profiles)
			{
				bool transient;
				using (var hive = LoadUserProfileKey(profile.Sid, profile.ProfilePath, out transient))
				{
					if (hive != null)
					{
						foreach (var template in templates)
						{
							Withdraw(template, hive);
						}
					}
					else
					{
						logger.WriteLine($"user hive not loaded, skipping {profile.Sid}");
					}
				}

				if (transient)
				{
					if (!RegistryHelper.UnloadUserHive(profile.Sid))
					{
						logger.WriteLine($"continuing after unsuccessful user hive unload {profile.Sid}");
					}
				}
			}

			return SUCCESS;
		}


		private void Withdraw(string keypath, RegistryKey hive)
		{
			var path = Path.GetDirectoryName(keypath);
			var name = Path.GetFileName(keypath);

			using (var key = hive.OpenSubKey(path))
			{
				if (key != null)
				{
					logger.WriteLine($"withdrawing {keypath}");
					////key.DeleteSubKey(name, false);
				}
				else
				{
					logger.WriteLine($"path already withdrawn {path}");
				}
			}
		}
	}
}
