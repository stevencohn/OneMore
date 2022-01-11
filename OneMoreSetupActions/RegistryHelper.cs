//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using Microsoft.Win32;
	using System.IO;
	using System.Security.AccessControl;


	internal static class RegistryHelper
	{
		public static readonly RegistryRights Rights =
			RegistryRights.CreateSubKey |
			RegistryRights.EnumerateSubKeys |
			RegistryRights.QueryValues |
			RegistryRights.ReadKey |
			RegistryRights.SetValue |
			RegistryRights.WriteKey;


		/// <summary>
		/// Copy a given Registry key and its descendants to a new target parent.
		/// </summary>
		/// <param name="source">The current key to copy</param>
		/// <param name="target">
		/// The new parent key into which source is copied; this must be writable
		/// </param>
		/// <returns>The newly created Registry key</returns>
		public static RegistryKey CopyTo(this RegistryKey source, RegistryKey target, Logger logger)
		{
			var clone = target.CreateSubKey(Path.GetFileName(source.Name), true);
			logger.WriteLine($"created clone {clone.Name}");

			CopyTree(source, clone, logger);
			return clone;
		}

		private static void CopyTree(RegistryKey source, RegistryKey target, Logger logger)
		{
			// copy all source.properties
			foreach (var name in source.GetValueNames())
			{
				logger.WriteLine($"clone property {name} = {source.GetValue(name)} ({source.GetValueKind(name)})");
				target.SetValue(name, source.GetValue(name), source.GetValueKind(name));
			}

			// copy all source.subkeys
			foreach (var name in source.GetSubKeyNames())
			{
				using (var srckey = source.OpenSubKey(name))
				{
					using (var trgkey = target.CreateSubKey(name, true))
					{
						logger.WriteLine($"created subkey {trgkey.Name}");
						CopyTree(srckey, trgkey, logger);
					}
				}
			}
		}
	}
}
