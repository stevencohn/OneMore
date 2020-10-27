//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("OneMore Add-in for OneNote")]
[assembly: AssemblyDescription("OneMore Add-In for Microsoft OneNote 2016")]
[assembly: AssemblyConfiguration(River.OneMore.AssemblyInfo.Configuration)]
[assembly: AssemblyCompany("River Software")]
[assembly: AssemblyProduct(River.OneMore.AssemblyInfo.Product)]
[assembly: AssemblyCopyright("Copyright \u00a9 2016 Steven M Cohn. All rights reserved.")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

[assembly: Guid("EE5423E9-D70C-4553-9C61-5B8E9D973736")]

[assembly: AssemblyVersion(River.OneMore.AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(River.OneMore.AssemblyInfo.Version)]

[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.MainAssembly)]

// To use, open LINQPad and set Preferences/Advanced "Allows LINPAad to access internals"...
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LINQPadQuery")]


namespace River.OneMore
{
	internal static class AssemblyInfo
	{
		/*
		 * NOTE - also update the version in the Setup project
		 * by clicking on the Setup project node in VS and update its properties
		 */
		public const string Version = "3.4.1";

		public const string Product = "OneMore";

		public const string Configuration =
#if DEBUG
		"Debug"
#elif RELESE
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
