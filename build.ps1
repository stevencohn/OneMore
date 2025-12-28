<#
.SYNOPSIS
Build OneMore full installer kit for the specified architecture, or default project builds.

.PARAMETER Architecture
Builds the installer kit for the specifies architecture: x86 (default), x64, ARM64, All, or x.
'x' is a shorthand for building x86 and x64, without the ARM64 build.

.PARAMETER Clean
Clean all projects in the solution, removing all bin and obj directories.
No build is performed.

.PARAMETER Detect
Detect the targeted CPU architecture of the specified DLL or EXE file.

.PARAMETER Fast
Build just the .csproj projects using default parameters:
OneMore, OneMorCalendar, OneMoreProtocolHandler, OneMoreSetupActions, and OneMoreTray.

.PARAMETER Kit
Skips recompiling the binaries, grabbing whatever is in the bin, and proceeds to build
the installer kit for the specified architecture.

.PARAMETER Local
Do not attempt to git restore the vdproj file. Keep the local version.
This is useful for testing vdproj changes without committing.

.PARAMETER Prep
Run DisableOutOfProcBuild. This only needs to be run once on a machine, or after upgrading
or reinstalling Visual Studio. It is required to build installer kits from the command line.
No build is performed.

.PARAMETER Stepped
When building All architectures, pause between each architecture build to allow examination
of output and configuration of vdproj. This is a debugging option.

.PARAMETER VLog
Enable verbose logging for MSBuild. This is useful for debugging build issues.
#>

[CmdletBinding(SupportsShouldProcess = $true)]
param (
	[ValidateSet('x86','x64','ARM64','All', 'x')]
	[string] $Architecture = 'x86',

	[ValidateScript({ Test-Path $_ -PathType Leaf })]
	[string] $Detect,

	[switch] $Clean,
	[switch] $Fast,
	[switch] $Kit,
	[switch] $Local,
	[switch] $Main,
	[switch] $Prep,
	[switch] $Stepped,
	[switch] $VLog
	)

Begin
{
	. "$PSScriptRoot\vdparser.ps1"

	$script:guid = '{88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61}'
	$script:checksums = @()

	# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
	# Helpers...

	function FindVisualStudio
	{
		$cmd = Get-Command devenv -ErrorAction:SilentlyContinue
		if ($cmd -ne $null)
		{
			$script:devenv = $cmd.Source
			$script:ideroot = Split-Path -Parent $devenv
			$script:vsregedit = Join-Path $ideroot 'VSRegEdit.exe'
			write-Host "... devenv found at $devenv" -Fore DarkGray
			return $true
		}

		$0 = 'C:\Program Files\Microsoft Visual Studio\18' # VS 2026
		if (FindVS $0) { return $true }

		$0 = 'C:\Program Files\Microsoft Visual Studio\2022'
		if (FindVS $0) { return $true }

		$0 = 'C:\Program Files (x86)\Microsoft Visual Studio\2019'
		return FindVS $0
	}

	function FindVS
	{
		param($vsroot)
		$script:devenv = Join-Path $vsroot 'Professional\Common7\IDE\devenv.com'
		if (!(Test-Path $devenv))
		{
			$script:devenv = Join-Path $vsroot 'Enterprise\Common7\IDE\devenv.com'
			if (!(Test-Path $devenv))
			{
				$script:devenv = Join-Path $vsroot 'Community\Common7\IDE\devenv.com'
				if (!(Test-Path $devenv))
				{
					Write-Host "devenv not found in $vsroot" -ForegroundColor Yellow
					return $false
				}
			}
		}

		$script:ideroot = Split-Path -Parent $devenv
		$script:vsregedit = Join-Path $ideroot 'VSRegEdit.exe'
		write-Host "... devenv found at $devenv" -Fore DarkGray
		return $true
	}

	function OneNoteRunning
	{
		$processId = gcim Win32_Process | select ProcessId, Name | `
			where { $_.Name -eq 'ONENOTE.EXE' } | `
			foreach { $_.ProcessId }

		if ($processId)
		{
			Write-Host "`n*** ONENOTE.EXE is running; please close it and try again" -Fore Red
			return $true
		}

		$processId = gcim Win32_Process | select ProcessId, CommandLine | `
			where { $_.CommandLine -and $_.CommandLine.Contains($guid) } | `
			foreach { $_.ProcessId }

		if ($processId)
		{
			Write-Host "`n*** OneMoreAddIn dllhost ($processId) is running; please close it and try again" -Fore Red
			return $true
		}

		return $false
	}

	# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
	# Single Commands...

	function CleanSolution
	{
		$pushpop = (!(Test-Path OneMore.sln))
		if ($pushpop) { Push-Location .. }
		CleanProject 'OneMore'
		CleanProject 'OneMoreCalendar'
		CleanProject 'OneMoreProtocolHandler'
		CleanProject 'OneMoreSetup'
		CleanProject 'OneMoreSetupActions'
		CleanProject 'OneMoreTray'
		if ($pushpop) { Pop-Location }
	}
	
	function CleanProject
	{
		param([string]$project)
		Push-Location $project
		try
		{
			Write-Host "... cleaning $project" -Fore DarkGray
			$progpref = $ProgressPreference
			$ProgressPreference = 'SilentlyContinue' 
			if (Test-Path bin) { Remove-Item bin -Recurse -Force -Confirm:$false | Out-Null }
			if (Test-Path obj) { Remove-Item obj -Recurse -Force -Confirm:$false | Out-Null }
			if (Test-Path Debug) { Remove-Item Debug -Recurse -Force -Confirm:$false | Out-Null }
			if (Test-Path Release) { Remove-Item Release -Recurse -Force -Confirm:$false | Out-Null }
			$ProgressPreference = $progpref
		}
		finally
		{
			Pop-Location
		}
	}

	function DisablOutOfProcBuild
	{
		$0 = Join-Path $ideroot 'CommonExtensions\Microsoft\VSI\DisableOutOfProcBuild'
		if (Test-Path $0)
		{
			Push-Location $0
			if (Test-Path .\DisableOutOfProcBuild.exe) {
				.\DisableOutOfProcBuild.exe
			}
			Pop-Location
			Write-Host '... disabled out-of-proc builds; reboot is recommended'
			return
		}
		Write-Host "*** could not find $0\DisableOutOfProcBuild.exe" -ForegroundColor Yellow
	}

	function DetectArchitecture
	{
		param($dllPath)

		$resolved = Resolve-Path $dllPath -ErrorAction SilentlyContinue
		if (-not $resolved) {
			Write-Error "File not found: $dllPath"
			return
		}

		$resolved = $resolved.Path

		# Open the file as a readonly binary stream
		$stream = [System.IO.File]::OpenRead($resolved)

		try {
			$reader = New-Object System.Reflection.PortableExecutable.PEReader $stream
			$machine = [int]$reader.PEHeaders.CoffHeader.Machine

			switch ($machine) {
				# some ARM64X/ARM64EC hybrids still present 0x8664
                # Without external tools or parsing CHPE metadata, we can't be 100% certain.
				0x8664 { 'x64' }

				0x014c { 'x86' }
				0xaa64 { 'ARM64' }
				0xA641 { "ARM64EC" }
				0xA64E { "ARM64X" }
				0x01c4 { 'ARM' }
				0x0200 { 'Itanium' }

				default { 'Unknown Architecture: 0x{0:X4}' -f $machine }
			}
		} finally {
			$stream.Close()
		}
	}


	# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
	# Fast...

	function BuildFast
	{
		Write-Host "`n... fast build with default configs" -ForegroundColor Cyan

		NugetRestore 'OneMore'
		BuildProject 'OneMore'
		
		if ($Main) { return }

		NugetRestore 'OneMoreTray'
		BuildProject 'OneMoreTray'

		NugetRestore 'OneMoreCalendar'
		BuildProject 'OneMoreCalendar'

		BuildProject 'OneMoreProtocolHandler'

		BuildProject 'OneMoreSetupActions'

		ReportArchitectures
	}

	function NugetRestore
	{
		param($name)
		Push-Location $name
		Write-Host "`n... restoring $name" -ForegroundColor DarkCyan

		$cmd = "nuget restore .\$name.csproj -solutiondirectory .."
		write-Host $cmd -ForegroundColor DarkGray
		Invoke-Expression $cmd

		Pop-Location
	}

	function BuildProject
	{
		param($name)
		Push-Location $name
		Write-Host "`n... building $name" -ForegroundColor DarkCyan

		# output file cannot exist before build
		if (Test-Path .\Debug\*)
		{
			Remove-Item .\Debug\*.* -Force -Confirm:$false
		}

		$cmd = ". '$devenv' .\$name.csproj /project $name /projectconfig 'Debug|AnyCPU' /build"
		write-Host $cmd -ForegroundColor DarkGray
		Invoke-Expression $cmd

		Pop-Location
	}

	# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
	# Kit...

	function Build
	{
		param($arc)
		$script:Architecture = $arc

		if ($Stepped -and $arc -ne 'ARM64')
		{
			Write-Host "`n... press Enter to continue with $arc build: " -Fore Magenta -nonewline
			Read-Host
		}

		CleanSolution
		RestoreSolution

		if (BuildSolution)
		{
			ReportArchitectures
			BuildKit
		}
	}

	function RestoreSolution
	{
		Write-Host "`n... restoring nuget packages" -ForegroundColor Cyan

		NugetRestore 'OneMore'
		NugetRestore 'OneMoreTray'
		NugetRestore 'OneMoreCalendar'
	}

	function BuildSolution
	{
		Write-Host "`n... building $Architecture solution" -ForegroundColor Cyan
		Write-Host

		SetBuildVerbosity 4

		try
		{
			$log = "$($env:TEMP)\OneMoreBuild.log"
			if (Test-Path $log) { Remove-Item $log -Force -Confirm:$false }

			$cmd = ". '$devenv' .\OneMore.sln /build 'Debug|$Architecture' /out '$log'"
			Write-Host $cmd -ForegroundColor DarkGray
			Invoke-Expression $cmd
		}
		finally
		{
			SetBuildVerbosity 1
		}

		$succeeded = 0;
		$failed = 1

		Get-Content $env:TEMP\OneMoreBuild.log -ErrorAction SilentlyContinue | `
			where { $_ -match '== Build: (\d+) succeeded, (\d+) failed'} | `
			select -last 1 | `
			foreach {
				$succeeded = $matches[1]
				$failed = $matches[2]
				$color = $failed -eq 0 ? 'Green' : 'Red'
				write-Host "`n... build completed: $succeeded succeeded, $failed failed" -ForegroundColor $color
			}

		return [bool]($succeeded -gt 0 -and $failed -eq 0)
	}

	function SetBuildVerbosity
	{
		param($level)
		if ($VLog)
		{
			$desc = $level -eq 4 ? 'enabling' : 'disabling'
			Write-Host "... $desc MSBuild verbose logging" -ForegroundColor DarkYellow
			$cmd = ". '$vsregedit' set local HKCU General MSBuildLoggerVerbosity dword $level`n | Out-Null"
			write-Host $cmd -ForegroundColor DarkGray
			Invoke-Expression $cmd
		}
	}

	function ReportArchitectures
	{
		#$arc = (DetectArchitecture .\OneMore\bin\$Architecture\Debug\River.OneMoreAddIn.dll)
		$arc = (DetectArchitecture .\OneMore\bin\Debug\River.OneMoreAddIn.dll)
		Write-Host "... OneMore: $arc" -ForegroundColor DarkGray

		ReportModuleArchitecture 'OneMoreCalendar'
		ReportModuleArchitecture 'OneMoreProtocolHandler'
		ReportModuleArchitecture 'OneMoreSetupActions'
		ReportModuleArchitecture 'OneMoreTray'
		Write-Host
	}

	function ReportModuleArchitecture
	{
		param($name)
		#$arc = (DetectArchitecture .\$name\bin\Debug\$name.exe)
		$arc = (DetectArchitecture .\OneMore\bin\Debug\$name.exe)
		Write-Host "... $name`: $arc" -ForegroundColor DarkGray
	}

	function BuildKit
	{
		Write-Host "`n... building $Architecture kit" -ForegroundColor Cyan
		Write-Host

		Push-Location OneMoreSetup
		$vdproj = Resolve-Path .\OneMoreSetup.vdproj

		PreserveVdproj $vdproj

		try
		{
			ConfigureSetupProject $vdproj

			$log = "$($env:TEMP)\OneMoreBuild.log"
			$cmd = ". '$devenv' .\OneMoreSetup.vdproj /build 'Debug|$Architecture' /project Setup /out '$log'"
			
			Write-Host
			Write-Host $cmd -ForegroundColor DarkGray

			if ($Stepped)
			{
				Write-Host "`n... press Enter to continue, S to skip Setup: " -Fore Magenta -nonewline
				if ((Read-Host) -eq 's') { return }
			}

			Invoke-Expression $cmd

			if ($LASTEXITCODE -eq 0)
			{
				$0 = Get-ChildItem .\Debug\OneMore_*.msi | select -first 1
				if (Test-Path $0)
				{
					# move msi to Downloads for safe-keeping and to allow next Platform build
					$1 = "$home\Downloads\OneMore_$productVersion`_Setup$Architecture.msi"
					Move-Item $0 $1 -Force -Confirm:$false
					Write-Host "... $Architecture MSI moved to $1" -ForegroundColor DarkYellow

					if (Get-Command checksum -ErrorAction SilentlyContinue)
					{
						if (Test-Path $1)
						{
							$sum = (checksum -t sha256 $1)
							Write-Host "... $Architecture checksum: $sum" -ForegroundColor DarkYellow
							$script:checksums += "$Architecture checksum: $sum"
						}
					}
				}
			}
		}
		finally
		{
			RestoreVdproj $vdproj
			Pop-Location
		}
	}

	function PreserveVdproj
	{
		param($vdproj)
		Write-Host '... preserving vdproj' -ForegroundColor DarkGray

		if ($Local) {
			Write-Host '... using local copy of vdproj' -Fore DarkGray
		} else {
			Write-Host '... restoring vdproj from git' -Fore DarkGray
			git restore $vdproj
		}

		Copy-Item $vdproj .\vdproj.tmp -Force -Confirm:$false
	}

	function RestoreVdproj
	{
		param($vdproj)
		Write-Host '... restoring vdproj' -ForegroundColor DarkGray
		$0 = (Resolve-Path .\vdproj.tmp)
		if (Test-Path $0)
		{
			Copy-Item $0 $vdproj -Force -Confirm:$false
			Remove-Item $0 -Force -Confirm:$false
		}
	}

	function ConfigureSetupProject
	{
		param($vdproj)

		$json = ConvertVdprojToJson $vdproj
		$folders = GetArcFolders $json

		$lines = (Get-Content $vdproj)

		$script:productVersion = $lines | `
			where { $_ -match '"ProductVersion" = "8:(.+?)"' } | `
			foreach { $matches[1] }

		Write-Host
		Write-Host "... configuring vdproj for $Architecture build of $productVersion" -ForegroundColor Yellow

		'' | Out-File $vdproj -nonewline

		$lines | foreach `
		{
			if ($_ -match '"OutputFileName" = "')
			{
				# "OutputFilename" = "8:Debug\\OneMore_v_Setupx86.msi"
				$line = $_.Replace('OneMore_v_', "OneMore_$($productVersion)_")
				$line.Replace('x86', $Architecture) | Out-File $vdproj -Append
			}
			elseif ($_ -match '"DefaultLocation" = "')
			{
				# "DefaultLocation" = "8:[ProgramFilesFolder][Manufacturer]\\[ProductName]"
				if ($Architecture -ge 'x86') {
					$_.Replace('ProgramFiles64Folder', 'ProgramFilesFolder') | Out-File $vdproj -Append
				} else {
					$_.Replace('ProgramFilesFolder', 'ProgramFiles64Folder') | Out-File $vdproj -Append
				}
			}
			elseif ($_ -match '"TargetPlatform" = "')
			{
				# x86 -> "3:0"
				# x64 -> "3:1"
				if ($Architecture -ge 'x86') {
					'"TargetPlatform" = "3:0"' | Out-File $vdproj -Append
				} else {
					'"TargetPlatform" = "3:1"' | Out-File $vdproj -Append
				}
			}
			elseif (($_ -match ' --x86'))
			{
				# "Name" = "8:OneMoreSetupActions --install --x86"
				# "Arguments" = "8:--install --x86"
				$_.Replace('x86', $Architecture) | Out-File $vdproj -Append
			}
			elseif ($_ -match '"SourcePath" = .*WebView2Loader\.dll"$')
			{
				if ($Architecture -ne 'x86')
				{
					$_.Replace('x86', $Architecture.ToLower()) | Out-File $vdproj -Append
				}
				else
				{
					$_ | Out-File $vdproj -Append
				}
			}
			elseif ($_ -match '"SourcePath" = .*SQLite.Interop\.dll"$')
			{
				if ($Architecture -eq 'x64')
				{
					$_.Replace('x86', 'x64') | Out-File $vdproj -Append
				}
				elseif ($Architecture -eq 'ARM64')
				{
					$_.Replace('bin\\x86\\Debug\\x86', 'bin\\ARM64\\Debug\\x64') | Out-File $vdproj -Append
				}
				else
				{
					$_ | Out-File $vdproj -Append
				}
			}
			elseif ($_.Trim() -eq """Folder"" = ""8:$($folders.x86)""" -and $Architecture -ne 'x86')
			{
				# SQLite.Interop.dll Folder location
				Write-Host "... updating SQLite.Interop x86 folder from $($folders.x86) to $($folders.x64)" -Fore DarkGray
				"""Folder"" = ""8:$($folders.x64)""" | Out-File $vdproj -Append
			}
			elseif ($_.Trim() -eq """Folder"" = ""8:$($folders.win86)""" -and $Architecture -ne 'x86')
			{
				# WebView2Loader.dll Folder location
				Write-Host "... updating WebView2Loader win-x86 folder from $($folders.win86) to $($folders.win64)" -Fore DarkGray
				"""Folder"" = ""8:$($folders.win64)""" | Out-File $vdproj -Append
			}
			elseif ($_ -notmatch '^"Scc')
			{
				$_ | Out-File $vdproj -Append
			}
		}
	}

	function GetArcFolders
	{
		param($json)

		$folder = $json.Deployable.Folder
		$folders = ($folder | ExplodeNoteProperties | where { $_.Property -eq '8:TARGETDIR' }).Folders
		# $json.Deployable.Folder['TARGETDIR'].Folders['8:x86'].omKey
		$x86 = $folders | ExplodeNoteProperties | where { $_.Name -eq '8:x86' } | select -expand omKey
		# $json.Deployable.Folder['TARGETDIR'].Folders['8:x64'].omKey
		$x64 = $folders | ExplodeNoteProperties | where { $_.Name -eq '8:x64' } | select -expand omKey

		$runtimes = ($folders | ExplodeNoteProperties | where { $_.Name -eq '8:runtimes' }).Folders

		# $json.Deployable.Folder['TARGETDIR'].Folders['8:runtimes']['8:win-x86'].Folders['8:native'].omKey
		$win86 = ($runtimes | `
			ExplodeNoteProperties | where { $_.Name -eq '8:win-x86' }).Folders | `
			ExplodeNoteProperties | where { $_.Name -eq '8:native' } | `
			select -expand omKey

		# $json.Deployable.Folder['TARGETDIR'].Folders['8:runtimes']['8:win-x64'].Folders['8:native'].omKey
		$win64 = ($runtimes | `
			ExplodeNoteProperties | where { $_.Name -eq '8:win-x64' }).Folders | `
			ExplodeNoteProperties | where { $_.Name -eq '8:native' } | `
			select -expand omKey

		$rex = '{[0-9A-F\-]+}:(_[0-9A-F]+)$'
		if ($x86 -match $rex) { $x86 = $matches[1] }
		if ($x64 -match $rex) { $x64 = $matches[1] }
		if ($win86 -match $rex) { $win86 = $matches[1] }
		if ($win64 -match $rex) { $win64 = $matches[1] }

		Write-Host
		Write-Host "... x86 folder: $x86" -Fore DarkGray
		Write-Host "... x64 folder: $x64" -Fore DarkGray
		Write-Host "... win-x86 folder: $win86" -Fore DarkGray
		Write-Host "... win-x64 folder: $win64" -Fore DarkGray

		return [PSCustomObject]@{
			x86 = $x86
			x64 = $x64
			win86 = $win86
			win64 = $win64
		}
	}

	function ReportChecksums
	{
		if ($script:checksums.Count -gt 0)
		{
			Write-Host "`n... checksums" -ForegroundColor Cyan
			$script:checksums | foreach { Write-Host "... $_" -ForegroundColor DarkYellow }
		}
	}
}
Process
{
	$script:verboseColor = $PSStyle.Formatting.Verbose
	$PSStyle.Formatting.Verbose = $PSStyle.Foreground.BrightBlack

	if (-not (FindVisualStudio)) { return }

	if ($Detect) { DetectArchitecture $Detect; return }

	if ($Prep) { DisablOutOfProcBuild; return }

	if (OneNoteRunning) { return }

	if ($Clean) { CleanSolution; return }

	if ($Fast) { BuildFast; return }

	if ($Kit) { BuildKit; return }

	if ($Architecture -eq 'All' -or $Architecture -eq 'x')
	{
		if ($Architecture -eq 'All') { Build 'ARM64' }

		Build 'x64'
		Build 'x86'

		ReportChecksums
	}
	else
	{
		if ($Architecture -eq 'arm64') { $Architecture = 'ARM64' }

		Build $Architecture
	}
}
End
{
	$PSStyle.Formatting.Verbose = $verboseColor
}
