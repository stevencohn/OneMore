<#
.SYNOPSIS
Build OneMore full installer kit for the specified architecture, or default project builds.

.PARAMETER Architecture
Builds the installer kit for the specifies architecture: x86 (default), x64, ARM64, or All.

.PARAMETER Clean
Clean all projects in the solution, removing all bin and obj directories.
No build is performed.

.PARAMETER Fast
Build just the .csproj projects using default parameters:
OneMore, OneMorCalendar, OneMoreProtocolHandler, OneMoreSetupActions, and OneMoreTray.

.PARAMETER Prep
Run DisableOutOfProcBuild. This only needs to be run once on a machine, or after upgrading
or reinstalling Visual Studio. It is required to build installer kits from the command line.
No build is performed.
#>

[CmdletBinding(SupportsShouldProcess = $true)]
param (
	[ValidateSet('x86','x64','ARM64','All')][string] $Architecture = 'x86',
	[string] $Detect,
	[switch] $Clean,
	[switch] $Fast,
	[switch] $Prep
	)

Begin
{
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
			write-Verbose "... devenv found at $devenv"
			return $true
		}

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
		write-Verbose "... devenv found at $devenv"
		return $true
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
			Write-verbose "... cleaning $project"
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

	function DisablPrepOutOfProcBuild
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
		param($dllpath)

		$dllpath = resolve-Path $dllpath
		if (-not (Test-Path $DllPath)) {
			Write-Error "File not found: $DllPath"
			return
		}

		# Open the file as a binary stream
		$stream = [System.IO.File]::OpenRead($DllPath)
		try {
			$reader = New-Object System.Reflection.PortableExecutable.PEReader $stream
			$machine = $reader.PEHeaders.CoffHeader.Machine

			switch ([int]$machine) {
				0x014c { "x86" }
				0x0200 { "Itanium" }
				0x8664 { "x64" }
				0x01c4 { "ARM" }
				0xaa64 { "ARM64" }
				default { "Unknown Architecture: $machine" }
			}
		} finally {
			$stream.Close()
		}
	}

	# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
	# Fast...

	function BuildFast
	{
		Write-Host "... fast build with default configs" -ForegroundColor Yellow

		NugetRestore 'OneMore'
		BuildProject 'OneMore'

		NugetRestore 'OneMoreTray'
		BuildProject 'OneMoreTray'

		NugetRestore 'OneMoreCalendar'
		BuildProject 'OneMoreCalendar'

		BuildProject 'OneMoreProtocolHandler'

		BuildProject 'OneMoreSetupActions'
	}

	function NugetRestore
	{
		param($name)
		Push-Location $name

		$cmd = "nuget restore .\$name.csproj"
		write-Host $cmd -ForegroundColor DarkGray
		nuget restore .\$name.csproj -solutiondirectory ..

		Pop-Location
	}

	function BuildProject
	{
		param($name)
		Push-Location $name

		# output file cannot exist before build
		if (Test-Path .\Debug\*)
		{
			Remove-Item .\Debug\*.* -Force -Confirm:$false
		}

		$cmd = "$devenv .\$name.csproj /project $name /projectconfig 'Debug|AnyCPU' /build"
		write-Host $cmd -ForegroundColor DarkGray
		. $devenv .\$name.csproj /project $name /projectconfig 'Debug|AnyCPU' /build

		Pop-Location
	}

	# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
	# Kit...

	function BuildKit
	{
		CleanSolution

		NugetRestore 'OneMore'
		NugetRestore 'OneMoreTray'
		NugetRestore 'OneMoreCalendar'

		$log = "$($env:TEMP)\OneMoreBuild.log"
		if (Test-Path $log) { Remove-Item $log -Force -Confirm:$false }

		$cmd = "$devenv .\OneMore.sln /build ""Debug|$Architecture"" /out ""$log"""
		Write-Host $cmd -ForegroundColor DarkGray
		. $devenv .\OneMore.sln /build "Debug|$Architecture" /out $log

		$succeeded = 0; $failed = 1
		Get-Content $env:TEMP\OneMoreBuild.log -ErrorAction SilentlyContinue | where {
			$_ -match '== Build: (\d+) succeeded, (\d+) failed'
		} | select -last 1 | foreach {
			$succeeded = $matches[1]
			$failed = $matches[2]
			$color = $failed -eq 0 ? 'Green' : 'Red'
			write-Host
			write-Host "... build completed: $succeeded succeeded, $failed failed" -ForegroundColor $color
		}

		if ($succeeded -gt 0 -and $failed -eq 0)
		{
			write-host 'good'
		}
	}
}
Process
{
	$script:verboseColor = $PSStyle.Formatting.Verbose
	$PSStyle.Formatting.Verbose = $PSStyle.Foreground.BrightBlack

	if ($Detect) { DetectArchitecture $Detect; return }

	if (-not (FindVisualStudio)) { return }

	if ($Prep) { DisablPrepOutOfProcBuild; return }
	if ($Clean) { CleanSolution; return }

	if ($Fast) { BuildFast; return }

	BuildKit
}
End
{
	$PSStyle.Formatting.Verbose = $verboseColor
}

<#
Begin
{
	function PreserveVdproj
	{
		Write-Host '... preserving vdproj' -ForegroundColor DarkGray
		Copy-Item $vdproj .\vdproj.tmp -Force -Confirm:$false
	}

	function RestoreVdproj
	{
		Write-Host '... restoring vdproj' -ForegroundColor DarkGray
		Copy-Item .\vdproj.tmp $vdproj -Force -Confirm:$false
		Remove-Item .\vdproj.tmp -Force
	}

	function Configure
	{
		param([int]$bits)
		$config = $bits -eq 65 ? 'ARM64' : "x$bits"

		$lines = @(Get-Content $vdproj)

		$script:productVersion = $lines | `
			Where-Object { $_ -match '"ProductVersion" = "8:(.+?)"' } | `
			ForEach-Object { $matches[1] }

		Write-Host
		Write-Host "... configuring vdproj for $config build of $productVersion" -ForegroundColor Yellow

		'' | Out-File $vdproj -nonewline

		$lines | ForEach-Object `
		{
			if ($_ -match '"OutputFileName" = "')
			{
				# "OutputFilename" = "8:Debug\\OneMore_v_Setupx64.msi"
				$line = $_.Replace('OneMore_v_', "OneMore_$($productVersion)_")
				$line.Replace('x64', $config) | Out-File $vdproj -Append
			}
			elseif ($_ -match '"DefaultLocation" = "')
			{
				# "DefaultLocation" = "8:[ProgramFilesFolder][Manufacturer]\\[ProductName]"
				if ($bits -ge 64) {
					$_.Replace('ProgramFilesFolder', 'ProgramFiles64Folder') | Out-File $vdproj -Append
				} else {
					$_.Replace('ProgramFiles64Folder', 'ProgramFilesFolder') | Out-File $vdproj -Append
				}
			}
			elseif ($_ -match '"TargetPlatform" = "')
			{
				# x86 -> "3:0"
				# x64 -> "3:1"
				if ($bits -ge 64) {
					'"TargetPlatform" = "3:1"' | Out-File $vdproj -Append
				} else {
					'"TargetPlatform" = "3:0"' | Out-File $vdproj -Append
				}
			}
			#elseif ($_ -match '"SourcePath" = .*WebView2Loader\.dll"$')
			#{
			#    if ($bits -eq 64) {
			#        $_.Replace('\\x86', '\\x64') | Out-File $vdproj -Append
			#        $_.Replace('win-x86', 'win-x64') | Out-File $vdproj -Append
			#    } else {
			#        $_.Replace('\\x64', '\\x86') | Out-File $vdproj -Append
			#        $_.Replace('win-x64', 'win-x86') | Out-File $vdproj -Append
			#    }
			#}
			elseif (($_ -match '"Name" = "8:OneMoreSetupActions --install ') -or `
					($_ -match '"Arguments" = "8:--install '))
			{
				# "Name" = "8:OneMoreSetupActions --install --x86"
				# "Arguments" = "8:--install --x86"
				$_.Replace('x86', $config) | Out-File $vdproj -Append
			}
			elseif ($_ -notmatch '^"Scc')
			{
				$_ | Out-File $vdproj -Append
			}
		}
	}



	function Build
	{
		param([int]$bits)
		$config = $bits -eq 65 ? 'ARM64' : "x$bits"

		Write-Host "... building $config MSI" -ForegroundColor Yellow

		# output file cannot exist before build
		if (Test-Path .\Debug\*)
		{
			Remove-Item .\Debug\*.* -Force -Confirm:$false
		}

		if ($PSCmdlet.MyInvocation.BoundParameters["Verbose"].IsPresent)
		{
			Write-Host '... enabling MSBuild verbose logging' -ForegroundColor DarkYellow
			$cmd = ". '$vsregedit' set local HKCU General MSBuildLoggerVerbosity dword 4`n"
			write-Host $cmd -ForegroundColor DarkGray
			. $vsregedit set local HKCU General MSBuildLoggerVerbosity dword 4 | Out-Null
		}

		$cmd = "$devenv .\OneMoreSetup.vdproj /build ""Debug|$config"" /project Setup /projectconfig Debug /out `$env:TEMP\OneMoreBuild.log"
		write-Host $cmd -ForegroundColor DarkGray

		# build
		. $devenv .\OneMoreSetup.vdproj /build "Debug|$config" /project Setup /projectconfig Debug /out $env:TEMP\OneMorebuild.log

		# move msi to Downloads for safe-keeping and to allow next Platform build
		Move-Item .\Debug\*.msi $home\Downloads -Force
		Write-Host "... $config MSI copied to $home\Downloads\" -ForegroundColor DarkYellow

		#. $devenv .\OneMoreSetup.vdproj /clean

		if ($PSCmdlet.MyInvocation.BoundParameters["Verbose"].IsPresent)
		{
			Write-Host '... disabling MSBuild verbose logging' -ForegroundColor DarkYellow
			$cmd = ". '$vsregedit' set local HKCU General MSBuildLoggerVerbosity dword 1`n"
			write-Host $cmd -ForegroundColor DarkGray
			. $vsregedit set local HKCU General MSBuildLoggerVerbosity dword 1 | Out-Null
		}
	}
	
	function BuildConfig
	{
		param([int]$bits)
		$config = $bits -eq 65 ? 'ARM64' : "x$bits"

		CleanSolution

		Write-Host "... nuget restores" -ForegroundColor Yellow
		nuget restore ..\OneMore\OneMore.csproj
		nuget restore ..\OneMoreTray\OneMoreTray.csproj
		nuget restore ..\OneMoreCalendar\OneMoreCalendar.csproj

		PreserveVdproj

		try
		{
			Configure $bits
			Build $bits

			if ($checkable)
			{
				$sum = (checksum -t sha256 $home\Downloads\OneMore_$($productVersion)_Setup$config.msi)
				Write-Host "... $config checksum: $sum" -ForegroundColor DarkYellow
			}
		}
		finally
		{
			RestoreVdproj
		}
	}
}
Process
{
	# BUILD	

	Push-Location OneMoreSetup
	$script:vdproj = Resolve-Path .\OneMoreSetup.vdproj
		
	git restore $vdproj

	$checkable = Get-Command checksum -ErrorAction SilentlyContinue

	if ($ARM) { $ConfigBits = 65 }

	if ($AllConfigs)
	{
		# ordered so we end up with a clean 86 build for dev
		#BuildConfig 65
		BuildConfig 64
		BuildConfig 86
	}
	else
	{
		BuildConfig $ConfigBits
	}

	Pop-Location
}
#>