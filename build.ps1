<#
.SYNOPSIS
Build OneMore full installer kit for the specified architecture, or default project builds.

.PARAMETER Architecture
Builds the installer kit for the specifies architecture: x86, x64 (default), ARM64, All, or x.
'x' is a shorthand for building x86 and x64, without the ARM64 build.

.PARAMETER Beta
Flags this as a beta version of the installer kit.

.PARAMETER Clean
Clean all projects in the solution, removing all bin and obj directories.
No build is performed. This is a standalone command that executes and exits.

.PARAMETER DetailedLog
Enable verbose logging for MSBuild. This is useful for debugging build issues.

.PARAMETER Detect
Detect and report the targeted CPU architecture of the specified DLL or EXE file.
This is a standalone command that executes and exits.

.PARAMETER EchoLog
When building with MSBuild, echo the build log to the console in addition to writing it to a
file. This is useful for debugging build issues.

.PARAMETER Fast
Build just the .csproj projects using default parameters: OneMore, OneMorCalendar, 
OneMoreProtocolHandler, OneMoreSetupActions, and OneMoreTray.
This is a standalone command that executes and exits.

.PARAMETER Kit
Skips recompiling the binaries, grabbing whatever is in the bin, and proceeds to build
the installer kit for the specified architecture.

.PARAMETER Main
When building with -Fast, only build the main OneMore add-in project, skipping the tray, 
calendar, protocol handler, and setup actions projects. This is a debugging option.

.PARAMETER Stepped
When building All architectures, pause between each architecture build to allow examination
of output and configuration of vdproj. This is a debugging option.

.PARAMETER Test
Run the OneMoreTests automation tests using vstest.console.exe. The test assembly must
already be built (run -Fast or a full build first). This is a standalone command that
executes and exits.

.COPYRIGHT
Copyright © 2016 Steven M Cohn. All rights reserved.
#>

[CmdletBinding(SupportsShouldProcess = $true)]
param (
	[ValidateSet('x86','x64','ARM64','All', 'x')]
	[string] $Architecture = 'x64',

	[ValidateScript({ Test-Path $_ -PathType Leaf })]
	[string] $Detect,

	[switch] $Beta,
	[switch] $Clean,
	[switch] $EchoLog,
	[switch] $Fast,
	[switch] $Kit,
	[switch] $Main,
	[switch] $Stepped,
	[switch] $Test,
	[switch] $DetailedLog
	)

Begin
{
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
		CleanProject 'OneMoreCli'
		CleanProject 'OneMoreProtocolHandler'
		CleanProject 'OneMoreSetup'
		CleanProject 'OneMoreSetupActions'
		CleanProject 'OneMoreTests'
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
				# some arm64X/arm64EC hybrids still present 0x8664
                # Without external tools or parsing CHPE metadata, we can't be 100% certain.
				0x8664 { 'x64' }

				0x014c { 'x86' }
				0xaa64 { 'ARM64' }
				0xA641 { 'ARM64EC' }
				0xA64E { 'ARM64X' }
				0x01c4 { 'ARM' }
				0x0200 { 'Itanium' }

				default { 'Unknown Architecture: 0x{0:X4}' -f $machine }
			}
		} finally {
			$stream.Close()
		}
	}


	function RunTests
	{
		Write-Host "`n... running OneMore tests" -ForegroundColor Cyan

		$testDll = '.\OneMoreTests\bin\Debug\OneMoreTests.dll'
		if (-not (Test-Path $testDll))
		{
			Write-Host "... test assembly not found at $testDll; run a build first" -ForegroundColor Red
			return $false
		}

		$vstest = Join-Path $script:ideroot 'Extensions\TestPlatform\vstest.console.exe'
		if (-not (Test-Path $vstest))
		{
			Write-Host "... vstest.console.exe not found at $vstest" -ForegroundColor Red
			return $false
		}

		$tempdir = $env:RUNNER_TEMP ?? $env:TEMP
		$trx = Join-Path $tempdir 'OneMoreTests.trx'

		$vstestArgs = @(
			$testDll,
			'/Platform:x64',
			'/Logger:trx;LogFileName=OneMoreTests.trx',
			"/ResultsDirectory:$tempdir"
		)
		Write-Host "... & '$vstest' $vstestArgs" -ForegroundColor DarkGray
		& $vstest @vstestArgs

		$exitCode = $LASTEXITCODE
		$color = $exitCode -eq 0 ? 'Green' : 'Red'
		Write-Host "`n... vstest exit code: $exitCode" -ForegroundColor $color
		return $exitCode -eq 0
	}

	# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
	# Fast...

	function BuildFast
	{
		Write-Host "`n... fast build with default configs ($Architecture)" -ForegroundColor Cyan

		NugetRestore 'OneMore'
		BuildProject 'OneMore'
		
		if ($Main) { return }

		NugetRestore 'OneMoreTray'
		BuildProject 'OneMoreTray'

		NugetRestore 'OneMoreCalendar'
		BuildProject 'OneMoreCalendar'

		NugetRestore 'OneMoreCli'
		BuildProject 'OneMoreCli'

		BuildProject 'OneMoreProtocolHandler'
		BuildProject 'OneMoreSetupActions'

		NugetRestore 'OneMoreTests'
		BuildProject 'OneMoreTests'

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

		$cmd = ". '$devenv' .\$name.csproj /project $name /projectconfig 'Debug|$Architecture' /build"
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
		NugetRestore 'OneMoreCli'
		NugetRestore 'OneMoreTests'
	}

	function BuildSolution
	{
		Write-Host "`n... building $Architecture solution" -ForegroundColor Cyan
		Write-Host

		$vsRoot = Split-Path -Parent (Split-Path -Parent $script:ideroot)
		$msbuild = Join-Path $vsRoot 'MSBuild\Current\Bin\MSBuild.exe'
		if (-not (Test-Path $msbuild))
		{
			Write-Host "... MSBuild not found at $msbuild" -ForegroundColor Red
			return $false
		}

		# RUNNER_TEMP is set by GitHub Actions; locally falls back to TEMP
		$tempdir = $env:RUNNER_TEMP ?? $env:TEMP
		$log = Join-Path $tempdir 'OneMoreBuild.log'

		$verbosity = $DetailedLog ? 'detailed' : 'minimal'
		Write-Host "& '$msbuild' .\OneMore.sln /p:Configuration=Debug /p:Platform=$Architecture /m /nologo /verbosity:$verbosity" -ForegroundColor DarkGray
		Write-Host "... build log: $log" -ForegroundColor DarkGray

		if ($EchoLog)
		{
			& $msbuild .\OneMore.sln `
				/p:Configuration=Debug `
				/p:Platform=$Architecture `
				/m /nologo /verbosity:$verbosity | Tee-Object -FilePath $log | Write-Host
		}
		else
		{
			& $msbuild .\OneMore.sln `
				/p:Configuration=Debug `
				/p:Platform=$Architecture `
				/m /nologo /verbosity:$verbosity | Out-File -FilePath $log
		}

		$exitCode = $LASTEXITCODE
		$color = $exitCode -eq 0 ? 'Green' : 'Red'
		Write-Host "`n... msbuild exit code: $exitCode" -ForegroundColor $color
		return $exitCode -eq 0
	}

	function ReportArchitectures
	{
		#$arc = (DetectArchitecture .\OneMore\bin\$Architecture\Debug\River.OneMoreAddIn.dll)
		$arc = (DetectArchitecture .\OneMore\bin\Debug\River.OneMoreAddIn.dll)
		Write-Host "... OneMore: $arc" -ForegroundColor DarkGray

		ReportModuleArchitecture 'OneMoreCalendar'
		ReportModuleArchitecture 'OneMoreCli'
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

		# Read product version from the built DLL
		$dllPath = '.\OneMore\bin\Debug\River.OneMoreAddIn.dll'
		if (-not (Test-Path $dllPath))
		{
			Write-Host "... $dllPath not found; run a solution build first" -ForegroundColor Red
			return
		}
		$ver = (Get-Item $dllPath).VersionInfo.ProductVersion
		# Normalise x.y.z.w → x.y.z (strip revision component)
		if ($ver -match '^(\d+\.\d+\.\d+)\.\d+$') { $ver = $matches[1] }
		$script:productVersion = $ver

		Write-Host "... building $Architecture kit for v$ver" -ForegroundColor Yellow

		# Locate MSBuild from the VS installation found by FindVisualStudio
		$vsRoot = Split-Path -Parent (Split-Path -Parent $script:ideroot)
		$msbuild = Join-Path $vsRoot 'MSBuild\Current\Bin\MSBuild.exe'
		if (-not (Test-Path $msbuild))
		{
			Write-Host "... MSBuild not found at $msbuild; ensure Visual Studio is installed" -ForegroundColor Red
			return
		}

		Push-Location OneMoreSetup
		try
		{
			$cmd = "& '$msbuild' OneMoreSetup.wixproj" +
				" /Restore" +
				" /p:Platform=$Architecture" +
				" /p:Configuration=Debug" +
				" /p:ProductVersion=$ver" +
				" /nologo /m"

			Write-Host $cmd -ForegroundColor DarkGray

			if ($Stepped)
			{
				Write-Host "`n... press Enter to continue, S to skip Setup: " -Fore Magenta -nonewline
				if ((Read-Host) -eq 's') { return }
			}

			Invoke-Expression $cmd

			if ($LASTEXITCODE -eq 0)
			{
				$msi = Get-ChildItem "bin\$Architecture\Debug\OneMore_*.msi" | Select-Object -First 1
				if ($msi)
				{
					# Build Burn bundle (.exe) while the MSI is still in its output location;
					# Bundle.wxs resolves the MSI via a relative path from OneMoreBundle/.
					$bundleExe = $null
					Push-Location ..\OneMoreBundle
					try
					{
						$bundleCmd = "& '$msbuild' OneMoreBundle.wixproj" +
							" /Restore" +
							" /p:Platform=$Architecture" +
							" /p:Configuration=Debug" +
							" /p:ProductVersion=$ver" +
							" /nologo /m"
						Write-Host $bundleCmd -ForegroundColor DarkGray
						Invoke-Expression $bundleCmd
						if ($LASTEXITCODE -eq 0)
						{
							$bundleExe = Get-ChildItem "bin\$Architecture\Debug\OneMore_*.exe" | Select-Object -First 1
						}
					}
					finally
					{
						Pop-Location
					}

					# move msi to Downloads for safe-keeping and to allow next Platform build
					$dest = "$home\Downloads\OneMore_${ver}_Setup${Architecture}.msi"
					Move-Item $msi $dest -Force -Confirm:$false
					Write-Host "... $Architecture MSI moved to $dest" -ForegroundColor DarkYellow

					if (Get-Command checksum -ErrorAction SilentlyContinue)
					{
						$sum = (checksum -t sha256 $dest)
						Write-Host "... $Architecture checksum: $sum" -ForegroundColor DarkYellow
						$script:checksums += "$Architecture checksum: $sum"
					}

					# move bundle to Downloads
					if ($bundleExe)
					{
						$exeDest = "$home\Downloads\OneMore_${ver}_Setup${Architecture}.exe"
						Move-Item $bundleExe $exeDest -Force -Confirm:$false
						Write-Host "... $Architecture bundle moved to $exeDest" -ForegroundColor DarkYellow

						if (Get-Command checksum -ErrorAction SilentlyContinue)
						{
							$sum = (checksum -t sha256 $exeDest)
							Write-Host "... $Architecture bundle checksum: $sum" -ForegroundColor DarkYellow
							$script:checksums += "$Architecture bundle checksum: $sum"
						}
					}
				}
			}
		}
		finally
		{
			Pop-Location
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

	if ($Test) { if (-not (RunTests)) { exit 1 }; return }

	if ($Beta) { $env:Beta = 'true' } else { Remove-Item env:Beta -ErrorAction SilentlyContinue }

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
		if ($Architecture -eq 'ARM64') { $Architecture = 'ARM64' }

		Build $Architecture
	}
}
End
{
	$PSStyle.Formatting.Verbose = $verboseColor
}
