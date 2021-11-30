//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("OneMore Add-in for OneNote Custom Setup Actions")]
[assembly: AssemblyDescription("Custom Setup Actions")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("River Software")]
[assembly: AssemblyProduct("OneMore")]
[assembly: AssemblyCopyright("Copyright \u00a9 2021 Steven M Cohn. All rights reserved.")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: Guid("200381e7-37c7-4b30-9f5e-81fa44bf83cc")]

[assembly: AssemblyVersion(OneMoreSetupActions.AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(OneMoreSetupActions.AssemblyInfo.Version)]

namespace OneMoreSetupActions
{
	internal static class AssemblyInfo
	{
		public const string Version = "4.11";
	}
}
