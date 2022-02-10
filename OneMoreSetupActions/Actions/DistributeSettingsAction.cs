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
	/// Distribute Registry settings to all known users on the current system.
	/// </summary>
	internal class DistributeSettingsAction : CustomAction
	{
		private sealed class Profile
		{
			public string Sid;
			public string ProfilePath;
		}

		public DistributeSettingsAction(Logger logger, Stepper stepper)
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
