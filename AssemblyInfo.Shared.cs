//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

#pragma warning disable CS0436	// Type redefined (AssemblyInfo)
#pragma warning disable S3903	// Types should be defined in named namespaces

[assembly: AssemblyTitle(AssemblyInfo.Title)]
[assembly: AssemblyDescription(AssemblyInfo.Product)]
[assembly: AssemblyConfiguration(AssemblyInfo.Configuration)]
[assembly: AssemblyCompany(AssemblyInfo.Company)]
[assembly: AssemblyProduct(AssemblyInfo.Product)]
[assembly: AssemblyCopyright(AssemblyInfo.Copyright)]
[assembly: ComVisible(false)]
[assembly: System.CLSCompliant(true)]

[assembly: Guid(AssemblyInfo.Guid)]

[assembly: AssemblyVersion(AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(AssemblyInfo.FileVersion)]

[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.MainAssembly)]


/// <summary>
/// Define the constants used across all projects in solution.
/// Note that the Guid and Title fields are defined in AssemblyInfo.cs
/// </summary>

internal static partial class AssemblyInfo
{
    public const string Product = "OneMore";
    public const string Description = Product + "Add-In for Microsoft OneNote 2016";
	public const string Company = "River Software";
	public const string Copyright = "Copyright \u00a9 2016 Steven M Cohn. All rights reserved.";

	/*
	 * NOTE - also update the version in the Setup project
	 * by clicking on the Setup project node in VS and update its properties
	 */
	public const string Version = "2.19";


	public const string FileVersion = Version;

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