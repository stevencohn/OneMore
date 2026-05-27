//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("OneMore Add-in for OneNote CLI")]
[assembly: AssemblyDescription("Interactive command-line runner for OneMore commands")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("River Software")]
[assembly: AssemblyProduct("OneMore")]
[assembly: AssemblyCopyright("Copyright © 2026 Steven M Cohn. All rights reserved.")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: Guid("ec9687bc-a329-44e1-9e47-fd1dda7c0258")]

[assembly: AssemblyVersion(OneMoreCli.AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(OneMoreCli.AssemblyInfo.Version)]

namespace OneMoreCli
{
	internal static class AssemblyInfo
	{
		public const string Version = "7.0.1";
	}
}
