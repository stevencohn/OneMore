//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("OneMoreService")]
[assembly: AssemblyDescription("Windows Service for OneMore Add-In")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("River Software")]
[assembly: AssemblyProduct("OneMore")]
[assembly: AssemblyCopyright("Copyright \u00a9 2024 Steven M Cohn. All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
[assembly: Guid("18c12911-be44-426a-875a-576f5897fe26")]

[assembly: AssemblyVersion(OneMoreService.AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(OneMoreService.AssemblyInfo.Version)]

// To use, open LINQPad and set Preferences/Advanced "Allows LINPAad to access internals"...
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LINQPadQuery")]

namespace OneMoreService
{
	internal static class AssemblyInfo
	{
		public const string Version = "6.2.0";
	}
}
