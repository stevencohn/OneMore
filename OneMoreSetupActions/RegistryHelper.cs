//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using Microsoft.Win32;
	using System;
	using System.IO;
	using System.Runtime.InteropServices;
	using System.Security.AccessControl;
	using System.Security.Principal;

	internal static class RegistryHelper
	{
		public const string OneNoteID = "{88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61}";

		public static readonly RegistryRights DeleteRights =
			RegistryRights.EnumerateSubKeys |
			RegistryRights.Delete |
			RegistryRights.QueryValues |
			RegistryRights.ReadKey;

		public static readonly RegistryRights ReadRights =
			RegistryRights.EnumerateSubKeys |
			RegistryRights.QueryValues |
			RegistryRights.ReadKey;

		public static readonly RegistryRights WriteRights =
			RegistryRights.CreateSubKey |
			RegistryRights.EnumerateSubKeys |
			RegistryRights.QueryValues |
			RegistryRights.ReadKey |
			RegistryRights.SetValue |
			RegistryRights.WriteKey;


		#region Interop
		public const uint HKEY_USERS = 0x80000003;

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int RegLoadKey(uint hKey, string lpSubKey, string lpFile);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int RegUnLoadKey(uint hKey, string lpSubKey);
		#endregion Interop

		private static Logger logger;


		/// <summary>
		/// Sets the logger; only called from CustomAction base class
		/// </summary>
		/// <param name="logger"></param>
		public static void SetLogger(Logger logger)
		{
			RegistryHelper.logger = logger;
		}



		/// <summary>
		/// Copy a given Registry key and its descendants to a new target parent.
		/// </summary>
		/// <param name="source">The current key to copy</param>
		/// <param name="target">
		/// The new parent key into which source is copied; this must be writable
		/// </param>
		/// <returns>The newly created Registry key</returns>
		public static RegistryKey CopyTo(this RegistryKey source, RegistryKey target)
		{
			var clone = target.CreateSubKey(Path.GetFileName(source.Name), true);
			logger?.WriteLine($"created clone {clone.Name}");

			CopyTree(source, clone);
			return clone;
		}

		private static void CopyTree(RegistryKey source, RegistryKey target)
		{
			// copy all source.properties
			foreach (var name in source.GetValueNames())
			{
				logger?.WriteLine($"clone property {name} = {source.GetValue(name)} ({source.GetValueKind(name)})");
				target.SetValue(name, source.GetValue(name), source.GetValueKind(name));
			}

			// copy all source.subkeys
			foreach (var name in source.GetSubKeyNames())
			{
				using (var srckey = source.OpenSubKey(name))
				{
					using (var trgkey = target.CreateSubKey(name, true))
					{
						logger?.WriteLine($"created subkey {trgkey.Name}");
						CopyTree(srckey, trgkey);
					}
				}
			}
		}


		/// <summary>
		/// Returns the SID of the currently logged in user that is running the installer, 
		/// which may differ from the current user context because the installer may be
		/// impersonating an elevated user on behalf of the logged in user.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="note"></param>
		/// <returns></returns>
		public static string GetUserSid(string note)
		{
			var domain = Environment.GetEnvironmentVariable("USERDOMAIN");
			var username = Environment.GetEnvironmentVariable("USERNAME");

			var userdom = domain != null
				? $@"{domain.ToUpper()}\{username.ToLower()}"
				: username.ToLower();

			logger?.WriteLine($"translating user {userdom} to SID");

			var tries = 0;
			while (tries <= 2)
			{
				try
				{
					var account = new NTAccount(userdom);
					var sid = ((SecurityIdentifier)account.Translate(typeof(SecurityIdentifier))).ToString();
					logger?.WriteLine($"{note} for user {userdom} ({sid})");
					return sid;
				}
				catch (Exception exc)
				{
					tries++;
					logger?.WriteLine($"error translating, retrying {tries} of 2");
					logger?.WriteLine(exc);
					System.Threading.Thread.Sleep(200 * tries);
				}
			}

			logger?.WriteLine("fallback to search username in HKEY_USERS");

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


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sid">The SID of the user whose hive to load</param>
		/// <param name="path">The absolute path of the ntuser.dat to load</param>
		/// <returns>True if the load succeeds</returns>
		public static bool LoadUserHive(string sid, string path)
		{
			path = Path.Combine(path, "ntuser.dat");
			if (File.Exists(path))
			{
				try
				{
					var result = RegLoadKey(HKEY_USERS, sid, path);
					logger?.WriteLine($"loaded registry hive from {path} ({sid}) result=[{result}]");
					return result == 0;
				}
				catch (Exception exc)
				{
					logger?.WriteLine($"error loading registry hive from {path} ({sid})");
					logger?.WriteLine(exc);
					return false;
				}
			}

			return true;
		}


		/// <summary>
		/// Unload the ntuser.dat user hive that was loaded by LoadUserHive
		/// </summary>
		/// <param name="sid">The SID of the user whose hive to unload</param>
		/// <returns>True if the unload succeeds</returns>
		public static bool UnloadUserHive(string sid)
		{
			try
			{
				var result = RegUnLoadKey(HKEY_USERS, sid);
				logger?.WriteLine($"unloaded registry hive for {sid} result=[{result}]");
				return result == 0;
			}
			catch (Exception exc)
			{
				logger?.WriteLine($"error unloading registry hive for {sid}");
				logger?.WriteLine(exc);
				return false;
			}
		}
	}
}
