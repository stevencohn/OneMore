# Upgrade the OneMore Installer
Convert the existing OneMore installer from a Visual Studio Installer Project
(.vdproj) to a modern WiX installer.

This initial lift is a pure migration from vdproj to Wix with no added
functionality beyond what is specified below.

Wix Toolset v7 CLI is installed on this machine, ready to use.

## Requirements

Use component-based design for modularity and maintainability.

Use Heat style harvesting where possible.

The output must be a fully buildable WiX v7 project that produces an MSI 
functionally equivalent to the original .vdproj.

There must be an installer capable of running on a Windows x86 machine that 
harvests the x86 executables built by the OneMore solution and installs those
executables, its components, and proper architecture versions of dependencies 
such as SQLite and WebView2Loader as described below.

There must be an installer capable of running on a Windows x64 machine that 
harvests the x64 executables built by the OneMore solution and installs those
executables, its components, and proper architecture versions of dependencies 
such as SQLite and WebView2Loader as described below.

There must be an installer capable of running on a Windows arm64 machine that 
harvests the arm64 executables built by the OneMore solution and installs those
executables, its components, and proper architecture versions of dependencies 
such as SQLite and WebView2Loader as described below.

## File System

The full .vdproj file is OneMoreSetup.vdproj.

The only additional resource file is Resources\Banner.jpg used as a
banner image in the installer UI.

Include the output of the OneMore projects including:
- OneMore\OneMore.vdproj
- OneMoreCalendar\OneMoreCalendar.vdproj
- OneMoreProtocolHandler\OneMoreProtocolHandler.vdproj
- OneMoreTray\OneMoreTray.vdproj

Include DLLs from NuGet packages and other dependencies such as Office Interop assemblies.

Copy updated files from OneMore\Colorizer\Languages to the output directory
Languages\ folder.

Copy updated files from OneMore\Colorizer\Themes to the output directory
Themes\ folder.

Copy updated files from OneMore\Themes to the user's %APPDATA%\Onemore\Themes\ folder.

In the case of SQLite, architecture based DLLs are found under the output folder
in x64\SQLite.Interop.dll, x86\SQLite.Interop.dll, and arm64\SQLite.Interop.dll.

In the case of WebView2Loader.dll, different architecture version are found
under the output folder runtimes\win-x64\native, runtimes\win-x86\native,
and runtimes\win-arm64\native.

## Registry Entries

The add-in requires COM registration and for the protocol handler. 
These registry entries must be included in the Wix installer.

## Custom Actions

The OneMoreSetupActions projects contains custom actions that needed to be included 
in the vdproj installer, some to ensure configuration and some to add setup
configuration beyond the capabilities of the vdproj. These custom actions must be 
included in the Wix installer as well.


## Launch Condition
The add-in requires .NET Framework 4.8 or later to be installed on the target machine.

## Suggestion Prompt (Ignore this, Claude!)
Convert OneMoreSetup\OneMoreSetup.vdproj it into a complete WiX v7 installer project 
following the specification in installer.md. Produce a buildable .wixproj, 
Product.wxs, and fragment files. Preserve all functionality including registry entries,
custom actions, file system, prerequisites, and user interface.
