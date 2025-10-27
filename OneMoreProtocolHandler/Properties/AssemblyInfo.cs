//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("OneMore Add-in for OneNote Protocol Handler")]
[assembly: AssemblyDescription("URL Protocol Handler")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("River Software")]
[assembly: AssemblyProduct("OneMore")]
[assembly: AssemblyCopyright("Copyright \u00a9 2021 Steven M Cohn. All rights reserved.")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: Guid("bd0fa4fc-8ca9-4056-93dd-14537a758d48")]

[assembly: AssemblyVersion(OneMoreProtocolHandler.AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(OneMoreProtocolHandler.AssemblyInfo.Version)]

namespace OneMoreProtocolHandler
{
	internal static class AssemblyInfo
	{
		public const string Version = "6.8.1";
	}
}
