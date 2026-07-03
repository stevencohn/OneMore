//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("OneMore Add-in for OneNote")]
[assembly: AssemblyDescription("OneMore Add-In for Microsoft OneNote")]
[assembly: AssemblyConfiguration(River.OneMoreAddIn.AssemblyInfo.Configuration)]
[assembly: AssemblyCompany("River Software")]
[assembly: AssemblyProduct(River.OneMoreAddIn.AssemblyInfo.Product)]
[assembly: AssemblyCopyright("Copyright \u00a9 2016 Steven M Cohn. All rights reserved.")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

[assembly: Guid("EE5423E9-D70C-4553-9C61-5B8E9D973736")]

[assembly: AssemblyVersion(River.OneMoreAddIn.AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(River.OneMoreAddIn.AssemblyInfo.Version)]
[assembly: AssemblyInformationalVersion(River.OneMoreAddIn.AssemblyInfo.Version + River.OneMoreAddIn.AssemblyInfo.BuildTag)]

[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.MainAssembly)]

// To use, open LINQPad and set Preferences/Advanced "Allows LINPAad to access internals"...
[assembly: InternalsVisibleTo("LINQPadQuery")]
[assembly: InternalsVisibleTo("OneMoreCalendar")]
[assembly: InternalsVisibleTo("OneMoreCli")]
[assembly: InternalsVisibleTo("OneMoreTests")]
[assembly: InternalsVisibleTo("OneMoreTray")]


namespace River.OneMoreAddIn
{
	internal static class AssemblyInfo
	{
		public const string Version = "7.2.0";

		public const string BuildTag =
#if BETA
		" Beta"
#else
		""
#endif
		;

		public const string Product = "OneMore";

		public const string Configuration =
#if DEBUG
		"Debug"
#else
        "Release"
#endif
#if CODE_ANALYSIS
        + " (Code analysis)"
#endif
#if TRACE
		+ " (Trace)"
#endif
		;
	}
}
