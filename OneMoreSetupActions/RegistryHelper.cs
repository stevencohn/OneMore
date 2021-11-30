//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
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
	}
}
