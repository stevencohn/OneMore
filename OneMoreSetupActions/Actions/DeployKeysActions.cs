//************************************************************************************************
// Copyright © 20221 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using Microsoft.Win32;
	using System;
	using System.Collections.Generic;
	using System.Linq;


	/// <summary>
	/// Deploys Registry settings to all known users on the current system.
	/// </summary>
	internal class DeployKeysAction : CustomAction
	{
		private sealed class Profile
		{
			public string Sid;
			public string ProfilePath;
		}

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

		private const string AppID = @"SOFTWARE\Classes\AppID\{88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61}";
		private const string AddIn = @"SOFTWARE\Microsoft\Office\OneNote\AddIns\River.OneMoreAddIn";
		private const string OPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\River.OneMoreAddIn.dll";


		public DeployKeysAction(Logger logger, Stepper stepper)
			: base(logger, stepper)
		{
		}


		//========================================================================================

		public override int Install()
		{
			logger.WriteLine();
			logger.WriteLine("DistributeSettingsAction.Install ---");

			var sid = RegistryHelper.GetUserSid(logger, "mapping sid");
			var profiles = GetProfiles(sid);

			if (!profiles.Any())
			{
				return SUCCESS;
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
				.Where(n => n.StartsWith("S-1-5-21-") && n != excludeSid);

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

			return profiles;
		}



		//========================================================================================

		public override int Uninstall()
		{
			logger.WriteLine();
			logger.WriteLine("DistributeSettingsAction.Uninstall ---");

			return SUCCESS;
		}
	}
}
