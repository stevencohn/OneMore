//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("OneMoreTray")]
[assembly: AssemblyDescription("OneMore Add-In Tray Services")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("River Software")]
[assembly: AssemblyProduct("OneMore")]
[assembly: AssemblyCopyright("Copyright \u00a9 2024 Steven M Cohn. All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
[assembly: Guid("383abb76-4f97-4c78-ac59-bc851aa15b5a")]

[assembly: AssemblyVersion(OneMoreService.AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(OneMoreService.AssemblyInfo.Version)]

// To use, open LINQPad and set Preferences/Advanced "Allows LINPAad to access internals"...
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LINQPadQuery")]

namespace OneMoreService
{
	internal static class AssemblyInfo
	{
		public const string Version = "6.7.2";
	}
}
